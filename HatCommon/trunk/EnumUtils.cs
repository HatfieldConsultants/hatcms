using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;

namespace Hatfield.Common
{
    public class EnumUtils
    {
        /// <summary>
        /// Gets a display value of an enum that is set within a description attribute on the enum value.
        /// This is useful for displaying user friendly enum values.
        /// 
        /// Code from here: http://stackoverflow.com/questions/4367723/get-enum-from-description-attribute
        /// </summary>
        /// <param name="value"> the value of the enum which we want the display text for</param>
        /// <returns>a string containing the display text. IF no attribute is found then the default enum value is return
        /// as a string. </returns>
        public static string GetDescriptionFromDescriptionAttribute(Enum value)
        {            
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }

        /// <summary>
        /// http://stackoverflow.com/questions/4367723/get-enum-from-description-attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetEnumValueFromDescription<T>(string description)
        {
            Type type = typeof(T);
            if(!type.IsEnum) throw new InvalidOperationException();
            foreach(FieldInfo field in type.GetFields())
            {
                DescriptionAttribute attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if(attribute != null)
                {
                    if(attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if(field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
        }

    }
    
}
