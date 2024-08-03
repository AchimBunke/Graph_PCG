using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.PCG.Database;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Achioto.Gamespace_PCG.Runtime.Graph.PCG.Database
{
    public class PCGAssetManagerDatabase : PCGAssetDatabase
    {
        public override IEnumerable<string> GetAssetPaths()
        {
            return HGraphAssetRegistry.Instance.Assets.Keys.Select(k => AssetDatabase.GUIDToAssetPath(k));
        }
    }
}