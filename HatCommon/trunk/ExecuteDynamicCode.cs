using System;
using System.Collections;
using System.Reflection;

namespace Hatfield.Web.Portal
{
    /// <summary>
    /// Source: http://www.codeproject.com/csharp/DynLoadClassInvokeMethod.asp
    /// </summary>	
    public class ExecuteDynamicCode
    {

        private static Hashtable AssemblyCache = new Hashtable();

        public static Object InvokeMethod(string AssemblyLocation, string ClassName, string MethodName, Object[] args)
        {
            // load the assembly
            Assembly assembly = null;
            if (AssemblyCache[AssemblyLocation] != null)
            {
                assembly = AssemblyCache[AssemblyLocation] as Assembly;
            }
            else
            {
                AssemblyName asmName = AssemblyName.GetAssemblyName(AssemblyLocation);
                assembly = AppDomain.CurrentDomain.Load(asmName);
                AssemblyCache[AssemblyLocation] = assembly;
            }


            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.ToLower().EndsWith("." + ClassName.ToLower()))
                    {
                        // create an instance of the object
                        object ClassObj = Activator.CreateInstance(type);

                        // Dynamically Invoke the method
                        object Result = type.InvokeMember(MethodName,
                            BindingFlags.Default | BindingFlags.InvokeMethod,
                            null,
                            ClassObj,
                            args);
                        return (Result);
                    } // if
                } // if
            } // foreach
            throw (new System.Exception("could not invoke method " + MethodName + " in class " + ClassName + " in Assembly " + AssemblyLocation));
        } // InvokeMethod

    } // class
}
