using System;
using System.Collections.Generic;
using System.Text;
using HatCMS.Controls.Admin;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Collections;

namespace HatCMS.controls.Admin
{
    public class ZoneAuthority : AdminController
    {
        protected CmsZoneDb db = new CmsZoneDb();
        protected CmsZoneUserRoleDb roleDb = new CmsZoneUserRoleDb();

        /// <summary>
        /// Create the html form hidden fields
        /// </summary>
        /// <returns></returns>
        protected string generateFormHiddenField()
        {
            StringBuilder html = new StringBuilder();
            html.Append(PageUtils.getHiddenInputHtml("AdminTool", Audit.AdminTool.ZoneAuthority.ToString()) + EOL);
            html.Append(PageUtils.getHiddenInputHtml("update", "updateZoneAuthority") + EOL);
            return html.ToString();
        }

        /// <summary>
        /// Create the role entity object by reading the html form params
        /// </summary>
        /// <param name="z"></param>
        /// <param name="r"></param>
        /// <param name="accessMode"></param>
        /// <returns></returns>
        protected CmsZoneUserRole createUserRoleEntity(CmsPageSecurityZone z, WebPortalUserRole r, string[] accessMode)
        {
            CmsZoneUserRole entity = new CmsZoneUserRole();
            entity.ZoneId = z.ZoneId;
            entity.UserRoleId = r.RoleID;
            foreach (string s in accessMode)
            {
                if (s.ToLower() == "r")
                    entity.ReadAccess = true;
                if (s.ToLower() == "w")
                    entity.WriteAccess = true;
            }
            if (r.RoleID == WebPortalUserRole.DUMMY_PUBLIC_ROLE_ID)
                entity.WriteAccess = false;
               
            return entity;
        }

        /// <summary>
        /// Handle form submit, delete all from table and re-insert
        /// </summary>
        /// <param name="zoneList"></param>
        /// <returns></returns>
        protected string handleZoneAuthorityUpdate(List<CmsPageSecurityZone> zoneList, List<WebPortalUserRole> roleList)
        {
            if (PageUtils.getFromForm("update", "") != "updateZoneAuthority")
                return "";

            List<CmsZoneUserRole> authority = new List<CmsZoneUserRole>();
            foreach (CmsPageSecurityZone z in zoneList)
            {
                roleDb.deleteByZone(z);
                foreach (WebPortalUserRole r in roleList)
                {
                    // for each zone and role, we expect a pair of html input elements: R, W
                    string htmlInputName = "z" + z.ZoneId + "r" + r.RoleID;
                    string[] accessMode = PageUtils.getFromForm(htmlInputName);
                    if (accessMode.Length == 0)
                        continue;

                    CmsZoneUserRole entity = createUserRoleEntity(z, r, accessMode);
                    authority.Add(entity);
                }
            }
            if (roleDb.insert(authority))
                return formatNormalMsg("Updated successfully.");
            else
                return formatErrorMsg("Database error, please contract administrator.");
        }

        /// <summary>
        /// Render the table column header
        /// </summary>
        /// <param name="roleList"></param>
        /// <returns></returns>
        protected string RenderZoneAuthorityHeader(List<WebPortalUserRole> roleList)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<caption><h2>Zone authority</h2></caption>" + EOL);
            html.Append("<tr>" + EOL);
            html.Append("<th rowspan=\"2\"> </th>" + EOL);
            foreach (WebPortalUserRole r in roleList)
            {
                if (r.RoleID != WebPortalUserRole.DUMMY_PUBLIC_ROLE_ID) // non-public user, show 'update' column
                    html.Append("<th colspan=\"2\" style=\"font-size: x-small;\">" + r.Description + "</td>" + EOL);
                else
                    html.Append("<th style=\"font-size: x-small;\">" + r.Description + "</td>" + EOL);
            }
            html.Append("</tr>" + EOL);

            html.Append("<tr>" + EOL);
            foreach (WebPortalUserRole r in roleList)
            {
                html.Append("<th style=\"font-size: x-small;\">Read</td>" + EOL);
                if (r.RoleID != WebPortalUserRole.DUMMY_PUBLIC_ROLE_ID) // non-public user, show 'update' column
                    html.Append("<th style=\"font-size: x-small;\">Update</td>" + EOL);
            }
            html.Append("</tr>" + EOL);

            return html.ToString();
        }

