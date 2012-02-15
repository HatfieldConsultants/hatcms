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
using HatCMS.Placeholders;

namespace HatCMS
{
    public class CMSUserInterfaceDependencies
    {
        /// <summary>
        /// Collect/Gather all User Interface related dependencies.
        /// </summary>
        /// <returns></returns>
        public static CmsDependency[] CollectUserInterfaceDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            // -- some project directories should be removed from production sites
            #if  ! DEBUG
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("classes"));
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("setup"));
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("placeholders"));
			ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("_system/_AdminDocs"));            
            #endif

            ret.Add(CmsFileDependency.UnderAppPath("default.aspx"));

            // -- writable directories
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/writable/js"));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/writable/css"));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/writable/Modules"));

            // -- ensure that the HtmlContent placeholders do not have the old link to the showThumb.aspx page (note: this validation is slow, but very useful.)
            ret.Add(new CmsPlaceholderContentDependency("HtmlContent", "_system/showthumb.aspx", CmsDependency.ExistsMode.MustNotExist, StringComparison.CurrentCultureIgnoreCase));


            // -- all pages should have valid templates, placeholders and controls
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage page = allPages[pageId];
                ret.Add(new CmsTemplateDependency(page.TemplateName, "Page ID #" + pageId.ToString()));


                string[] placeholderNames = new string[0];
                try
                {
                    placeholderNames = page.getAllPlaceholderNames();
                }
                catch (Exception ex)
                {
                    ret.Add(new CmsConfigItemDependency("GatherAllDependencies: Could not get page (pageid:" + pageId + ") placeholder names: " + ex.Message));
                }

                foreach (string phName in placeholderNames)
                {
                    ret.Add(new CmsPlaceholderDependency(phName, page.TemplateName));
                    if (PlaceholderUtils.PlaceholderExists(phName))
                        ret.AddRange(PlaceholderUtils.getDependencies(phName));
                }

                string[] controlPaths = new string[0];
                try
                {
                    controlPaths = page.getAllControlPaths();
                }
                catch (Exception ex)
                {
                    ret.Add(new CmsConfigItemDependency("GatherAllDependencies: Could not get page control paths (pageid:" + pageId + ") : " + ex.Message));
                }
                foreach (string controlPath in controlPaths)
                {
                    ret.Add(new CmsControlDependency(controlPath));
                    ret.AddRange(CmsContext.currentPage.TemplateEngine.getControlDependencies(controlPath));
                }
            } // foreach page

            // -- all templates should have valid controls and placeholders (regardless of if the template is implemented in a page or not)
            string[] templates = CmsContext.getTemplateNamesForCurrentUser();
            CmsPage dummyPage = new CmsPage();
            foreach (string template in templates)
            {
                dummyPage.TemplateName = template;

                string[] placeholderNames = dummyPage.getAllPlaceholderNames();

                foreach (string phName in placeholderNames)
                {
                    ret.Add(new CmsPlaceholderDependency(phName, template));
                    if (PlaceholderUtils.PlaceholderExists(phName))
                        ret.AddRange(PlaceholderUtils.getDependencies(phName));
                }


                CmsControlDefinition[] controlDefs = dummyPage.TemplateEngine.getAllControlDefinitions();
                foreach (CmsControlDefinition controlDef in controlDefs)
                {
                    ret.Add(new CmsControlDependency(controlDef));
                    ret.AddRange(dummyPage.TemplateEngine.getControlDependencies(controlDef.ControlNameOrPath));
                }
            } // foreach

            return ret.ToArray();
        }
    }
}
