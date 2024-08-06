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
