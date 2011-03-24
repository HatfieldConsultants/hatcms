using System;
using System.Web.UI;
using System.Collections.Generic;
using Hatfield.Web.Portal;
using System.Text;
using HatCMS.Placeholders.NewsDatabase;

namespace HatCMS.Controls._system
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
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE `NewsArticleAggregator` (
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `LangCode` varchar(2) NOT NULL,
                  `DefaultYearToDisplay` int(11) NOT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE `NewsArticleDetails` (
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `LangCode` varchar(2) NOT NULL,
                  `DateOfNews` datetime DEFAULT NULL,
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`PageId`,`Identifier`,`LangCode`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));

            // -- REQUIRED config entries
            ret.Add(new CmsConfigItemDependency("MostRecentNews.Count"));
            ret.Add(new CmsConfigItemDependency("MostRecentNews.Image"));
            ret.Add(new CmsConfigItemDependency("MostRecentNews.TitleText"));
            ret.Add(new CmsConfigItemDependency("MostRecentNews.NoNewsText"));            

            ret.Add(CmsFileDependency.UnderAppPath("images/_system/calendar/arrowRight.jpg", new DateTime(2011, 3, 1)));

            return ret.ToArray();
        }

        /// <summary>
        /// Get the multi-language text from config file: MostRecentNews.TitleText
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected string getTitleText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("MostRecentNews.TitleText", "", lang);
        }

        /// <summary>
        /// Get the multi-language text from config file: MostRecentNews.NoNewsText
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected string getNoNewsText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("MostRecentNews.NoNewsText", "(No news articles are available)", lang);
        }

        /// <summary>
        /// Get the multi-language text from config file: NewsArticle.ReadArticleText
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected string getReadArticleText(CmsLanguage lang)
        {
            return CmsConfig.getConfigValue("NewsArticle.ReadArticleText", "read article", lang);
        }

        /// <summary>
        /// Format the header
        /// </summary>
        /// <returns></returns>
        protected string renderHeader(CmsLanguage lang)
        {
            string imgUrl = CmsConfig.getConfigValue("MostRecentNews.Image", "");
            StringBuilder html = new StringBuilder("<table width=\"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"3\"><tr>");
            html.Append("<td width=\"50%\" valign=\"middle\">");
            html.Append((imgUrl == "") ? "" : "<img src=\"" + imgUrl + "\" />");
            html.Append("</td>");
            html.Append("<td width=\"50%\" valign=\"middle\">");

            string titleText = getTitleText(lang);
            if (titleText.Trim() != "")
            {
                html.Append("<div class=\"MostRecentNewsTitle\">");
                html.Append(getTitleText(lang));
                html.Append("</div>");
            }
            html.Append("</td>");
            html.Append("</tr></table>");
            return html.ToString();
        }

        /// <summary>
        /// Format the main content
        /// </summary>
        /// <param name="articleArray"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected string renderContent(List<NewsArticleDb.NewsArticleDetailsData> articleArray, CmsLanguage lang, string template, int maxLengthOfSummary, string dateOfNewsToStringFormat)
        {
            StringBuilder html = new StringBuilder();
            int count = CmsConfig.getConfigValue("MostRecentNews.Count", 1);
            if (articleArray.Count == 0)
            {
                return getNoNewsText(lang);
            }

            
            foreach (NewsArticleDb.NewsArticleDetailsData article in articleArray)
            {
                CmsPage newsArticleDetailsPage = CmsContext.getPageById(article.DetailsPageId);
                if (newsArticleDetailsPage.ID < 0)
                    continue;

                string url = newsArticleDetailsPage.getUrl(lang);
                string articleContent = newsArticleDetailsPage.renderPlaceholdersToString("HtmlContent", lang, CmsPage.RenderPlaceholderFilterAction.ReturnUnfiltered);
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

                string dateOfNews = article.DateOfNews.ToString(dateOfNewsToStringFormat);

                string output = String.Format(template, new string[] { url, summaryOutput, newsArticleDetailsPage.Title, dateOfNews });
                html.Append(output);

                string readArticleText = getReadArticleText(lang);
                if (readArticleText.Trim() != "")
                {
                    string extraAnchor = "<div style=\"text-align: right; padding-right: 3px; margin-top: -10px; margin-bottom: 5px;\"><a href=\"{0}\" class=\"readNewsArticle\">{1}</a></div>";
                    html.Append(string.Format(extraAnchor, new string[] { url, readArticleText }));
                }
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
            /// [newsIndex] index to display (integer)
            /// [template]: Output Template
            ///     template parameters: url, summaryOutput, newsArticleDetailsPage.Title, dateOfNews
            ///     {0} = url
            ///     {1} = snippet
            ///     {2} = title    
            ///     {3} = date
            /// [dateFormat]: dateOutputFormat
            /// [summaryLength]: maxLengthOfSummary (integer)

            int indexToDisplay = 0;
            string template = "{3}: <a href=\"{0}\">{2}</a><br>{1}";
            string dateOutputFormat = "MMMM d, yyyy";
            int maxLengthOfSummary = 115;

            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                indexToDisplay = CmsControlUtils.getControlParameterKeyValue(this, "newsIndex", indexToDisplay);
                template = CmsControlUtils.getControlParameterKeyValue(this, "template", template);
                dateOutputFormat = CmsControlUtils.getControlParameterKeyValue(this, "dateFormat", dateOutputFormat);
                maxLengthOfSummary = CmsControlUtils.getControlParameterKeyValue(this, "summaryLength", maxLengthOfSummary);
                dateOutputFormat = CmsControlUtils.getControlParameterKeyValue(this, "dateOutputFormat", dateOutputFormat);
            }
            else
            {
                throw new ArgumentException("Invalid CmsTemplateEngineVersion");
            }


            CmsLanguage lang = CmsContext.currentLanguage;
            int count = CmsConfig.getConfigValue("MostRecentNews.Count", 1);
            List<NewsArticleDb.NewsArticleDetailsData> articleArray = new NewsArticleDb().fetchNewsDetailsByCount(lang, indexToDisplay, count);

            StringBuilder html = new StringBuilder();
            html.Append(renderHeader(lang));
            html.Append(renderContent(articleArray, lang, template, maxLengthOfSummary, dateOutputFormat));

            writer.Write(html.ToString());
        } // Render
    }
}