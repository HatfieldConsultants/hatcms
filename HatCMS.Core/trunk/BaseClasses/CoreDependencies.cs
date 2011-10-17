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
    public class CmsDependencyUtils
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
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `pages` (
                  `pageId` int(11) NOT NULL AUTO_INCREMENT,
                  `showInMenu` int(10) unsigned NOT NULL DEFAULT '1',
                  `template` varchar(255) NOT NULL,
                  `parentPageId` int(11) NOT NULL DEFAULT '0',
                  `SortOrdinal` int(11) NOT NULL DEFAULT '0',
                  `CreatedDateTime` datetime NOT NULL,
                  `LastUpdatedDateTime` datetime NOT NULL,
                  `LastModifiedBy` varchar(255) NOT NULL DEFAULT '',
                  `RevisionNumber` int(11) NOT NULL DEFAULT '1',
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`pageId`),
                  KEY `pages_secondary` (`pageId`,`Deleted`),
                  KEY `pages_tertiary` (`parentPageId`,`Deleted`),
                  KEY `pages_quartinary` (`parentPageId`,`Deleted`) USING BTREE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `pagelocks` (
                  `pageid` int(11) NOT NULL,
                  `LockedByUsername` varchar(255) NOT NULL,
                  `LockExpiresAt` datetime NOT NULL,
                  PRIMARY KEY (`pageid`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `pagelanginfo` (
                  `pageId` int(10) unsigned NOT NULL,
                  `langCode` varchar(255) NOT NULL,
                  `name` varchar(255) DEFAULT NULL,
                  `title` varchar(255) NOT NULL,
                  `menuTitle` varchar(255) NOT NULL,
                  `searchEngineDescription` text NOT NULL,
                  PRIMARY KEY (`pageId`,`langCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));

            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `resourceitemmetadata` (
                  `AutoIncId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `ResourceId` int(10) unsigned NOT NULL,
                  `ResourceRevisionNumber` int(10) unsigned NOT NULL,
                  `Name` varchar(255) NOT NULL,
                  `Value` longtext NOT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`AutoIncId`),
                  KEY `ResourceId` (`ResourceId`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
                "));

            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `resourceitems` (
                  `AutoIncId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `ResourceId` int(11) NOT NULL,
                  `RevisionNumber` int(11) NOT NULL,
                  `Filename` varchar(255) NOT NULL,
                  `FilePath` text NOT NULL,
                  `FileDirectory` text NOT NULL,
                  `FileSize` int(10) unsigned NOT NULL,
                  `FileTimestamp` datetime NOT NULL,
                  `MimeType` varchar(255) NOT NULL,
                  `ModifiedBy` varchar(255) NOT NULL,
                  `ModificationDate` datetime NOT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`AutoIncId`),
                  UNIQUE KEY `ResourceItemsUniqueIdRevisionNumber` (`ResourceId`,`RevisionNumber`),
                  KEY `RevisionNumIndex` (`RevisionNumber`,`FileDirectory`(255),`Deleted`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
                "));

            ret.Add(new CmsDatabaseTableDependency(@"
                    CREATE TABLE `PersistentVariables` (
                      `PersistentVariableId` INT NOT NULL AUTO_INCREMENT ,
                      `Name` VARCHAR(255) NOT NULL ,
                      `PersistedValue` BLOB NULL ,
                      PRIMARY KEY (`PersistentVariableId`) ,
                      UNIQUE INDEX `Name_UNIQUE` (`Name` ASC) );
                "));

            // -- some project directories should be removed from production sites
            #if  ! DEBUG
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("classes"));
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("setup"));
            ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("placeholders"));
			ret.Add(CmsDirectoryDoesNotExistDependency.UnderAppPath("_system/_AdminDocs"));

            if (!Hatfield.Web.Portal.PageUtils.IsRunningOnMono())
            {
                ret.Add(CmsFileDependency.UnderAppPath("bin/XmpToolkit.dll")); // ensure that the XmpToolkit is being copied over
            }

            #endif

            // -- files
            ret.Add(CmsFileDependency.UnderAppPath("default.aspx"));
            
            foreach(string filePath in CmsConfig.URLsToNotRemap)
                ret.Add(CmsFileDependency.UnderAppPath(filePath));

            // -- config entries            
            ret.Add(new CmsConfigItemDependency("URLsToNotRemap"));            
            ret.Add(new CmsConfigItemDependency("AdminUserRole"));
            ret.Add(new CmsConfigItemDependency("LoginUserRole"));
            ret.Add(new CmsConfigItemDependency("AuthorAccessUserRole"));
            ret.Add(new CmsConfigItemDependency("TemplateEngineVersion", CmsDependency.ExistsMode.MustNotExist)); // removed Mar 18, 2011. Only v2 templates are now supported.
            ret.Add(new CmsConfigItemDependency("RequireAnonLogin", CmsDependency.ExistsMode.MustNotExist)); // deprecated 10 Feb 2011. Use the Zones system to manage anon logins
            ret.Add(new CmsConfigItemDependency("PathSpaceReplacementChar", CmsDependency.ExistsMode.MustNotExist)); // always set to "+".
            ret.Add(new CmsConfigItemDependency("RewriteEngineOn", CmsDependency.ExistsMode.MustNotExist)); // RewriteEngine is always on.
            ret.Add(new CmsConfigItemDependency("ThumbImageCacheDirectory", CmsDependency.ExistsMode.MustNotExist)); // thumbnail cache is always in _system/writable/ThumbnailCache
            ret.Add(new CmsConfigItemDependency("Languages"));
            ret.Add(new CmsConfigItemDependency("useInternal404NotFoundErrorHandler"));
            ret.Add(new CmsConfigItemDependency("smtpServer"));
            ret.Add(new CmsConfigItemDependency("TechnicalAdministratorEmail"));                  
      
            // -- writable directories
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/writable/js"));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/writable/css"));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("_system/writable/Modules"));
            
            
            bool useInternal404NotFoundErrorHandler = CmsConfig.getConfigValue("useInternal404NotFoundErrorHandler", false);
            if (useInternal404NotFoundErrorHandler)
            {
                ret.Add(new CmsConfigItemDependency("Internal404NotFoundErrorHandlerPageUrl"));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("Internal404NotFoundErrorHandlerPageUrl", "/_internal/error404"), CmsConfig.Languages));
            }

            ret.Add(new CmsConfigItemDependency("CreateNewBlogPostPath", CmsDependency.ExistsMode.MustNotExist)); // blogging is no more.
            ret.Add(new CmsConfigItemDependency("blogPostTemplate", CmsDependency.ExistsMode.MustNotExist)); // blogging is no more.
            ret.Add(new CmsConfigItemDependency("DefaultImageThumbnailSize", CmsDependency.ExistsMode.MustNotExist)); // not used any more
            
            // -- ensure that the HtmlContent placeholders do not have the old link to the showThumb.aspx page (note: this validation is slow, but very useful.)
            ret.Add(new CmsPlaceholderContentDependency("HtmlContent", "_system/showthumb.aspx", CmsDependency.ExistsMode.MustNotExist, StringComparison.CurrentCultureIgnoreCase));

            ret.Add(new CmsControlDependency("_system/internal/EditCalendarCategoriesPopup", CmsDependency.ExistsMode.MustNotExist)); // deprecated. now "EventCalendarCategoryPopup"

            ret.Add(new CmsPageDependency("/_admin/actions/deleteNews", CmsConfig.Languages, CmsDependency.ExistsMode.MustNotExist)); // deleteNews page is deprecated; news deletion is handled on the page level.
            ret.Add(new CmsPageDependency("/_admin/actions/deleteJob", CmsConfig.Languages, CmsDependency.ExistsMode.MustNotExist)); // deleteJob page is deprecated; news deletion is handled on the page level.
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("LoginPath", "/_login"), CmsConfig.Languages));

            // -- gather all admin tool dependencies
            ret.AddRange(HatCMS.Admin.BaseCmsAdminTool.getAllAdminToolDependencies());

            // -- gather all Module-level dependencies
            ret.AddRange(CmsModuleUtils.getAllModuleLevelDependencies());
            
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
                catch(Exception ex)
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

            // remove all duplicates based on the content of each dependency.

            return CmsDependency.RemoveDuplicates(ret.ToArray());
        }
  
    }
}
