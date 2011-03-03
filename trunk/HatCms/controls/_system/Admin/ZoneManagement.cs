using System;
using System.Text;
using System.Collections.Generic;
using HatCMS.Controls.Admin;
using Hatfield.Web.Portal;
using System.Collections.Specialized;

namespace HatCMS.controls.Admin
{
    public class ZoneManagement : AdminController
    {
        protected CmsZoneDb zoneDb = new CmsZoneDb();

        /// <summary>
        /// Create the Zone entity object for adding record
        /// </summary>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected CmsPageSecurityZone createAddRecord(string controlId)
        {
            CmsPageSecurityZone data = new CmsPageSecurityZone();
            data.StartingPageId = PageUtils.getFromForm(controlId + "addStartingPageId", -999);
            data.ZoneName = PageUtils.getFromForm(controlId + "addName", "");
            return data;
        }

        /// <summary>
        /// Create the Zone entity object for updating record
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected CmsPageSecurityZone createUpdateRecord(string controlId, int id)
        {
            CmsPageSecurityZone data = new CmsPageSecurityZone();
            data.ZoneId = id;
            data.StartingPageId = PageUtils.getFromForm(controlId + "startingPageId", -999);
            data.ZoneName = PageUtils.getFromForm(controlId + "name", "");
            return data;
        }

        /// <summary>
        /// Get all the page IDs and paths
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private NameValueCollection getParentPageOptions(CmsPage page)
        {
            if (page == null || page.ID == -1)
                return new NameValueCollection();

            NameValueCollection nvc = new NameValueCollection();
            if (page.isVisibleForCurrentUser)
            {
                nvc.Add(page.ID.ToString(), page.Path);
                foreach (CmsPage subPage in page.ChildPages)
                {
                    NameValueCollection ret = getParentPageOptions(subPage);
                    foreach (string key in ret.Keys)
                        nvc.Add(key, ret[key]);
                }
            }
            return nvc;
        }

        /// <summary>
        /// Create the option tag list for path selection
        /// </summary>
        /// <param name="targetValue"></param>
        /// <returns></returns>
        protected string generatePathOption(string targetValue)
        {
            NameValueCollection coll = getParentPageOptions(CmsContext.HomePage);

            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"\">- select -</option>" + EOL);

            foreach(string key in coll.Keys) {
                string path = coll.Get(key);
                html.Append("<option value=\"" + key + "\"");
                if (key == targetValue)
                    html.Append(" selected=\"selected\"");
                html.Append(" >" + path + "</option>");
            }

            return html.ToString();
        }

