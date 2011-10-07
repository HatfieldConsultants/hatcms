using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using Hatfield.Web.Portal;
using HatCMS.Placeholders;

namespace HatCMS.Controls
{
    public class FileLibraryCategoryPopup : BaseCmsControl
    {

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/FileLibrary/FileLibraryCategory.js"));
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-1.4.1.min.js"));
            return ret.ToArray();
        }

        public override string RenderToString(CmsControlDefinition controlDefnToRender, CmsLanguage langToRenderFor)
        {
            CmsPage page = CmsContext.currentPage;
            addJavascript(CmsConfig.Languages, page);
            page.HeadSection.AddJSOnReady("updateOpener( window.name );");

            
            StringBuilder html = new StringBuilder();

            string controlId = "fileLibraryCategory_";
            CmsLanguage[] langArray = CmsConfig.Languages;

            
            html.Append(handleFormSubmit(langArray, controlId) + EOL);

            string tHeader = generateHeader(langArray);
            string tContent = generateContent(langArray, controlId);
            string tFooter = generateFooter(langArray, controlId);

            string url = page.getUrl(CmsContext.currentLanguage);
            html.Append(page.getFormStartHtml(controlId + "form") + EOL);
            html.Append(PageUtils.getHiddenInputHtml(controlId + "id", controlId + "id", "") + EOL);
            html.Append(PageUtils.getHiddenInputHtml(controlId + "delete", controlId + "delete", "") + EOL);
            html.Append("<table style=\"border-collapse: collapse; width: 100%;\" border=\"1\" cellspacing=\"0\" cellpadding=\"2\">" + EOL);
            html.Append(tHeader + tContent + tFooter + EOL);
            html.Append("</table>" + EOL);
            html.Append("</form>" + EOL);
            

            return html.ToString();
        }

        /// <summary>
        /// Add javascript to the page head section.  It also create a string array
        /// containing the CmsConfig.Languages.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="page"></param>
        protected void addJavascript(CmsLanguage[] langArray, CmsPage page)
        {
            StringBuilder js = new StringBuilder("langCode = [");
            foreach (CmsLanguage lang in langArray)
            {
                js.Append("'");
                js.Append(lang.shortCode);
                js.Append("',");
            }
            js.Remove(js.Length - 1, 1);
            js.Append("];");
            page.HeadSection.AddJSStatements(js.ToString());

            page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/jquery/jquery-1.4.1.min.js");
            page.HeadSection.AddJavascriptFile(JavascriptGroup.ControlOrPlaceholder, "js/_system/FileLibrary/FileLibraryCategory.js");
        }

        /// <summary>
        /// Format error message in red
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected string formatErrorMsg(string msg)
        {
            return "<div style=\"font-weight: bold; color: red;\">" + msg + "</div>" + EOL;
        }

        /// <summary>
        /// Format normal message in green
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected string formatNormalMsg(string msg)
        {
            return "<div style=\"color: green;\">" + msg + "</div>" + EOL;
        }

        /// <summary>
        /// Create a table header.  According to the CmsLanguages, add extra columns for
        /// other languages.
        /// </summary>
        /// <param name="langArray"></param>
        /// <returns></returns>
        protected string generateHeader(CmsLanguage[] langArray)
        {
            int step = langArray.Length;

            StringBuilder html = new StringBuilder();
            html.Append("<tr>" + EOL);
            html.Append("<th rowspan=\"2\" style=\"width: 9em;\"> </th>" + EOL);
            html.Append("<th rowspan=\"2\">Event required?</th>" + EOL);
            html.Append("<th rowspan=\"2\">Sort ordinal</th>" + EOL);

            for (int y = 0; y < step; y++)
                html.Append("<th>" + langArray[y].shortCode + "</th>" + EOL);

            html.Append("</tr><tr>" + EOL);

            for (int y = 0; y < step; y++)
                html.Append("<th>Category name</th>" + EOL);

            html.Append("</tr>" + EOL);

            return html.ToString();
        }

