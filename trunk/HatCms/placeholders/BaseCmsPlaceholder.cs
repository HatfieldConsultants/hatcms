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
        public abstract bool revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language);
        
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
        /// Get a place holder type according to the name
        /// (usually it is the CmsPage template name)
        /// </summary>
        /// <param name="placeHolderName"></param>
        /// <returns></returns>
        public static Type getPlaceHolder(string placeHolderName)
        {
            const string currentNamespace = "HatCMS.Placeholders.";
            return Assembly.GetExecutingAssembly().GetType(currentNamespace + placeHolderName);
        }

        /// <summary>
        /// Invoke a static method within a place holder.  The method returns a string
        /// of html anchor.  Then string will be displayed under the EditMenu.
        /// </summary>
        /// <param name="placeHolderName"></param>
        /// <param name="editMenuMethodName"></param>
        /// <param name="parm"></param>
        /// <returns></returns>
        public static string invokeEditMenuMethod(string placeHolderName, string editMenuMethodName, object[] parm)
        {
            Type t = getPlaceHolder(placeHolderName);
            object obj = t.InvokeMember(editMenuMethodName, BindingFlags.InvokeMethod, null, t, parm);
            return (string)obj;
        }
        
	}
}
