using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services
{
    public static class HGraphSerializationController
    {
        public static JsonSerializerSettings JSONSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Newtonsoft.Json.Formatting.Indented,
        };
        public static XmlWriterSettings XmlWriterSettings = new XmlWriterSettings()
        {
            Encoding = Encoding.Unicode,
            Indent = true,
        };
        public enum SerializationTypes
        {
            JSON,
            XML
        }
        public static void SerializeToFile(HGraph graph, string filePath)
        {
            var serializedData = SerializeToData(graph);
            SerializeToFile(serializedData, filePath);
        }
        public static void SerializeToFile(PCGGraph graph, string filePath)
        {
            var serializedData = SerializeToData(graph);
            SerializeToFile(serializedData, filePath);
        }
        public static void SerializeToFile(HGraphData graphData, string filePath)
        {
            //
            string serializedData;
            if (filePath.EndsWith(".json"))
            {

                serializedData = SerializeToString(graphData, SerializationTypes.JSON);
            }
            else
            {
                serializedData = SerializeToString(graphData, SerializationTypes.XML);
            }

            if (!File.Exists(filePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.Create(filePath).Close();
            }
            File.WriteAllText(filePath, serializedData);
        }

        //public static void SerializePCGInputGraphFromToFile(string loadFilePath, string saveFilePath)
        //{
        //    var pcgGraph = new PCGGraphStructure();
        //    pcgGraph.Load(loadFilePath);
        //    pcgGraph.ApplyRules();
        //    pcgGraph.FlattenNodeAtributes();
        //    if (!File.Exists(saveFilePath))
        //    {
        //        Directory.CreateDirectory(Path.GetDirectoryName(saveFilePath));
        //        File.Create(saveFilePath).Close();
        //    }
        //    pcgGraph.Save(saveFilePath);
        //}
        //public static string SerializePCGInputGraphToString(HGraph graph)
        //{
        //    var pcgGraph = new PCGGraphStructure();
        //    pcgGraph.LoadFromGraph(graph);
        //    pcgGraph.ApplyRules();
        //    pcgGraph.FlattenNodeAtributes();
        //    return pcgGraph.Serialized();
        //}
        //public static HGraphData SerializeHGraph(HGraph graph)
        //{
        //    var pcgGraph = new PCGGraphStructure();
        //    pcgGraph.LoadFromGraph(graph);
        //    pcgGraph.ApplyRules();
        //    pcgGraph.FlattenNodeAtributes();
        //    var data = pcgGraph.GraphData;
        //    return data;
        //}

        //public static string SerializeToString(HGraph graph, SerializationTypes type)
        //{
        //    var data = SerializeToData(graph);
        //    return SerializeToString(data, type);
        //}
        public static string SerializeToString(HGraphData data, SerializationTypes type)
        {
            string serializedData;
            switch (type)
            {
                case SerializationTypes.JSON:
                    serializedData = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, JSONSerializerSettings);
                    break;
                case SerializationTypes.XML:
                    {
                        var sb = new StringBuilder();
                        using (XmlWriter xmlWriter = XmlWriter.Create(sb, XmlWriterSettings))
                        {
                            new XmlSerializer(typeof(HGraphData)).Serialize(xmlWriter, data);
                            serializedData = sb.ToString();
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            return serializedData;
        }
        public static HGraphData SerializeToData(HGraph graph)
        {
            var data = new HGraphData();
            data.enumData = new List<HGraphEnumData>();
            foreach (var d in graph.EnumDefinitions.Values)
            {
                data.enumData.Add(Serialize(d));
            }
            data.categories = new List<HGraphCategoryData>();
            foreach (var c in graph.Categories.Values)
            {
                data.categories.Add(Serialize(c));
            }
            data.nodes = new List<HGraphNodeData>();
            foreach (var n in graph.Nodes.Values)
            {
                data.nodes.Add(Serialize(n));
            }
            data.relations = new List<HGraphRelationData>();
            foreach (var r in graph.Relations.Values)
            {
                data.relations.Add(Serialize(r));
            }
            data.rulesGUID = "";
            if (graph.Rules.Value != null)
            {
                data.rulesGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(graph.Rules.Value));
            }
            return data;
        }
        public static HGraphData SerializeToData(PCGGraph graph)
        {
            var data = new HGraphData();
            data.enumData = graph.Enums.Values.ToList();
            data.categories = graph.Categories.Values.ToList();
            data.nodes = graph.Nodes.Values.ToList();
            data.relations = graph.Relations.Values.ToList();
            data.rulesGUID = graph.RulesGUID;
            return data;
        }

        public static HGraphData DeserializeFromFile(string filePath)
        {

            if (!File.Exists(filePath))
                throw new FileNotFoundException();
            var s = File.ReadAllText(filePath);
            HGraphData data;
            if (filePath.EndsWith(".json") && s.TrimStart().StartsWith("{"))
            {
                data = DeserializeFromString(s, SerializationTypes.JSON);
            }
            else
            {
                data = DeserializeFromString(s, SerializationTypes.XML);
            }
            return data;
        }
        public static HGraphData DeserializeFromString(string dataString, SerializationTypes type)
        {
            HGraphData data;
            if (type == SerializationTypes.JSON)
            {
                try
                {
                    data = JsonConvert.DeserializeObject<HGraphData>(dataString, JSONSerializerSettings);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            else if (type == SerializationTypes.XML)
            {
                var ss = new StringReader(dataString);
                data = new XmlSerializer(typeof(HGraphData)).Deserialize(ss) as HGraphData;
            }
            else throw new NotImplementedException();
            UpdateAttributeTypeData(data);
            return data;
        }
        internal static void UpdateAttributeTypeData(HGraphData data)
        {
            foreach (var n in data.nodes)
            {
                foreach (var a in n.attributes)
                {
                    UpdateAttributeDataType(a);
                }
            }
        }
        private static void UpdateAttributeDataType(HGraphAttributeData attributeData)
        {
            attributeData.type = attributeData.data.GetAttributeType();
        }
        private static HGraphEnumData Serialize(HGraphEnum hEnum)
        {
            var data = new HGraphEnumData();
            data.id = hEnum.HGraphId.Value;
            data.Entries = hEnum.Entries.ToList();
            data.Flags = hEnum.Flags;
            return data;
        }
        private static HGraphCategoryData Serialize(HGraphCategory category)
        {
            var data = new HGraphCategoryData();
            data.id = category.HGraphId.Value;
            data.name = category.Name.Value;
            data.type = category.Type.Value;
            data.defaultContent = category.DefaultData;
            data.displayColor = category.DisplayColor;
            data.minValue = category.MinValue;
            data.maxValue = category.MaxValue;
            return data;
        }
        private static HGraphNodeData Serialize(HGraphNode node)
        {
            var data = new HGraphNodeData();
            data.id = node.HGraphId.Value;
            data.name = node.Name.Value;
            data.superNode = node.SuperNode.Value;
            data.relations = node.Relations.ToList();
            data.attributes = node.LocalAttributes.Values.Select(a => Serialize(a)).ToList();
            data.spaceData = SerializeSpaceData(node);
            return data;
        }
        private static HGraphSpaceData SerializeSpaceData(HGraphNode node)
        {
            if (node.SceneNode.Value == null)
                return node.SpaceData;
            var sceneNode = node.SceneNode.Value;
            var space = sceneNode.NodeSpace;
            if (space == null)
            {
                return new HGraphSpaceData()
                {
                    nodePosition = sceneNode.transform.position,
                    isAtomic = true,
                    isImplicit = false,
                };
            }
            else
            {
                var data = new HGraphSpaceData()
                {
                    nodePosition = sceneNode.transform.position,
                    isAtomic = false,
                    isImplicit = space.ImplicitSpace,
                };

                if (space.Space is ColliderSpace cs)
                {
                    switch (cs.Collider)
                    {
                        case BoxCollider b:
                            {
                                data.colliderType = ColliderType.BoxCollider;
                                data.center = b.center;
                                data.size = b.size;
                                break;
                            }
                        case CapsuleCollider c:
                            {
                                data.colliderType = ColliderType.CapsuleCollider;
                                data.center = c.center;
                                data.radius = c.radius;
                                data.height = c.height;
                                data.direction = c.direction;
                                break;
                            }
                        case SphereCollider s:
                            {
                                data.colliderType = ColliderType.SphereCollider;
                                data.center = s.center;
                                data.radius = s.radius;
                                break;
                            }
                        default:
                            throw new NotImplementedException();
                    }
                }
                return data;
            }
        }
        private static HGraphRelationData Serialize(HGraphRelation relation)
        {
            var data = new HGraphRelationData();
            data.id = relation.HGraphId.Value;
            data.source = relation.Source.Value;
            data.target = relation.Target.Value;
            data.attributeRelations = relation.AttributeRelations.Values.Select(ar => Serialize(ar)).ToList();
            return data;
        }
        public static HGraphAttributeData Serialize(HGraphAttribute attribute)
        {
            var data = new HGraphAttributeData();
            //data.id = attribute.HGraphId.Value;
            data.category = attribute.Category.Value;
            data.type = attribute.Type;
            data.data = attribute.Data;
            return data;
        }
        //private static HGraphRuleData Serialize(HGraphRule hRule)
        //{
        //    var data = new HGraphRuleData();
        //    data.id = hRule.HGraphId.Value;
        //    data.Conditions = hRule.Conditions.ToList();
        //    data.TargetElementQuery = hRule.TargetElementQuery;
        //    data.ValueQuery = hRule.ValueQuery;
        //    data.TargetAttribute = hRule.TargetAttribute;
        //    return data;
        //}

        private static HGraphAttributeRelationData Serialize(HGraphAttributeRelation attributeRelation)
        {
            var data = new HGraphAttributeRelationData();
            data.id = attributeRelation.HGraphId.Value;
            data.category = attributeRelation.Category.Value;
            data.data = attributeRelation.Data.Value;
            return data;
        }

        public static string Serialize(HGraphAssetData assetData)
        {
            var json = JsonConvert.SerializeObject(assetData, Newtonsoft.Json.Formatting.Indented, JSONSerializerSettings);
            return json;
        }
        public static bool TryDeserializeAssetData(string assetData, out HGraphAssetData data)
        {
            try
            {
                data = JsonConvert.DeserializeObject<HGraphAssetData>(assetData, JSONSerializerSettings);
                if (data != null)
                {
                    foreach (var a in data.attributes)
                    {
                        UpdateAttributeDataType(a);
                    }
                }
                return data != null;
            }
            catch (JsonReaderException)
            {
                data = default;
                return false;
            }
        }


        public static HGraphAssetData CreateAssetDataFromNode(HGraphNode node)
        {
            var data = new HGraphAssetData();
            var serializedNode = Serialize(node);
            data.attributes = serializedNode.attributes.ToArray();
            return data;
        }
    }
}