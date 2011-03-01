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
        public static int getControlParameterKeyValue(System.Web.UI.UserControl control, string key, int defaultValue)
        {
            return CmsContext.currentPage.TemplateEngine.getControlParameterKeyValue(control, key, defaultValue);
        } // getControlParameterKeyValue

        public static bool getControlParameterKeyValue(System.Web.UI.UserControl control, string key, bool defaultValue)
        {
            return CmsContext.currentPage.TemplateEngine.getControlParameterKeyValue(control, key, defaultValue);

        } // getControlParameterKeyValue


        public static string getControlParameterKeyValue(System.Web.UI.UserControl control, string key, string defaultValue)
        {            
            return CmsContext.currentPage.TemplateEngine.getControlParameterKeyValue(control, key, defaultValue);
        }

        public static string[] getControlParameterKeys(System.Web.UI.UserControl control)
        {
            return CmsContext.currentPage.TemplateEngine.getControlParameterKeys(control);
        } // getControlParameterKeys

        public static bool hasControlParameterKey(System.Web.UI.UserControl control, string keyName)
        {
            string[] keys = getControlParameterKeys(control);
            if (StringUtils.IndexOf(keys, keyName, StringComparison.CurrentCultureIgnoreCase) > -1)
                return true;
            return false;
        }

        public static CmsDependency[] getControlDependencies(string controlPath)
        {
            System.Reflection.Assembly callingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Type[] assemblyTypes = callingAssembly.GetTypes();

            List<CmsDependency> dMsgs = new List<CmsDependency>();

            try
            {
                string virtualPath = CmsContext.ApplicationPath + "controls/" + controlPath + ".ascx";

                Control c = (new Page()).LoadControl(virtualPath);
                Type controlType = c.GetType();

                foreach (Type type in assemblyTypes)
                {
                    try
                    {
                        if (type.FullName == controlType.BaseType.FullName && type.IsClass == true && !type.IsAbstract && type.GetMethod("getDependencies") != null)
                        {
                            CmsDependency[] dependencies = (CmsDependency[])ExecuteDynamicCode.InvokeMethod(callingAssembly.Location, type.Name, "getDependencies", new object[0]);

                            dMsgs.AddRange(dependencies);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }


                } // foreach type
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return dMsgs.ToArray();

        }


    }
}
