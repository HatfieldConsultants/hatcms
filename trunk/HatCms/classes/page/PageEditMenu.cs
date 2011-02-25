using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;
using HatCMS.Placeholders;

namespace HatCMS
{
    public enum CmsEditMenuActionCategory { ThisPageAction, PageLockMaintenance, RevisionInformation, SubPageAction, SiteTool };
    
    public enum CmsEditMenuActionItem
    {
        EditThisPage, RenameThisPage, MoveThisPage, ChangeTemplate, ViewRevisions, DeleteThisPage,
        SaveAndViewThisPage, ExitFromEditing, SwitchEditLanguage,
        RemoveEditLock, RefreshEditLockStatus,
        ViewCurrentPageRevisionInformation,
        CreateNewPage, SortSubPages, 
        ChangeMenuVisibility,
        AdminReportsAndTools, UserManagement, Logoff, 
        CustomAction
    };
    
    public class CmsPageEditMenuAction
    {
        public CmsEditMenuActionCategory ActionCategory;
        public CmsEditMenuActionItem ActionItem;
        public CmsEditMode RequiredEditMode;
        public int SortOrdinal;
        public delegate string RenderToString(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor);
        public RenderToString doRenderToString;
        public object ActionPayload = null; // a place in memory to pass custom items to the doRenderToString function

        /// <summary>
        /// If this action is CreateNewPage, these options are used to help create that page.
        /// </summary>
        public CmsCreateNewPageOptions CreateNewPageOptions;

        public static string CurrentPageActionPath = "~.~";

        public CmsPageEditMenuAction(CmsEditMenuActionCategory actionCategory, CmsEditMenuActionItem actionItem, CmsEditMode requiredEditMode, int sortOrdinal, RenderToString renderDelegate)
        {
            ActionCategory = actionCategory;
            ActionItem = actionItem;
            RequiredEditMode = requiredEditMode;            
            SortOrdinal = sortOrdinal;
            doRenderToString = renderDelegate;
            CreateNewPageOptions = new CmsCreateNewPageOptions();
        }

        /// <summary>
        /// Creates a new instance of this object with all of the members having the same value.
        /// Note: termed "Copy" and not "Clone" to avoid confusion of the scope of the copy made (http://stackoverflow.com/questions/78536/cloning-objects-in-c/78856#78856)
        /// </summary>
        /// <returns></returns>
        public CmsPageEditMenuAction Copy()
        {
            CmsPageEditMenuAction ret = new CmsPageEditMenuAction(this.ActionCategory, this.ActionItem, this.RequiredEditMode, this.SortOrdinal, this.doRenderToString);
            ret.CreateNewPageOptions = this.CreateNewPageOptions;
            return ret;
        }

        public static int CompareBySortOrdinal(CmsPageEditMenuAction x, CmsPageEditMenuAction y)
        {
            return x.SortOrdinal.CompareTo(y.SortOrdinal);
        }

        #region ICloneable Members

        

        #endregion
    }

    public class CmsPageEditMenu
    {
        private CmsPage _page;

