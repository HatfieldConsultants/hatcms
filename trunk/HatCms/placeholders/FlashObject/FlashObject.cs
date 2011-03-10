using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections;
using System.IO;

using Hatfield.Web.Portal;
using HatCMS.WebEditor.Helpers;

namespace HatCMS.Placeholders
{

    public class FlashObjectData
    {
        public int FlashObjectId = -1;
        public string SWFPath = "";
        public int DisplayWidth = FlashObject.DefaultDisplayWidth;
        public int DisplayHeight = FlashObject.DefaultDisplayHeight;        

    }
    
    /// <summary>
	/// Summary description for PageTitle.
	/// </summary>
    public class FlashObject : BaseCmsPlaceholder
	{

        public static int DefaultDisplayWidth = 700;
        public static int DefaultDisplayHeight = 500;


        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("_system/tools/FlashObject/PopupFlashObjectBrowser.aspx"));
            ret.Add(CmsWritableDirectoryDependency.UnderAppPath("UserFiles/Flash"));
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `flashobject` (
                  `FlashObjectId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `SWFPath` varchar(255) NOT NULL DEFAULT '',
                  `DisplayWidth` int(11) NOT NULL,
                  `DisplayHeight` int(11) NOT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`FlashObjectId`),
                  KEY `flashobject_secondary` (`PageId`,`Identifier`,`Deleted`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            return ret.ToArray();
        }


        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }

        public FlashObject()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		

		/// <summary>
        /// renders the FlashObject control to the HtmlTextWriter in View Mode
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="page"></param>
		/// <param name="identifier"></param>
		/// <param name="paramList"></param>		
        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            FlashObjectDb db = (new FlashObjectDb());
            FlashObjectData flash = db.getFlashObject(page, identifier, true);
            StringBuilder html = new StringBuilder();
            if (flash.SWFPath != "" && flash.DisplayHeight > 0 && flash.DisplayWidth > 0)
            {
                string swfPath = flash.SWFPath;
                if (!swfPath.StartsWith(CmsContext.ApplicationPath))
                    swfPath = CmsContext.ApplicationPath + swfPath;

                // -- use the base parameter so that loadMovie and loadClip work
                string swfFilename = Path.GetFileName(swfPath);
                string baseUrl = swfPath.Substring(0, swfPath.Length - swfFilename.Length);

                html.Append("<div class=\"FlashObjectPlaceholder\">");
                // -- http://kb.adobe.com/selfservice/viewContent.do?externalId=50c1cf38
                html.Append("<object classid=\"clsid:d27cdb6e-ae6d-11cf-96b8-444553540000\" codebase=\" http://fpdownload.adobe.com/pub/shockwave/cabs/flash/swflash.cab#version=8,0,0,0\" width=\"" + flash.DisplayWidth.ToString() + "\" height=\"" + flash.DisplayHeight.ToString() + "\" align=\"middle\">" + Environment.NewLine);
                html.Append("        <param name=\"movie\" value=\"" + swfPath + "\">");
                html.Append("        <param name=\"allowScriptAccess\" value=\"always\">");
                html.Append("        <param name=\"base\" value=\"" + baseUrl + "\">");
                html.Append("        <embed type=\"application/x-shockwave-flash\" pluginspage=\"http://www.adobe.com/go/getflashplayer\" width=\"" + flash.DisplayWidth.ToString() + "\" height=\"" + flash.DisplayHeight.ToString() + "\" align=\"middle\" src=\"" + swfPath + "\" allowScriptAccess=\"always\" base=\"" + baseUrl + "\"></embed>" + Environment.NewLine);
                html.Append("</object>" + Environment.NewLine);
                html.Append("</div>");
            }
            writer.WriteLine(html.ToString());

        } // RenderView

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {

            string formName = "editFlashObject_" + page.ID.ToString() + identifier.ToString() + langToRenderFor.shortCode;            

            FlashObjectDb db = (new FlashObjectDb());
            FlashObjectData flashObject = db.getFlashObject(page, identifier, true);

            StringBuilder html = new StringBuilder();
            // ------- CHECK THE FORM FOR ACTIONS
            string action = Hatfield.Web.Portal.PageUtils.getFromForm(formName + "_FlashObjectAction", "");
            if (action.Trim().ToLower() == "saveflashobject")
            {
                flashObject.SWFPath = PageUtils.getFromForm(formName + "SWFPath", "");
                flashObject.DisplayWidth = PageUtils.getFromForm(formName + "displayWidth", FlashObject.DefaultDisplayWidth);
                flashObject.DisplayHeight = PageUtils.getFromForm(formName + "displayHeight", FlashObject.DefaultDisplayHeight);
                bool b = db.saveUpdatedFlashObject(page, identifier, flashObject);
                if (!b)
                {
                    html.Append("Error: Flash Object not saved - database error<p><p>");
                }
            }

            // ------- START RENDERING            
                        
            // note: no need to put in the <form></form> tags.
            
            html.Append("<table>");
            
            html.Append("<tr><td>");
            html.Append("Flash (SWF) Object:");
            html.Append("</td><td>");


            string JSCallbackFunctionName = formName+"_selectPath";
            
            StringBuilder js = new StringBuilder();
            js.Append("function " + JSCallbackFunctionName + "(selText, selVal) { " + Environment.NewLine);
            // html.Append("alert(selVal);" + Environment.NewLine);
            js.Append(" var selectBox = document.getElementById('" + formName + "SWFPath'); " + Environment.NewLine);
            js.Append(" var found= false; " + Environment.NewLine);
            js.Append(" for (var i =0; i < selectBox.options.length; i++) {" + Environment.NewLine);
            js.Append("   if (selectBox.options[i].text == selText){ " + Environment.NewLine);
            js.Append("      selectBox.options[i].selected = true; found = true; " + Environment.NewLine);
            js.Append("   } // if" + Environment.NewLine);
            js.Append(" } // for" + Environment.NewLine);
            js.Append(" if (!found) { " + Environment.NewLine);
            js.Append("   var newOption = new Option(selText,selVal); " + Environment.NewLine);
            js.Append("   selectBox.options[selectBox.options.length]= newOption;" + Environment.NewLine);
            js.Append("   newOption.selected = true; " + Environment.NewLine);
            js.Append(" } // if ! found" + Environment.NewLine);
            js.Append("}" + Environment.NewLine);

            page.HeadSection.AddJSStatements(js.ToString());

            string SWFPathDropDownHtml = getSWFPathDropdown(formName + "SWFPath", flashObject);

            html.Append(SWFPathDropDownHtml);

            string onclick = "window.open(this.href, 'newWin', 'resizable,height=" + PopupFlashObjectBrowser.PopupHeight + ",width=" + PopupFlashObjectBrowser.PopupWidth + "'); return false;";
            html.Append(" <a href=\"" + PopupFlashObjectBrowser.getUrl(JSCallbackFunctionName) + "\" onclick=\"" + onclick + "\">browse for flash file</a>");

            html.Append("</td></tr>");
            
            html.Append("<tr><td>");
            html.Append("Width:");
            html.Append("</td><td>");
            html.Append(PageUtils.getInputTextHtml(formName + "displayWidth", formName + "displayWidth", flashObject.DisplayWidth.ToString(), 7, 5));
            html.Append("<br><em>values < 1 will not display the SWF</em>");
            html.Append("</td></tr>");

            html.Append("<tr><td>");
            html.Append("Height:");
            html.Append("</td><td>");
            html.Append(PageUtils.getInputTextHtml(formName + "displayHeight", formName + "displayHeight", flashObject.DisplayHeight.ToString(), 7, 5));
            html.Append("<br><em>values < 1 will not display the SWF</em>");
            html.Append("</td></tr>");

            html.Append("</table>");                        

            // -- hidden field actions
            html.Append("<input type=\"hidden\" name=\"" + formName + "_FlashObjectAction\" value=\"saveflashobject\">");

            writer.WriteLine(html.ToString());

        }

        private string getSWFPathDropdown(string DropDownFormName, FlashObjectData flashObject)
        {
            string UserFilesDir = System.Web.HttpContext.Current.Server.MapPath(InlineImageBrowser2.UserFilesPath + "Flash/");
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(UserFilesDir);
            NameValueCollection options = new NameValueCollection();
            options.Add("", "(no flash file selected)");
            AddSWFFilesToListRecursive(options, di);
            return PageUtils.getDropDownHtml(DropDownFormName, DropDownFormName, options, flashObject.SWFPath);

        }


        private void AddSWFFilesToListRecursive(NameValueCollection ret, System.IO.DirectoryInfo di)
        {
            if (!CmsContext.currentUserIsSuperAdmin && di.Name.StartsWith("_"))
                return ;	


            if (PopupFlashObjectBrowser.DirHasSWFFiles(di))
            {
                foreach (System.IO.FileInfo fi in PopupFlashObjectBrowser.GetFlashFiles(di))
                {
                    if ((fi.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                        (fi.Attributes & FileAttributes.System) != FileAttributes.System)
                    {
                        string fileUrl = InlineImageBrowser2.ReverseMapPath(fi.FullName);
                        // the key is the dropdown's value, options[key] is the display text
                        string displayText = fileUrl;
                        if (displayText.StartsWith("UserFiles/"))
                            displayText = displayText.Substring("UserFiles/".Length);                        

                        ret.Add(fileUrl, displayText);
                    } // if
                } // foreach
            } // if

            System.IO.DirectoryInfo[] dirs = di.GetDirectories();
            foreach (System.IO.DirectoryInfo d in dirs)
            {
                AddSWFFilesToListRecursive(ret, d);
            }

        } // AddImageFilesToListRecursive

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            Rss.RssItem rssItem = base.CreateAndInitRssItem(page, langToRenderFor);
            rssItem.Description = page.renderPlaceholderToString(placeholderDefinition, langToRenderFor);

            return new Rss.RssItem[] { rssItem };
        }  


	}
}
