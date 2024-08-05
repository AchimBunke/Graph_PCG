using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services
{
    /// <summary>
    /// Provides tools to generate and parse categories and attributes from PCGGraphModules.
    /// </summary>
    public static class PCGGraphModuleManager
    {
        const string prefix = "pcg:";
        static Dictionary<Type, PCGModule> modules = new Dictionary<Type, PCGModule>();
        static Dictionary<string, PCGAttributeModule> attributeModules = new Dictionary<string, PCGAttributeModule>();
        static Dictionary<string, PCGEnumModule> enumModules = new Dictionary<string, PCGEnumModule>();
        class PCGModule
        {
            public Type Type { get; set; }
            public string NamespaceName { get; set; }
            public bool RegisterAllMembers { get; set; }
            public PCGModuleSerializationMode SerializationMode {  get; set; }
            public Dictionary<string, PCGAttributeModule> Attributes { get; set; } = new Dictionary<string, PCGAttributeModule>();
            public PCGModule(string namespaceName, bool registerAllMembers, Type type, PCGModuleSerializationMode serializationMode) 
            {
                Type = type;
                NamespaceName = namespaceName;
                RegisterAllMembers = registerAllMembers;
                SerializationMode = serializationMode;
            }
        }
        class PCGAttributeModule
        {
            public string Name { get; set; }
            public string CategoryId { get; set; }
            public object DefaultValue {  get; set; }
            public PCGModuleSerializationMode SerializationMode { get; set; }
            public PCGAttributeModule(string categoryId, string name, object defaultValue, PCGModuleSerializationMode serializationMode)
            {
                CategoryId = categoryId;
                Name = name;
                DefaultValue = defaultValue;
                SerializationMode = serializationMode;
            }
        }
        class PCGEnumModule
        {
            public string EnumId { get; set; }
            public PCGModuleSerializationMode SerializationMode { get; set; }
            public PCGEnumModule(string enumId, PCGModuleSerializationMode serializationMode) 
            {  
                SerializationMode = serializationMode; 
                EnumId = enumId;
            }
        }

        public static bool IsModuleCategory(string categoryName) => attributeModules.ContainsKey(categoryName);
        public static bool IsModuleEnum(string enumName) => enumModules.ContainsKey(enumName);
        public static bool SerializeModuleCategory(string categoryName) => attributeModules[categoryName].SerializationMode == PCGModuleSerializationMode.Serialize;
        public static bool SerializeModuleEnum(string enumName) => enumModules[enumName].SerializationMode == PCGModuleSerializationMode.Serialize;

        /// <summary>
        /// Load all modules within an assembly into the current HGraph.
        /// </summary>
        /// <param name="assembly"></param>
        public static void LoadModules(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(t => (t.IsClass || t.IsValueType) && t.IsDefined(typeof(PCGGraphModuleAttribute), true));
            foreach (var type in types)
            {
                LoadModule(type);
            }
        }
        private static void LoadModule(Type type)
        {
            var att = type.GetCustomAttribute<PCGGraphModuleAttribute>();
            var namespaceName = att.namespaceName;
            if (!modules.TryGetValue(type, out var pcgModule))
            {
                pcgModule = new PCGModule(namespaceName, att.registerAllMembers, type, att.serializationMode);
                modules.Add(type, pcgModule);
            }

            foreach (var member in
                type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => att.registerAllMembers || m.IsDefined(typeof(PCGGraphAttributeAttribute), true))
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => att.registerAllMembers || m.IsDefined(typeof(PCGGraphAttributeAttribute), true))))

            {

                object instance = null;
                if (IsStruct(type))
                {
                    var defaultProperty = type.GetProperty("Default", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (defaultProperty != null && defaultProperty.PropertyType == type)
                        instance = defaultProperty.GetValue(type);
                    else
                        instance = Activator.CreateInstance(type);
                }

                LoadMember(member, namespaceName, instance, pcgModule);
            }
        }
        private static bool IsStruct(Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }
        private static string MemberToAttributeName(string memberName, string namespaceName)
        {
            var namespacePart = string.IsNullOrWhiteSpace(namespaceName) ? "" : (namespaceName + ":");
            return prefix + namespacePart + memberName;
        }
        private static void LoadMember(MemberInfo memberInfo, string namespaceName, object instance, PCGModule module)
        {
            var att = memberInfo.GetCustomAttribute<PCGGraphAttributeAttribute>();
            var providedName = att?.name;
            var targetName = string.IsNullOrWhiteSpace(providedName) ? memberInfo.Name : providedName;
            var categoryName = MemberToAttributeName(targetName, namespaceName);

            if (HGraph.Instance.Categories.ContainsKey(categoryName))
            {
                //Debug.LogError($"Cannot register module {memberInfo.DeclaringType}.{memberInfo.Name} for category {categoryName} because the category already exists.");
                return;
            }
            var serializationMode = att?.serializationMode ?? module.SerializationMode;
            if (!attributeModules.TryGetValue(categoryName, out var pcgAttribute))
            {
                pcgAttribute = new PCGAttributeModule(categoryName, providedName, att?.defaultValue, serializationMode);
                module.Attributes.Add(categoryName, pcgAttribute);
                attributeModules.Add(categoryName, pcgAttribute);
            }

            var (categoryType, defaultValue) = GetAttributeType(memberInfo, instance);
            if (categoryType == HGraphAttributeType.Enum || categoryType == HGraphAttributeType.FlagsEnum)
            {
                var enumName = LoadEnum(memberInfo, targetName, serializationMode);
                defaultValue = Tuple.Create(enumName, Convert.ToInt32(defaultValue));
            }
            float min = float.NaN;
            float max = float.NaN;

            var minAtt = memberInfo.GetCustomAttribute<MinAttribute>();
            if (minAtt != null)
                min = minAtt.min;
            var rangeAtt = memberInfo.GetCustomAttribute<RangeAttribute>();
            if (rangeAtt != null)
            {
                min = rangeAtt.min;
                max = rangeAtt.max;
            }
            var newCategory = HGraphCategory.Construct(categoryName);
            newCategory.Name.Value = categoryName;
            newCategory.Type.Value = categoryType;
            newCategory.MinValue = min;
            newCategory.MaxValue = max;

            newCategory.DefaultData = categoryType.CreateData();
            if (defaultValue != null)
                newCategory.DefaultData.TrySetValue(defaultValue);


            HGraph.Instance.Categories.Add(categoryName, newCategory);

        }
        private static (HGraphAttributeType type, object defaultValue) GetAttributeType(MemberInfo memberInfo, object instance)
        {
            Type memberType;
            object defaultValue = null;
            if (memberInfo is PropertyInfo pinfo)
            {
                memberType = pinfo.PropertyType;
                if (instance != null)
                {
                    defaultValue = pinfo.GetValue(instance);
                }
            }
            else if (memberInfo is FieldInfo finfo)
            {
                memberType = finfo.FieldType;
                if (instance != null)
                {
                    defaultValue = finfo.GetValue(instance);
                }
            }
            else { throw new ArgumentException("Cannot parse MemberInfo!"); }



            switch (Type.GetTypeCode(memberType))
            {
                case TypeCode.Boolean:
                    return (HGraphAttributeType.Boolean, defaultValue);

                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Decimal:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    if (memberType.IsEnum)
                    {
                        if (memberType.GetCustomAttribute<FlagsAttribute>() != null)
                            return (HGraphAttributeType.FlagsEnum, defaultValue);
                        return (HGraphAttributeType.Enum, defaultValue);
                    }
                    else
                        return (HGraphAttributeType.Nominal, defaultValue);

                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    throw new ArgumentException($"Type {Type.GetTypeCode(memberType)} not supported");

                case TypeCode.Double:
                case TypeCode.Single:
                    return (HGraphAttributeType.Float, defaultValue);

                case TypeCode.Object:
                    {
                        if (memberType == typeof(Vector3))
                            return (HGraphAttributeType.Vector3, defaultValue);
                        else if (memberType == typeof(Vector2))
                            return (HGraphAttributeType.Vector2, defaultValue);

                        throw new ArgumentException($"Type {Type.GetTypeCode(memberType)} not supported");
                    }

                case TypeCode.String:
                    throw new ArgumentException($"Type {Type.GetTypeCode(memberType)} not supported");

                default:
                    throw new ArgumentException($"Type {Type.GetTypeCode(memberType)} not supported");
            }

        }
        private static string LoadEnum(MemberInfo enumMember, string enumName, PCGModuleSerializationMode serializationMode)
        {
            Type memberType;
            if (enumMember is PropertyInfo pinfo)
            {
                memberType = pinfo.PropertyType;
            }
            else if (enumMember is FieldInfo finfo)
            {
                memberType = finfo.FieldType;
            }
            else { throw new ArgumentException("Cannot parse MemberInfo!"); }

            if (!memberType.IsEnum)
                throw new ArgumentException("Cannot parse MemberInfo!");

            if (HGraph.Instance.EnumDefinitions.ContainsKey(enumName))
            {
                //Debug.LogError($"Cannot register module {enumMember.DeclaringType}.{enumMember.Name} for enum {enumName} because the enum already exists.");
                return enumName;
            }
            if (!enumModules.TryGetValue(enumName, out var pcgEnum))
            {
                pcgEnum = new PCGEnumModule(enumName, serializationMode);
                enumModules.Add(enumName, pcgEnum);
            }
           

            var hEnum = HGraphEnum.Construct(enumName);
            string[] enumNames = Enum.GetNames(memberType);
            for (int i = 0; i < enumNames.Length; i++)
            {
                hEnum.Entries.Add(new EnumEntry() { Name = enumNames[i], Value = (int)Enum.Parse(memberType, enumNames[i]) });
            }
            hEnum.Flags = memberType.GetCustomAttribute<FlagsAttribute>() != null;
            HGraph.Instance.EnumDefinitions.TryAdd(enumName, hEnum);
            return enumName;
        }

        /// <summary>
        /// Converts the attributes on a node into members of a given type. Use this if the attributes of a node can directly be translated into C# data.
        /// The members must have the PCGGraphAttributeAttribute or the instance must have the PCGGraphModuleAttribute attribute.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        public static object LoadGraphAttributesIntoMembers(object instance, HGraphNodeData nodeData)
        {
            var type = instance.GetType();
            var returnInstance = instance;
            var moduleAtt = type.GetCustomAttribute<PCGGraphModuleAttribute>();
            if (moduleAtt == null)
                return instance;
            var namespaceName = moduleAtt.namespaceName;
            foreach (var memberInfo in
                type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => moduleAtt.registerAllMembers || m.IsDefined(typeof(PCGGraphAttributeAttribute), true))
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => moduleAtt.registerAllMembers || m.IsDefined(typeof(PCGGraphAttributeAttribute), true))))
            {
                var att = memberInfo.GetCustomAttribute<PCGGraphAttributeAttribute>();
                var name = att?.name;
                var expectedCategoryName = MemberToAttributeName(string.IsNullOrWhiteSpace(name) ? memberInfo.Name : name, namespaceName);
                var attribute = nodeData.attributes.FirstOrDefault(a => a.category == expectedCategoryName);
                if (attribute == null)
                    continue;
                object value = GetValueFromAttribute(attribute);
                if (memberInfo is PropertyInfo pinfo)
                    pinfo.SetValue(returnInstance, value);
                else if (memberInfo is FieldInfo finfo)
                    finfo.SetValue(returnInstance, value);
            }
            return returnInstance;
        }
        /// <summary>
        /// Converts the attributes on a node into members of a given type. Use this if the attributes of a node can directly be translated into C# data.
        /// The members must have the PCGGraphAttributeAttribute or the instance must have the PCGGraphModuleAttribute attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        public static T LoadGraphAttributesIntoMembers<T>(T instance, HGraphNodeData nodeData) => (T)LoadGraphAttributesIntoMembers((object)instance, nodeData);
        private static object GetValueFromAttribute(HGraphAttributeData a)
        {
            object value = a.data.GetValue();
            return value;
        }

        /// <summary>
        /// Returns the name of the attribute for a given member.
        /// E.g. [PCGGraphModuleAttribute(namespace='myNamespace')]MyClass.myMember => myNamespace:myMember.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static string GetMemberAttributeName(object instance, string memberName)
        {
            Type instanceType = instance.GetType();
            var moduleAtt = instanceType.GetCustomAttribute<PCGGraphModuleAttribute>();
            string namespaceName = "";
            if (moduleAtt != null && !string.IsNullOrWhiteSpace(moduleAtt.namespaceName))
                namespaceName = moduleAtt.namespaceName;

            var memberInfo = (instanceType.GetField(memberName) as MemberInfo) ?? instanceType.GetProperty(memberName);
            var memberAtt = memberInfo.GetCustomAttribute<PCGGraphAttributeAttribute>();
            var providedName = memberAtt?.name;
            var targetName = string.IsNullOrWhiteSpace(providedName) ? memberInfo.Name : providedName;
            return MemberToAttributeName(targetName, namespaceName);

        }
    }
}