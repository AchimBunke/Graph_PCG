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

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{

    /// <summary>
    /// Editor for HGraphNode.
    /// </summary>
    [CustomEditor(typeof(HGraphSceneNode_Autoconnect), editorForChildClasses: true)]
    public class HGraphSceneNode_AutoconnectEditor : HGraphSceneNodeEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var v = base.CreateInspectorGUI();
            var parentProperty = serializedObject.FindProperty("_autoconnectSuperNode");
            var parentField = new PropertyField(parentProperty);
            parentField.BindProperty(parentProperty);
            v.Insert(0, parentField);
            return v;
        }
    }
}