using System.Linq;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Assets.Placeholders
{
    public class WeaponPlaceholder : AssetPlaceholder
    {
        public WeaponType WeaponType { get; set; }
        public GameObject[] AvailableWeapons { get; set; }

        public override GameObject GetAsset()
        {
            var possibleAssets = AvailableWeapons.Where(w => w.name.Contains(WeaponType.ToString())).ToArray();
            if (possibleAssets.Length == 0)
                return null;
            else
                return possibleAssets[UnityEngine.Random.Range(0, possibleAssets.Length)];
        }
    }
    public enum WeaponType
    {
        Sword,
        Staff,
        Axe,
        Shield,
        Musket
    }
}
