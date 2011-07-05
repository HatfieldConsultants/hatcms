using System;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal.Html
{
    public class PageNumberAnchorUtils
    {
        protected static string HTML_ANCHOR = "<a href=\"{0}\" class=\"{1}\">{2}</a> &#160;";
        protected static string HTML_SPAN = "<span style=\"font-weight: bold;\" class=\"{0}\">{1}</span> &#160;";

        /// <summary>
        /// Generate a shorten page number list, e.g.
        /// 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        /// With current = 4, siblingsCount = 1, output is 1, 3, 4, 5, 10
        /// With current = 2, siblingsCount = 2, output is 1, 2, 3, 4, 10
        /// </summary>
        /// <param name="anchorList"></param>
        /// <param name="current"></param>
        /// <param name="siblingsCount"></param>
        /// <returns></returns>
        public static List<PageNumberAnchor> digestList(List<PageNumberAnchor> anchorList, int current, int siblingsCount)
        {
            List<PageNumberAnchor> shortList = new List<PageNumberAnchor>();
            if (anchorList.Count == 0)
                return shortList;

            shortList.Add(anchorList[current]);
            for (int x = 0; x < siblingsCount; x++)
            {
                try
                {   // add the next one
                    shortList.Add(anchorList[current + x + 1]);
                }
                catch { }
                try
                {   // add the previous one
                    shortList.Insert(0, anchorList[current - x - 1]);
                }
                catch { }
            }

            if (shortList.Contains(anchorList[0]) == false)
                shortList.Insert(0, anchorList[0]); // add the first one

            if (shortList.Contains(anchorList[anchorList.Count - 1]) == false)
                shortList.Add(anchorList[anchorList.Count - 1]); // add the last one

            return shortList;
        }

        /// <summary>
        /// Generate a shorten html string for page separation
        /// 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
        /// With current = 4, siblingsCount = 1, output is [1]...[3] 4 [5]...[10]
        /// With current = 2, siblingsCount = 2, output is [1] 2 [3][4]...[10]
        /// * [] indicates html anchor
        /// </summary>
        /// <param name="anchorList"></param>
        /// <param name="current"></param>
        /// <param name="siblingsCount"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string toHtmlString(List<PageNumberAnchor> anchorList, int current, int siblingsCount, string separator)
        {
            List<PageNumberAnchor> shortList = digestList(anchorList, current, siblingsCount);
            if (anchorList.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            int prevPageNum = anchorList[0].PageNum;
            for (int x = 0; x < shortList.Count; x++)
            {
                PageNumberAnchor a = shortList[x];
                if (a.PageNum == current + 1)
                    sb.Append(String.Format(HTML_SPAN, new string[] { a.CssClass, a.PageNum.ToString() }));
                else
                    sb.Append(String.Format(HTML_ANCHOR, new string[] { a.Href, a.CssClass, a.PageNum.ToString() }));

                PageNumberAnchor nextAnchor = shortList[Math.Min(x + 1, shortList.Count - 1)];
                if (a.PageNum + 1 < nextAnchor.PageNum)
                    sb.Append(separator + "&#160;");
            }

            return sb.ToString();
        }
    }
}
