using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Assets
{
    public class StaticAssetPlaceholder : AssetPlaceholder
    {
        public GameObject Asset { get; set; }

        public override GameObject GetAsset() => Asset;


    }
}
