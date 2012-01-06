using System;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Common
{
    public class ReflectionUtils
    {

        /// <summary>
        /// Determines if the object is an instance of the generic type.
        /// http://stackoverflow.com/questions/982487/testing-if-object-is-of-generic-type-in-c-sharp
        /// </summary>
        /// <param name="genericType"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool IsInstanceOfGenericType(Type genericType, object instance)
        {
            Type type = instance.GetType();

            List<Type> interfaces = new List<Type>(instance.GetType().GetInterfaces());
            Type matchingInterface = interfaces.Find(delegate (Type iface)
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genericType)
                    return true;
                else
                    return false;
            });

            if (matchingInterface != null)
                return true;

            while (type != null)
            {
                type.GetInterfaces();
                if (type.IsGenericType)
                    
                {
                    Type genericTypeOfInstance = type.GetGenericTypeDefinition();
                    if (genericTypeOfInstance.Equals(genericType.GetGenericTypeDefinition()))
                        return true;
                }
                type = type.BaseType;
            }
            return false;
        }

    }
}
