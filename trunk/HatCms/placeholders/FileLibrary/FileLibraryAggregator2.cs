using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    public class FileLibraryAggregator2: FileLibraryAggregator
    {
        private string EOL = Environment.NewLine;

        public class RenderParameters
        {
            public static RenderParameters fromParamList(string[] paramList)
            {
                return new RenderParameters(paramList);
            }

            public string ListingTitle = "Files:";

            // public enum FileLinkMode { LinkToPage, LinkToFile }
            // public FileLinkMode fileLinkMode = FileLinkMode.LinkToPage;

            /// <summary>
            /// Set to Int32.MinValue for the current page.
            /// </summary>
            public int PageIdToGatherFilesFrom = Int32.MinValue;

            /// <summary>
            /// if set to false, only gathers files from child pages. If true, gathers from all 
            /// </summary>
            public bool RecursiveGatherFiles = false;

            // public bool ShowByCategory = false;


            public RenderParameters(string[] paramList)
            {
                if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v1)
                {
                    throw new Exception("Error: FileLibraryAggregator does not work with TemplateEngine.v1");
                }
                else if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
                {
                    /*
                    string sLinkMode = "";
                    try
                    {
                        sLinkMode = PlaceholderUtils.getParameterValue("filelinks", Enum.GetName(typeof(FileLinkMode), displayMode), paramList);
                        fileLinkMode = (FileLinkMode)Enum.Parse(typeof(FileLinkMode), sLinkMode, true);
                    }
                    catch
                    {
                        throw new Exception("Error: invalid FileLibraryAggregator filelinks parameter. Valid values: " + String.Join(", ", Enum.GetNames(typeof(FileLinkMode))));
                    }
                    */
                    PageIdToGatherFilesFrom = PlaceholderUtils.getParameterValue("gatherfrompageid", Int32.MinValue, paramList);
                    RecursiveGatherFiles = PlaceholderUtils.getParameterValue("gatherrecusive", RecursiveGatherFiles, paramList);
                    ListingTitle = PlaceholderUtils.getParameterValue("listingtitle", ListingTitle, paramList);
                    // ShowByCategory = PlaceholderUtils.getParameterValue("ShowByCategory", false, paramList);
                }
            }
            
        } // RenderParameters  

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            ret.Add(CmsFileDependency.UnderAppPath("js/_system/FileLibrary/FileLibraryCategory.js"));

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `filelibraryaggregator2` (
                  `SimpleFileAggregatorId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `LangCode` varchar(5) NOT NULL,
                  `LinkedPageId` int(11) NOT NULL,
                  `LinkedIdentifier` int(11) NOT NULL,
                  `LinkedLangCode` varchar(5) NOT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`SimpleFileAggregatorId`),
                  KEY `simplefileaggregatorPageIndex` (`PageId`,`Identifier`,`LangCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));

            ret.AddRange(base.getDependencies());
            
            return ret.ToArray();
        }

        public override BaseCmsPlaceholder.RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented;
        }

        /// <summary>
        /// get the list of pages to show. This list includes auto-linked pages, and manually linked-in files.
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="renderParams"></param>
        /// <returns></returns>
        private CmsPage[] FetchAllPagesToShow(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang, RenderParameters renderParams)
        {
            CmsPage rootPageToGatherFrom = aggregatorPage;
            if (renderParams.PageIdToGatherFilesFrom >= 0)
                rootPageToGatherFrom = CmsContext.getPageById(renderParams.PageIdToGatherFilesFrom);

            CmsContext.PageGatheringMode gatherMode = CmsContext.PageGatheringMode.ChildPagesOnly;
            if (renderParams.RecursiveGatherFiles)
                gatherMode = CmsContext.PageGatheringMode.FullRecursion;

            CmsPage[] autoLinkedPages = CmsContext.getAllPagesWithPlaceholder("FileLibraryDetails", rootPageToGatherFrom, gatherMode);

            List<CmsPage> ret = new List<CmsPage>(autoLinkedPages);
            // -- get the pages that are manually linked to this aggregator
            int[] linkedPageIds = new filelibraryaggregator2Db().FetchPageIdsAssociatedWithPage(aggregatorPage, aggIdentifier, aggLang);
            foreach (int linkedPageId in linkedPageIds)
            {
                if (!CmsPage.ArrayContainsPageId(ret.ToArray(), linkedPageId))
                    ret.Add(CmsContext.getPageById(linkedPageId));
            } // foreach

            return ret.ToArray();
        }

        private string renderAssociateExistingForm(CmsPage page, string controlId, CmsPage[] pagesAlreadyShown)
        {
            // -- find files to show.
            CmsPage[] allFilePages = CmsContext.getAllPagesWithPlaceholder("FileLibraryDetails", CmsContext.HomePage, CmsContext.PageGatheringMode.FullRecursion);
            NameValueCollection dropDownOptions = new NameValueCollection();
            dropDownOptions.Add("-1","-- select an existing file -- ");
            foreach (CmsPage p in allFilePages)
            {
                if (!CmsPage.ArrayContainsPage(pagesAlreadyShown, p))
                {
                     // this page is not already shown, so add it to the drop down
                    dropDownOptions.Add(p.ID.ToString(), p.Title);
                }                 
            }

            if (dropDownOptions.Keys.Count <= 1)
                return ""; // do not render anything if there aren't any files already shown.
                        

            // Traditional upload
            StringBuilder html = new StringBuilder();
            html.Append("<p>" + EOL);
            string formId = controlId + "associateFileForm";
            html.Append(page.getFormStartHtml(formId) + EOL);
            html.Append("<div>Add an existing file to this display:</div>" + EOL);
            html.Append("<p>" + EOL);
            html.Append(PageUtils.getDropDownHtml(controlId + "AssociatePageId", controlId + "AssociatePageId", dropDownOptions, ""));
            html.Append("</p>" + EOL);

            html.Append("<p>" + EOL);
            html.Append(PageUtils.getHiddenInputHtml(controlId + "action", "associateFile") + EOL);
            html.Append("</p>" + EOL);
            html.Append("<input type=\"submit\" name=\"" + controlId + "FileUpload\" value=\"Add to listing\" />" + EOL);
            html.Append(page.getFormCloseHtml(formId) + EOL);
            html.Append("</p>" + EOL);
            return html.ToString();

        }

        private string handleAssociateExistingSubmit(CmsPage aggPage, int aggIdentifier, CmsLanguage aggLang, string controlId, List<CmsPage> pagesToShow)
        {
            if (PageUtils.getFromForm(controlId + "action", "") != "associateFile")
                return "";

            int pageIdToAssociate = PageUtils.getFromForm(controlId + "AssociatePageId", -1);
            CmsPage pageToAssociate = CmsContext.getPageById(pageIdToAssociate);
            if (pageToAssociate.ID >= 0)
            {
                CmsPlaceholderDefinition[] phDefs = pageToAssociate.getPlaceholderDefinitions("FileLibraryDetails");
                foreach (CmsPlaceholderDefinition phDef in phDefs)
                {
                    bool b = new filelibraryaggregator2Db().AssociateDetailsPageWithAggregator(aggPage, aggIdentifier, aggLang, pageToAssociate, phDef.Identifier, aggLang);
                    if (!b)
                        return formatErrorMsg("Error: could not add '" + pageToAssociate.Title + "' to this listing: there was a database error");
                } // foreach
                pagesToShow.Add(pageToAssociate);
            }
            return "";
        }

        /// <summary>
        /// Render the placeholder in ViewMode
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="langToRenderFor"></param>
        /// <param name="paramList"></param>
        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            base.categoryList = db.fetchCategoryList(langToRenderFor);
            
            string controlId = "simplefileaggregator_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode;
            RenderParameters renderParams = new RenderParameters(paramList);

            List<CmsPage> pagesToShow = new List<CmsPage>( FetchAllPagesToShow(page, identifier, langToRenderFor, renderParams));

            StringBuilder html = new StringBuilder();
            bool canWrite = page.currentUserCanWrite;
            if (canWrite)
            {
                html.Append("<p>" + handleAssociateExistingSubmit(page, identifier, langToRenderFor, controlId, pagesToShow) + "</p>" + EOL);
                html.Append("<p>" + base.handleUploadSubmit(page, identifier, langToRenderFor, controlId) + "</p>" + EOL);
            }


            if (pagesToShow.Count > 0)
            {
                CmsPage[] sortedPagesToShow = CmsPage.SortPagesByTitle(pagesToShow.ToArray(), langToRenderFor);
                html.Append("<span class=\"SimpleFileAggregatorHeader\">" + renderParams.ListingTitle + "</span>");
                html.Append("<ul class=\"SimpleFileAggregator\">");
                foreach (CmsPage pageToShow in sortedPagesToShow)
                {
                    html.Append("<li><a href=\"" + pageToShow.getUrl(langToRenderFor) + "\">" + pageToShow.getTitle(langToRenderFor) + "</a></li>");
                }
                html.Append("</ul>");
            }

            if (canWrite)
            {
                html.Append(renderAssociateExistingForm(page, controlId, pagesToShow.ToArray()));
                html.Append(base.renderUploadForm(page, langToRenderFor, controlId));
            }

            // List<FileLibraryDetailsData> filesToDisplay = FileLibraryDetailsData.FetchDetailsDataForPages(pagesToGetDetailsFrom);

            writer.Write(html.ToString());
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            RenderInViewMode(writer, page, identifier, langToRenderFor, paramList);
        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            return new Rss.RssItem[0];
        }


        private class filelibraryaggregator2Db : PlaceholderDb
        {
            public int[] FetchPageIdsAssociatedWithPage(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang)
            {
                List<int> ret = new List<int>();

                string sql = "select distinct LinkedPageId from filelibraryaggregator2 ";
                sql += " where PageId = " + aggregatorPage.ID + " ";
                sql += " AND Identifier = " + aggIdentifier.ToString() + " ";
                sql += " AND LangCode = '" + dbEncode(aggLang.shortCode) + "'; ";

                DataSet ds = RunSelectQuery(sql);
                if (hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int LinkedPageId = Convert.ToInt32(dr["LinkedPageId"]);
                        ret.Add(LinkedPageId);
                    } // foreach
                }

                return ret.ToArray();
            }

            public bool AssociateDetailsPageWithAggregator(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang, CmsPage targetPage, int targetIdentifier, CmsLanguage targetLang)
            {
                string sql = "INSERT INTO filelibraryaggregator2 ";
                sql += "(PageId, Identifier, LangCode, LinkedPageId, LinkedIdentifier, LinkedLangCode)";
                sql += " VALUES ( ";
                sql += aggregatorPage.ID + ", ";
                sql += aggIdentifier + ", ";
                sql += "'" + dbEncode(aggLang.shortCode) + "'" + ", ";
                sql += targetPage.ID.ToString() + ", ";
                sql += targetIdentifier.ToString() + ", ";
                sql += "'" + dbEncode(targetLang.shortCode) + "'" + " ";
                sql += " ); ";

                int newInserted = this.RunUpdateQuery(sql);
                if (newInserted == 1)
                {                    
                    return true;
                }
                return false;
            }
        }

    } // SimpleFileAggregator placeholder class
}
