using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    public enum HGraphAttributeRelationType
    {
        None,
        Linear,
        Curve,
        Step
    }


    public static class HGraphAttributeRelationTypeExtensions
    {
        public static HGraphAttributeRelationContent CreateData(this HGraphAttributeRelationType type)
        {
            var contentType = GetContentType(type);
            ConstructorInfo constructor = contentType?.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                HGraphAttributeRelationContent instance = constructor.Invoke(null) as HGraphAttributeRelationContent;
                instance.type = type;
                return instance;
            }
            return null;
        }
        public static Type GetContentType(this HGraphAttributeRelationType type)
        {
            switch (type)
            {
                case HGraphAttributeRelationType.None:
                case HGraphAttributeRelationType.Linear:
                    return typeof(HGraphAttributeRelationContent);
                case HGraphAttributeRelationType.Step:
                    return typeof(HGraphAttributeRelationContent_Step);
                default:
                    return null;
            }
        }
    }
}
