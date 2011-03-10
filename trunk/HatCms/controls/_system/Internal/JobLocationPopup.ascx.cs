using System;
using System.Text;
using System.Web.UI;
using HatCMS.Placeholders;
using Hatfield.Web.Portal;

namespace HatCMS.Controls._system.Internal
{
    public partial class JobLocationPopup : System.Web.UI.UserControl
    {
        protected string EOL = Environment.NewLine;
        protected JobPostingDb db = new JobPostingDb();

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
            foreach (CmsLanguage lang in langArray)
            {
                js.Append("'");
                js.Append(lang.shortCode);
                js.Append("',");
            }
            js.Remove(js.Length - 1, 1);
            js.Append("];");
            page.HeadSection.AddJSStatements(js.ToString());

            page.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");
            page.HeadSection.AddJavascriptFile("js/_system/JobDatabase/JobLocation.js");
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
            html.Append("<th rowspan=\"2\">Sort Ordinal</th>" + EOL);
            html.Append("<th rowspan=\"2\">Is All Locations</th>" + EOL);

            for (int y = 0; y < step; y++)
                html.Append("<th>" + langArray[y].shortCode + "</th>" + EOL);

            html.Append("</tr><tr>" + EOL);

            for (int y = 0; y < step; y++)
                html.Append("<th>Location Name</th>" + EOL);

            html.Append("</tr>" + EOL);

            return html.ToString();
        }

        /// <summary>
        /// Create table body to show the job location records.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string generateContent(CmsLanguage[] langArray, string controlId)
        {
            JobPostingLocation[] list = JobPostingLocation.FetchAll();

            int step = langArray.Length;
            StringBuilder html = new StringBuilder();
            for (int x = 0; x < list.Length; x++)
            {
                html.Append("<tr>" + EOL);
                JobPostingLocation data1 = list[x];
                html.Append("<td>" + EOL);
                html.Append("<input class=\"" + controlId + "chgButton\" type=\"button\" value=\"Edit\" title=\"" + data1.JobLocationId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "chgSaveButton\" type=\"button\" value=\"Save\" title=\"" + data1.JobLocationId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "chgCancelButton\" type=\"button\" value=\"Cancel\" title=\"" + data1.JobLocationId + "\" />" + EOL);
                html.Append("</td>" + EOL);

                html.Append("<td><div id=\"" + controlId + "sortOrdinal_" + data1.JobLocationId + "\">" + data1.SortOrdinal.ToString() + "</div></td>" + EOL);

                html.Append("<td>" + EOL);
                html.Append("<select title=\"" + Convert.ToInt32(data1.IsAllLocations).ToString() + "\" class=\"" + controlId + "chg\" disabled=\"disabled\" id=\"" + controlId + "isAllLocations_" + data1.JobLocationId + "\" name=\"" + controlId + "isAllLocations\">" + EOL);
                html.Append(generateIsAllLocationsOption(Convert.ToInt32(data1.IsAllLocations).ToString()));
                html.Append("</select>" + EOL);
                html.Append("</td>" + EOL);

                for (int y = 0; y < step; y++)
                {
                    CmsLanguage l = langArray[y];
                    string locName = data1.getLocationText(l);
                    html.Append("<td><div id=\"" + controlId + "name_" + l.shortCode + "_" + data1.JobLocationId + "\">" + locName + "</div></td>" + EOL);
                }

                html.Append("</tr>" + EOL);
            }
            return html.ToString();
        }

        /// <summary>
        /// Generate HTML SELECT and OPTION tags elements for Is All Locations
        /// </summary>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        protected string generateIsAllLocationsOption(string targetValue)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"\">- select -</option>" + EOL);

            if (targetValue == "1")
            {
                html.Append("<option value=\"1\" selected=\"selected\">Yes</option>" + EOL);
                html.Append("<option value=\"0\">No</option>" + EOL);
            }
            else if (targetValue == "0")
            {
                html.Append("<option value=\"1\">Yes</option>" + EOL);
                html.Append("<option value=\"0\" selected=\"selected\">No</option>" + EOL);
            }
            else
            {
                html.Append("<option value=\"1\">Yes</option>" + EOL);
                html.Append("<option value=\"0\">No</option>" + EOL);
            }

            return html.ToString();
        }

        /// <summary>
        /// Generate an ADD new record row as the footer.  This allows users to add a new
        /// job location.
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

            html.Append("<td><input class=\"" + controlId + "add\" type=\"text\" id=\"" + controlId + "addSortOrdinal\" value=\"\" size=\"15\" maxlength=\"40\" />" + EOL);

            html.Append("<td>" + EOL);
            html.Append("<select class=\"" + controlId + "add\" id=\"" + controlId + "addIsAllLocations\" name=\"" + controlId + "addIsAllLocations\">" + EOL);
            html.Append(generateIsAllLocationsOption(""));
            html.Append("</select>" + EOL);
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
        /// Handle form submit, either updating an existing job location, or adding
        /// a new job location.
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

            JobPostingLocation data;
            if (id == -1) // add new record
                data = createAddRecord(langArray, controlId);
            else // update existing record
                data = createUpdateRecord(langArray, controlId, id);

            if (data.SaveToDatabase() == false)
            {
                if (id == -1)
                    return "<p style=\"color: red;\">Error adding record.</p>";
                else
                    return "<p style=\"color: red;\">Error updating record.</p>";
            }
            else
                return "<p>Record saved.</p>";
        }

        /// <summary>
        /// Create the entity object by reading the new values from html form.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected JobPostingLocation createAddRecord(CmsLanguage[] langArray, string controlId)
        {
            int step = langArray.Length;
            JobPostingLocation data = new JobPostingLocation();
            data.SortOrdinal = PageUtils.getFromForm(controlId + "addSortOrdinal", 0);
            data.IsAllLocations = Convert.ToBoolean(PageUtils.getFromForm(controlId + "addIsAllLocations", 0));

            string[] locationNamePortion = new string[step];
            for (int x = 0; x < step; x++)
            {
                string l = langArray[x].shortCode;
                locationNamePortion[x] = PageUtils.getFromForm(controlId + "addName_" + l, "");
            }
            data.setLocationText(String.Join("|", locationNamePortion));

            return data;
        }

        /// <summary>
        /// Create the entity object by reading the updated values from html form.
        /// </summary>
        /// <param name="langArray"></param>
        /// <param name="controlId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected JobPostingLocation createUpdateRecord(CmsLanguage[] langArray, string controlId, int id)
        {
            int step = langArray.Length;
            JobPostingLocation data = new JobPostingLocation();
            data.JobLocationId = id;
            data.SortOrdinal = PageUtils.getFromForm(controlId + "sortOrdinal", 0);
            data.IsAllLocations = Convert.ToBoolean(PageUtils.getFromForm(controlId + "isAllLocations", 0));

            string[] locationNamePortion = new string[step];
            for (int x = 0; x < step; x++)
            {
                string l = langArray[x].shortCode;
                locationNamePortion[x] = PageUtils.getFromForm(controlId + "name_" + l, "");
            }
            data.setLocationText(String.Join("|", locationNamePortion));

            return data;
        }
        
        protected override void Render(HtmlTextWriter writer)
        {
            string controlId = "jobLocation_";
            CmsLanguage[] langArray = CmsConfig.Languages;

            CmsPage page = CmsContext.currentPage;
            writer.Write(handleFormSubmit(langArray, controlId) + EOL);

            string tHeader = generateHeader(langArray);
            string tContent = generateContent(langArray, controlId);
            string tFooter = generateFooter(langArray, controlId);

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