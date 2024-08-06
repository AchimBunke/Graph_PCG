/*
 * MIT License
 *
 * Copyright (c) 2024 Achim Bunke
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using System;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.PCG.Generator
{
    [ExecuteInEditMode]
    public abstract class HGraphGenerator : MonoBehaviour
    {
        protected ReactiveProperty<bool> _isGenerating = new ReactiveProperty<bool>(false);
        public IReadOnlyReactiveProperty<bool> IsGenerating => _isGenerating;

#if UNITY_EDITOR

#endif
        [SerializeField,Tooltip("Allows automatic triggering.")] protected bool _allowAutoInvocation = true;// allow auto triggering
        public bool AllowAutoInvocation => _allowAutoInvocation;
        [SerializeField,Tooltip("Allows triggering by generators on parent nodes.")] protected bool _allowHierarchicalInvocation = true;// Can be called by other generators
        public bool AllowHierarchicalInvocation => _allowHierarchicalInvocation;
        [SerializeField, Tooltip("Triggers other generators within child nodes.")] protected bool _allowHierarchicalDelegation = true;// Calls other generators
        public bool AllowHierarchicalDelegation => _allowHierarchicalDelegation;
        [SerializeField, Tooltip("Prevents content changes.")] protected bool _isContentLocked = false;// can generate
        public bool IsContentLocked => _isContentLocked;
        [SerializeField, Tooltip("Types for which this generator is triggered.")] protected string[] _generatorTypes = new string[0];// decides which sub-generators to call/if being called
        public string[] GeneratorTypes => _generatorTypes;

        [SerializeField, Tooltip("Generate before child generators or after.")] protected HGraphGenerationOrder _generationOrder = HGraphGenerationOrder.Last;
        public HGraphGenerationOrder GenerationOrder => _generationOrder;

        private void HierarchicalGeneratorDelegation(HGraphNode node, string[] generationTypes, Action<HGraphGenerator> generatorTask, bool prepare)
        {
            var sceneNode = node.SceneNode.Value;
            if (sceneNode != null)
            {
                // Get generators on scene node which can generate
                var gens = sceneNode.GetComponents<HGraphGenerator>().Where(g => g.CanGenerate(HGraphGenerationCallerType.Hierarchical, generationTypes)).ToArray();
                var pre_gens = gens.Where(g => g.GenerationOrder == HGraphGenerationOrder.First);
                var post_gens = gens.Where(g => g.GenerationOrder == HGraphGenerationOrder.Last);
                bool delegateGenerationToChildNodes = gens.Count() == 0 || gens.Any(g => g.AllowHierarchicalDelegation);
                if (prepare)
                {
                    foreach (var generator in gens)// Prepare ALL Generators
                    {
                        generator.PrepareGeneration();
                    }
                }
                foreach (var generator in pre_gens)// Execute Order=First Generators
                {
                    generatorTask.Invoke(generator);
                }
                if (delegateGenerationToChildNodes)// Delegate to Subnodes
                {
                    foreach (var child in node.GetChildren())
                    {
                        HierarchicalGeneratorDelegation(child, generationTypes, generatorTask, prepare);
                    }
                }
                foreach (var generator in post_gens)// Execute Order=Last Generators
                {
                    generatorTask.Invoke(generator);
                }
            }
            else// No scene node.. continue in children
            {
                foreach (var child in node.GetChildren())
                {
                    HierarchicalGeneratorDelegation(child, generationTypes, generatorTask, prepare);
                }
            }

        }

        public void Generate(HGraphGenerationCallerType callerType)
        {
            Generate(callerType, GeneratorTypes);
        }

        protected void Generate(HGraphGenerationCallerType callerType, string[] generationTypes)
        {
            if (!CanGenerate(callerType, generationTypes))
                return;
            PrepareGeneration();

            // Actual generation
            if (GenerationOrder == HGraphGenerationOrder.First)
                Generate();
            // Get hierarchical generators and call them
            if (AllowHierarchicalDelegation)
            {
                var hNode = GetComponent<HGraphSceneNode>();
                if (hNode != null && hNode.IsHGraphConnected)
                {
                    // All child nodes which have a scene node
                    var childNodes = hNode.NodeData.Value.GetChildren().ToList();
                    childNodes.Sort((a, b) =>
                    {
                        if (a.SceneNode.Value == null)
                            return 1;
                        if (b.SceneNode.Value == null)
                            return -1;
                        return a.SceneNode.Value.transform.GetSiblingIndex().CompareTo(b.SceneNode.Value.transform.GetSiblingIndex());
                    });
                    foreach (var childNode in childNodes)
                    {
                        HierarchicalGeneratorDelegation(childNode, generationTypes, (g) => g.Generate(), prepare: true);
                    }
                }
            }
            // Actual generation
            if (GenerationOrder == HGraphGenerationOrder.Last)
                Generate();
        }
        private bool CanGenerate(HGraphGenerationCallerType callerType, string[] generationTypes)
        {
            return !IsContentLocked &&
                    MatchesGenerationTypes(generationTypes) &&
                    MatchesAllowedCallerTypes(callerType);
        }
        private bool MatchesGenerationTypes(string[] generationTypes)
        {
            return GeneratorTypes.Length == 0 || generationTypes.Length == 0 ||
                generationTypes.Intersect(GeneratorTypes).Count() > 0;
        }
        private bool MatchesAllowedCallerTypes(HGraphGenerationCallerType callerType) => callerType switch
        {
            HGraphGenerationCallerType.User => true,
            HGraphGenerationCallerType.Data => AllowAutoInvocation,
            HGraphGenerationCallerType.Hierarchical => AllowHierarchicalInvocation && AllowAutoInvocation,
            _ => throw new System.NotImplementedException(),
        };

        protected abstract void PrepareGeneration();
        /// <summary>
        /// Calls internal PrepareGeneration. Use with care as it does not integrate in default generator execution flow.
        /// </summary>
        public void ForcePrepareGeneration() => PrepareGeneration();
        protected abstract void Generate();
        /// <summary>
        /// Calls internal generation. Use with care as it does not integrate in default generator execution flow.
        /// </summary>
        public void ForceGenerate() => Generate();

        public void Clear(HGraphGenerationCallerType callerType)
        {
            Clear(callerType, GeneratorTypes);
        }
        /// <summary>
        /// Clears the generated content.
        /// TODO: Order reverse or not?
        /// </summary>
        /// <param name="callerType"></param>
        /// <param name="generationTypes"></param>
        protected void Clear(HGraphGenerationCallerType callerType, string[] generationTypes)
        {
            if (!CanGenerate(callerType, generationTypes))
                return;

            // Actual generation
            if (GenerationOrder == HGraphGenerationOrder.First)
                Clear();
            // Get hierarchical generators and call them
            if (AllowHierarchicalDelegation)
            {
                var hNode = GetComponent<HGraphSceneNode>();
                if (hNode != null && hNode.IsHGraphConnected)
                {
                    // All child nodes which have a scene node
                    var childNodes = hNode.NodeData.Value.GetChildren();
                    foreach (var childNode in childNodes)
                    {
                        HierarchicalGeneratorDelegation(childNode, generationTypes, (g) => g.Clear(), prepare: false);
                    }
                }
            }
            // Actual generation
            if (GenerationOrder == HGraphGenerationOrder.Last)
                Clear();
        }
        protected abstract void Clear();
        /// <summary>
        /// Calls internal Clear. Use with care as it does not integrate in default generator execution flow.
        /// </summary>
        public void ForceClear() => Clear();
    }
    public enum HGraphGenerationCallerType
    {
        User,
        Data,
        Hierarchical
    }
    public enum HGraphGenerationOrder
    {
        First,
        Last
    }

    [CustomEditor(typeof(HGraphGenerator), true)]
    public class HGraphGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var generator = (HGraphGenerator)target;
            EditorGUI.BeginDisabledGroup(generator.IsGenerating.Value);
            if (GUILayout.Button("Generate"))
            {
                generator.Generate(HGraphGenerationCallerType.User);
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Clear Generation"))
            {
                generator.Clear(HGraphGenerationCallerType.User);
            }
        }
    }
}