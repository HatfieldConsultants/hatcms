using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS
{
    public class CmsControlUtils
    {
        /// <summary>
        /// Caches all BaseCmsControl types on a per-request basis. This makes the fetching of control types much faster.
        /// </summary>
        /// <returns></returns>
        public static Type[] GetAllCachedBaseCmsControlDerivedClasses()
        {
            string cacheKey = "GetAllCachedBaseCmsControlDerivedClasses";
            if (PerRequestCache.CacheContains(cacheKey))
                return (Type[])PerRequestCache.GetFromCache(cacheKey, new Type[0]);

            Type[] ret = AssemblyHelpers.LoadAllAssembliesAndGetAllSubclassesOf(typeof(HatCMS.Controls.BaseCmsControl));
            PerRequestCache.AddToCache(cacheKey, ret);
            return ret;
        }

        public static int getControlParameterKeyValue(CmsPage page, CmsControlDefinition controlDefinition, string key, int defaultValue)
        {
            return page.TemplateEngine.getControlParameterKeyValue(controlDefinition, key, defaultValue);
        } // getControlParameterKeyValue

        public static bool getControlParameterKeyValue(CmsPage page, CmsControlDefinition controlDefinition, string key, bool defaultValue)
        {
            return page.TemplateEngine.getControlParameterKeyValue(controlDefinition, key, defaultValue);

        } // getControlParameterKeyValue


        public static string getControlParameterKeyValue(CmsPage page, CmsControlDefinition controlDefinition, string key, string defaultValue)
        {
            return page.TemplateEngine.getControlParameterKeyValue(controlDefinition, key, defaultValue);
        }


        public static int getControlParameterKeyValue(CmsPage page, System.Web.UI.UserControl control, string key, int defaultValue)
        {
            return page.TemplateEngine.getControlParameterKeyValue(control, key, defaultValue);
        } // getControlParameterKeyValue

        public static bool getControlParameterKeyValue(CmsPage page, System.Web.UI.UserControl control, string key, bool defaultValue)
        {
            return page.TemplateEngine.getControlParameterKeyValue(control, key, defaultValue);

        } // getControlParameterKeyValue


        public static string getControlParameterKeyValue(CmsPage page, System.Web.UI.UserControl control, string key, string defaultValue)
        {
            return page.TemplateEngine.getControlParameterKeyValue(control, key, defaultValue);
        }

        public static string[] getControlParameterKeys(CmsPage page, System.Web.UI.UserControl control)
        {
            return page.TemplateEngine.getControlParameterKeys(control);
        } // getControlParameterKeys

        public static string[] getControlParameterKeys(CmsPage page, CmsControlDefinition controlDefinition)
        {
            return page.TemplateEngine.getControlParameterKeys(controlDefinition);
        } // getControlParameterKeys



        public static bool hasControlParameterKey(CmsPage page, CmsControlDefinition controlDefinition, string keyName)
        {
            string[] keys = getControlParameterKeys(page, controlDefinition);
            if (StringUtils.IndexOf(keys, keyName, StringComparison.CurrentCultureIgnoreCase) > -1)
                return true;
            return false;
        }


        public static bool hasControlParameterKey(CmsPage page, System.Web.UI.UserControl control, string keyName)
        {
            string[] keys = getControlParameterKeys(page, control);
            if (StringUtils.IndexOf(keys, keyName, StringComparison.CurrentCultureIgnoreCase) > -1)
                return true;
            return false;
        }

        /// <summary>
        /// Add a control that is located on disk or as a class name to the parentToAddControlTo.
        /// </summary>
        /// <param name="controlNameOrPathToAdd"></param>
        /// <param name="controlDefn"></param>
        /// <param name="parentToAddControlTo"></param>
        /// <param name="langToRenderFor"></param>
        /// <returns></returns>
        public static bool AddControlToPage(string controlNameOrPathToAdd, CmsControlDefinition controlDefnToAdd, System.Web.UI.UserControl parentToAddControlTo, CmsLanguage langToRenderFor)
        {
            if (ControlOnDiskExists(controlNameOrPathToAdd))
            {
                Control ascxControl = LoadControlOnDisk(controlNameOrPathToAdd);
                if (ascxControl != null)
                {
                    parentToAddControlTo.Controls.Add(ascxControl);
                    ascxControl.ID = controlDefnToAdd.RawTemplateParameters;

                    return true;
                }
            }
            else if (ControlClassExists(controlNameOrPathToAdd))
            {
                HatCMS.Controls.BaseCmsControl c = LoadControlFromClass(controlNameOrPathToAdd);
                if (c != null)
                {
                    parentToAddControlTo.Controls.Add(c.ToLiteralControl(controlDefnToAdd, langToRenderFor));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// returns DateTime.MinValue if the control's last modified date could not be determined.
        /// </summary>
        /// <param name="controlNameOrPath"></param>
        /// <returns></returns>
        public static DateTime getControlLastModifiedDate(string controlNameOrPath)
        {
            try
            {
                if (ControlOnDiskExists(controlNameOrPath))
                {
                    string virtualPath = "~/" + TemplateEngine.TemplateEngineV2.CONTROLS_SUBDIR + controlNameOrPath + ".ascx";
                    string controlFNOnDisk = System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);
                    return new System.IO.FileInfo(controlFNOnDisk).LastWriteTime;
                }
                else if (ControlClassExists(controlNameOrPath))
                {
                    HatCMS.Controls.BaseCmsControl baseControl = LoadControlFromClass(controlNameOrPath);
                    return new System.IO.FileInfo(baseControl.GetType().Assembly.Location).LastWriteTime;                    
                }
            }
            catch
            { }
            return DateTime.MinValue;
        }

        /// <summary>
        /// Checks if a control exists. The control can either be located on disk, or as a class type.
        /// </summary>
        /// <param name="controlNameOrPath"></param>
        /// <returns></returns>
        public static bool ControlExists(string controlNameOrPath)
        {
            try
            {                
                if (ControlOnDiskExists(controlNameOrPath))
                    return true;
                
                if (ControlClassExists(controlNameOrPath))
                    return true;
            }
            catch
            { }
            return false;
        }

        /// <summary>
        /// Checks if a control exists on disk. Note that the control name NEVER contains the .ascx extension.
        /// </summary>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public static bool ControlOnDiskExists(string controlName)
        {
            try
            {
                string virtualPath = "~/" + TemplateEngine.TemplateEngineV2.CONTROLS_SUBDIR + controlName + ".ascx";
                string contronFNOnDisk = System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);
                if (System.IO.File.Exists(contronFNOnDisk))
                {
                    return true;
                }
            }
            catch
            { }

            return false;
        }

        /// <summary>
        /// Checks of a control class name exists. For a control to be valid, it must inherit from HatCMS.Controls.BaseCmsControl.
        /// Note that control classes can be placed in Assemblies copied into the application's BIN directory. These classes will be
        /// dynamically loaded and found using this method.
        /// </summary>
        /// <param name="controlClassName"></param>
        /// <returns></returns>
        public static bool ControlClassExists(string controlClassName)
        {
            try
            {
                // -- load by class name
                Type[] controlTypes = GetAllCachedBaseCmsControlDerivedClasses();
                foreach (Type cType in controlTypes)
                {
                    if (cType.FullName.EndsWith("." + controlClassName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                } // foreach

            }
            catch { }

            return false;
        }

        /// <summary>
        /// gets all the dependencies for a control. The control can either be located on disk, or as a class type.
        /// </summary>
        /// <param name="controlNameOrPath"></param>
        /// <returns></returns>
        public static CmsDependency[] getControlDependencies(string controlNameOrPath)
        {

            List<CmsDependency> dMsgs = new List<CmsDependency>();

            try
            {
                Control ascxControl = LoadControlOnDisk(controlNameOrPath);
                if (ascxControl != null)
                {
                    Type ascxControlType = ascxControl.GetType();
                    // -- ascx Controls can implement the magic "getDependecies" method
                    if (ascxControlType.GetMethod("getDependencies") != null)
                    {
                        CmsDependency[] dependencies = (CmsDependency[])ExecuteDynamicCode.InvokeMethod(ascxControlType.Assembly.Location, ascxControlType.Name, "getDependencies", new object[0]);

                        dMsgs.AddRange(dependencies);
                    }
                }
                else
                {

                    HatCMS.Controls.BaseCmsControl c = LoadControlFromClass(controlNameOrPath);
                    if (c != null)
                    {
                        dMsgs.AddRange(c.getDependencies());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return dMsgs.ToArray();

        }



        /// <summary>
        /// Loads a user control located on disk. Note that the control name NEVER contains the .ascx extension.
        /// </summary>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public static Control LoadControlOnDisk(string controlName)
        {

            string virtualPath = "~/" + TemplateEngine.TemplateEngineV2.CONTROLS_SUBDIR + controlName + ".ascx";
            string contronFNOnDisk = System.Web.Hosting.HostingEnvironment.MapPath(virtualPath);
            if (System.IO.File.Exists(contronFNOnDisk))
            {
                return (new Page()).LoadControl(virtualPath);
            }
            
            return null;
        }

       

        

        /// <summary>
        /// Trys to load a control based on its type-name or path.
        /// Returns NULL if the name wasn't found, or couldn't be instantiated.
        /// </summary>
        /// <param name="controlNameOrPath"></param>
        /// <returns></returns>
        public static HatCMS.Controls.BaseCmsControl LoadControlFromClass(string controlClassName)
        {
            try
            {
                // -- load by class name
                Type[] controlTypes = GetAllCachedBaseCmsControlDerivedClasses();
                foreach (Type cType in controlTypes)
                {
                    if (cType.FullName.EndsWith("." + controlClassName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return (cType.Assembly.CreateInstance(cType.FullName) as HatCMS.Controls.BaseCmsControl);
                    }
                } // foreach

            }
            catch { }

            return null;
        }


    }
}
