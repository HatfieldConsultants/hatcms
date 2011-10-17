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
        /// <summary>
        /// Goes through all assemblies in the directory and loads them if they haven't already been loaded.
        /// Returns a list of all loaded assemblies.
        /// <para>This function is safe to run multiple times</para>
        /// </summary>
        /// <returns></returns>
        public static Assembly[] LoadAllDirectoryAssemblies(DirectoryInfo dirContainingAssembliesToLoad)
        {
            // based on http://stackoverflow.com/questions/1288288/how-to-load-all-assemblies-from-within-your-bin-directory
            // reference: http://msdn.microsoft.com/en-us/library/ms173100(v=vs.80).aspx
            // note: should probably take "assemblyBinding/probing" config entry into consideration http://msdn.microsoft.com/en-us/library/823z9h8w.aspx                        
            
            // -- prepopulate the list of assemblies with those that already exist.
            Assembly[] allLoadedAssemblies = GetAllLoadedAssemblies();
            Dictionary<string, Assembly> loadedAssembliesDict = ToFullNameKeyedDictionary(allLoadedAssemblies);

            foreach (FileInfo dll in dirContainingAssembliesToLoad.GetFiles("*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    AssemblyName asmName = AssemblyName.GetAssemblyName(dll.FullName);
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

        /// <summary>
        /// returns NULL if the assembly couldn't be loaded.
        /// </summary>
        /// <param name="assemblyFileToLoad"></param>
        /// <returns></returns>
        public static Assembly LoadAssembly(FileInfo assemblyFileToLoad)
        {
            // -- prepopulate the list of assemblies with those that already exist.
            Assembly[] allLoadedAssemblies = GetAllLoadedAssemblies();
            Dictionary<string, Assembly> loadedAssembliesDict = ToFullNameKeyedDictionary(allLoadedAssemblies);

            try
            {
                AssemblyName asmName = AssemblyName.GetAssemblyName(assemblyFileToLoad.FullName);
                if (asmName!= null && loadedAssembliesDict.ContainsKey(asmName.FullName))
                    return loadedAssembliesDict[asmName.FullName];

                if (asmName != null && !loadedAssembliesDict.ContainsKey(asmName.FullName))
                {
                    // note: don't use Assembly.Load - it loads the assembly into the wrong appDomain.
                    Assembly loadedAssembly = AppDomain.CurrentDomain.Load(asmName);
                    return loadedAssembly;
                }
            }
            catch (FileLoadException loadEx)
            { } // The Assembly has already been loaded.
            catch (BadImageFormatException imgEx)
            { } // If a BadImageFormatException exception is thrown, the file is not an assembly.

            return null;
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
            return LoadAllAssembliesAndGetAllAvailableTypes(getBinDirectory());
        }

        public static Type[] LoadAllAssembliesAndGetAllAvailableTypes(DirectoryInfo directoryContainingAssembliesToLoad)
        {
            Assembly[] assemblies = LoadAllDirectoryAssemblies(directoryContainingAssembliesToLoad);
            List<Type> ret = new List<Type>();
            foreach (Assembly asm in assemblies)
            {
                ret.AddRange(asm.GetTypes());
            }

            return ret.ToArray();
        }

        public static bool TypeInheritsFrom(Type sourceType, Type testParentType)
        {
            if (sourceType.IsClass && sourceType.IsSubclassOf(testParentType) )
            {
                return true;                
            }

            // note: if .IsSubclassOf isn't working (and it should), check out this post: http://stackoverflow.com/questions/465984/type-issubclassof-doesnt-work-across-appdomains 

            return false;
            
        }

        private static DirectoryInfo getBinDirectory()
        {
            // note for bin path: could use HttpRuntime.BinDirectory for web projects.
            string binPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "bin"); // note: don't use CurrentEntryAssembly or anything like that.

            return new DirectoryInfo(binPath);
        }

        /// <summary>
        /// This will be made public in a future version (when modules can be dynamically loaded)
        /// 
        /// </summary>
        /// <param name="parentClassType"></param>
        /// <returns></returns>
        private static Type[] GetAllSubclassesOfTypeFromAllLoadedAssemblies(Type parentClassType)
        {
            Assembly[] loadedAssemblies = GetAllLoadedAssemblies();

            List<Type> ret = new List<Type>();
            foreach (Assembly asm in loadedAssemblies)
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (TypeInheritsFrom(t, parentClassType))
                        ret.Add(t);
                } // foreach type
            } // foreach assembly

            return ret.ToArray();


        }

        /// <summary>
        /// Load All assemblies in the BIN directory, and get all the types that are a subClass of the parentClassType
        /// <para>This function is safe to run multiple times</para>
        /// </summary>
        /// <param name="parentClassType"></param>
        /// <returns></returns>
        public static Type[] LoadAllAssembliesAndGetAllSubclassesOf(Type parentClassType)
        {
            return LoadAllAssembliesAndGetAllSubclassesOf(parentClassType, getBinDirectory());
        }

        /// <summary>
        /// Load All assemblies in the specified directory, and get all the types that are a subClass of the parentClassType
        /// <para>This function is safe to run multiple times</para>
        /// </summary>
        /// <param name="parentClassType"></param>
        /// <returns></returns>
        private static Type[] LoadAllAssembliesAndGetAllSubclassesOf(Type parentClassType, DirectoryInfo directoryContainingAssembliesToLoad)
        {
            Assembly[] assemblies = LoadAllDirectoryAssemblies(directoryContainingAssembliesToLoad);
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

        public static Type[] GetAllSubclassesOf(Type parentClassType, Assembly[] assembliesToSearchIn)
        {            
            List<Type> ret = new List<Type>();
            foreach (Assembly asm in assembliesToSearchIn)
            {
                foreach (Type t in asm.GetTypes())
                {
                    if (TypeInheritsFrom(t, parentClassType))
                        ret.Add(t);
                } // foreach type
            } // foreach assembly

            return ret.ToArray();
        }


        private static Dictionary<string, Assembly> GetAllEmbeddedResourcesWithExtensionsFromLoadedAssemblies(string[] extensions)
        {
            Assembly[] assemblies = GetAllLoadedAssemblies();
            Dictionary<string, Assembly> ret = new Dictionary<string, Assembly>();
            foreach (Assembly asm in assemblies)
            {
                try
                {
                    foreach (string resName in asm.GetManifestResourceNames())
                    {
                        if (StringUtils.IndexOf(extensions, Path.GetExtension(resName), StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            ret[resName] = asm;
                        }
                    }
                }
                catch (System.NotSupportedException notSupEx)
                {
                    Console.Write("Could not read dynamic resources!");
                }
            }

            return ret;

        }

        /// <summary>
        /// Load All assemblies in the BIN directory, and
        /// returns a dictionary in the format [ResourceName] => AssemblyContainingTheResource
        /// </summary>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static Dictionary<string, Assembly> LoadAllAssembliesAndGetAllEmbeddedResourcesWithExtensions(string[] extensions)
        {
            return LoadAllAssembliesAndGetAllEmbeddedResourcesWithExtensions(extensions, getBinDirectory());
        }

        /// <summary>
        /// Load All assemblies in the BIN directory, and
        /// returns a dictionary in the format [ResourceName] => AssemblyContainingTheResource
        /// </summary>
        /// <param name="extensions"></param>
        /// <returns></returns>
        private static Dictionary<string, Assembly> LoadAllAssembliesAndGetAllEmbeddedResourcesWithExtensions(string[] extensions, DirectoryInfo directoryContainingAssembliesToLoad)
        {
            Assembly[] assemblies = LoadAllDirectoryAssemblies(directoryContainingAssembliesToLoad);
            Dictionary<string, Assembly> ret = new Dictionary<string,Assembly>();
            foreach (Assembly asm in assemblies)
            {
                try
                {
                    foreach (string resName in asm.GetManifestResourceNames())
                    {
                        if (StringUtils.IndexOf(extensions, Path.GetExtension(resName), StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            ret[resName] = asm;
                        }
                    }
                }
                catch (System.NotSupportedException notSupEx)
                {
                    Console.Write("Could not read dynamic resources!");
                }
            }

            return ret;
        }

        /// <summary>
        /// Takes the full name of a resource and loads it in to a stream.
        /// </summary>
        /// <param name="resourceName">Assuming an embedded resource is a file
        /// called info.png and is located in a folder called Resources, it
        /// will be compiled in to the assembly with this fully qualified
        /// name: Full.Assembly.Name.Resources.info.png. That is the string
        /// that you should pass to this method.</param>
        /// <returns></returns>
        public static Stream GetEmbeddedResourceStream(Assembly assemblyWithTheResource, string resourceName)
        {
            return assemblyWithTheResource.GetManifestResourceStream(resourceName);
        }

    }
}
