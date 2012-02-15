using System.Collections.Generic;
using NHibernate;
using System.Reflection;
using SharpArch.Core;
using SharpArch.Core.PersistenceSupport;
using NHibernate.Criterion;
using System.Collections.Specialized;
using System;
using SharpArch.Core.PersistenceSupport.NHibernate;
using SharpArch.Core.DomainModel;
using SharpArch.Data.NHibernate;
using HatCMS.Core.DataInterface;

namespace HatCMS.Core.DataRepository
{
    public class PageRepository : NHibernateRepository<CmsPage>, IPageRepository
    {
        #region IPageRepository Members

        public IList<CmsPage> fetallpage()
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPage))
                 .Add(Expression.IsNull("Deleted"));
            IList<CmsPage> pagelist = criteria.List<CmsPage>();
            return pagelist;
        }

        public IList<CmsPage> FetchPagesByTemplateName(string encodedTemplateName)
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPage))
                 .Add(Expression.Like("TemplateName", encodedTemplateName))
                 .Add(Expression.IsNull("Deleted"));
            IList<CmsPage> pagelist = criteria.List<CmsPage>();
            return pagelist;
        }

        public int CreateNewRepository(ref CmsPage page, CmsPageRevisionData newrevision)
        { 
            PageRevisionDataRepository revisionrepository = new PageRevisionDataRepository();
            if (revisionrepository.SaveOrUpdate(newrevision).Id < 0)
                return -1;
            else
            {
                page.RevisionNumber = newrevision.RevisionNumber;
                if (this.SaveOrUpdate(page).Id < 0)
                    return -1;
                else
                    return page.RevisionNumber;

            } 
        }

        public CmsPageLanguageInfo FetchPageInfo(IRepository<CmsPageLanguageInfo> langRepository, CmsPage page, CmsLanguage pageLanguage)
        {
            langRepository = new Repository<CmsPageLanguageInfo>();
            Dictionary<string, object> searchvaluepair = new Dictionary<string, object>();
            searchvaluepair.Add("PageId", page.Id);
            searchvaluepair.Add("LanguageShortCode", pageLanguage.shortCode);
            CmsPageLanguageInfo foundLanguageInfo = langRepository.FindOne(searchvaluepair);
            return foundLanguageInfo;
        }
        public CmsPage UpdatePageLanguageTitle(CmsPage page, string newTitle, CmsLanguage pageLanguage)
        {
            //IRepository<CmsPageLanguageInfo> langRepository = new Repository<CmsPageLanguageInfo>();
           // CmsPageLanguageInfo foundLanguageInfo = this.FetchPageInfo(langRepository, page, pageLanguage);
            IList<CmsPageLanguageInfo> languageinfolist = page.LanguageInfo;
            foreach(CmsPageLanguageInfo languageinfo in languageinfolist)
            {
                if (languageinfo.LanguageShortCode == pageLanguage.shortCode)
                    languageinfo.Title = newTitle;
            
            }
            //foundLanguageInfo.Title = newTitle;
            //return langRepository.SaveOrUpdate(foundLanguageInfo);
            return this.SaveOrUpdate(page);
        }

        public CmsPage UpdatePageLanguageMenuTitle(CmsPage page, string newmenuTitle, CmsLanguage pageLanguage)
        {
            IList<CmsPageLanguageInfo> languageinfolist = page.LanguageInfo;
            foreach (CmsPageLanguageInfo languageinfo in languageinfolist)
            {
                if (languageinfo.LanguageShortCode == pageLanguage.shortCode)
                    languageinfo.MenuTitle = newmenuTitle;

            }
            return this.SaveOrUpdate(page);
        }

        public CmsPage UpdatePageLanguageSearchEngineDescription(CmsPage page, string newDescription, CmsLanguage pageLanguage)
        {
            IList<CmsPageLanguageInfo> languageinfolist = page.LanguageInfo;
            foreach (CmsPageLanguageInfo languageinfo in languageinfolist)
            {
                if (languageinfo.LanguageShortCode == pageLanguage.shortCode)
                    languageinfo.SearchEngineDescription = newDescription;

            }
            return this.SaveOrUpdate(page);
        }

        public CmsPage UpdatePageLanguageName(CmsPage page, string newName, CmsLanguage pageLanguage)
        {
            IList<CmsPageLanguageInfo> languageinfolist = page.LanguageInfo;
            foreach (CmsPageLanguageInfo languageinfo in languageinfolist)
            {
                if (languageinfo.LanguageShortCode == pageLanguage.shortCode)
                    languageinfo.Name = newName;

            }
            return this.SaveOrUpdate(page);
        }

        public void clearAllExpiredPageLocks()
        {
            IRepository<CmsPageLockData> pagelockrepository = new Repository<CmsPageLockData>();
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageLockData))
                .Add(Expression.Lt("LockExpiresAt", DateTime.Now));
            IList<CmsPageLockData> expiredLockDataList = criteria.List<CmsPageLockData>();
            if(expiredLockDataList.Count > 0)
            {
                foreach (CmsPageLockData expiredlockdata in expiredLockDataList)
                {
                    pagelockrepository.Delete(expiredlockdata);
                }
            }
            
        }

        public CmsPageLockData getPageLockData(CmsPage page)
        { 
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageLockData))
                .Add(Expression.Eq("PageId", page.Id));
            IList<CmsPageLockData> lockdatalist = criteria.List<CmsPageLockData>();
            if (lockdatalist.Count > 0)
                return lockdatalist[0];
            else
                return null;        
        }

        public void clearCurrentPageLock(CmsPage pagetounlock)
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageLockData));
            criteria.Add(Restrictions.Or(Restrictions.Eq("PageId", pagetounlock.Id), Restrictions.Lt("LockExpiresAt", DateTime.Now)));
            IList<CmsPageLockData> datatounlocklist = criteria.List<CmsPageLockData>();
            
            if(datatounlocklist.Count > 0)
            {
                IRepository<CmsPageLockData> pagelockdatarepository = new Repository<CmsPageLockData>();
                foreach(CmsPageLockData datatounlock in datatounlocklist)
                    pagelockdatarepository.Delete(datatounlock);
            }            

        }

        public CmsPageLockData lockPageForEditing(CmsPageLockData datatolock)
        {
            IRepository<CmsPageLockData> pagelockdatarepository = new Repository<CmsPageLockData>();
            CmsPageLockData insertedLockData = pagelockdatarepository.SaveOrUpdate(datatolock);
            if (insertedLockData.PageId > 0)
                return insertedLockData;
            else
                return null;
        
        }

        #endregion
    }
}
