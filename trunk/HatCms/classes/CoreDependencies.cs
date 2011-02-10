using System;
using System.Reflection;
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
    /// <summary>
    /// This class gathers dependencies from all placeholders, controls, and core systems.
    /// </summary>
    public class CmsDependencies
    {
        /// <summary>
        /// This gathers dependencies from all placeholders, controls, and core systems.
        /// This function is called when checking for dependencies (during setup or audit).        
        /// </summary>
        /// <returns></returns>
        public static CmsDependency[] GatherAllDependencies()
        {
            
            List<CmsDependency> ret = new List<CmsDependency>();
            // -- tables
            ret.Add(new CmsDatabaseTableDependency("pages",
                new string[] { "pageId", "showInMenu", "template", "parentPageId", "SortOrdinal", "CreatedDateTime", "LastUpdatedDateTime", "LastModifiedBy", "RevisionNumber", "Deleted" }));
            ret.Add(new CmsDatabaseTableDependency("pagelocks",
                new string[] { "pageid", "LockedByUsername", "LockExpiresAt" }));
            ret.Add(new CmsDatabaseTableDependency("pagelanginfo",
                new string[] { "pageId", "langCode", "name", "title", "menuTitle", "searchEngineDescription" }));
            
            ret.Add(new CmsDatabaseTableDependency("resourceitemmetadata"));
            ret.Add(new CmsDatabaseTableDependency("resourceitems"));

            // -- some project directories should be removed from production sites
            #if  ! DEBUG
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("classes"));
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("setup"));
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("placeholders"));
			ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("_system/_AdminDocs"));
            #endif

            // -- files
            ret.Add(CmsFileDependency.UnderAppPath("default.aspx"));
            ret.Add(CmsFileDependency.UnderAppPath("_system/showThumb.aspx"));

            // -- config entries
            ret.Add(new CmsConfigItemDependency("TemplateEngineVersion"));
            ret.Add(new CmsConfigItemDependency("AdminUserRole"));
            ret.Add(new CmsConfigItemDependency("LoginUserRole"));
            ret.Add(new CmsConfigItemDependency("AuthorAccessUserRole"));
            ret.Add(new CmsConfigItemDependency("RequireAnonLogin", CmsDependency.ExistsMode.MustNotExist)); // deprecated 10 Feb 2011. Use the Zones system to manage anon logins
            ret.Add(new CmsConfigItemDependency("PathSpaceReplacementChar", CmsDependency.ExistsMode.MustNotExist)); // always set to "+".
            ret.Add(new CmsConfigItemDependency("RewriteEngineOn", CmsDependency.ExistsMode.MustNotExist)); // RewriteEngine is always on.
            ret.Add(new CmsConfigItemDependency("Languages"));
            ret.Add(new CmsConfigItemDependency("useInternal404NotFoundErrorHandler"));
            
            bool useInternal404NotFoundErrorHandler = CmsConfig.getConfigValue("useInternal404NotFoundErrorHandler", false);
            if (useInternal404NotFoundErrorHandler)
            {
                ret.Add(new CmsConfigItemDependency("Internal404NotFoundErrorHandlerPageUrl"));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("Internal404NotFoundErrorHandlerPageUrl", "/_internal/error404"), CmsConfig.Languages));
            }

            ret.Add(new CmsConfigItemDependency("CreateNewBlogPostPath", CmsDependency.ExistsMode.MustNotExist)); // blogging is no more.
            ret.Add(new CmsConfigItemDependency("blogPostTemplate", CmsDependency.ExistsMode.MustNotExist)); // blogging is no more.
            ret.Add(new CmsConfigItemDependency("DefaultImageThumbnailSize", CmsDependency.ExistsMode.MustNotExist)); // not used any more
            
            
            
          
            // -- all pages should have valid templates, placeholders and controls
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage page = allPages[pageId];
                ret.Add(new CmsTemplateDependency(page.TemplateName));


                string[] placeholderNames = new string[0];
                try
                {
                    placeholderNames = page.getAllPlaceholderNames();
                }
                catch(Exception ex)
                {
                    Console.Write("Could not get page placeholder names: " + ex.Message);
                }
                
                foreach (string phName in placeholderNames)
                {
                    ret.AddRange(PlaceholderUtils.getDependencies(phName));
                }

                string[] controlNames = new string[0];
                try
                {
                    controlNames = page.getAllControlPaths();
                }
                catch(Exception ex)
                {
                    Console.Write("Could not get page control names: " + ex.Message);
                }
                foreach (string controlName in controlNames)
                {
                    ret.Add(new CmsControlDependency(controlName));
                    ret.AddRange(CmsContext.currentPage.TemplateEngine.getControlDependencies(controlName));
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
                    ret.AddRange(PlaceholderUtils.getDependencies(phName));
                }


                string[] controlNames = dummyPage.TemplateEngine.getAllControlPaths();
                foreach (string controlName in controlNames)
                {
                    ret.Add(new CmsControlDependency(controlName));
                    ret.AddRange(dummyPage.TemplateEngine.getControlDependencies(controlName));
                }
            } // foreach


            return CmsDependency.RemoveDuplicates(ret.ToArray());
        }
  
    }
}
