using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Reflection;

using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    public class PlaceholderUtils
    {
        /// <summary>
        /// the assemblies are staticly cached so that they are held as long as the assemblies are
        /// </summary>
        private static Dictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();
        
        /// <summary>
        /// Invokes a method on a placeholder. The placeholder's assembly must be in the bin directory to be found, and must
        /// inherit from the <see cref="BaseCmsPlaceholder"/> type.
        /// </summary>
        /// <param name="PlaceholderType"></param>
        /// <param name="MethodName"></param>
        /// <param name="MethodParams"></param>
        /// <returns></returns>
        public static object InvokePlaceholderFunction(string PlaceholderType, string MethodName, object[] MethodParams)
        {
            // -- get a list of assemblies to search through
            List<Assembly> assembliesToSearch = new List<Assembly>();
            
            // -- if the placeholderType was previously found in a particular assembly, get that assembly from the cache
            string cacheKey = PlaceholderType.ToLower();
            if (assemblyCache.ContainsKey(cacheKey))
            {
                assembliesToSearch.Add(assemblyCache[cacheKey]);
            }
            else
            {
                Assembly exAssembly = Assembly.GetExecutingAssembly();
                Assembly callAssembly = Assembly.GetCallingAssembly();
                Assembly entryAssemly = Assembly.GetEntryAssembly();
                
                assembliesToSearch.Add(exAssembly);
                if (callAssembly != null && callAssembly.FullName != exAssembly.FullName)
                    assembliesToSearch.Add(callAssembly);

                if (entryAssemly != null && entryAssemly.FullName != exAssembly.FullName && exAssembly.FullName != callAssembly.FullName)
                    assembliesToSearch.Add(entryAssemly);

                assembliesToSearch.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            }
            
            // -- go through each assembly looking for the specified type
            foreach(Assembly assembly in assembliesToSearch)
            {
                // Walk through each type in the assembly looking for our class
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass == true && 
                        type.BaseType != null && type.BaseType.FullName == typeof(BaseCmsPlaceholder).FullName && 
                        type.FullName.ToLower().EndsWith("." + PlaceholderType.ToLower()))
                    {
                        // -- cache the found assembly for next time around
                        assemblyCache[cacheKey] = assembly;

                        foreach (MethodInfo method in type.GetMethods())
                        {
                            if (String.Compare(method.Name, MethodName) == 0)
                            {
                                // create an instance of the object
                                object ClassObj = Activator.CreateInstance(type);

                                // Dynamically Invoke the method
                                object Result = type.InvokeMember(MethodName,
                                    BindingFlags.Default | BindingFlags.InvokeMethod,
                                    null,
                                    ClassObj,
                                    MethodParams);
                                return (Result);
                            }
                        }

                        throw new Exception("Could not invoke method " + MethodName + " in placeholder " + PlaceholderType + " - the method could not be found");

                    } // if
                } // foreach type
            } // foreach


            throw new Exception("Could not invoke method " + MethodName + " in placeholder " + PlaceholderType + " - the placeholder could not be found");
        }
        

        /// <summary>
        /// returns string.empty on error, or when there's nothing to display
        /// </summary>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static string renderPlaceholderToString(CmsPage page, CmsLanguage langToRender, CmsPlaceholderDefinition placeholderDef)
        {
            try
            {
                StringBuilder html = new StringBuilder();
                System.IO.StringWriter output = new System.IO.StringWriter(html);
                HtmlTextWriter writer = new HtmlTextWriter(output);
                RenderInViewMode(placeholderDef.PlaceholderType, writer, page, placeholderDef.Identifier, langToRender, placeholderDef.ParamList, page.TemplateName);

                return html.ToString();
            }
            catch
            { }
            return string.Empty;
        }                        
        
        /// <summary>
        /// calls the .getDependencies function for the specified placeholder Type.
        /// </summary>
        /// <param name="PlaceholderType"></param>
        /// <returns></returns>
        public static CmsDependency[] getDependencies(string PlaceholderType)
        {
            try
            {
                object ret = InvokePlaceholderFunction(PlaceholderType, "getDependencies", new object[0]);

                if (ret is CmsDependency[])
                    return (ret as CmsDependency[]);
            }
            catch { }
            return new CmsDependency[0];

        }

        public static bool revertToRevision(string PlaceholderType, CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage phLanguage)
        {
            // public abstract bool revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, AppLanguage language);
            object ret = InvokePlaceholderFunction(PlaceholderType, "revertToRevision", new object[] { oldPage, currentPage, identifiers, phLanguage });

            if (ret is bool)
                return (bool)ret;

            return false;

        }

        public static void RenderInViewMode(string PlaceholderType, HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] subParamsArray, string templateFilename)
        {
            doRender(PlaceholderType, "RenderInViewMode", writer, page, identifier, langToRenderFor, subParamsArray, templateFilename);
        }

        public static void RenderInEditMode(string PlaceholderType, HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] subParamsArray, string templateFilename)
        {
            doRender(PlaceholderType, "RenderInEditMode", writer, page, identifier, langToRenderFor, subParamsArray, templateFilename);
        }


        private static void doRender(string PlaceholderType, string RenderFunction, HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] subParamsArray, string templateFilename)
        {
            try
            {
                // Render() parameters are set in BaseCmsPlaceholder                
                PlaceholderUtils.InvokePlaceholderFunction(PlaceholderType, RenderFunction, new object[] { writer, page, identifier, langToRenderFor, subParamsArray });
            }
            catch (Exception ex)
            {
                if (ex is System.Threading.ThreadAbortException)
                {
                    // the placeholder or control did a Response.End
                    return;
                }
                else if (ex is CmsPageNotFoundException || ex.InnerException is CmsPageNotFoundException)
                {
                    CmsContext.HandleNotFoundException();
                }
                else if (ex is CmsPlaceholderNeedsRedirectionException || ex.InnerException is CmsPlaceholderNeedsRedirectionException)
                {
                    // due to the dynamic nature of placeholders,
                    // placeholders can not redirect on their own, so must raise an Exception to do so.
                    System.Web.HttpResponse resp = System.Web.HttpContext.Current.Response;
                    resp.Clear();
                    resp.ClearContent();
                    resp.ClearHeaders();
                    string targetUrl = (ex.InnerException as CmsPlaceholderNeedsRedirectionException).TargetUrl;
                    resp.StatusCode = 301; // Moved Permanently
                    resp.AddHeader("Location", targetUrl);
                    resp.Redirect(targetUrl, true);
                }
                else
                {
                    throw new TemplateExecutionException(ex.InnerException, templateFilename, "Error in Placeholder, or Unknown Placeholder in " + PlaceholderType + " Template<p>root exception: " + ex.Message);
                }
            }
        } // callExternalPlaceholderRender

        public static string getParameterValue(string paramKey, string returnOnErrorOrNotFound, string[] placeholderParamList)
        {
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                if (placeholderParamList.Length < 1)
                    return returnOnErrorOrNotFound;

                Dictionary<string, string> tokens = HatCMS.TemplateEngine.TemplateEngineV2.tokenizeCommandParameters(placeholderParamList[0]);
                if (tokens.ContainsKey(paramKey))
                    return tokens[paramKey];
                if (tokens.ContainsKey(paramKey.ToLower()))
                    return tokens[paramKey.ToLower()];
                if (tokens.ContainsKey(paramKey.ToUpper()))
                    return tokens[paramKey.ToUpper()];

                List<string> keys = new List<string>(tokens.Keys);
                int index = StringUtils.IndexOf(keys.ToArray(), paramKey, StringComparison.CurrentCultureIgnoreCase);
                if (index >= 0)
                    return tokens[keys[index]];

                return returnOnErrorOrNotFound;
            }
            else
            {
                throw new NotImplementedException("Error: PlaceholderUtils.getParameterValue() can only be used for template engine v2");
            }
        } // getParameterValue

        public static bool getParameterValue(string paramKey, bool returnOnErrorOrNotFound, string[] placeholderParamList)
        {
            string v = getParameterValue(paramKey, returnOnErrorOrNotFound.ToString(), placeholderParamList);
            try
            {
                return Convert.ToBoolean(v);
            }
            catch
            { }
            return returnOnErrorOrNotFound;
        }

        public static int getParameterValue(string paramKey, int returnOnErrorOrNotFound, string[] placeholderParamList)
        {
            string v = getParameterValue(paramKey, returnOnErrorOrNotFound.ToString(), placeholderParamList);
            try
            {
                return Convert.ToInt32(v);
            }
            catch
            { }
            return returnOnErrorOrNotFound;
        }

        
    }
}
