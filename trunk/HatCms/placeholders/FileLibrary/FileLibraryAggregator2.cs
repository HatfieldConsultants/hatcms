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

            public enum FileLinkMode { LinkToPage, LinkToFile }
            public FileLinkMode fileLinkMode = FileLinkMode.LinkToPage;

            /// <summary>
            /// Set to Int32.MinValue for the current page.
            /// </summary>
            public int PageIdToGatherFilesFrom = Int32.MinValue;

            /// <summary>
            /// if set to false, only gathers files from child pages. If true, gathers from all 
            /// </summary>
            public bool RecursiveGatherFiles = false;

            public string LinkTarget = "_blank";

            public bool ShowByCategory = false;

            /// <summary>
            /// If true, PageFile placeholders are aggregated as well.
            /// </summary>
            public bool IncludePageFiles = false;

            public RenderParameters(string[] paramList)
            {
                if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
                {
                    
                    string sLinkMode = "";
                    try
                    {
                        sLinkMode = PlaceholderUtils.getParameterValue("filelinks", Enum.GetName(typeof(FileLinkMode), fileLinkMode), paramList);
                        fileLinkMode = (FileLinkMode)Enum.Parse(typeof(FileLinkMode), sLinkMode, true);
                        // set the default link target based on the link mode. This can be overridden by the linktarget parameter.
                        switch (fileLinkMode)
                        {                            
                            case FileLinkMode.LinkToFile: LinkTarget = "_blank"; break;
                            case FileLinkMode.LinkToPage: LinkTarget = "_self"; break;                                
                        }
                    }
                    catch
                    {
                        throw new Exception("Error: invalid FileLibraryAggregator2 filelinks parameter. Valid values: " + String.Join(", ", Enum.GetNames(typeof(FileLinkMode))));
                    }
                    
                    PageIdToGatherFilesFrom = PlaceholderUtils.getParameterValue("gatherfrompageid", Int32.MinValue, paramList);
                    RecursiveGatherFiles = PlaceholderUtils.getParameterValue("gatherrecusive", RecursiveGatherFiles, paramList);
                    ListingTitle = PlaceholderUtils.getParameterValue("listingtitle", ListingTitle, paramList);
                    LinkTarget = PlaceholderUtils.getParameterValue("linktarget", LinkTarget, paramList);
                    ShowByCategory = PlaceholderUtils.getParameterValue("showbycategory", ShowByCategory, paramList);
                    IncludePageFiles = PlaceholderUtils.getParameterValue("includepagefiles", IncludePageFiles, paramList);
                }
                else                
                    throw new ArgumentException("Invalid CmsTemplateEngineVersion");
                
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

        private class FileAggItem
        {
            public string PageDisplayURL;
            public string FileDownloadURL;
            public string Title;
            public string Description;
            public string CategoryName;

            public FileAggItem(string pageDisplayURL, string fileDownloadURL, string title, string description, string categoryName)
            {
                PageDisplayURL = pageDisplayURL;
                FileDownloadURL = fileDownloadURL;
                Title = title;
                Description = description;
                CategoryName = categoryName;
            } // constructor

            public string GetContentHash()
            {
                StringBuilder ret = new StringBuilder();
                ret.Append(PageDisplayURL);
                ret.Append(FileDownloadURL);
                ret.Append(Title);
                ret.Append(Description);
                ret.Append(CategoryName);
                return ret.ToString();
            }

            public string getHtmlLink(RenderParameters renderParams, bool canWrite)
            {
                StringBuilder html = new StringBuilder();
                if (renderParams.fileLinkMode == RenderParameters.FileLinkMode.LinkToPage || canWrite)
                {
                    html.Append("<a target=\"" + renderParams.LinkTarget + "\" href=\"" + this.PageDisplayURL + "\">" + this.Title + "</a>");
                }
                else if (renderParams.fileLinkMode == RenderParameters.FileLinkMode.LinkToFile)
                {
                    html.Append("<a target=\"" + renderParams.LinkTarget + "\" href=\"" + this.FileDownloadURL + "\">" + this.Title + "</a>");
                }
                return html.ToString();
            }

            public static bool ArrayContainsFile(FileAggItem[] haystack, FileAggItem fileToFind)
            {
                Dictionary<string, FileAggItem> hash = new Dictionary<string, FileAggItem>();
                foreach (FileAggItem f in haystack)
                {
                    hash.Add(f.GetContentHash(), f);
                }

                return hash.ContainsKey(fileToFind.GetContentHash());
            } 

            public static string[] GetAllCategoryNames(FileAggItem[] files)
            {
                List<string> ret = new List<string>();
                foreach (FileAggItem file in files)
                {
                    if (ret.IndexOf(file.CategoryName) < 0)
                        ret.Add(file.CategoryName);
                }
                ret.Sort();
                return ret.ToArray();
            }

            public static FileAggItem[] GetAllInCategory(FileAggItem[] haystack, string categoryNameToMatch)
            {
                List<FileAggItem> ret = new List<FileAggItem>();
                foreach (FileAggItem file in haystack)
                {
                    if (string.Compare(file.CategoryName, categoryNameToMatch) == 0)
                        ret.Add(file);
                } // foreach
                return ret.ToArray();
            }

            public static FileAggItem FromFileLibraryDetailsData(FileLibraryDetailsData sourceDetails, List<FileLibraryCategoryData> categoryList)
            {
                CmsPage detailsPage = CmsContext.getPageById(sourceDetails.DetailsPageId);
                string PageDisplayURL = detailsPage.getUrl(sourceDetails.Lang);
                string FileDownloadURL = FileLibraryDetailsData.getDownloadUrl(detailsPage, sourceDetails.Identifier, sourceDetails.Lang, sourceDetails.FileName);
                string Title = detailsPage.getTitle(sourceDetails.Lang);
                string Description = sourceDetails.Description;
                string CategoryName = FileLibraryCategoryData.getCategoryFromList(categoryList, sourceDetails.CategoryId).CategoryName;
                return new FileAggItem(PageDisplayURL, FileDownloadURL, Title, Description, CategoryName);
            }

            public static FileAggItem[] FromFileLibraryDetailsData(FileLibraryDetailsData[] sourceDetails, List<FileLibraryCategoryData> categoryList)
            {
                List<FileAggItem> ret = new List<FileAggItem>();
                foreach (FileLibraryDetailsData file in sourceDetails)
                {
                    ret.Add(FromFileLibraryDetailsData(file, categoryList));
                }
                return ret.ToArray();
            }

            public static FileAggItem FromPageFilesItemData(PageFilesItemData sourceDetails)
            {
                CmsPage detailsPage = CmsContext.getPageById(sourceDetails.DetailsPageId);
                Dictionary<string, string> pageUrlParams = new Dictionary<string, string>();
                pageUrlParams.Add(PageFiles.CurrentFileIdFormName, sourceDetails.Id.ToString());
                string PageDisplayURL = detailsPage.getUrl(pageUrlParams, sourceDetails.Lang);
                string FileDownloadURL = sourceDetails.getDownloadUrl();
                string Title = sourceDetails.Title;
                string Description = sourceDetails.Abstract;
                string CategoryName = detailsPage.getTitle(sourceDetails.Lang); // use the page title as the category
                return new FileAggItem(PageDisplayURL, FileDownloadURL, Title, Description, CategoryName);
            }

            public static FileAggItem[] FromPageFilesItemData(PageFilesItemData[] sourceDetails)
            {
                List<FileAggItem> ret = new List<FileAggItem>();
                foreach (PageFilesItemData file in sourceDetails)
                {
                    ret.Add(FromPageFilesItemData(file));
                }
                return ret.ToArray();
            }

            public static FileAggItem[] RemoveDuplicates(List<FileAggItem> list)
            {
                Dictionary<string, FileAggItem> ret = new Dictionary<string, FileAggItem>();
                foreach (FileAggItem item in list)
                {
                    string key = item.GetContentHash();
                    if (!ret.ContainsKey(key))
                        ret.Add(key, item);
                } // foreach

                return new List<FileAggItem>(ret.Values).ToArray();

            } // RemoveDuplicates

            public static FileAggItem[] SortFilesByTitle(FileAggItem[] toSort)
            {
                List<FileAggItem> ret = new List<FileAggItem>(toSort);
                ret.Sort(CompareFilesByTitle);
                return ret.ToArray();
            }

            private static int CompareFilesByTitle(FileAggItem x, FileAggItem y)
            {
                return string.Compare(x.Title, y.Title);
            }
        }

        private FileAggItem[] FetchAutoAggregatedFileLibraryDetails(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang, RenderParameters renderParams)
        {
            CmsPage rootPageToGatherFrom = aggregatorPage;
            if (renderParams.PageIdToGatherFilesFrom >= 0)
                rootPageToGatherFrom = CmsContext.getPageById(renderParams.PageIdToGatherFilesFrom);

            CmsContext.PageGatheringMode gatherMode = CmsContext.PageGatheringMode.ChildPagesOnly;
            if (renderParams.RecursiveGatherFiles)
                gatherMode = CmsContext.PageGatheringMode.FullRecursion;

            List<string> phTypesToSearchFor = new List<string>();
            phTypesToSearchFor.Add("FileLibraryDetails");
            CmsPage[] fileDetailsPages = CmsContext.getAllPagesWithPlaceholder("FileLibraryDetails", rootPageToGatherFrom, gatherMode);
            List<FileLibraryDetailsData> filesToShow = db.fetchDetailsData(fileDetailsPages, aggLang);

            return FileAggItem.FromFileLibraryDetailsData(filesToShow.ToArray(), base.categoryList);
        }

        private FileAggItem[] FetchManuallyLinkedFileLibraryDetails(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang, RenderParameters renderParams)
        {
            List<CmsPage> pages = new List<CmsPage>();
            int[] linkedPageIds = new filelibraryaggregator2Db().FetchPageIdsAssociatedWithPage(aggregatorPage, aggIdentifier, aggLang);
            foreach (int linkedPageId in linkedPageIds)
            {
                if (!CmsPage.ArrayContainsPageId(pages.ToArray(), linkedPageId))
                    pages.Add(CmsContext.getPageById(linkedPageId));
            } // foreach

            List<FileLibraryDetailsData> filesToShow = db.fetchDetailsData(pages.ToArray(), aggLang);

            return FileAggItem.FromFileLibraryDetailsData(filesToShow.ToArray(), base.categoryList);
        }

        private FileAggItem[] FetchAutoAggregatedPageFiles(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang, RenderParameters renderParams)
        {
            CmsPage rootPageToGatherFrom = aggregatorPage;
            if (renderParams.PageIdToGatherFilesFrom >= 0)
                rootPageToGatherFrom = CmsContext.getPageById(renderParams.PageIdToGatherFilesFrom);

            CmsContext.PageGatheringMode gatherMode = CmsContext.PageGatheringMode.ChildPagesOnly;
            if (renderParams.RecursiveGatherFiles)
                gatherMode = CmsContext.PageGatheringMode.FullRecursion;

            List<string> phTypesToSearchFor = new List<string>();
            phTypesToSearchFor.Add("FileLibraryDetails");
            CmsPage[] pageFilePages = CmsContext.getAllPagesWithPlaceholder("PageFiles", rootPageToGatherFrom, gatherMode);
            PageFilesDb pageFilesDb = new PageFilesDb();
            PageFilesItemData[] fileItems = pageFilesDb.getPageFilesItemDatas(pageFilePages, aggLang);

            return FileAggItem.FromPageFilesItemData(fileItems);
        }

        /// <summary>
        /// get the list of FileAggItems to show. The returned array is not sorted, and can have multiple copies of the same file.
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="renderParams"></param>
        /// <returns></returns>
        private FileAggItem[] FetchAllFilesToShow(CmsPage aggregatorPage, int aggIdentifier, CmsLanguage aggLang, RenderParameters renderParams)
        {
            List<FileAggItem> ret = new List<FileAggItem>();
            // -- auto aggregated FileLibraryDetail
            ret.AddRange(FetchAutoAggregatedFileLibraryDetails(aggregatorPage, aggIdentifier, aggLang, renderParams));
            // -- manually linked FileLibraryDetail
            ret.AddRange(FetchManuallyLinkedFileLibraryDetails(aggregatorPage, aggIdentifier, aggLang, renderParams));
            // -- auto aggregated PageFiles
            if (renderParams.IncludePageFiles)
            {
                ret.AddRange(FetchAutoAggregatedPageFiles(aggregatorPage, aggIdentifier, aggLang, renderParams));
            }
            
            return ret.ToArray();
        } // FetchAllFilesToShow

        private string renderAssociateExistingForm(CmsPage page, string controlId, CmsLanguage lang, FileAggItem[] filesAlreadyShown)
        {
            // -- find files to show.
            CmsPage[] allFilePages = CmsContext.getAllPagesWithPlaceholder("FileLibraryDetails", CmsContext.HomePage, CmsContext.PageGatheringMode.FullRecursion);
            NameValueCollection dropDownOptions = new NameValueCollection();
            dropDownOptions.Add("-1","-- select an existing file -- ");

            List<FileLibraryDetailsData> allFileDetailsData = db.fetchDetailsData(allFilePages, lang);

            foreach (FileLibraryDetailsData file in allFileDetailsData)
            {
                if (!FileAggItem.ArrayContainsFile(filesAlreadyShown, FileAggItem.FromFileLibraryDetailsData(file, base.categoryList)))
                {
                    CmsPage p = CmsContext.getPageById(file.DetailsPageId);
                    // this page is not already shown, so add it to the drop down
                    dropDownOptions.Add(file.DetailsPageId.ToString(), p.getTitle(lang));
                }
            } // foreach file

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

        private string handleAssociateExistingSubmit(CmsPage aggPage, int aggIdentifier, CmsLanguage aggLang, string controlId, List<FileAggItem> filesToShow)
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

                List<FileLibraryDetailsData> arr = db.fetchDetailsData(pageToAssociate);

                filesToShow.AddRange(FileAggItem.FromFileLibraryDetailsData(arr.ToArray(), base.categoryList));
                
            }
            return "";
        }

        private bool renderFromEditMode = false;

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

            List<FileAggItem> filesToShow = new List<FileAggItem>(FetchAllFilesToShow(page, identifier, langToRenderFor, renderParams));

            StringBuilder html = new StringBuilder();
            bool canWrite = page.currentUserCanWrite;
            if (renderFromEditMode) // don't render forms if we are in Edit Mode.
                canWrite = false;

            if (canWrite)
            {
                html.Append("<p>" + handleAssociateExistingSubmit(page, identifier, langToRenderFor, controlId, filesToShow) + "</p>" + EOL);
                html.Append("<p>" + base.handleUploadSubmit(page, identifier, langToRenderFor, controlId) + "</p>" + EOL);
            }

            FileAggItem[] uniqueFilesToShow = FileAggItem.RemoveDuplicates(filesToShow);

            if (uniqueFilesToShow.Length > 0)
            {                
                html.Append("<div class=\"SimpleFileAggregatorHeader\">" + renderParams.ListingTitle + "</div>");
                
                if (renderParams.ShowByCategory)
                {
                    string[] categories = FileAggItem.GetAllCategoryNames(uniqueFilesToShow);

                    foreach (string fileCat in categories)
                    {
                        FileAggItem[] filesInCat = FileAggItem.GetAllInCategory(uniqueFilesToShow, fileCat);                        
                        if (filesInCat.Length > 0)
                        {
                            html.Append("<div class=\"SimpleFileAggregatorCategoryHeader\">" + fileCat + "</div>");
                            html.Append("<ul class=\"SimpleFileAggregator\">");
                            filesInCat = FileAggItem.SortFilesByTitle(filesInCat);
                            foreach (FileAggItem fileToShow in filesInCat)
                            {
                                string htmlLink = fileToShow.getHtmlLink(renderParams, canWrite);
                                html.Append("<li>" + htmlLink + "</li>");
                                
                            } // foreach
                            html.Append("</ul>");
                        }
                    } // foreach category
                }
                else
                {
                    // -- non-categorized display
                    FileAggItem[] sortedFilesToShow = FileAggItem.SortFilesByTitle(uniqueFilesToShow);
                    html.Append("<ul class=\"SimpleFileAggregator\">");
                    foreach (FileAggItem fileToShow in sortedFilesToShow)
                    {
                        string htmlLink = fileToShow.getHtmlLink(renderParams, canWrite);
                        html.Append("<li>" + htmlLink + "</li>");
                    } // foreach
                    html.Append("</ul>");
                } // if not show by category
                
            }

            if (canWrite)
            {
                html.Append(renderAssociateExistingForm(page, controlId, langToRenderFor, uniqueFilesToShow));
                html.Append(base.renderUploadForm(page, langToRenderFor, controlId));
            }

            // List<FileLibraryDetailsData> filesToDisplay = FileLibraryDetailsData.FetchDetailsDataForPages(pagesToGetDetailsFrom);

            writer.Write(html.ToString());
        }

        public new void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            renderFromEditMode = true;
            RenderInViewMode(writer, page, identifier, langToRenderFor, paramList);
        }

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            RenderParameters renderParams = new RenderParameters(placeholderDefinition.ParamList);

            FileAggItem[] filesToShow = FetchAllFilesToShow(page, placeholderDefinition.Identifier, langToRenderFor, renderParams);                        

            List<Rss.RssItem> ret = new List<Rss.RssItem>();
            foreach (FileAggItem file in filesToShow)
            {
                Rss.RssItem rssItem = CreateAndInitRssItem(page, langToRenderFor);

                // -- link directly to the file url
                rssItem.Link = new Uri(file.FileDownloadURL, UriKind.RelativeOrAbsolute);
                rssItem.Guid = new Rss.RssGuid(rssItem.Link);

                rssItem.Title = file.Title;
                rssItem.Description = file.Description;

                ret.Add(rssItem);

            } // foreach file
            return ret.ToArray();

        } // GetRssFeedItems


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
