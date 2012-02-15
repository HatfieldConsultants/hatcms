using System;
using System.Collections.Generic;
using System.Text;
using HatCMS;

namespace HatCMS.Modules.Glossary.BackgroundTasks
{
    public class FetchUpdatedRSSGlossaryItems : CmsBackgroundTask
    {
        public override CmsBackgroundTaskInfo getBackgroundTaskInfo()
        {
            return new CmsBackgroundTaskInfo(CmsBackgroundTaskInfo.CmsTaskType.Periodic, CmsBackgroundTaskInfo.CmsPeriodicTaskPeriod.EveryDay);
        }

        public override void RunBackgroundTask()
        {
            // this task only works if the RSS feed is configured to be the datasource.
            if (GlossaryPlaceholderData.DataSource == GlossaryPlaceholderData.GlossaryDataSource.RssFeed)
            {
                string dataCacheKey = GlossaryPlaceholderData.getRssDataPersistentVariableName();
                string lastRunCacheKey = dataCacheKey + "_LastRun";

                CmsPersistentVariable persistedLastRun = CmsPersistentVariable.Fetch(lastRunCacheKey);
                bool doFetch = false;
                if (persistedLastRun.Name == "" || persistedLastRun.PersistedValue == null)
                    doFetch = true;
                if (persistedLastRun.Name != "" && !doFetch)
                {
                    DateTime lastRunTime = (DateTime)persistedLastRun.PersistedValue;
                    if (lastRunTime < DateTime.Now.AddDays(-1))
                        doFetch = true;
                }

                if (doFetch)
                {
                    FetchAndSaveRemoteRSSGlossaryData();
                }

            }
        } // RunBackgroundTask

        public bool FetchAndSaveRemoteRSSGlossaryData()
        {
            string dataCacheKey = GlossaryPlaceholderData.getRssDataPersistentVariableName();
            string lastRunCacheKey = dataCacheKey + "_LastRun";
            
            string rssUrl = GlossaryPlaceholderData.getRssDataSourceUrl();
            Rss.RssFeed glossaryRss = Rss.RssFeed.Read(rssUrl);
            if (glossaryRss.Channels.Count == 0)
            {
                // html.Append("<em>Error: could not retrieve Glossary from "+rssUrl+"</em>");
            }
            else
            {
                GlossaryData[] items = GlossaryData.FromRSSItems(glossaryRss.Channels[0].Items);
                CmsPersistentVariable persistedData = CmsPersistentVariable.Fetch(dataCacheKey);
                persistedData.Name = dataCacheKey;
                persistedData.PersistedValue = new List<GlossaryData>(items);
                bool b = persistedData.SaveToDatabase();

                if (b)
                {
                    CmsPersistentVariable persistedLastRun = CmsPersistentVariable.Fetch(lastRunCacheKey);
                    persistedLastRun.PersistedValue = DateTime.Now;
                    persistedLastRun.Name = lastRunCacheKey;
                    return persistedLastRun.SaveToDatabase();
                }

            }

            return false;

        } // FetchAndSaveRemoteRSSGlossaryData 

    }
}
