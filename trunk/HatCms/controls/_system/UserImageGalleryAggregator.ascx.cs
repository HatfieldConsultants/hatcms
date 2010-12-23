using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Specialized;

namespace HatCMS.controls._system
{
    public partial class UserImageGalleryAggregator : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CmsPage p = CmsContext.currentPage;
            AddGalleryCommandToEditMenu(p, p);
        }

        /// <summary>
        /// Render the "Add an user image gallery"
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pageToRenderFor"></param>
        /// <param name="langToRenderFor"></param>
        /// <returns></returns>
        protected static string AddGalleryEditMenuRender(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
        {
            NameValueCollection createPageParams = action.CreateNewPageOptions.GetCreatePagePopupParams();
            if (action.CreateNewPageOptions.RequiresUserInput())
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderPopupLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> an image gallery", 500, 400);
            else
                return CmsPageEditMenu.DefaultStandardActionRenderers.RenderLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Add</strong> an image gallery");
        }

        /// <summary>
        /// Adds the "Add an image gallery" menu item to the Edit Menu.
        /// </summary>
        /// <param name="pageToAddCommandTo"></param>
        /// <param name="userImageGalleryAggregator"></param>
        public static void AddGalleryCommandToEditMenu(CmsPage pageToAddCommandTo, CmsPage userImageGalleryAggregator)
        {
            // -- only add the command if the user can author
            if (!CmsContext.currentUserCanAuthor)
                return;

            // -- base the command off the existing "create new sub-page" command
            CmsPageEditMenuAction createNewSubPage = pageToAddCommandTo.EditMenu.getActionItem(CmsEditMenuActionItem.CreateNewPage);

            if (createNewSubPage == null)
                return;

            CmsPageEditMenuAction newAction = createNewSubPage.Copy(); // copy everything from the CreateNewPage entry            

            // -- configure this command to not prompt authors for any information.
            //    the minimum information needed to create a page is the new page's filename (page.name)
            //      -- get the next unique filename            
            string newPageName = "";
            for (int eventNum = 1; eventNum < int.MaxValue; eventNum++)
            {
                string pageNameToTest = "Gallery" + eventNum.ToString();
                if (!CmsContext.childPageWithNameExists(userImageGalleryAggregator.ID, pageNameToTest))
                {
                    newPageName = pageNameToTest;
                    break;
                }
            }

            string newPageTitle = "";
            string newPageMenuTitle = "";
            string newPageSearchEngineDescription = "";
            bool newPageShowInMenu = true;
            string newPageTemplate = CmsConfig.getConfigValue("UserImageGallery.DetailsTemplateName", "UserImageGallery");

            newAction.CreateNewPageOptions = CmsCreateNewPageOptions.GetInstanceWithNoUserPrompts(newPageName, newPageTitle, newPageMenuTitle, newPageSearchEngineDescription, newPageShowInMenu, newPageTemplate, userImageGalleryAggregator.ID);

            newAction.CreateNewPageOptions.ParentPageId = userImageGalleryAggregator.ID;
            newAction.SortOrdinal = createNewSubPage.SortOrdinal + 1;
            newAction.doRenderToString = AddGalleryEditMenuRender;

            pageToAddCommandTo.EditMenu.addCustomActionItem(newAction);
        }
    }
}