        /// <summary>
        /// Create table body to show the Event Calendar Category records.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string generateContent(CmsLanguage[] langArray, string controlId)
        {

            FileLibraryDb db = new FileLibraryDb();
            List<FileLibraryCategoryData> list = db.fetchCategoryList();

            int step = langArray.Length;
            StringBuilder html = new StringBuilder();
            for (int x = 0; x < list.Count; x = x + step)
            {
                html.Append("<tr>" + EOL);
                FileLibraryCategoryData data1 = list[x];
                html.Append("<td>" + EOL);
                html.Append("<input class=\"" + controlId + "chgButton\" type=\"button\" value=\"Edit\" title=\"" + data1.CategoryId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "delButton\" type=\"button\" value=\"Delete\" title=\"" + data1.CategoryId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "chgSaveButton\" type=\"button\" value=\"Save\" title=\"" + data1.CategoryId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "chgCancelButton\" type=\"button\" value=\"Cancel\" title=\"" + data1.CategoryId + "\" />" + EOL);
                html.Append("</td>" + EOL);

                html.Append("<td>" + EOL);
                html.Append("<select title=\"" + data1.EventRequired.ToString().ToLower() + "\" class=\"" + controlId + "chg\" disabled=\"disabled\" id=\"" + controlId + "eventRequired_" + data1.CategoryId + "\" name=\"" + controlId + "eventRequired\">" + EOL);
                html.Append(generateEventRequiredOption(data1.EventRequired.ToString().ToLower()));
                html.Append("</select>" + EOL);
                html.Append("</td>" + EOL);

                html.Append("<td>" + EOL);
                html.Append("<div id=\"" + controlId + "sortOrdinal_" + data1.CategoryId.ToString() + "\">" + data1.SortOrdinal.ToString() + "</td>" + EOL);
                html.Append("</td>" + EOL);

                for (int y = 0; y < step; y++)
                {
                    string l = langArray[y].shortCode;
                    FileLibraryCategoryData data2 = list[y + x];

                    string catName = data2.CategoryName;
                    html.Append("<td><div id=\"" + controlId + "name_" + l + "_" + data1.CategoryId + "\">" + catName + "</div></td>" + EOL);
                }

                html.Append("</tr>" + EOL);
            }
            return html.ToString();
        }

