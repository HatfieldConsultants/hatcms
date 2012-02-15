using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Text;
using Hatfield.Web.Portal.Collections;

namespace HatCMS
{
	/// <summary>
	/// Caches pages in memory, per request so that multiple calls to the database can be avoided.
	/// </summary>
	public class CmsPageCache
	{
		/// <summary>
		/// use a NameObject collection so that this cache is only a memory cache for the current request
		/// </summary>
        private Dictionary<string, CmsPage> cache;

		private int homepageId;

        private bool allPagesHaveBeenCached;

        private Object cacheLock;

		/// <summary>
		/// creates a new PageCache object.
		/// </summary>
		public CmsPageCache()
		{
            cache = new Dictionary<string, CmsPage>();
            homepageId = Int32.MinValue;
            allPagesHaveBeenCached = false;
            cacheLock = new Object();
		}

        private string getKey(int requestedPageId)
        {
            return "cachedPageRequest" + requestedPageId.ToString();
        }
		

		/// <summary>
		/// clear all static variables used for the cache
		/// </summary>
		public void Clear()
		{
            lock (cacheLock)
            {
                while (cache.Keys.Count > 0)
                {
                    cache.Clear();
                }
                homepageId = Int32.MinValue;
                allPagesHaveBeenCached = false;
            }									
		}

        public void AddAllPagesToCache(CmsPage[] allPages)
        {
            lock (cacheLock)
            {
                foreach (CmsPage p in allPages)
                {
                    Add(p.Id, p);
                } // foreach
                allPagesHaveBeenCached = true;
            }
        }

        public CmsPage[] getAllPagesFromCache()
        {
            lock (cacheLock)
            {
                if (allPagesHaveBeenCached)
                {

                    List<CmsPage> ret = new List<CmsPage>();
                    foreach (string k in cache.Keys)
                    {
                        ret.Add(cache[k] as CmsPage);
                    }
                    return ret.ToArray();

                }
                else
                    throw new ArgumentException("do not call getAllPagesFromCache without putting all pages into the cache using  AddAllPagesToCache!!");
            }
        }

        public bool AllPagesHaveBeenCached
        {
            get
            {
                return allPagesHaveBeenCached;
            }
        }
		
        /// <summary>
        /// adds a fully initialized CmsPage to the cache.
        /// </summary>
        /// <param name="page"></param>
        public void Add(int requestedPageId, CmsPage page)
        {
            if (requestedPageId < 0)
                return;

            string cacheKey = getKey(requestedPageId);
            lock (cacheLock)
            {
                if (cache.ContainsKey(cacheKey))
                {
                    cache.Remove(cacheKey);
                }

                cache.Add(cacheKey, page);
            }
        }

        public void Remove(CmsPage pageToRemove)
        {
            string cacheKey = getKey(pageToRemove.Id);
            lock (cacheLock)
            {
                if (cache.ContainsKey(cacheKey))
                {
                    cache.Remove(cacheKey);
                }
            }
        } // Remove

        public void Update(CmsPage pageToUpdate)
        {
            Remove(pageToUpdate);
            Add(pageToUpdate.Id, pageToUpdate);
        }

		/// <summary>
		/// gets a page from the cache. Returns NULL if the page was not in the cache.
		/// </summary>
		/// <param name="pageId"></param>
		/// <returns>the page. If not found, returns NULL.</returns>
		public CmsPage Get(int pageId)
		{
            string cacheKey = getKey(pageId);
            lock (cacheLock)
            {
                if (cache.ContainsKey(cacheKey))
                    return cache[cacheKey];
            }
            return null;            
		}

		
		/// <summary>
		/// returns Int32.MinValue if the HomePageId has not been set
		/// </summary>
		/// <returns></returns>
		public int GetHomePageId()
		{
			return homepageId;
		}

		public void SetHomePageId(int HomePageId)
		{
			homepageId = HomePageId;
		}

		/// <summary>
		/// gets a child page from the cache. If not found, returns NULL
		/// </summary>
		/// <param name="parentPageId"></param>
		/// <param name="childName"></param>
		/// <returns></returns>
        public CmsPage Get(int parentPageId, string childName, CmsLanguage childNameLanguage)
		{
			CmsPage parentPage = Get(parentPageId);
			if (parentPage != null && parentPage.Id != -1)
			{
				foreach(CmsPage child in parentPage.AllChildPages)
				{					
                    if (String.Compare(child.getName(childNameLanguage), childName, true) == 0)
						return child;
				}
			}
			return null;
		}
        
	}
	
}
