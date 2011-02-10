using System;
using System.Web.UI;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// Summary description for BaseCmsPlaceholder.
	/// </summary>
    [Obsolete("BaseCmsMasterDetailsPlaceholder class has been replaced with the idea of 'Aggregator' and 'Details' placeholders")]
	public abstract class BaseCmsMasterDetailsPlaceholder: BaseCmsPlaceholder
	{
        public enum PlaceholderDisplay { MultipleItems, SelectedItem }

        public BaseCmsMasterDetailsPlaceholder()
		{ }

        public abstract PlaceholderDisplay getCurrentDisplayMode();

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            PlaceholderDisplay display = getCurrentDisplayMode();
            if (display == PlaceholderDisplay.MultipleItems)
            {
                RenderMultipleItems_InViewMode(writer, page, identifier, langToRenderFor, paramList);
            }
            else if (display == PlaceholderDisplay.SelectedItem)
            {
                RenderSelectedItem_InViewMode(writer, page, identifier, langToRenderFor, paramList);
            }
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            PlaceholderDisplay display = getCurrentDisplayMode();
            if (display == PlaceholderDisplay.MultipleItems)
            {
                RenderMultipleItems_InEditMode(writer, page, identifier, langToRenderFor, paramList);
            }
            else if (display == PlaceholderDisplay.SelectedItem)
            {
                RenderSelectedItem_InEditMode(writer, page, identifier, langToRenderFor, paramList);
            }
        }
                

        public abstract void RenderSelectedItem_InEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList);
        public abstract void RenderSelectedItem_InViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList);
        public abstract void RenderMultipleItems_InEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList);
        public abstract void RenderMultipleItems_InViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList);
	}
}
