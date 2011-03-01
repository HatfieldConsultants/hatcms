using System;
using System.Web.UI;
using System.Text;
using System.Reflection;

namespace HatCMS.Placeholders
{
	/// <summary>
    /// BaseCmsPlaceholder is the base type for all placeholders.
	/// </summary>
	public abstract class BaseCmsPlaceholder
	{
        [Obsolete("Access levels should be handled only by Zones, never by particular placeholders")]
        public enum AccessLevel { Anonymous, CmsAuthor, LoggedInUser }

        public enum RevertToRevisionResult { Failure = 0, Success = 1, NotImplemented = 2}
        
        public BaseCmsPlaceholder()
		{ }

        /// <summary>
        /// gets all dependencies for the placeholder.
        /// </summary>
        /// <returns></returns>
        public abstract CmsDependency[] getDependencies();
                        
        
        /// <summary>
        /// reverts a placeholder to a prior revision.
        /// </summary>
        /// <param name="oldPage">the page to revert to.</param>
        /// <param name="currentPage">the version of the page that is currently displayed to users</param>
        /// <param name="identifiers">the placeholder identifiers to revert in-bulk</param>
        /// <param name="language">the page language to revert</param>
        /// <returns></returns>
        public abstract RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language);
        
        /// <summary>
        /// Renders the placeholder to the HtmlTextWriter in view mode.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="paramList"></param>
        public abstract void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList);

        /// <summary>
        /// Renders the placeholder to the HtmlTextWriter in user-editable mode
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="paramList"></param>
        public abstract void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList);


        /// <summary>
        /// Renders the placeholder to the RssFeed in view mode.
        /// </summary>
        /// <param name="rssFeed"></param>
        // public static Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition , CmsLanguage langToRenderFor)
        public abstract Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor);        

        public Rss.RssItem CreateAndInitRssItem(CmsPage page, CmsLanguage langToRenderFor)
        {
            return InitRssItem(new Rss.RssItem(), page, langToRenderFor);
        }

        public Rss.RssItem InitRssItem(Rss.RssItem newRssItem, CmsPage page, CmsLanguage langToRenderFor)
        {
            newRssItem.Title = page.getTitle(langToRenderFor);
            newRssItem.Link = new Uri(page.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName, langToRenderFor));
            newRssItem.Guid = new Rss.RssGuid(newRssItem.Link);
            newRssItem.Author = page.LastModifiedBy;

            return newRssItem;
        }
        
	}
}
