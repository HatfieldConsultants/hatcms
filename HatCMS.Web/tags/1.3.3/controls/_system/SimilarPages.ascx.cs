using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal.Search.Lucene;
using Hatfield.Web.Portal;

namespace HatCMS.Controls
{
    public partial class SimilarPages : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        private string getPageBodyText(CmsPage page, CmsLanguage language)
        {
            string content = page.renderAllPlaceholdersToString(language, CmsPage.RenderPlaceholderFilterAction.RunAllPageAndPlaceholderFilters);

            content = PageUtils.StripTags(content.ToString());
            return content;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            int maxResultsToShow = 5;
            
            
            StringBuilder html = new StringBuilder();

            CmsPage currentPage = CmsContext.currentPage;
            string title = currentPage.Title;
            string bodyText = getPageBodyText(currentPage, CmsContext.currentLanguage);

            string keywordIndexDir = SearchResults.IndexStorageDirectory;
            string spellingIndexDir = SearchResults.SpellCheckIndexStorageDirectory;

            LuceneKeywordSearch search = new LuceneKeywordSearch(keywordIndexDir, spellingIndexDir);

            IndexableFileInfo[] related = search.getRelatedFiles(title, maxResultsToShow+1); // always returns the current page

            if (related.Length > 1) 
            {
                html.Append("<div class=\"SimilarPages\">");
                html.Append("Related Pages:");
                html.Append("<ul>");
                foreach (IndexableFileInfo f in related)
                {
                    if (String.Compare(currentPage.Path, f.Filename, true) == 0) // skip current page.
                        continue;
                    string url = CmsContext.getUrlByPagePath(f.Filename);
                    html.Append("<li>");
                    html.Append("<a href=\"" + url + "\">");
                    html.Append(f.Title);
                    html.Append("</a>");
                    html.Append("</li>");
                } // foreach
                html.Append("</ul>");
                html.Append("</div>");
            }
            

            writer.Write(html.ToString());
        }
    }
}