        /// <summary>
        /// Create the html form hidden fields
        /// </summary>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string generateFormHiddenField(string controlId)
        {
            StringBuilder html = new StringBuilder();
            html.Append(PageUtils.getHiddenInputHtml("AdminTool", Audit.AdminTool.ZoneManagement.ToString()) + EOL);
            html.Append(PageUtils.getHiddenInputHtml(controlId + "id", controlId + "id", "") + EOL);
            html.Append(PageUtils.getHiddenInputHtml(controlId + "delete", controlId + "delete", "") + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Handle form submit, add/update record
        /// </summary>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string handleFormSubmit(string controlId)
        {
            int id = PageUtils.getFromForm(controlId + "id", -999);
            if (id == -999)
                return "";

            if (id == -1)
            {   // when ID is -1, it is ADD mode
                if (zoneDb.insert(createAddRecord(controlId)) == false)
                    return formatErrorMsg("Error adding record.");
            }
            else
            {
                if (PageUtils.getFromForm(controlId + "delete", "") == "true")
                {   // when ID != -1 and DELETE flag is set, DEL mode
                    if (validateDeleteDefaultZone(controlId, id) == false)
                        return formatErrorMsg("Default zone cannot be deleted.");

                    if (zoneDb.delete(createUpdateRecord(controlId, id)) == false)
                        return formatErrorMsg("Error deleting record.");
                }
                else
                {   // when ID != -1 and no DELETE flag, CHG mode
                    if (validateUpdateDefaultZone(controlId, id) == false)
                        return formatErrorMsg("Default zone must start from home page (i.e. '/').");

                    if (zoneDb.update(createUpdateRecord(controlId, id)) == false)
                        return formatErrorMsg("Error updating record.");
                }
            }

            return formatNormalMsg("Record saved.");
        }
        
        /// <summary>
        /// Create the table column header
        /// </summary>
        /// <returns></returns>
        protected string RenderHeader()
        {
            StringBuilder html = new StringBuilder("<caption><h2>Create/Edit Security Zones</h2></caption>");
            html.Append("<tr>" + EOL);
            html.Append("<th style=\"width: 10em;\"> </th>" + EOL);
            html.Append("<th>Zone Name</th>" + EOL);
            html.Append("<th>Starting Page</th>" + EOL);
            html.Append("</tr>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Create the table body
        /// </summary>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string RenderContent(string controlId)
        {
            List<CmsPageSecurityZone> list = zoneDb.fetchAll();

            StringBuilder html = new StringBuilder();
            for (int x = 0; x < list.Count; x++)
            {
                html.Append("<tr>" + EOL);
                CmsPageSecurityZone data1 = list[x];
                html.Append("<td>" + EOL);
                html.Append("<input class=\"" + controlId + "chgButton\" type=\"button\" value=\"Edit\" title=\"" + data1.ZoneId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "delButton\" type=\"button\" value=\"Delete\" title=\"" + data1.ZoneId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "chgSaveButton\" type=\"button\" value=\"Save\" title=\"" + data1.ZoneId + "\" />" + EOL);
                html.Append("<input class=\"" + controlId + "chgCancelButton\" type=\"button\" value=\"Cancel\" title=\"" + data1.ZoneId + "\" />" + EOL);
                html.Append("</td>" + EOL);

                string zName = data1.ZoneName;
                html.Append("<td><div id=\"" + controlId + "name_" + data1.ZoneId + "\">" + zName + "</div></td>" + EOL);

                html.Append("<td>" + EOL);
                html.Append("<select title=\"" + Convert.ToInt32(data1.StartingPageId).ToString() + "\" class=\"" + controlId + "chg\" disabled=\"disabled\" id=\"" + controlId + "startingPageId_" + data1.ZoneId + "\" name=\"" + controlId + "startingPageId\">" + EOL);
                html.Append(generatePathOption(Convert.ToInt32(data1.StartingPageId).ToString()));
                html.Append("</select>" + EOL);
                html.Append("</td>" + EOL);

                html.Append("</tr>" + EOL);
            }
            return html.ToString();
        }

        /// <summary>
        /// Create the table footer for adding record
        /// </summary>
        /// <param name="controlId"></param>
        /// <returns></returns>
        protected string RenderFooter(string controlId)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<tr>" + EOL);

            html.Append("<td>" + EOL);
            html.Append("<input class=\"" + controlId + "addButton\" type=\"button\" value=\"Add\" />" + EOL);
            html.Append("<input class=\"" + controlId + "addSaveButton\" type=\"button\" value=\"Save\" />" + EOL);
            html.Append("<input class=\"" + controlId + "addCancelButton\" type=\"button\" value=\"Cancel\" />" + EOL);
            html.Append("</td>" + EOL);

            html.Append("<td>" + EOL);
            html.Append("<input class=\"" + controlId + "add\" type=\"text\" id=\"" + controlId + "addName \" name=\"" + controlId + "addName\" value=\"\" size=\"15\" maxlength=\"40\" />" + EOL);
            html.Append("</td>" + EOL);

            html.Append("<td>" + EOL);
            html.Append("<select class=\"" + controlId + "add\" disabled=\"disabled\" id=\"" + controlId + "addStartingPageId\" name=\"" + controlId + "addStartingPageId\">" + EOL);
            html.Append(generatePathOption(""));
            html.Append("</select>" + EOL);
            html.Append("</td>" + EOL);

            html.Append("</tr>" + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Add Js file and Js code to HTML HEAD
        /// </summary>
        /// <returns></returns>
        protected void RenderHtmlHead()
        {
            CmsPageHeadSection h = CmsContext.currentPage.HeadSection;
            h.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");
            h.AddJavascriptFile("js/_system/Zone/ZoneManagement.js");
        }

        /// <summary>
        /// Render the Zone Management interface.
        /// </summary>
        /// <returns></returns>
        public override string Render()
        {
            RenderHtmlHead();
            string controlId = "zoneManagement_";
            StringBuilder html = new StringBuilder();
            html.Append(handleFormSubmit(controlId));

            string tHeader = RenderHeader();
            string tContent = RenderContent(controlId);
            string tFooter = RenderFooter(controlId);

            CmsPage p = CmsContext.currentPage;
            html.Append(p.getFormStartHtml(controlId + "Form") + EOL);
            html.Append(generateFormHiddenField(controlId));
            html.Append(TABLE_START_HTML + EOL);
            html.Append(tHeader + tContent + tFooter);
            html.Append(TABLE_END_HTML + EOL);
            html.Append(p.getFormCloseHtml(controlId + "Form") + EOL);

            return html.ToString();
        }

        /// <summary>
        /// For the default zone which starting page is
        /// the home page, delete is not allowed.
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected bool validateDeleteDefaultZone(string controlId, int id)
        {
            CmsPageSecurityZone z = zoneDb.fetch(id);
            if (z.ZoneId < 0)
                return true;

            if (z.StartingPageId == CmsContext.HomePage.ID)
                return false;
            else
                return true;
        }

        /// <summary>
        /// For the default zone, it must start from home page.
        /// (i.e. only name can be updated)
        /// </summary>
        /// <param name="controlId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected bool validateUpdateDefaultZone(string controlId, int id)
        {
            CmsPageSecurityZone z = zoneDb.fetch(id);
            if (z.ZoneId < 0)
                return true;

            if (z.StartingPageId == CmsContext.HomePage.ID && PageUtils.getFromForm(controlId + "startingPageId", -999) != CmsContext.HomePage.ID)
                return false;
            else
                return true;
        }
    }
}
