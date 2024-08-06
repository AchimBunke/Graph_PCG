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
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Achioto.Gamespace_PCG.Editor.Graph
{
    [InitializeOnLoad]
    public static class PCGModuleLoader
    {
        static PCGModuleLoader()
        {
            Instance_GraphLoaded();
            HGraph.Instance.GraphLoaded += Instance_GraphLoaded;
        }

        private static void Instance_GraphLoaded()
        {
            // Load default modules from package
            PCGGraphModuleManager.LoadModules(typeof(HGraph).Assembly);
            PCGGraphModuleManager.LoadModules(typeof(PCGModuleLoader).Assembly);

            // Load modules from project
            var executingAssembly = GetExecutingProjectAssembly();
            if (executingAssembly == null)
                return;
            PCGGraphModuleManager.LoadModules(executingAssembly);
        }

        private static Assembly GetExecutingProjectAssembly()
        {
            // Assuming the main project assembly is Assembly-CSharp
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly => assembly.GetName().Name == "Assembly-CSharp");
        }
    }
}

