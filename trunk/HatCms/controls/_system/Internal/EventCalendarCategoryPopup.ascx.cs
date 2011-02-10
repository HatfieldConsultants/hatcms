using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.placeholders.Calendar;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Hatfield.Web.Portal;

namespace HatCMS.controls._system.Internal
{
    public partial class EventCalendarCategoryPopup : System.Web.UI.UserControl
    {
        protected string EOL = Environment.NewLine;
        protected EventCalendarDb db = new EventCalendarDb();

        protected void Page_Load(object sender, EventArgs e)
        {
            CmsPage page = CmsContext.currentPage;
            addJavascript(CmsConfig.Languages, page);
            page.HeadSection.AddJSOnReady("updateOpener( window.name );");
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
            foreach(CmsLanguage lang in langArray)
            {
                js.Append("'");
                js.Append(lang.shortCode);
                js.Append("',");
            }
            js.Remove(js.Length - 1, 1);
            js.Append("];");
            page.HeadSection.AddJSStatements(js.ToString());

            page.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");
            page.HeadSection.AddJavascriptFile("js/_system/EventCalendar/EventCalendarCategory.js");
        }

        /// <summary>
        /// From config file, read the possible options for category color.
        /// </summary>
        /// <returns></returns>
        protected string[] GetColorCode()
        {
            string color = CmsConfig.getConfigValue("EventCalendar.CategoryColor", "#0000FF");
            return color.Split(new char[] { '|' });
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
            html.Append("<th rowspan=\"2\" style=\"width: 9em;\">Category Id</th>" + EOL);
            html.Append("<th rowspan=\"2\">Color</th>" + EOL);

            for (int y = 0; y < step; y++)
                html.Append("<th colspan=\"2\">" + langArray[y].shortCode + "</th>" + EOL);

            html.Append("</tr><tr>" + EOL);

            for (int y = 0; y < step; y++)
            {
                html.Append("<th>Title</th>" + EOL);
                html.Append("<th>Description</th>" + EOL);
            }

            html.Append("</tr>" + EOL);

            return html.ToString();
        }

        /// <summary>
        /// Create table body to show the Event Calendar Category records.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="colorArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string generateContent(CmsLanguage[] langArray, string[] colorArray, string controlId)
        {
            List<EventCalendarDb.EventCalendarCategoryData> list = db.fetchCategoryList();

            int step = langArray.Length;
            StringBuilder html = new StringBuilder();
            for (int x = 0; x < list.Count; x = x + step)
            {
                html.Append("<tr>" + EOL);
                EventCalendarDb.EventCalendarCategoryData data1 = list[x];
                html.Append("<td>" + EOL);
                html.Append("<input class=\"" + controlId + "chgButton\" type=\"button\" value=\"Edit\" title=\"" + data1.CategoryId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "chgSaveButton\" type=\"button\" value=\"Save\" title=\"" + data1.CategoryId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "chgCancelButton\" type=\"button\" value=\"Cancel\" title=\"" + data1.CategoryId + "\" />" + EOL);
                html.Append("</td>" + EOL);

                html.Append("<td>" + EOL);
                html.Append("<select title=\"" + data1.ColorHex + "\" class=\"" + controlId + "chg\" disabled=\"disabled\" id=\"" + controlId + "colorHex_" + data1.CategoryId + "\" name=\"" + controlId + "colorHex\" onchange=\"this.style.backgroundColor=this.value;\">" + EOL);
                html.Append(generateColorOption(colorArray, data1.ColorHex));
                html.Append("</select>" + EOL);
                html.Append("</td>" + EOL);

                for (int y = 0; y < step; y++)
                {
                    string l = langArray[y].shortCode;
                    EventCalendarDb.EventCalendarCategoryData data2 = list[y + x];

                    string title = data2.Title;
                    html.Append("<td><div id=\"" + controlId + "title_" + l + "_" + data1.CategoryId + "\">" + title + "</div></td>" + EOL);

                    string description = StringUtils.nl2br(data2.Description);
                    html.Append("<td><div id=\"" + controlId + "description_" + l + "_" + data1.CategoryId + "\">" + description + "</div></td>" + EOL);
                }

                html.Append("</tr>" + EOL);
            } 
            return html.ToString();
        }

        /// <summary>
        /// Generate HTML SELECT and OPTION tags elements for color selection.
        /// </summary>
        /// <param name="colorArray"></param>
        /// <param name="targetColor"></param>
        /// <returns></returns>
        protected string generateColorOption(string[] colorArray, string targetColor)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<option style=\"background-color: #FFFFFF;\" value=\"\">- select -</option>" + EOL);

