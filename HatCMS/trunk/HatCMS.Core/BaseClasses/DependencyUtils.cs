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

            // -- HatCMS.Core dependencies
            ret.AddRange(CollectCoreDependencies());

            // -- HatCMS.Web dependencies
            ret.AddRange(CmsContext.UserInterface.CollectUIDependencies());

            // -- gather all admin tool dependencies
            ret.AddRange(HatCMS.Admin.BaseCmsAdminTool.getAllAdminToolDependencies());

            // -- gather all Module-level dependencies
            ret.AddRange(CmsModuleUtils.getAllModuleLevelDependencies());

            // -- remove all duplicates based on the content of each dependency.
            return CmsDependency.RemoveDuplicates(ret.ToArray());
            
        }


        /// <summary>
        /// Collect the dependecies embedded in the HatCMS.Core library.
        /// </summary>
        /// <returns></returns>
        private static CmsDependency[] CollectCoreDependencies()
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
                    CREATE TABLE `persistentvariables` (
                      `PersistentVariableId` int(11) NOT NULL AUTO_INCREMENT,
                      `Name` varchar(255) NOT NULL,
                      `PersistedValue` longblob,
                      PRIMARY KEY (`PersistentVariableId`),
                      UNIQUE KEY `Name_UNIQUE` (`Name`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
                "));

            // -- some project directories should be removed from production sites
            #if  ! DEBUG            
            if (!Hatfield.Web.Portal.PageUtils.IsRunningOnMono())
            {
                ret.Add(CmsFileDependency.UnderAppPath("bin/XmpToolkit.dll")); // ensure that the XmpToolkit is being copied over
            }

            #endif

            // -- files            
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
                  
            
            bool useInternal404NotFoundErrorHandler = CmsConfig.getConfigValue("useInternal404NotFoundErrorHandler", false);
            if (useInternal404NotFoundErrorHandler)
            {
                ret.Add(new CmsConfigItemDependency("Internal404NotFoundErrorHandlerPageUrl"));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("Internal404NotFoundErrorHandlerPageUrl", "/_internal/error404"), CmsConfig.Languages));
            }

            ret.Add(new CmsConfigItemDependency("CreateNewBlogPostPath", CmsDependency.ExistsMode.MustNotExist)); // blogging is no more.
            ret.Add(new CmsConfigItemDependency("blogPostTemplate", CmsDependency.ExistsMode.MustNotExist)); // blogging is no more.
            ret.Add(new CmsConfigItemDependency("DefaultImageThumbnailSize", CmsDependency.ExistsMode.MustNotExist)); // not used any more
            

            ret.Add(new CmsControlDependency("_system/internal/EditCalendarCategoriesPopup", CmsDependency.ExistsMode.MustNotExist)); // deprecated. now "EventCalendarCategoryPopup"

            ret.Add(new CmsPageDependency("/_admin/actions/deleteNews", CmsConfig.Languages, CmsDependency.ExistsMode.MustNotExist)); // deleteNews page is deprecated; news deletion is handled on the page level.
            ret.Add(new CmsPageDependency("/_admin/actions/deleteJob", CmsConfig.Languages, CmsDependency.ExistsMode.MustNotExist)); // deleteJob page is deprecated; news deletion is handled on the page level.
            ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("LoginPath", "/_login"), CmsConfig.Languages));


            return ret.ToArray();
        }
  
    }
}
