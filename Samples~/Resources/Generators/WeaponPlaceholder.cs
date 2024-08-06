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

using Achioto.Gamespace_PCG.Runtime.PCG.Generators.Assets;
using System.Linq;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Samples.Resources
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
        Shield
    }
}
