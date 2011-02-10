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
        /// <param name="oldPage"></param>
        /// <param name="currentPage"></param>
        /// <param name="identifiers"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public abstract RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language);
        
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

        
	}
}