        /// <summary>
        /// Render the table body
        /// </summary>
        /// <param name="zoneList"></param>
        /// <param name="roleList"></param>
        /// <param name="adminRoleName"></param>
        /// <returns></returns>
        protected string RenderZoneAuthorityContent(List<CmsPageSecurityZone> zoneList, List<WebPortalUserRole> roleList, string adminRoleName)
        {
            StringBuilder html = new StringBuilder();
            foreach (CmsPageSecurityZone z in zoneList)
            {
                html.Append("<tr>" + EOL);
                html.Append(RenderZoneAuthorityRow(z, roleList, adminRoleName));
                html.Append("</tr>" + EOL);
            }
            return html.ToString();
        }

        /// <summary>
        /// Render the table body row
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="roleList"></param>
        /// <param name="adminRoleName"></param>
        /// <returns></returns>
        protected string RenderZoneAuthorityRow(CmsPageSecurityZone zone, List<WebPortalUserRole> roleList, string adminRoleName)
        {
            int zID = zone.ZoneId;
            List<CmsZoneUserRole> authority = roleDb.fetchAllByZone(zone);
            Set rSet = new Set();
            Set wSet = new Set();
            foreach (CmsZoneUserRole z in authority)
            {
                if (z.ReadAccess)
                    rSet.Add(z.UserRoleId);
                if (z.WriteAccess)
                    wSet.Add(z.UserRoleId);
            }

            StringBuilder html = new StringBuilder();
            html.Append("<td>" + zone.ZoneName + "</td>" + EOL);

            foreach (WebPortalUserRole r in roleList)
            {
                string rID = r.RoleID.ToString();
                bool checkR = rSet.Contains(r.RoleID);
                bool checkW = wSet.Contains(r.RoleID);
                bool disabled = false;
                if (r.Name == adminRoleName)
                {
                    checkR = true;
                    checkW = true;
                    disabled = true;
                }
                string htmlInputName = "z" + zID + "r" + rID;
                string checkboxR = PageUtils.getCheckboxHtml("", htmlInputName, htmlInputName, "r", checkR, "", disabled);
                html.Append("<td style=\"width: 3em;\" align=\"center\">" + checkboxR + "</td>" + EOL);
                if (r.RoleID != WebPortalUserRole.DUMMY_PUBLIC_ROLE_ID) // non-public user, show 'update' column
                {
                    string checkboxW = PageUtils.getCheckboxHtml("", htmlInputName, htmlInputName, "w", checkW, "", disabled);
                    html.Append("<td style=\"width: 3em;\" align=\"center\">" + checkboxW + "</td>" + EOL);
                }
            }

            return html.ToString();
        }

        /// <summary>
        /// Read the role names define in config file, and select the role
        /// entitys from database by the role names.
        /// </summary>
        /// <returns></returns>
        protected List<WebPortalUserRole> getRoleList()
        {
            List<WebPortalUserRole> roleList = new List<WebPortalUserRole>();
            WebPortalUserDB db = new WebPortalUserDB();
            WebPortalUserRole r;

            r = db.FetchUserRole(CmsConfig.getConfigValue("AdminUserRole", ""));
            if (r != null)
                roleList.Add(r);

            r = db.FetchUserRole(CmsConfig.getConfigValue("AuthorAccessUserRole", ""));
            if (r != null)
                roleList.Add(r);

            r = db.FetchUserRole(CmsConfig.getConfigValue("LoginUserRole", ""));
            if (r != null)
                roleList.Add(r);

            r = WebPortalUserRole.dummyPublicUserRole; // dummy public role, not from web portal DB
            roleList.Add(r);

            return roleList;
        }

        /// <summary>
        /// Render the Zone Authority interface
        /// </summary>
        /// <returns></returns>
        public override string Render()
        {
            string controlId = "zoneAuthority_";
            StringBuilder html = new StringBuilder();
            List<CmsPageSecurityZone> zoneList = db.fetchAll();
            List<WebPortalUserRole> roleList = getRoleList();
            html.Append(handleZoneAuthorityUpdate(zoneList, roleList));

            string adminRoleName = CmsConfig.getConfigValue("AdminUserRole", "?");
            if (zoneList.Count == 0)
                return "<p><em>No Zone defined</em></p>";

            CmsPage p = CmsContext.currentPage;
            html.Append(p.getFormStartHtml(controlId + "Form") + EOL);
            html.Append(TABLE_START_HTML + EOL);
            html.Append(RenderZoneAuthorityHeader(roleList));
            html.Append(RenderZoneAuthorityContent(zoneList, roleList, adminRoleName));
            html.Append(TABLE_END_HTML + EOL);
            html.Append("<input type=\"submit\" value=\"Update\" />" + EOL);
            html.Append(generateFormHiddenField());
            html.Append(p.getFormCloseHtml(controlId + "Form"));

            return html.ToString();
        }
    }
}
