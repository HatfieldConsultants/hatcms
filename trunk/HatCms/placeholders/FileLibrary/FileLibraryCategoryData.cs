using System;
using System.Text;
using System.Collections.Generic;

namespace HatCMS.Placeholders
{
    public class FileLibraryCategoryData
    {
        private int categoryId = -1;
        public int CategoryId
        {
            get { return categoryId; }
            set { categoryId = value; }
        }

        private CmsLanguage lang = CmsConfig.Languages[0];
        public CmsLanguage Lang
        {
            get { return lang; }
            set { lang = value; }
        }

        private bool eventRequired = false;
        public bool EventRequired
        {
            get { return eventRequired; }
            set { eventRequired = value; }
        }

        private string categoryName = "";
        public string CategoryName
        {
            get { return categoryName; }
            set { categoryName = value; }
        }

        private int sortOrdinal = 0;
        public int SortOrdinal
        {
            get { return sortOrdinal; }
            set { sortOrdinal = value; }
        }

        public int EventRequiredAsInt
        {
            get { return Convert.ToInt32(EventRequired); }
        }

        /// <summary>
        /// Search for a file library category from a list, given a category Id
        /// </summary>
        /// <param name="list"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static FileLibraryCategoryData getCategoryFromList(List<FileLibraryCategoryData> list, int categoryId)
        {
            foreach (FileLibraryCategoryData c in list)
            {
                if (c.CategoryId == categoryId)
                    return c;
            }
            return null;
        }

        /// <summary>
        /// Check from all the category records to see if event is requried, given a
        /// selected category id
        /// </summary>
        /// <param name="list"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public static bool isEventRequired(List<FileLibraryCategoryData> list, int categoryId)
        {
            FileLibraryCategoryData c = getCategoryFromList(list, categoryId);
            if (c != null)
                return c.EventRequired;

            return false;
        }

        public static bool atLeastOneCategoryRequiresAnEvent(FileLibraryCategoryData[] haystack)
        {
            foreach (FileLibraryCategoryData cat in haystack)
            {
                if (cat.EventRequired)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get the popup link in html
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="htmlDomId"></param>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static string getEditPopupAnchor(CmsLanguage lang, string htmlDomId, string displayText)
        {
            try
            {
                CmsPage editCategoryPage = new CmsPageDb().getPage("_admin/FileLibraryCategory");
                string anchor = "<a href=\"{0}\" onclick=\"window.open(this.href,'{1}','resizable=1,scrollbars=1,width=800,height=400'); return false;\">{2}</a>";
                return string.Format(anchor, new string[] { editCategoryPage.getUrl(lang), htmlDomId, displayText });
            }
            catch (Exception ex)
            {
                return " <span>Cannot setup Edit Category Link: " + ex.Message + "</span>";
            }
        }

        /// <summary>
        /// Get the option html tag
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="categoryList"></param>
        /// <param name="selectedId"></param>
        /// <returns></returns>
        public static string getCategoryOptionTag(CmsLanguage lang, List<FileLibraryCategoryData> categoryList, int selectedId)
        {
            StringBuilder html = new StringBuilder();
            string optionTag = "<option value=\"{0}\" id=\"{1}\" title=\"{2}\" {3}>{4}</option>" + Environment.NewLine;
            foreach (FileLibraryCategoryData c in categoryList)
            {
                string selected = (c.CategoryId == selectedId) ? "selected=\"selected\"" : "";
                string[] parm = new string[] {
                    c.CategoryId.ToString(),
                    "fileLibrary_catName_" + lang.shortCode + "_" + c.CategoryId.ToString(),
                    c.EventRequired.ToString().ToLower(),
                    selected,
                    c.CategoryName };
                html.Append(string.Format(optionTag, parm));
            }
            return html.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{");
            sb.Append(CategoryId.ToString() + ",");
            sb.Append(lang.shortCode + ",");
            sb.Append(Convert.ToString(EventRequired) + ",");
            sb.Append(CategoryName + ",");
            sb.Append(SortOrdinal.ToString() + "}");
            return sb.ToString();
        }
    }
}
