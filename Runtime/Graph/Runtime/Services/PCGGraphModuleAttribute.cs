using System;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PCGGraphModuleAttribute : Attribute
    {
        public readonly string namespaceName;
        public readonly bool registerAllMembers;
        public readonly PCGModuleSerializationMode serializationMode;
        public PCGGraphModuleAttribute() { }
        public PCGGraphModuleAttribute(string nameSpaceName = null, bool registerAllMembers = false,
            PCGModuleSerializationMode serializationMode = PCGModuleSerializationMode.None)
        {
            this.namespaceName = nameSpaceName;
            this.registerAllMembers = registerAllMembers;
            this.serializationMode = serializationMode;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PCGGraphAttributeAttribute : Attribute
    {
        public readonly string name;
        public readonly object defaultValue;
        public readonly PCGModuleSerializationMode serializationMode;
        public PCGGraphAttributeAttribute() { }
        public PCGGraphAttributeAttribute(string name = null, object defaultValue = null,
            PCGModuleSerializationMode serializationMode = PCGModuleSerializationMode.None)
        {
            this.name = name;
            this.defaultValue = defaultValue;
            this.serializationMode = serializationMode;
        }
    }
    public enum PCGModuleSerializationMode
    {
        None,
        NonSerialized,
        Serialize
    }
}
