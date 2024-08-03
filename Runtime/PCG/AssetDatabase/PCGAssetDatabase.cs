using System.Collections.Generic;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Database
{
    public abstract class PCGAssetDatabase : MonoBehaviour
    {
        public abstract IEnumerable<string> GetAssetPaths();
    }
}