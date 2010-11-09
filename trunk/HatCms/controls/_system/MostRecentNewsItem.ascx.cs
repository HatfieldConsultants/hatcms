using System;
using System.Web.UI;
using System.Collections.Generic;
using Hatfield.Web.Portal;
using System.Text;
using HatCMS.placeholders.NewsDatabase;

namespace HatCMS.controls._system
{
    public partial class MostRecentNewsItem : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            // -- database tables
            ret.Add(new CmsDatabaseTableDependency("NewsArticleAggregator"));
            ret.Add(new CmsDatabaseTableDependency("NewsArticleDetails"));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("MostRecentNews.Count"));
            ret.Add(new CmsConfigItemDependency("MostRecentNews.Image"));
            ret.Add(new CmsConfigItemDependency("MostRecentNews.TitleText"));
            ret.Add(new CmsConfigItemDependency("MostRecentNews.NoNewsText"));

            return ret.ToArray();
        }

        /// <summary>
        /// Get the multi-language text from config file: MostRecentNews.TitleText
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected string getTitleText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("MostRecentNews.TitleText", "News", lang);
        }

        /// <summary>
        /// Get the multi-language text from config file: MostRecentNews.NoNewsText
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected string getNoNewsText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("MostRecentNews.NoNewsText", "(No article available)", lang);
        }

        /// <summary>
        /// Format the header
        /// </summary>
        /// <returns></returns>
        protected string renderHeader(CmsLanguage lang)
        {
            StringBuilder html = new StringBuilder("<div class=\"MostRecentNewsTitle\">");
            html.Append(getTitleText(lang));
            html.Append("</div>");

            string imgUrl = CmsConfig.getConfigValue("MostRecentNews.Image", "");
            if (imgUrl == "")
                return html.ToString();
            else
                html.Append("<img src=\"" + imgUrl + "\" />");

            return html.ToString();
        }

        /// <summary>
        /// Format the main content
        /// </summary>
        /// <param name="articleArray"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected string renderContent(List<NewsArticleDb.NewsArticleDetailsData> articleArray, CmsLanguage lang, string template, int maxLengthOfSummary)
        {
            StringBuilder html = new StringBuilder();
            int count = CmsConfig.getConfigValue("MostRecentNews.Count", 1);
            if (articleArray.Count == 0)
            {
                html.Append(String.Format(template, new string[] { "", getNoNewsText(lang) }));
                return html.ToString();
            }

            CmsPageDb pageDb = new CmsPageDb();
            foreach (NewsArticleDb.NewsArticleDetailsData article in articleArray)
            {
                CmsPage newsArticleDetailsPage = pageDb.getPage(article.PageId);
                if (newsArticleDetailsPage.ID < 0)
                    continue;

                string url = newsArticleDetailsPage.getUrl(lang);
                string articleContent = newsArticleDetailsPage.renderPlaceholdersToString("HtmlContent", lang);
                string summary = StringUtils.StripHTMLTags(articleContent).Trim();
                string summaryOutput = summary;
                if (maxLengthOfSummary > 0 && summary.Length > maxLengthOfSummary)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] words = summary.Split(new char[] { ' ' });
                    foreach (string word in words)
                    {
                        if (sb.ToString().Length > maxLengthOfSummary)
                            break;
                        sb.Append(word.Trim() + " ");
                    }
                    summaryOutput = sb.ToString() + "...";
                }

                string output = String.Format(template, new string[] { url, summaryOutput });
                html.Append(output);
            }

            return html.ToString();
        }

        /// <summary>
        /// renders the MostRecentNewsItem control to the HtmlTextWriter
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="page"></param>
        /// <param name="identifier"></param>
        /// <param name="paramList"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            /// Parameters:
            /// [0]: newsIndexToDisplay (integer, 0 based)
            /// [1]: Output Template
            /// [2]: dateOutputFormat
            /// [3]: maxLengthOfSummary (integer)

            int newsIndexToDisplay = 0;
            Dictionary<string, string> urlTemplateParam = new Dictionary<string, string>();
            urlTemplateParam.Add("nid", "{1}");
            string urlTemplate = CmsContext.currentPage.getUrl(urlTemplateParam);
            string template = "{0}<br><a href=\"" + urlTemplate + "\">{2}</a><br>{3}";
            string dateOutputFormat = "MMMM d, yyyy";
            int maxLengthOfSummary = 115;

            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v1)
            {
                throw new Exception("MostRecentNewsItem does not support version 1.");
            }
            else if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                newsIndexToDisplay = CmsControlUtils.getControlParameterKeyValue(this, "newsIndex", newsIndexToDisplay);
                template = CmsControlUtils.getControlParameterKeyValue(this, "template", template);
                dateOutputFormat = CmsControlUtils.getControlParameterKeyValue(this, "dateFormat", dateOutputFormat);
                maxLengthOfSummary = CmsControlUtils.getControlParameterKeyValue(this, "summaryLength", maxLengthOfSummary);
            }

            if (newsIndexToDisplay < 0)
            {
                writer.Write("Template Error: 'newsIndexToDisplay' placeholder parameter can not be less than zero");
                return;
            }

            CmsLanguage lang = CmsContext.currentLanguage;
            int count = CmsConfig.getConfigValue("MostRecentNews.Count", 1);
            List<NewsArticleDb.NewsArticleDetailsData> articleArray = new NewsArticleDb().fetchNewsDetailsByCount(lang, newsIndexToDisplay, count);

            StringBuilder html = new StringBuilder();
            html.Append(renderHeader(lang));
            html.Append(renderContent(articleArray, lang, template, maxLengthOfSummary));

            writer.Write(html.ToString());
        } // Render
    }
}