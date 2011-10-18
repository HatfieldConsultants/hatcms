using System;
using System.Collections.Generic;
using System.Text;
using HatCMS;

namespace HatCMS.Modules.Glossary
{
    public class GlossaryModuleInfo: CmsModuleInfo
    {
        public override CmsDependency[] getModuleDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            ret.Add(new CmsControlDependency("RSSGlossary", CmsDependency.ExistsMode.MustNotExist));

            ret.Add(new CmsDatabaseTableDependency(@"
                    CREATE TABLE  `glossary` (
                      `glossaryid` int(10) unsigned NOT NULL AUTO_INCREMENT,
                      `pageid` int(10) unsigned NOT NULL,
                      `identifier` int(10) unsigned NOT NULL,
                      `langShortCode` varchar(255) NOT NULL,
                      `sortOrder` varchar(255) NOT NULL,
                      `ViewMode` varchar(255) NOT NULL,
                      `deleted` datetime DEFAULT NULL,
                      PRIMARY KEY (`glossaryid`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;"));

            ret.Add(new CmsDatabaseTableDependency(@"
                    CREATE TABLE  `glossarydata` (
                      `GlossaryDataId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                      `phGlossaryId` int(10) unsigned NOT NULL,
                      `isAcronym` int(10) unsigned NOT NULL,
                      `word` varchar(255) NOT NULL,
                      `description` text NOT NULL,
                      `deleted` datetime DEFAULT NULL,
                      PRIMARY KEY (`GlossaryDataId`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8;"));

            ret.Add(new CmsMessageDependency("Validated Glossary module dependencies"));

            return ret.ToArray();
        }
    }
}
