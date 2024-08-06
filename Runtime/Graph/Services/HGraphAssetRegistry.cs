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

using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System.Collections.Generic;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Services
{
    public class HGraphAssetRegistry
    {
        private static HGraphAssetRegistry _instance;
        public static HGraphAssetRegistry Instance => _instance ?? (_instance = new HGraphAssetRegistry());

        /// <summary>
        /// Guids - HGraphId (node)
        /// </summary>
        public Dictionary<string, string> PrefabAssets { get; set; } = new();

        /// <summary>
        /// Guids - AssetData
        /// </summary>
        public Dictionary<string, HGraphAssetData> Assets { get; set; } = new();
        public void RegisterAsset(string assetGUID, HGraphAssetData data) => Assets.Add(assetGUID, data);
    }
}