        /// <summary>
        /// Generate HTML SELECT and OPTION tags elements.
        /// </summary>
        /// <param name="targetVal"></param>
        /// <returns></returns>
        protected string generateEventRequiredOption(string targetVal)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"\">- select -</option>" + EOL);

            if (targetVal == "true")
            {
                html.Append("<option value=\"true\" selected=\"selected\">Yes</option>" + EOL);
                html.Append("<option value=\"false\">No</option>" + EOL);
            }
            else if (targetVal == "false")
            {
                html.Append("<option value=\"true\">Yes</option>" + EOL);
                html.Append("<option value=\"false\" selected=\"selected\">No</option>" + EOL);
            }
            else
            {
                html.Append("<option value=\"true\">Yes</option>" + EOL);
                html.Append("<option value=\"false\">No</option>" + EOL);
            }
            return html.ToString();
        }

        /// <summary>
        /// Generate an ADD new record row as the footer.  This allows users to add a new
        /// Event Calendar Category.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string generateFooter(CmsLanguage[] langArray, string controlId)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<tr>" + EOL);

            html.Append("<td>" + EOL);
            html.Append("<input class=\"" + controlId + "addButton\" type=\"button\" value=\"Add\" />" + EOL);
            html.Append("<input class=\"" + controlId + "addSaveButton\" type=\"button\" value=\"Save\" />" + EOL);
            html.Append("<input class=\"" + controlId + "addCancelButton\" type=\"button\" value=\"Cancel\" />" + EOL);
            html.Append("</td>" + EOL);

            html.Append("<td>" + EOL);
            html.Append("<select class=\"" + controlId + "add\" id=\"" + controlId + "addEventRequired\" name=\"" + controlId + "addEventRequired\">" + EOL);
            html.Append(generateEventRequiredOption(""));
            html.Append("</select>" + EOL);
            html.Append("</td>" + EOL);

            html.Append("<td>" + EOL);
            html.Append(PageUtils.getInputTextHtml(controlId + "addSortOrdinal", controlId + "addSortOrdinal", "", 15, 20, controlId + "add", new JavascriptEvent[0]) + EOL);
            html.Append("</td>" + EOL);

            foreach (CmsLanguage lang in langArray)
            {
                string l = lang.shortCode;
                html.Append("<td>" + EOL);
                html.Append("<input class=\"" + controlId + "add\" type=\"text\" id=\"" + controlId + "addName_" + l + "\" name=\"" + controlId + "addName_" + l + "\" value=\"\" size=\"15\" maxlength=\"40\" />" + EOL);
                html.Append("</td>" + EOL);
            }

            html.Append("</tr>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Delete record from category table
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        protected string handleDelete(FileLibraryCategoryData d)
        {
            try
            {
                FileLibraryDb db = new FileLibraryDb();
                if (db.fetchCountByCategory(d) > 0)
                    return formatErrorMsg("Some files under this category, cannot delete category record.");
                else if (db.deleteCategoryData(d) == false)
                    return formatErrorMsg("Error deleting record.");
                else
                    return "";
            }
            catch (Exception ex)
            {
                return formatErrorMsg("Error deleting record. " + ex.Message);
            }
        }

        /// <summary>
        /// Handle form submit, either updating an existing category, or adding
        /// a new category.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string handleFormSubmit(CmsLanguage[] langArray, string controlId)
        {
            int id = PageUtils.getFromForm(controlId + "id", -999);
            if (id == -999)
                return "";

            int step = langArray.Length;
            FileLibraryCategoryData[] data = null;

            lock (this) // lock this code segment because auto-id cannot be used during insert
            {
                if (id == -1) // add new record
                    data = createAddRecord(langArray, controlId);
                else // update existing record
                    data = createUpdateRecord(langArray, controlId, id);

                FileLibraryDb db = new FileLibraryDb();

                foreach (FileLibraryCategoryData d in data)
                {
                    if (id == -1)
                    {   // when ID is -1, it is ADD mode
                        if (db.insertCategoryData(d) == false)
                            return formatErrorMsg("Error adding record.");
                    }
                    else
                    {
                        if (PageUtils.getFromForm(controlId + "delete", "") == "true")
                        {   // when ID != -1 and DELETE flag is set, DEL mode
                            string msg = handleDelete(d);
                            if (msg != "")
                                return msg;
                        }
                        else
                        {   // when ID != -1 and no DELETE flag, CHG mode
                            if (db.updateCategoryData(d) == false)
                                return formatErrorMsg("Error updating record.");
                        }
                    }
                }
            }

            return formatNormalMsg("Updated successfully.");
        }

        /// <summary>
        /// Create the entity object by reading the new values from html form.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected FileLibraryCategoryData[] createAddRecord(CmsLanguage[] langArray, string controlId)
        {
            int step = langArray.Length;
            FileLibraryCategoryData[] data = new FileLibraryCategoryData[step];
            FileLibraryDb db = new FileLibraryDb();
            int newId = db.fetchNextCategoryId();
            for (int x = 0; x < step; x++)
            {
                string l = langArray[x].shortCode;
                data[x] = new FileLibraryCategoryData();
                data[x].CategoryId = newId;
                data[x].Lang = langArray[x];
                data[x].EventRequired = PageUtils.getFromForm(controlId + "addEventRequired", false);
                data[x].CategoryName = PageUtils.getFromForm(controlId + "addName_" + l, "");
                data[x].SortOrdinal = PageUtils.getFromForm(controlId + "addSortOrdinal", 0);
            }
            return data;
        }

        /// <summary>
        /// Create the entity object by reading the updated values from html form.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected FileLibraryCategoryData[] createUpdateRecord(CmsLanguage[] langArray, string controlId, int id)
        {
            int step = langArray.Length;
            FileLibraryCategoryData[] data = new FileLibraryCategoryData[step];

            for (int x = 0; x < step; x++)
            {
                string l = langArray[x].shortCode;
                data[x] = new FileLibraryCategoryData();
                data[x].CategoryId = id;
                data[x].Lang = langArray[x];
                data[x].EventRequired = PageUtils.getFromForm(controlId + "eventRequired", false);
                data[x].CategoryName = PageUtils.getFromForm(controlId + "name_" + l, "");
                data[x].SortOrdinal = PageUtils.getFromForm(controlId + "sortOrdinal", 0);
            }

            return data;
        }
        
    }
}
