using System;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PCGGraphModuleAttribute : Attribute
    {
        public readonly string namespaceName;
        public readonly bool registerAllMembers;
        public PCGGraphModuleAttribute() { }
        public PCGGraphModuleAttribute(string nameSpaceName = null, bool registerAllMembers = false)
        {
            this.namespaceName = nameSpaceName;
            this.registerAllMembers = registerAllMembers;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PCGGraphAttributeAttribute : Attribute
    {
        public readonly string name;
        public readonly object defaultValue;
        public PCGGraphAttributeAttribute() { }
        public PCGGraphAttributeAttribute(string name = null, object defaultValue = null)
        {
            this.name = name;
            this.defaultValue = defaultValue;
        }
    }
}
