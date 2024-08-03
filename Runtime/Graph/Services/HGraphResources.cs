using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Services
{
    public static class HGraphResources
    {
        public static bool IsHGraphIdValid(string id)
        {
            return !string.IsNullOrEmpty(id);
        }
        public static string CreateCategoryId(string categoryName)
        {
            return categoryName;
        }
        public static string CreateRelationId(HGraphNode sourceNode, HGraphNode targetNode) => CreateRelationId(sourceNode.HGraphId.Value, targetNode.HGraphId.Value);
        public static string CreateRelationId(string sourceNodeId, string targetNodeId)
        {
            return string.Compare(sourceNodeId, targetNodeId) < 0 ? $"{sourceNodeId}-{targetNodeId}" : $"{targetNodeId}-{sourceNodeId}";
        }
        public static string CreateAttributeRelationId(HGraphRelation relation, HGraphCategory category) => CreateAttributeRelationId(relation.HGraphId.Value, category.HGraphId.Value);
        public static string CreateAttributeRelationId(string relationId, string categoryId)
        {
            return $"{relationId}:{categoryId}";
        }
        public static (HGraphRelation relation, HGraphAttributeRelation attributeRelation) EvaluateAttributeRelationId(string hGraphAttributeRelationId)
        {
            var s = hGraphAttributeRelationId.Split(":");
            var relationId = s[0];
            var categoryId = s[1];
            if (HGraph.Instance.Relations.TryGetValue(relationId, out var relation) &&
                HGraph.Instance.Categories.TryGetValue(categoryId, out var category))
            {

                if (relation.AttributeRelations.TryGetValue(category.Name.Value, out var attributeRelation))
                {
                    return (relation, attributeRelation);
                }
            }
            return (null, null);
        }
    }
}
