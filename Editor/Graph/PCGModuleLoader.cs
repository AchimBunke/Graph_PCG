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