            foreach (string c in colorArray)
            {
                if (c == targetColor)
                    html.Append("<option style=\"background-color: " + c + "\" value=\"" + c + "\" selected=\"selected\"> </option>" + EOL);
                else
                    html.Append("<option style=\"background-color: " + c + "\" value=\"" + c + "\"> </option>" + EOL);
            }
            return html.ToString();
        }

        /// <summary>
        /// Generate an ADD new record row as the footer.  This allows users to add a new
        /// Event Calendar Category.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="colorArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string generateFooter(CmsLanguage[] langArray, string[] colorArray, string controlId)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<tr>" + EOL);

            html.Append("<td>" + EOL);
            html.Append("<input class=\"" + controlId + "addButton\" type=\"button\" value=\"Add\" />" + EOL);
            html.Append("<input class=\"" + controlId + "addSaveButton\" type=\"button\" value=\"Save\" />" + EOL);
            html.Append("<input class=\"" + controlId + "addCancelButton\" type=\"button\" value=\"Cancel\" />" + EOL);
            html.Append("</td>" + EOL);

            html.Append("<td>" + EOL);
            html.Append("<select class=\"" + controlId + "add\" id=\"" + controlId + "addColorHex\" name=\"" + controlId + "addColorHex\" onchange=\"this.style.backgroundColor=this.value;\">" + EOL);
            html.Append(generateColorOption(colorArray, ""));
            html.Append("</select>" + EOL);
            html.Append("</td>" + EOL);

            foreach(CmsLanguage lang in langArray)
            {
                string l = lang.shortCode;
                html.Append("<td>" + EOL);
                html.Append("<input class=\"" + controlId + "add\" type=\"text\" id=\"" + controlId + "addTitle_" + l + "\" name=\"" + controlId + "addTitle_" + l + "\" value=\"\" size=\"15\" maxlength=\"40\" />" + EOL);
                html.Append("</td>" + EOL);

                html.Append("<td>" + EOL);
                html.Append("<textarea class=\"" + controlId + "add\" name=\"" + controlId + "addDescription_" + l + "\" id=\"" + controlId + "addDescription_" + l + "\" rows=\"3\" cols=\"20\"></textarea>" + EOL);
                html.Append("</td>" + EOL);
            }

            html.Append("</tr>" + EOL);
            return html.ToString();
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
            EventCalendarDb.EventCalendarCategoryData[] data = null;

            lock (this) // lock this code segment because auto-id cannot be used during insert
            {
                if (id == -1) // add new record
                    data = createAddRecord(langArray, controlId);
                else // update existing record
                    data = createUpdateRecord(langArray, controlId, id);

                foreach (EventCalendarDb.EventCalendarCategoryData d in data)
                {
                    if (id == -1)
                    {
                        if (db.insertCategoryData(d) == false)
                            return "<p style=\"color: red;\">Error adding record.</p>";
                    }
                    else
                    {
                        if (db.updateCategoryData(d) == false)
                            return "<p style=\"color: red;\">Error updating record.</p>";
                    }
                }
            }

            return "<p>Record saved.</p>";
        }

        /// <summary>
        /// Create the entity object by reading the new values from html form.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected EventCalendarDb.EventCalendarCategoryData[] createAddRecord(CmsLanguage[] langArray, string controlId)
        {
            int step = langArray.Length;
            EventCalendarDb.EventCalendarCategoryData[] data = new EventCalendarDb.EventCalendarCategoryData[step];
            int newId = db.fetchNextCategoryId();
            for (int x = 0; x < step; x++) 
            {
                string l = langArray[x].shortCode;
                data[x] = new EventCalendarDb.EventCalendarCategoryData();
                data[x].CategoryId = newId;
                data[x].ColorHex = PageUtils.getFromForm(controlId + "addColorHex", "");
                data[x].Lang = langArray[x];
                data[x].Title = PageUtils.getFromForm(controlId + "addTitle_" + l, "");
                data[x].Description = PageUtils.getFromForm(controlId + "addDescription_" + l, "");
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
        protected EventCalendarDb.EventCalendarCategoryData[] createUpdateRecord(CmsLanguage[] langArray, string controlId, int id)
        {
            int step = langArray.Length;
            EventCalendarDb.EventCalendarCategoryData[] data = new EventCalendarDb.EventCalendarCategoryData[step];

            for (int x = 0; x < step; x++)
            {
                string l = langArray[x].shortCode;
                data[x] = new EventCalendarDb.EventCalendarCategoryData();
                data[x].CategoryId = id;
                data[x].ColorHex = PageUtils.getFromForm(controlId + "colorHex", "");
                data[x].Lang = langArray[x];
                data[x].Title = PageUtils.getFromForm(controlId + "title_" + l, "");
                data[x].Description = PageUtils.getFromForm(controlId + "description_" + l, "");
            }

            return data;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            string[] colorArray = GetColorCode();
            string controlId = "eventCalendarCategory_";
            CmsLanguage[] langArray = CmsConfig.Languages;

            CmsPage page = CmsContext.currentPage;
            writer.Write(handleFormSubmit(langArray, controlId) + EOL);

            string tHeader = generateHeader(langArray);
            string tContent = generateContent(langArray, colorArray, controlId);
            string tFooter = generateFooter(langArray, colorArray, controlId);

            string url = page.getUrl(CmsContext.currentLanguage);
            writer.Write("<form name=\"" + controlId + "Form\" method=\"get\" action=\"" + url + "\" >" + EOL);
            writer.Write("<input type=\"hidden\" name=\"" + controlId + "id\" id=\"" + controlId + "id\" />" + EOL);
            writer.Write("<table style=\"border-collapse: collapse; width: 100%;\" border=\"1\" cellspacing=\"0\" cellpadding=\"2\">" + EOL);
            writer.Write(tHeader + tContent + tFooter + EOL);
            writer.Write("</table>" + EOL);
            writer.Write("</form>" + EOL);
        }
    }
}