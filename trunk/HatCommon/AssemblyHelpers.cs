using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

namespace Hatfield.Web.Portal
{
    public class AssemblyHelpers
    {
#if UseAssemblyCache
        /// <summary>
        /// the assemblies are staticly cached so that they are held as long as the assemblies are available.
        /// </summary>
        private static Dictionary<Type, Assembly> assemblyCache = new Dictionary<string, Assembly>();

        public static Assembly GetFromCache(Type typeKey)
        {
            if (assemblyCache.ContainsKey(typeKey))
                return assemblyCache[keytypeKey];
            
            return returnOnErrorOrInvalid;
        } // GetFromCache

        public static bool AssemblyCacheContains(Type typeKey)
        {
            return assemblyCache.ContainsKey(typeKey);
        }

        /// <summary>
        /// if the key already exists, the value will be overwritten
        /// </summary>
        /// <param name="key"></param>
        /// <param name="objToAdd"></param>
        /// <returns></returns>
        public static bool AddToAssemblyCache(Type typeKey, Assembly assemblyToAdd)
        {
            assemblyCache[typeKey] = assemblyToAdd;
        }
#endif

        /// <summary>
        /// Goes through all assemblies in the BIN directory and loads them if they haven't already been loaded.
        /// Returns a list of all loaded assemblies.
        /// <para>This function is safe to run multiple times</para>
        /// </summary>
        /// <returns></returns>
        public static Assembly[] LoadAllBinDirectoryAssemblies()
        {
            // based on http://stackoverflow.com/questions/1288288/how-to-load-all-assemblies-from-within-your-bin-directory
            // reference: http://msdn.microsoft.com/en-us/library/ms173100(v=vs.80).aspx
            // note: should probably take "assemblyBinding/probing" config entry into consideration http://msdn.microsoft.com/en-us/library/823z9h8w.aspx
            
            string binPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin"); // note: don't use CurrentEntryAssembly or anything like that.
            
            // -- prepopulate the list of assemblies with those that already exist.
            Assembly[] allLoadedAssemblies = GetAllLoadedAssemblies();
            Dictionary<string, Assembly> loadedAssembliesDict = ToFullNameKeyedDictionary(allLoadedAssemblies);

            foreach (string dll in Directory.GetFiles(binPath, "*.dll", SearchOption.AllDirectories))
            {
                try
                {                    
                    AssemblyName asmName = AssemblyName.GetAssemblyName(dll);
                    if (asmName != null && !loadedAssembliesDict.ContainsKey(asmName.FullName))
                    {
                        // note: don't use Assembly.Load - it loads the assembly into the wrong appDomain.
                        Assembly loadedAssembly = AppDomain.CurrentDomain.Load(asmName);
                        loadedAssembliesDict[loadedAssembly.FullName] = loadedAssembly;
                    }
                }
                catch (FileLoadException loadEx)
                { } // The Assembly has already been loaded.
                catch (BadImageFormatException imgEx)
                { } // If a BadImageFormatException exception is thrown, the file is not an assembly.


            } // foreach dll

            return new List<Assembly>(loadedAssembliesDict.Values).ToArray();

        }

        public static Dictionary<string, Assembly> ToFullNameKeyedDictionary(Assembly[] list)
        {
            Dictionary<string, Assembly> ret = new Dictionary<string, Assembly>();
            foreach (Assembly asm in list)
            {
                if (!ret.ContainsKey(asm.FullName))
                    ret.Add(asm.FullName, asm);
            }

            return ret;
        }


        /// <summary>
        /// gets a list of all assemblies that have been loaded into memory
        /// </summary>
        /// <returns></returns>
        public static Assembly[] GetAllLoadedAssemblies()
        {
            Dictionary<string, Assembly> dict = new Dictionary<string, Assembly>();

            Assembly exAssembly = Assembly.GetExecutingAssembly();
            Assembly callAssembly = Assembly.GetCallingAssembly();
            Assembly entryAssembly = Assembly.GetEntryAssembly();

            // -- executing assembly
            dict.Add(exAssembly.FullName, exAssembly);

            // -- calling assembly
            if (callAssembly != null && !dict.ContainsKey(callAssembly.FullName))
                dict.Add(callAssembly.FullName, entryAssembly);

            // -- entry assembly
            if (entryAssembly != null && !dict.ContainsKey(entryAssembly.FullName))
                dict.Add(entryAssembly.FullName, entryAssembly);

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {                           
                if (!dict.ContainsKey(asm.FullName))
                    dict.Add(asm.FullName, asm);
            }// foreach

            return new List<Assembly>(dict.Values).ToArray();
            

        }

        public static Type[] LoadAllAssembliesAndGetAllAvailableTypes()
        {
            Assembly[] assemblies = LoadAllBinDirectoryAssemblies();
            List<Type> ret = new List<Type>();
            foreach (Assembly asm in assemblies)
            {
                ret.AddRange(asm.GetTypes());
            }

            return ret.ToArray();
        }

        private static bool TypeInheritsFrom(Type sourceType, Type testParentType)
        {
            if (sourceType.IsClass && sourceType.IsSubclassOf(testParentType) )
            {
                return true;                
            }

            // note: if .IsSubclassOf isn't working (and it should), check out this post: http://stackoverflow.com/questions/465984/type-issubclassof-doesnt-work-across-appdomains 

            return false;
            
        }

        /// <summary>
        /// Load All assemblies in the BIN directory, and get all the types that are a subClass of the parentClassType
        /// <para>This function is safe to run multiple times</para>
        /// </summary>
        /// <param name="parentClassType"></param>
        /// <returns></returns>
        public static Type[] LoadAllAssembliesAndGetAllSubclassesOf(Type parentClassType)
        {
            Assembly[] assemblies = LoadAllBinDirectoryAssemblies();
            List<Type> ret = new List<Type>();
            foreach (Assembly asm in assemblies)
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (TypeInheritsFrom(t, parentClassType))
                        ret.Add(t);
                } // foreach type
            } // foreach assembly

            return ret.ToArray();
        }

    }
}
