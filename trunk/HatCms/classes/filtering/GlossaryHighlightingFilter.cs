using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;

namespace HatCMS
{
    /// <summary>
    /// Filter the page's HTML so that glossary terms have a span tag surrounding them.
    /// </summary>
    public class GlossaryHighlightingFilter : BaseCmsOutputFilter
    {
        /// <summary>
        /// Registers the GlossaryHighlightingFilter output filter. 
        /// </summary>
        /// <returns></returns>
        public override CmsOutputFilterInfo getOutputFilterInfo()
        {
            return new CmsOutputFilterInfo(CmsOutputFilterScope.SpecifiedPlaceholderTypes, new string[] { "HtmlContent" }, RunInlineGlossaryFilter);            
        }

        public string RunInlineGlossaryFilter(CmsPage pageBeingFiltered, string placeholderHtml)
        {
            try
            {
                bool enabled = CmsConfig.getConfigValue("GlossaryHighlightFilter:Enable", false); // disabled by default
                
                if (! enabled || CmsContext.currentEditMode == CmsEditMode.Edit)
                    return placeholderHtml;

#if DEBUG
                CmsContext.currentPage.HeadSection.AddCSSStyleStatements("span.InlineGlossaryTerm { border-bottom: 1px dotted red; }");              
#endif

                // -- get the glossaryID to get data for (language specific)
                int glossaryId = 2;
                string glossaryIds = CmsConfig.getConfigValue("GlossaryHighlightFilter:GlossaryId", "");
                try
                {
                    string[] glossaryIdsParts = glossaryIds.Split(new char[] { CmsConfig.PerLanguageConfigSplitter }, StringSplitOptions.RemoveEmptyEntries);
                    if (glossaryIdsParts.Length >= CmsConfig.Languages.Length)
                    {
                        int index = CmsLanguage.IndexOf(CmsContext.currentLanguage.shortCode, CmsConfig.Languages);
                        if (index >= 0)
                            glossaryId = Convert.ToInt32(glossaryIdsParts[index]);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error: GlossaryHighlightFilter is incorrectly configured!");
                }

                // -- get the glossary data from the database. The data is cached so that we don't hit the database for this info every time.
                GlossaryData[] gData;
                string cacheKey = "GlossaryHighlightFilter_Data_"+glossaryId;
                if (!CmsContext.currentUserIsLoggedIn && System.Web.HttpContext.Current.Cache[cacheKey] != null)
                {
                    gData = (GlossaryData[])System.Web.HttpContext.Current.Cache[cacheKey];
                }
                else
                {
                    GlossaryDb db = new GlossaryDb();
                    gData = db.getGlossaryData(glossaryId);
                    if (!CmsContext.currentUserIsLoggedIn)
                        System.Web.HttpContext.Current.Cache.Insert(cacheKey, gData, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration);                    

                    // go through longer words first (longer words/phrases are usually more specific than shorter ones) 
                    gData = GlossaryData.SortByWordLength(gData, SortDirection.Descending);
                }

                // -- short-circuit processing if there aren't any glossary terms in the system.
                if (gData.Length == 0)
                    return placeholderHtml;

                // -- process the placeholderHTML
                string html = placeholderHtml;

                List<string> toSurround = new List<string>();
                List<string> prefixs = new List<string>();
                List<string> suffixs = new List<string>();

                foreach (GlossaryData d in gData)
                {
                    int index = html.IndexOf(d.word.Trim(), StringComparison.CurrentCultureIgnoreCase);                    
                    if (index >= 0 && d.word.Trim().Length > 0)
                    {                                                
                        // string safeDesc = StringUtils.AddSlashes(d.description);
                        string safeDesc = HttpUtility.HtmlEncode(d.word+": "+d.description);
                        safeDesc = safeDesc.Replace("\n", " ");
                        safeDesc = safeDesc.Replace("\r", " ");
                        safeDesc = safeDesc.Replace("\t", " ");
                        safeDesc = safeDesc.Replace("  ", " ");

                        string prefix = "<span title=\"" + safeDesc + "\" class=\"InlineGlossaryTerm\">";
                        string suffix = "</span>";

                        toSurround.Add(d.word.Trim());
                        prefixs.Add(prefix);
                        suffixs.Add(suffix);                        
                    }
                } // foreach word

                html = SurroundInHtml(toSurround.ToArray(), prefixs.ToArray(), suffixs.ToArray(), html);
                
                return html.ToString();
            }
            catch (Exception ex)
            {
                placeholderHtml += ("<!-- GlossaryHighlightingFilter Error: " + ex.Message + " -->");
            }

            return placeholderHtml;

        }

        private string SurroundInHtml(string[] wordsToSearchFor, string[] prefixes, string[] suffixes, string haystack)
        {
            int lastIdx = 0;
            StringBuilder resultBuilder = new StringBuilder(haystack.Length);
            int matchLength = -1;
            for (; ; )
            {
                int[] foundIdx = indexesOfNext(wordsToSearchFor, lastIdx, haystack.ToString());
                int curIdx = foundIdx[1]; //  original.IndexOf(match, lastIdx, comparisonType);

                if (curIdx > -1 && curIdx != Int32.MaxValue)
                {
                    matchLength = wordsToSearchFor[foundIdx[0]].Length;

                    resultBuilder
                      .Append(haystack, lastIdx, curIdx - lastIdx)
                      .Append(prefixes[foundIdx[0]])
                      .Append(haystack, curIdx, matchLength)
                      .Append(suffixes[foundIdx[0]]);
                }
                else
                {
                    return resultBuilder.Append(haystack.Substring(lastIdx)).ToString();
                }

                lastIdx = curIdx + matchLength;
            }                                                              
        }

        private int[] indexesOfNext(string[] wordsToSearchFor, int startIndex, string haystack)
        {
            int[] ret = { -1, Int32.MaxValue }; // [0] = index in textToSearchFor array; [1] = index in haystack
            for (int i = 0; i < wordsToSearchFor.Length; i++)
            {
                // int idx =  haystack.IndexOf(textToSearchFor[i], startIndex, StringComparison.CurrentCultureIgnoreCase);
                int idx = FirstIndexOfWord(wordsToSearchFor[i], startIndex, ret[1], haystack);
                if (idx >= 0 && idx < ret[1] && WordStartsAndEndsWithWhitespace(wordsToSearchFor[i], idx, haystack))
                {
                    ret[0] = i;
                    ret[1] = idx;

                    if (idx == 0)
                        return ret;
                }
            } // for

            return ret;
        }

        private int FirstIndexOfWord(string word, int startIndex, int lastFoundIndex, string haystack)
        {
            int currIndex = startIndex;
            string initialHaystack = haystack;            
            // -- if we already have a found item, don't find items after that one.
            if (lastFoundIndex != Int32.MaxValue)
                haystack = haystack.Substring(0,lastFoundIndex);

            while (currIndex < haystack.Length)
            {
                int idx = haystack.IndexOf(word, currIndex, StringComparison.CurrentCultureIgnoreCase);
                if (idx < 0)
                {
                    return -1;
                }
                else if (idx >= 0 && WordStartsAndEndsWithWhitespace(word, idx, initialHaystack) && !WordIsInLinkOrTagOrEntity(word, idx, initialHaystack))
                {
                    return idx;
                }
                else if (idx >= 0)
                {
                    currIndex = idx + word.Length; // skip past the found word that failed the other filters
                }
                else
                {
                    currIndex = currIndex + word.Length;
                }
            }
            return -1;
        }

        private bool WordIsInLinkOrTagOrEntity(string word, int foundIdx, string haystack)
        {
            // - check entity first because it's the least resource intensive
            // -- in an entity if char before word == '&' and char after word == ';'
            if (foundIdx > 0 && haystack[foundIdx - 1] == '&' && ((foundIdx + word.Length) < (haystack.Length - 2)) && haystack[foundIdx + word.Length] == ';')
                return true;

            // the word is in an image or link tag if when looking to the left of  foundIdx in haystack that
            // - You find "<a" before "</a>" is found
            // - You find "<" before ">"
            // - In general, you find "<" before ">"

            string preIdx = haystack.Substring(0, foundIdx);
            // -- in link?            
            int startIdx = preIdx.LastIndexOf("<a", StringComparison.CurrentCultureIgnoreCase);
            int closeIdx = preIdx.LastIndexOf("</a>", StringComparison.CurrentCultureIgnoreCase);
            // start found, but end not found;
            // or start found and end is before start
            if (startIdx >= 0 && (closeIdx < 0 || (closeIdx < startIdx)))
                return true;

            // -- in tag?            
            startIdx = preIdx.LastIndexOf("<", StringComparison.CurrentCultureIgnoreCase);
            closeIdx = preIdx.LastIndexOf(">", StringComparison.CurrentCultureIgnoreCase);
            if (startIdx >= 0 && (closeIdx < 0 || (closeIdx < startIdx)))
                return true;          

            return false;

        }


        private bool WordStartsAndEndsWithWhitespace(string word, int foundIdx, string haystack)
        {
            // return true;

            // -- check the start
            if (foundIdx > 1)
            {
                Char c = haystack[foundIdx-1];
                if (!Char.IsWhiteSpace(c) && !Char.IsSeparator(c) && !Char.IsPunctuation(c) && !Char.IsNumber(c) && c != '<' && c != '>')
                    return false;
            }

            // -- check the end
            int endIdx = foundIdx + word.Length;
            if (endIdx < (haystack.Length))
            {
                Char c = haystack[endIdx];
                if (!Char.IsWhiteSpace(c) && !Char.IsSeparator(c) && !Char.IsPunctuation(c) && !Char.IsNumber(c) && c != '<' && c != '>')
                    return false;

            }
            return true;
                
        }
        
    }
}
