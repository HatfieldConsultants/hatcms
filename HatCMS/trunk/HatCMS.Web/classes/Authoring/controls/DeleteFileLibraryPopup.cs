using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using HatCMS.Placeholders;
using Hatfield.Web.Portal;

namespace HatCMS.Controls
{
    public class DeleteFileLibraryPopup : BaseCmsControl
    {

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            return ret.ToArray();
        }

        /// <summary>
        /// Format error msg in red
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected string formatErrorMsg(string msg)
        {
            return "<div style=\"text-align: center; font-weight: bold; color: red;\">" + msg + "</div>" + EOL;
        }

        /// <summary>
        /// Format normal msg in green
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected string formatNormalMsg(string msg)
        {
            return "<div style=\"text-align: center; font-weight: bold; color: green;\">" + msg + "</div>" + EOL;
        }

        /// <summary>
        /// Get the CmsPage by the page ID
        /// </summary>
        /// <returns></returns>
        protected CmsPage getCmsPage()
        {
            int targetPageId = PageUtils.getFromForm("target", Int32.MinValue);
            return CmsContext.getPageById(targetPageId);
        }

        /// <summary>
        /// Validation before delete (pageid, access rights, zone boundary, template name)
        /// </summary>
        /// <returns></returns>
        protected string validate()
        {
            if (!CmsContext.currentUserIsLoggedIn)
                return "Access Denied";

            int targetPageId = PageUtils.getFromForm("target", Int32.MinValue);
            if (targetPageId < 0)
                return "Invalid Target parameter. No page to delete.";

            if (!CmsContext.pageExists(targetPageId))
                return "Invalid Target parameter. No page to delete.";

            CmsPage p = getCmsPage();
            if (p.SecurityZone.canWrite(CmsContext.currentWebPortalUser) == false)
                return "Access Denied";

            if (p.isSecurityZoneBoundary == true)
                return "Delete failed because the page is located at the zone boundary.";

            string template = p.TemplateName;
            if (!p.hasPlaceholder("FileLibraryAggregator") || !p.hasPlaceholder("FileLibraryDetails") || !p.hasPlaceholder("FileLibraryAggregator2"))
                return "Not a FileLibraryAggregator or FileLibraryDetails page.";

            return "";
        }

        /// <summary>
        /// Delete the CmsPage
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected string deleteCmsPage(CmsPage p)
        {
            if (p.DeleteThisPage() == false)
                return "Error deleting file page (Id " + p.Id.ToString() + ").";

            return "";
        }

        /// <summary>
        /// Delete the FileLibraryAggregator (set Deleted timestamp)
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <returns></returns>
        protected string deleteFileLibraryAggregator(CmsPage aggregatorPage)
        {
            FileLibraryDb db = new FileLibraryDb(); 
            if (db.deleteAggregatorData(aggregatorPage) == false)
                return "Error deleting file aggregator details (Id " + aggregatorPage.Id.ToString() + ").";

            return "";
        }

        /// <summary>
        /// Delete the FileLibraryDetails (set Deleted timestamp)
        /// </summary>
        /// <param name="detailsPage"></param>
        /// <returns></returns>
        protected string deleteFileLibraryDetails(CmsPage detailsPage)
        {
            FileLibraryDb db = new FileLibraryDb(); 
            if (db.deleteDetailsData(detailsPage) == false)
                return "Error deleting file details (Id " + detailsPage.Id.ToString() + ").";

            return "";
        }

        /// <summary>
        /// Delete the file from disk (rename the file)
        /// </summary>
        /// <param name="detailsPage"></param>
        /// <returns></returns>
        protected string deleteFileFromDisk(CmsPage detailsPage, CmsLanguage language)
        {
            FileLibraryDb db = new FileLibraryDb(); 
            List<FileLibraryDetailsData> fileList = db.fetchDetailsData(detailsPage);
            if (fileList.Count == 0)
                return "";


            foreach (FileLibraryDetailsData f in fileList)
            {
                try
                {
                    string newFileName = "Deleted." + DateTime.Now.ToString("yyyyMMdd.HH.mm.ss.") + f.FileName;
                    string oldFileNameOnDisk = FileLibraryDetailsData.getTargetNameOnDisk(detailsPage, f.Identifier, language, f.FileName);
                    string newFileNameOnDisk = FileLibraryDetailsData.getTargetNameOnDisk(detailsPage, f.Identifier, language, newFileName);
                    if (File.Exists(oldFileNameOnDisk))
                        File.Move(oldFileNameOnDisk, newFileNameOnDisk);
                }
                catch (Exception ex)
                {
                    return "Error deleting file from disk (" + f.FileName + ": " + ex.Message + ").";
                }
            }
            return "";
        }

        /// <summary>
        /// If the page to be deleted is an aggregator page:
        /// - delete all children
        /// - delete the aggregator data
        /// - delete the aggregator cms page
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected string handleFileLibraryAggregatorDelete(CmsPage p)
        {
            string msg = "";
            CmsPage[] childPages = p.ChildPages;
            for (int x = 0; x < childPages.Length; x++)
            {
                msg = handleFileLibraryDetailsDelete(childPages[x]);
                if (msg != "")
                {
                    string err = "{0}<p>{1} file(s) deleted, {2} file(s) not deleted.</p>";
                    string[] parm = new string[] { msg, x.ToString(), (childPages.Length - x).ToString() };
                    return String.Format(err, parm);
                }
            }

            msg = deleteFileLibraryAggregator(p);
            if (msg != "")
                return msg;

            msg = deleteCmsPage(p);
            if (msg != "")
                return msg;

            return "";
        }

        /// <summary>
        /// If the page to be deleted is a details page:
        /// - delete the file from disk
        /// - delete the details data
        /// - delete the details cms page
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected string handleFileLibraryDetailsDelete(CmsPage p)
        {
            string msg = "";
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                msg = deleteFileFromDisk(p, lang);
                if (msg != "")
                    return msg;
            }

            msg = deleteFileLibraryDetails(p);
            if (msg != "")
                return msg;

            msg = deleteCmsPage(p);
            if (msg != "")
                return msg;

            return "";
        }

        /// <summary>
        /// Close popup button
        /// </summary>
        /// <param name="parentUrl"></param>
        /// <returns></returns>
        protected string renderCloseButton(string parentUrl)
        {
            StringBuilder html = new StringBuilder();

            CmsContext.currentPage.HeadSection.AddJSStatements("function go(url){opener.location.href = url; window.close(); }");


            html.Append(formatNormalMsg("The Page has successfully been deleted."));
            html.Append("<p>" + EOL);
            html.Append("<input type=\"button\" onclick=\"go('" + parentUrl + "');\" value=\"close this window\">" + EOL);
            html.Append("</p>" + EOL);

            return html.ToString();
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<div style=\"text-align: center;\">");

            string msg = validate();
            if (msg != "")
            {
                return (formatErrorMsg(msg));                
            }

            CmsPage p = getCmsPage();
            if (p.hasPlaceholder("FileLibraryAggregator"))
                msg += handleFileLibraryAggregatorDelete(p);

            if (p.hasPlaceholder("FileLibraryDetails"))
                msg += handleFileLibraryDetailsDelete(p);

            if (msg != "")
            {
                return (formatErrorMsg(msg));                
            }

            html.Append(renderCloseButton(p.ParentPage.Url));
            html.Append("</div>");
            return (html.ToString());
        }
        
    }
}