        public class DefaultStandardActionRenderers
        {
            public CmsDependency[] getDependencies()
            {
                List<CmsDependency> ret = new List<CmsDependency>();
                // -- required pages
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("KillLockPath", "/_admin/actions/killLock"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("LoginPath", "/_admin/login"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("GotoEditModePath", "/_admin/action/gotoEdit"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("GotoViewModePath", "/_admin/actions/gotoView"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("CreateNewPagePath", "/_admin/createPage"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("RenamePagePath", "/_admin/actions/renamePage"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("MovePagePath", "/_admin/actions/movePage"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("ChangePageTemplatePath", "/_admin/actions/changeTemplate"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("DeletePagePath", "/_admin/actions/deletePage"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("SortSubPagesPath", "/_admin/actions/sortSubPages"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("ChangeMenuVisibilityPath", "/_admin/actions/MenuVisibilityPopup"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("ViewRevisionsPagePath", "/_admin/ViewRevisions"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("AuditPagePath", "/_admin/Audit"), CmsConfig.Languages));
                ret.Add(new CmsPageDependency(CmsConfig.getConfigValue("EditUsersPagePath", "/_admin/EditUsers"), CmsConfig.Languages));
                return ret.ToArray();
            }

            public static string RenderLink(string actionConfigPagePathKey, string actionDefaultPagePath, CmsPage linkTargetPage, CmsLanguage langToRenderFor, string linkText)
            {
                return RenderLink(actionConfigPagePathKey, actionDefaultPagePath, new NameValueCollection(), linkTargetPage, langToRenderFor, linkText);
            }

            public static string RenderLink(string actionConfigPagePathKey, string actionDefaultPagePath, NameValueCollection actionPageParams, CmsPage linkTargetPage, CmsLanguage langToRenderFor, string linkText)
            {
                string actionPagePath = CmsConfig.getConfigValue(actionConfigPagePathKey, actionDefaultPagePath);
                actionPageParams.Add("target", linkTargetPage.ID.ToString());
                string toggleEditUrl = CmsContext.getUrlByPagePath(actionPagePath, actionPageParams, langToRenderFor);
                return "<a href=\"" + toggleEditUrl + "\">"+linkText+"</a>";
            }

            public static string RenderPopupLink(string actionConfigPagePathKey, string actionDefaultPagePath, CmsPage linkTargetPage, CmsLanguage langToRenderFor, string linkText, int popupWidth, int popupHeight)
            {
                return RenderPopupLink(actionConfigPagePathKey, actionDefaultPagePath, new NameValueCollection(), linkTargetPage, langToRenderFor, linkText, popupWidth, popupHeight);
            }

            public static string RenderPopupLink(string actionConfigPagePathKey, string actionDefaultPagePath, NameValueCollection actionPageParams, CmsPage linkTargetPage, CmsLanguage langToRenderFor, string linkText, int popupWidth, int popupHeight)
            {
                string actionPagePath = CmsConfig.getConfigValue(actionConfigPagePathKey, actionDefaultPagePath);
                actionPageParams.Add("target", linkTargetPage.ID.ToString());
                string toggleEditUrl = CmsContext.getUrlByPagePath(actionPagePath, actionPageParams, langToRenderFor);
                return "<a href=\"" + toggleEditUrl + "\" onclick=\"EditMenuShowModal(this.href," + popupWidth + "," + popupHeight + "); return false;\">" + linkText + "</a>";
            }

            public static string EditThisPage(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderLink("GotoEditModePath", "/_admin/action/gotoEdit", pageToRenderFor, langToRenderFor, "<strong>Edit</strong> this page");                
            }

            public static string Logoff(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                string actionPagePath = CmsConfig.getConfigValue("LoginPath", "/_admin/login");
                NameValueCollection paramList = new NameValueCollection();
                
                paramList.Add("action", "logoff");
                string actionUrl = CmsContext.getUrlByPagePath(actionPagePath, paramList, langToRenderFor);
                return "<a href=\"" + actionUrl + "\"><strong>Logoff</strong></a>";
            }

            public static string RenameThisPage(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderPopupLink("RenamePagePath", "/_admin/actions/renamePage", pageToRenderFor, langToRenderFor, "<strong>Rename</strong> this page", 500,400);
            }

            public static string MoveThisPage(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderPopupLink("MovePagePath", "/_admin/actions/movePage", pageToRenderFor, langToRenderFor, "<strong>Move</strong> this page", 500, 400);
            }

            public static string ChangeTemplate(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderPopupLink("ChangePageTemplatePath", "/_admin/actions/changeTemplate", pageToRenderFor, langToRenderFor, "Change page template", 500, 400);
            }

            public static string ViewRevisions(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderPopupLink("ViewRevisionsPagePath", "/_admin/ViewRevisions", pageToRenderFor, langToRenderFor, "Revisions", 500, 400);
            }

            public static string DeleteThisPage(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                NameValueCollection paramList = new NameValueCollection();
                paramList.Add("target", pageToRenderFor.ID.ToString());

                string confirmText = "Do you really want to delete this page?";
                int numPagesToDelete = pageToRenderFor.getLinearizedPages().Keys.Count;
                if (numPagesToDelete > 1)
                    confirmText = "Do you really want to delete this page and all " + (numPagesToDelete - 1) + " sub-pages?";

                string deletePageUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("DeletePagePath", "/_admin/actions/deletePage"), paramList, langToRenderFor);
                return "<a href=\"#\" onclick=\"EditMenuConfirmModal('" + confirmText + "','" + deletePageUrl + "',300, 300);\"><strong>Delete</strong> this page</a>";

            }

            public static string CreateNewPage(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                action.CreateNewPageOptions = CmsCreateNewPageOptions.CreateInstanceWithDefaultsForCurrentUser(pageToRenderFor);
                NameValueCollection createPageParams = action.CreateNewPageOptions.GetCreatePagePopupParams();
                if (action.CreateNewPageOptions.RequiresUserInput())
                {
                    return RenderPopupLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Create</strong> a new sub-page", 500, 400);
                }
                else
                {
                    return RenderLink("CreateNewPagePath", "/_admin/createPage", createPageParams, pageToRenderFor, langToRenderFor, "<strong>Create</strong> a new sub-page");
                }
            }

            public static string SortSubPages(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderPopupLink("SortSubPagesPath", "/_admin/actions/sortSubPages", pageToRenderFor, langToRenderFor, "<strong>Sort</strong> sub-pages", 500, 400);
            }

            public static string ChangeMenuVisibility(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderPopupLink("ChangeMenuVisibilityPath", "/_admin/actions/MenuVisibilityPopup", pageToRenderFor, langToRenderFor, "<strong>Change</strong> menu visibility", 650, 400);
            }

            public static string SaveAndViewThisPage(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                StringBuilder js = new StringBuilder();
                js.Append("function FloatingEditMenuSubmit() {" + Environment.NewLine);
                js.Append(" submitting = true;" + Environment.NewLine);
                js.Append(" document.getElementById('" + HatCMS.controls.StartEditForm.FormId + "').submit();" + Environment.NewLine);
                js.Append(" return false;" + Environment.NewLine);
                js.Append("}" + Environment.NewLine);
                pageToRenderFor.HeadSection.AddJSStatements(js.ToString());

                return "<input type=\"button\" onclick=\"return FloatingEditMenuSubmit();\" value=\"Save & view this page\">";
            }

            public static string ExitFromEditing(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                // return RenderLink("GotoEditModePath", "/_admin/action/gotoView", pageToRenderFor, langToRenderFor, "<strong>Exit from editing</strong>");
                
                NameValueCollection paramList = new NameValueCollection();
                paramList.Add("target", pageToRenderFor.ID.ToString());
                string url = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("GotoViewModePath", "/_admin/action/gotoView"), paramList, langToRenderFor);

                return "<input type=\"button\" onclick=\"window.location = '" + url + "';\" value=\"Exit from editing\">";
            }

            public static string SwitchEditLanguage(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                if (CmsConfig.Languages.Length > 1)
                {

                    pageToRenderFor.HeadSection.AddCSSStyleStatements(".FloatingLangCurrent { text-decoration: none; color: black; font-weight: bold; } ");
                    pageToRenderFor.HeadSection.AddCSSStyleStatements(".FloatingLangNotCurrent { text-decoration: underline; color: #444; font-weight: normal; } ");                                        

                    List<string> shortCodes = new List<string>();
                    foreach (CmsLanguage lang in CmsConfig.Languages)
                        shortCodes.Add(lang.shortCode.ToLower().Trim());

                    StringBuilder js = new StringBuilder();
                    js.Length = 0;
                    js.Append("function FloatingEditMenuSelectLanguage(langCode){" + Environment.NewLine);
                    js.Append("     var codes = [" + StringUtils.Join(",", shortCodes.ToArray(), "'", "'") + "];" + Environment.NewLine);
                    js.Append("     for(var i=0; i< codes.length; i++){" + Environment.NewLine);
                    js.Append("         if (langCode == codes[i]){" + Environment.NewLine);
                    js.Append("             document.getElementById('lang_'+codes[i]).style.display = 'block';" + Environment.NewLine);
                    js.Append("             document.getElementById('langSel_'+codes[i]).className = 'FloatingLangCurrent';" + Environment.NewLine);
                    js.Append("         } else {" + Environment.NewLine);
                    js.Append("             document.getElementById('lang_'+codes[i]).style.display = 'none';" + Environment.NewLine);
                    js.Append("             document.getElementById('langSel_'+codes[i]).className = 'FloatingLangNotCurrent';" + Environment.NewLine);
                    js.Append("         }" + Environment.NewLine);
                    js.Append("     }" + Environment.NewLine);
                    js.Append("}" + Environment.NewLine);
                    pageToRenderFor.HeadSection.AddJSStatements(js.ToString());

                    List<string> links = new List<string>();
                    foreach (CmsLanguage lang in CmsConfig.Languages)
                    {
                        string linkClass = "FloatingLangNotCurrent";
                        if (lang == CmsContext.currentLanguage)
                            linkClass = "FloatingLangCurrent";
                        links.Add("<a href=\"#\" class=\"" + linkClass + "\" id=\"langSel_" + lang.shortCode.ToLower() + "\" onclick=\"FloatingEditMenuSelectLanguage('" + lang.shortCode.ToLower() + "'); return false;\">" + lang.shortCode + "</a>");
                    } // foreach
                    
                    StringBuilder html = new StringBuilder();
                    html.Append("Edit Language: ");
                    html.Append(string.Join(" | ", links.ToArray()));
                    return html.ToString();
                }
                return "";
            }
            public static string RemoveEditLock(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                CmsPageLockData lockData = pageToRenderFor.getCurrentPageLockData();
                NameValueCollection paramList = new NameValueCollection();
                StringBuilder html = new StringBuilder();
                // -- Kill lock link
                if (lockData.LockedByUsername == CmsContext.currentWebPortalUser.UserName || CmsContext.currentUserIsSuperAdmin)
                {
                    paramList.Clear();
                    paramList.Add("target", pageToRenderFor.ID.ToString());
                    paramList.Add("action", "logoff");
                    string killLockLink = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("KillLockPath", "/_admin/actions/killLock"), paramList);
                    html.Append("<a href=\"#\" onclick=\"EditMenuConfirmModal('Do you really want to remove the edit lock?','" + killLockLink + "',300, 300);\"><strong>Remove</strong> edit lock</a><br />");
                }
                return html.ToString();
            }

            public static string RefreshEditLockStatus(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                CmsPageLockData lockData = pageToRenderFor.getCurrentPageLockData();
                StringBuilder html = new StringBuilder();
                if (lockData != null)
                {
                    html.Append("<strong>Editing is locked</strong><br />");
                    html.Append("Locked by: " + lockData.LockedByUsername + "<br />");
                    TimeSpan expiresIn = TimeSpan.FromTicks(lockData.LockExpiresAt.Ticks - DateTime.Now.Ticks);
                    int numMinutes = Convert.ToInt32(Math.Round(expiresIn.TotalMinutes));
                    html.Append("Lock expires in " + numMinutes + " minutes or when editing is complete<br />");
                    html.Append("<a href=\"" + pageToRenderFor.Url + "\">refresh status</a>");
                }
                return html.ToString();
            }
            

            public static string AdminReportsAndTools(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderPopupLink("AuditPagePath", "/_admin/Audit", pageToRenderFor, langToRenderFor, "<strong>Admin</strong> reports &amp; tools", 700, 400);
            }

            public static string UserManagement(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                return RenderPopupLink("EditUsersPagePath", "/_admin/EditUsers", pageToRenderFor, langToRenderFor, "<strong>User</strong> management", 500, 400);
            }

            public static string ViewCurrentPageRevisionInformation(CmsPageEditMenuAction action, CmsPage pageToRenderFor, CmsLanguage langToRenderFor)
            {
                StringBuilder html = new StringBuilder();
                CmsPageRevisionData revData = pageToRenderFor.getRevisionData(CmsContext.requestedPageVersionNumberToView);
                CmsPageRevisionData[] allRevs = pageToRenderFor.getAllRevisionData();
                bool isLiveVersion = false;
                if (revData.RevisionNumber == allRevs[allRevs.Length - 1].RevisionNumber)
                    isLiveVersion = true;

                if (isLiveVersion)
                    html.Append("Viewing live version");
                else
                    html.Append("Viewing page version #" + revData.RevisionNumber.ToString());
                html.Append("<br />");
                html.Append("Saved by " + revData.RevisionSavedByUsername + "<br />");
                html.Append("on " + revData.RevisionSavedAt.ToString("d MMM yyyy") + " at " + revData.RevisionSavedAt.ToString("%h:mm tt") + "<br />");

                // revert to this revision
                if (isLiveVersion)
                {
                    html.Append("<a href=\"" + pageToRenderFor.Url + "\">view full edit menu</a>");
                }
                else
                {
                    html.Append("<a href=\"" + pageToRenderFor.Url + "\">go to live version</a>");
                }
                
                return html.ToString();
            }
            

        }
        
        private CmsPageEditMenuAction[] AllStandardActions
        {
            get
            {
                return new CmsPageEditMenuAction[] {
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.EditThisPage, CmsEditMode.View, 0, DefaultStandardActionRenderers.EditThisPage),                    
                    
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.SubPageAction, CmsEditMenuActionItem.CreateNewPage, CmsEditMode.View, 10, DefaultStandardActionRenderers.CreateNewPage),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.SubPageAction, CmsEditMenuActionItem.SortSubPages, CmsEditMode.View, 20, DefaultStandardActionRenderers.SortSubPages),
                    
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.ChangeMenuVisibility, CmsEditMode.View, 25, DefaultStandardActionRenderers.ChangeMenuVisibility),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.RenameThisPage, CmsEditMode.View, 30, DefaultStandardActionRenderers.RenameThisPage),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.MoveThisPage, CmsEditMode.View, 40, DefaultStandardActionRenderers.MoveThisPage),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.ChangeTemplate, CmsEditMode.View, 50, DefaultStandardActionRenderers.ChangeTemplate),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.ViewRevisions, CmsEditMode.View, 60, DefaultStandardActionRenderers.ViewRevisions),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.DeleteThisPage, CmsEditMode.View, 70, DefaultStandardActionRenderers.DeleteThisPage),

                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.RevisionInformation, CmsEditMenuActionItem.ViewCurrentPageRevisionInformation, CmsEditMode.View, 600, DefaultStandardActionRenderers.ViewCurrentPageRevisionInformation),
                    
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.PageLockMaintenance, CmsEditMenuActionItem.RemoveEditLock, CmsEditMode.View, 80, DefaultStandardActionRenderers.RemoveEditLock),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.PageLockMaintenance, CmsEditMenuActionItem.RefreshEditLockStatus, CmsEditMode.View, 90, DefaultStandardActionRenderers.RefreshEditLockStatus),

                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.SiteTool, CmsEditMenuActionItem.AdminReportsAndTools, CmsEditMode.View, 100, DefaultStandardActionRenderers.AdminReportsAndTools),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.SiteTool, CmsEditMenuActionItem.UserManagement, CmsEditMode.View, 110, DefaultStandardActionRenderers.UserManagement),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.SiteTool, CmsEditMenuActionItem.Logoff, CmsEditMode.View, 1001, DefaultStandardActionRenderers.Logoff),

                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.SaveAndViewThisPage, CmsEditMode.Edit, 500, DefaultStandardActionRenderers.SaveAndViewThisPage),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.ExitFromEditing, CmsEditMode.Edit, 510, DefaultStandardActionRenderers.ExitFromEditing),
                    new CmsPageEditMenuAction( CmsEditMenuActionCategory.ThisPageAction, CmsEditMenuActionItem.SwitchEditLanguage, CmsEditMode.Edit, 520, DefaultStandardActionRenderers.SwitchEditLanguage),
                     
                };
            }
        }

        private List<CmsPageEditMenuAction> currentPageActions = new List<CmsPageEditMenuAction>();
        private bool currentPageActionsLoadedFromContext = false;
        public CmsPageEditMenuAction[] CurrentEditMenuActions
        {
            get
            {
                if (!currentPageActionsLoadedFromContext)
                {
                    // don't clear currentPageActions - it may already have items in it.
                    currentPageActions.AddRange(getAllPageActionsForCurrentContext());
                    currentPageActionsLoadedFromContext = true;
                }
                currentPageActions.Sort(CmsPageEditMenuAction.CompareBySortOrdinal);
                
                return currentPageActions.ToArray();
            }
        }

        private CmsPageEditMenuAction[] getAllPageActionsForCurrentContext()
        {
            // -- if the user can't author, they can't do anything.
            if (!CmsContext.currentPage.currentUserCanWrite)
                return new CmsPageEditMenuAction[0];

            List<CmsPageEditMenuAction> actionsByCategory = new List<CmsPageEditMenuAction>();            
            
            CmsEditMode currEditMode = CmsContext.currentEditMode;            
            CmsPageLockData lockData = _page.getCurrentPageLockData();
            
            // -- if the page is locked and we're viewing it, show PageLockMaintenance options
            if (lockData != null && currEditMode == CmsEditMode.View)
            {
                actionsByCategory.AddRange(getActionItemsByCategory(AllStandardActions, CmsEditMenuActionCategory.PageLockMaintenance));
            }
            else if (currEditMode == CmsEditMode.View && CmsContext.requestedPageVersionNumberToView >= 0)
            {
                actionsByCategory.AddRange(getActionItemsByCategory(AllStandardActions, CmsEditMenuActionCategory.RevisionInformation));
            }
            else
            {
                actionsByCategory.AddRange(getActionItemsByCategory(AllStandardActions, CmsEditMenuActionCategory.ThisPageAction));
                actionsByCategory.AddRange(getActionItemsByCategory(AllStandardActions, CmsEditMenuActionCategory.SubPageAction));
                actionsByCategory.AddRange(getActionItemsByCategory(AllStandardActions, CmsEditMenuActionCategory.SiteTool));
            }

            // -- Include those actions that match the current edit mode
            List<CmsPageEditMenuAction> ret = new List<CmsPageEditMenuAction>();
            foreach (CmsPageEditMenuAction a in actionsByCategory)
            {
                if (a.RequiredEditMode == currEditMode)
                    ret.Add(a);
            } // foreach

            // -- If we only have one language, don't include language switching
            if (CmsConfig.Languages.Length < 2)
                removeActionItem(ret, CmsEditMenuActionItem.SwitchEditLanguage);
                
            // -- don't allow the home page to be deleted nor renamed
            if (_page.ID == CmsContext.HomePage.ID)
            {
                removeActionItem(ret, CmsEditMenuActionItem.DeleteThisPage);
                removeActionItem(ret, CmsEditMenuActionItem.RenameThisPage);
            }

            // -- sort child pages only if there's more than one child page
            if (_page.ChildPages.Length <= 1)
                removeActionItem(ret, CmsEditMenuActionItem.SortSubPages);

            // -- remove Admin only tools
            if (!CmsContext.currentUserIsSuperAdmin)
            {
                removeActionItem(ret, CmsEditMenuActionItem.ChangeTemplate);
                removeActionItem(ret, CmsEditMenuActionItem.AdminReportsAndTools);
                removeActionItem(ret, CmsEditMenuActionItem.RemoveEditLock);
                removeActionItem(ret, CmsEditMenuActionItem.UserManagement);
            }

            return ret.ToArray();
            
        }

        public void addCustomActionItem(CmsPageEditMenuAction newActionItem)
        {
            // -- if the custom item already exists, do not add it
            foreach (CmsPageEditMenuAction action in currentPageActions)
            {
                if (action.ActionItem == newActionItem.ActionItem && action.doRenderToString == newActionItem.doRenderToString)
                    return;
            } 
            currentPageActions.Add(newActionItem);
        }

        private CmsPageEditMenuAction[] getActionItemsByCategory(CmsPageEditMenuAction[] haystack, CmsEditMenuActionCategory actionItemCategory)
        {
            List<CmsPageEditMenuAction> ret = new List<CmsPageEditMenuAction>();
            foreach (CmsPageEditMenuAction a in haystack)
            {
                if (a.ActionCategory == actionItemCategory)
                    ret.Add(a);
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Gets an action item from the CurrentPageActions.
        /// Returns NULL if the requested actionItem isn't in CurrentPageActions 
        /// </summary>
        /// <param name="actionItem"></param>
        /// <returns></returns>
        public CmsPageEditMenuAction getActionItem(CmsEditMenuActionItem actionItem)
        {
            return getActionItem(CurrentEditMenuActions, actionItem);
        }

        /// <summary>
        /// Gets an action item from the CurrentPageActions.
        /// Returns NULL if the requested actionItem isn't in CurrentPageActions 
        /// </summary>
        /// <param name="actionItem"></param>
        /// <returns></returns>
        private CmsPageEditMenuAction getActionItem(CmsPageEditMenuAction[] haystack, CmsEditMenuActionItem actionItem)
        {
            foreach (CmsPageEditMenuAction a in haystack)
            {
                if (a.ActionItem == actionItem)
                    return a;
            }
            return null;
        }

        public void removeActionItem(CmsEditMenuActionItem actionItemToRemove)
        {
            removeActionItem(currentPageActions, actionItemToRemove);             
        }

        private void removeActionItem(List<CmsPageEditMenuAction> haystack, CmsEditMenuActionItem actionItemToRemove)
        {
            // -- we need two steps to remove an item so that we don't modify the origional collection when removing.
            List<CmsPageEditMenuAction> toRemove = new List<CmsPageEditMenuAction>();
            foreach (CmsPageEditMenuAction a in haystack)
            {
                if (a.ActionItem == actionItemToRemove)
                    toRemove.Add(a);
            }

            foreach (CmsPageEditMenuAction a in toRemove)
                haystack.Remove(a);
            
        }



        /*
        public enum CreateNewPageOption { UseTargetPageTemplate, UseCustomTemplate };


        public List<CreateNewPageOption> CreateNewPageOptions = new List<CreateNewPageOption>();
        public string CreateNewPageCustomTemplateName = "";
        */

        public CmsPageEditMenu(CmsPage owningPage)
        {
            _page = owningPage;
            currentPageActions = new List<CmsPageEditMenuAction>();            
        }
    }
}
