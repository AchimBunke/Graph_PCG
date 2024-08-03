using System.Collections.Generic;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    [CreateAssetMenu(fileName = "GamespaceGraphRules", menuName = "ScriptableObjects/HGraph/Rules/RuleCollection", order = 1)]
    public class HGraphRuleCollection : ScriptableObject
    {
        public List<HGraphRule> Rules = new();
    }
}
