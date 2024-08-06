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
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    [Serializable]
    public class HGraphEnumSO : ScriptableObject
    {
        [SerializeField]
        public HGraphEnum Enum;
    }
    [Serializable]
    public class HGraphEnum : HGraphRuntimeBase
    {
        public bool Flags = false;
        public List<EnumEntry> Entries = new();

        private HGraphEnum()
        {

        }
        public static HGraphEnum Construct(string id)
        {
            var hEnum = new HGraphEnum();
            hEnum.HGraphId.Value = id;
            return hEnum;
        }
        public static HGraphEnum Construct(HGraphEnumData data)
        {
            var hEnum = Construct(data.id);  
            hEnum.Flags = data.Flags;
            foreach (var v in data.Entries)
            {
                hEnum.Entries.Add(v);
            }
            return hEnum;
        }

        public void Update(HGraphEnumData newData, bool additive = false)
        {      
            if (!additive)
            {
                var valuesToRemove = Entries.Except(newData.Entries);
                foreach (var toRemove in valuesToRemove)
                {
                    Entries.Remove(toRemove);
                }
            }
            Flags = newData.Flags;
            foreach (var toAdd in newData.Entries)
            {
               Entries.Add(toAdd);
            }
        }

        public int GetValue(string name)
        {
            var entry = Entries.FirstOrDefault(e => e.Name == name);
            return entry.Value;
        }
        public string GetStringValue(int intValue) => Flags ? throw new InvalidOperationException("Cannot get single value from flags enum") : Entries.First(e => e.Value == intValue).Name;
        public string[] GetFlags(int value)
        {
            if (!Flags)
                throw new InvalidOperationException("Cannot get flags from non-flags enum");
            List<string> flags = new List<string>();
            var includedFlags = Entries.Where(entry => (value & entry.Value) == entry.Value).ToList();
            if (includedFlags.Count == 1)
            {
                return new string[] { includedFlags[0].Name };
            }
            else if (includedFlags.Count > 1)
            {
                return includedFlags.Where(entry=> entry.Value != 0).Select(entry => entry.Name).ToArray();
            }
            else
            {
                return new string[] { "None" };
            }

            //if (Entries.Count(entry => entry.Value == value) == 1)
            //{
            //    flags.Add(Entries.First(e => e.Value == value).Name);
            //}
            //else
            //{
            //    var singleEntry = 

            //    return Entries.Where(entry => entry.Value != 0 && (value & entry.Value) == entry.Value)
            //        .Select(entry => entry.Name)
            //        .ToArray();
            //    //for (int i = 1; i < Entries.Count; i++)
            //    //{
            //    //    // Check if the flag is present in the value
            //    //    if ((value & Entries[i].Value) == )
            //    //    {
            //    //        flags.Add(Entries[i].Name);
            //    //    }
            //    //}
            //}
            //return flags.ToArray();
        }

        public bool IsExclusiveFlag(int value)
        {
            // Check if the value matches exactly one flag value
            return Entries.Exists(entry => entry.Value == value);
        }

        public bool IsCombinedValue(int value)
        {
            // Check if the value is a combination of flags
            return Entries.Any(entry => (value & entry.Value) == entry.Value && value != entry.Value && entry.Value != 0);
        }

    }
}
