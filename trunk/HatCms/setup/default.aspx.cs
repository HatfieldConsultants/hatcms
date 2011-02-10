using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Reflection;
using Hatfield.Web.Portal;

namespace HatCMS.setup
{
	/// <summary>
	/// Summary description for _default.
	/// </summary>
	public partial class setupPage : System.Web.UI.Page
	{
        private string RedirectTemplateName = "_Redirect";
        
        /// <summary>
        /// Gets the dependencies for the SetupPage.
        /// </summary>
        /// <returns></returns>
        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            ret.Add(CmsFileDependency.UnderAppPath("css/_system/Setup.css"));
            ret.Add(new CmsTemplateDependency(RedirectTemplateName));
            ret.Add(new CmsTemplateDependency("_login"));
            ret.Add(new CmsTemplateDependency("internal/_gotoEditMode"));
            ret.Add(new CmsTemplateDependency("internal/_gotoViewMode"));
            ret.Add(new CmsTemplateDependency("internal/_CreateNewPagePopup"));                        

            return ret.ToArray();
        }

		protected void Page_Load(object sender, System.EventArgs e)
		{
            
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion

		protected DataTable RunQuery(MySqlConnection conn, string sql, MySqlTransaction transaction)
		{
			DataTable dataTable = new DataTable();			

			MySqlDataAdapter sqlDA = new MySqlDataAdapter();
			sqlDA.SelectCommand = new MySqlCommand(sql, conn, transaction);

			sqlDA.Fill( dataTable );				
			
			return dataTable;
			

		} // RunQuery


        protected void b_db_Click(object sender, System.EventArgs e)
        {
            string connStr = "server=" + db_host.Text + ";uid=" + db_un.Text + ";pwd=" + db_pw.Text + ";"; // database=hatcms;
            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
            }
            catch (Exception err)
            {
                l_msg.Text = "Installation Failed : " + err.Message;
                return;
            }
            MySqlTransaction trans = conn.BeginTransaction();

            try
            {
                
                string createDBsql = "CREATE DATABASE `" + tb_DbName.Text.Trim() + "` /*!40100 DEFAULT CHARACTER SET utf8 */;";
                RunQuery(conn, createDBsql, trans);


                conn.ChangeDatabase(tb_DbName.Text.Trim());

                string SQLTableSetupFilename = Server.MapPath("~/setup/HatCMS_TableCreation.sql");
                string sqlTableSetup = System.IO.File.ReadAllText(SQLTableSetupFilename);
                RunQuery(conn, sqlTableSetup, trans);                               

                trans.Commit();
                conn.Close();

                b_db.Enabled = false;

                if (!connStr.EndsWith(";"))
                    connStr += ";";
                connStr += "database=" + tb_DbName.Text.Trim() + ";";

                l_msg.Text = "Step 1 Completed Successfully: the Database \""+tb_DbName.Text.Trim()+"\" has been created, and tables have been created.";
                l_NewConnStr.Text = "Edit the web.config file and set ConnectionString to \"<strong>" + connStr + "</strong>\"";
            }
            catch (Exception ex)
            {
                trans.Rollback();
                conn.Close();
                l_msg.Text = "database setup FAILED : " + ex.Message;
            }

        }

        private int InsertPage(string filename, string title, string menuTitle, string seDesc, string templateName, int parentPageId, int sortOrdinal, bool showInMenu)
        {
            int parentId = Convert.ToInt32(parentPageId);
            CmsPage newPage = new CmsPage();

            // -- setup the page's language info            
            List<CmsPageLanguageInfo> langInfos = new List<CmsPageLanguageInfo>();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                CmsPageLanguageInfo langInfo = new CmsPageLanguageInfo();
                langInfo.languageShortCode = lang.shortCode;
                langInfo.name = filename;
                langInfo.menuTitle = menuTitle;
                langInfo.title = title;
                langInfo.searchEngineDescription = seDesc;

                langInfos.Add(langInfo);
            } // foreach languages
            newPage.LanguageInfo = langInfos.ToArray();

            newPage.ShowInMenu = showInMenu;
            newPage.ParentID = parentId;
            newPage.TemplateName = templateName;
            newPage.ShowInMenu = showInMenu;

            newPage.SortOrdinal = 0;
            if (sortOrdinal < 0)
            {
                // -- set sortOrdinal
                int highestSiblingSortOrdinal = -1;
                if (parentId >= 0)
                {
                    CmsPage parentPage = CmsContext.getPageById(parentId);
                    foreach (CmsPage sibling in parentPage.ChildPages)
                    {
                        highestSiblingSortOrdinal = Math.Max(sibling.SortOrdinal, highestSiblingSortOrdinal);
                    }
                }
                if (highestSiblingSortOrdinal > -1)
                    newPage.SortOrdinal = highestSiblingSortOrdinal + 1;                                    
            }

            if (CmsContext.childPageWithNameExists(parentId, filename))
            {
                // _errorMessage = "a page with the specified filename and parent already exists!";
                return -1;
            }
            else
            {
                // -- page does not already exist, so create it
                CmsPageDb db = new CmsPageDb();
                bool success = db.createNewPage(newPage);
                if (!success)
                {
                    // _errorMessage = "database could not create new page.";
                    return -1;
                }
                return newPage.ID;
            }

        }

        /// <summary>
        /// Create the default zone and zone user role during setup.
        /// </summary>
        /// <returns></returns>
        private void InsertZone(int HomePageId)
        {
            CmsZone z = new CmsZone();
            z.ZoneName = "Default zone";
            z.StartingPageId = HomePageId;
            if (new CmsZoneDb().insert(z) == false)
                throw new Exception("Cannot insert Zone");

            CmsZoneUserRole r = new CmsZoneUserRole(z.ZoneId, WebPortalUserRole.DUMMY_PUBLIC_ROLE_ID, true, false);
            if (new CmsZoneUserRoleDb().insert(r) == false)
                throw new Exception("Cannot insert ZoneUserRole");
        }

        public class ConfigValidationMessage
        {
            public ConfigValidationMessage(bool IsValid, string Message)
            {
                isValid = IsValid;
                message = Message;
            }
            public bool isValid = false;
            public string message = "";  
          
            public static bool AllAreValid(ConfigValidationMessage[] msgs)
            {
                foreach (ConfigValidationMessage m in msgs)
                {
                    if (!m.isValid)
                        return false;
                } // foreach
                return true;
            }

            public static ConfigValidationMessage[] getAllInvalidMessages(ConfigValidationMessage[] msgs)
            {
                List<ConfigValidationMessage> ret = new List<ConfigValidationMessage>();
                foreach (ConfigValidationMessage m in msgs)
                {
                    if (!m.isValid)
                        ret.Add(m);
                }
                return ret.ToArray();
            }
        }
        
        

        public static ConfigValidationMessage[] VerifyConfig(System.Web.HttpContext Context)
        {
            List<ConfigValidationMessage> ret = new List<ConfigValidationMessage>();
            try
            {                
                ret.AddRange(testAllDependencies());

            }
            catch (Exception ex)
            {
                ret.Add(new ConfigValidationMessage(false, "Error validating configuration: " + ex.Message));
            }            

            return ret.ToArray();
        }


        private static ConfigValidationMessage[] testAllDependencies()
        {
            List<CmsDependencyMessage> dMsgs = new List<CmsDependencyMessage>();
            CmsDependency[] dependencies = CmsDependencies.GatherAllDependencies();
            foreach (CmsDependency d in dependencies)
            {
                try
                {
                    dMsgs.AddRange(d.ValidateDependency());
                }
                catch (Exception ex)
                {
                    dMsgs.Add(CmsDependencyMessage.Error(ex));
                }
            }
                        
            return CmsDependencyMessage.ToConfigValidationMessages(dMsgs.ToArray());
            
        }

		protected void b_verifyConfig_Click(object sender, System.EventArgs e)
		{
            ConfigValidationMessage[] messages = VerifyConfig(Context);
            ConfigValidationMessage[] errorMessages = ConfigValidationMessage.getAllInvalidMessages(messages);
            if (errorMessages.Length == 0)
            {
                l_msg.Text = "<div style=\"color: green;\">Configuration has been validated without errors</div>";                
            }
            else
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("<div style=\"color: red;\">Errors found in the configuration: </div>");
                msg.Append("<ul>");

                foreach (ConfigValidationMessage m in errorMessages)
                {
                    msg.Append("<li>" + m.message + "</li>");
                }
                msg.Append("</ul>");
                l_msg.Text = msg.ToString();                            
            }
		}

        protected void b_CreatePages_Click(object sender, EventArgs e)
        {
            try
            {
                

                // home page 
                int HomePageId = InsertPage("", "Home Page", "Home Page", "", "HomePage", 0, 0, true);

                // _Admin Page (hidden)
                int AdminPageId = InsertPage("_admin", "HatCMS Administration", "Admin", "", RedirectTemplateName, HomePageId, 0, false);

                // -- redirect the admin page to the home page.
                InsertRedirectPlaceholder(CmsContext.getPageById(AdminPageId), 1, "~/");

                //# /_admin/Login Page
                InsertPage("Login", "Login", "Login", "", "_login", AdminPageId, 0, false);


                //# Admin Actions Page 

                int AdminActionsPageId = InsertPage("actions", "Admin Actions", "Admin Actions", "", RedirectTemplateName, AdminPageId, -1, false);

                // -- redirect the admin actions page to the home page.
                InsertRedirectPlaceholder(CmsContext.getPageById(AdminActionsPageId), 1, "~/");


                //# Toggle Edit Admin Action Page
                InsertPage("gotoEdit", "Goto Edit Mode", "Goto Edit Mode", "", "internal/_gotoEditMode", AdminActionsPageId, -1, false);

                InsertPage("gotoView", "Goto View Mode", "Goto View Mode", "", "internal/_gotoViewMode", AdminActionsPageId, -1, false);


                //# /_admin/actions/createPage
                InsertPage("createPage", "Create Page", "Create Page", "", "internal/_CreateNewPagePopup", AdminActionsPageId, -1, false);


                // # Delete Page Admin Action Page
                InsertPage("deletePage", "Delete Page", "Delete Page", "", "internal/_DeletePagePopup", AdminActionsPageId, -1, false);


                //# Sort Sub Pages Admin Action Page
                InsertPage("sortSubPages", "Sort Sub Pages", "Sort Sub Pages", "", "internal/_SortSubPagesPopup", AdminActionsPageId, -1, false);

                //# Change Menu Visibiity (Show In Menu indicator) Admin Action Page
                InsertPage("MenuVisibilityPopup", "Change Menu Visibility", "Change Menu Visibility", "", "internal/_MenuVisibilityPopup", AdminActionsPageId, -1, false);

                // /_admin/actions/movePage
                InsertPage("movePage", "Move Page", "Move Page", "", "internal/_MovePagePopup", AdminActionsPageId, -1, false);


                // /_admin/actions/renamePage
                InsertPage("renamePage", "Rename Page", "Rename Page", "", "internal/_RenamePagePopup", AdminActionsPageId, -1, false);

                // /_admin/actions/deleteJob
                InsertPage("deleteJob", "Delete a Job Posting", "Delete a Job Posting", "", "internal/_DeleteJobPopup", AdminActionsPageId, -1, false);

                // /_admin/actions/killLock
                InsertPage("killLock", "Kill Edit Page Lock", "Kill Edit Page Lock", "", "internal/_KillLockPopup", AdminActionsPageId, -1, false);


                // /_admin/actions/changeTemplate
                InsertPage("changeTemplate", "Change Page's Template", "Change Page's Template", "", "internal/_ChangePageTemplatePopup", AdminActionsPageId, -1, false);


                // /_admin/actions/deleteFileLibrary
                InsertPage("deleteFileLibrary", "Delete a file library", "Delete a file library", "", "internal/_DeleteFileLibraryPopup", AdminActionsPageId, -1, false);

                // --------------------------------
                // /_Internal Page 
                int InternalPageId = InsertPage("_internal", "Internal CMS Functions", "Internal CMS Functions", "", RedirectTemplateName, HomePageId, -1, false);

                // -- redirect the /_internal page to the home page.
                InsertRedirectPlaceholder(CmsContext.getPageById(InternalPageId), 1, "~/");

                //# Show Single Image page (/_internal/showImage)
                InsertPage("showImage", "Show Image", "Show Image", "", "_SingleImageDisplay", InternalPageId, -1, false);


                //# Admin Tools page (/_admin/Audit)
                InsertPage("Audit", "Administration Tools", "Admin Tools", "", "internal/_AuditPopup", AdminPageId, -1, false);

                //# view revisions page (/_admin/ViewRevisions)
                InsertPage("ViewRevisions", "View Page Revisions", "View Page Revisions", "", "internal/_PageRevisionsPopup", AdminPageId, -1, false);

                //# EditUsers page (/_admin/EditUsers)
                InsertPage("EditUsers", "Edit Users", "Edit Users", "", "internal/_EditUsersPopup", AdminPageId, -1, false);

                // edit job location page
                InsertPage("JobLocation", "Job Location", "Job Location", "", "internal/_JobLocationPopup", AdminPageId, -1, false);

                // edit event calendar category page
                InsertPage("EventCalendarCategory", "Event Calendar Category", "Event Calendar Category", "", "internal/_EventCalendarCategoryPopup", AdminPageId, -1, false);

                // edit File Library category page
                InsertPage("FileLibraryCategory", "File Library Category", "File Library Category", "", "internal/_FileLibraryCategoryPopup", AdminPageId, -1, false);

                // delete File Library page
                InsertPage("deleteFileLibrary", "Delete File Library", "Delete File Library", "", "internal/_DeleteFileLibraryPopup", AdminPageId, -1, false);

				// create the default zone and zone user role
                InsertZone(HomePageId);

                l_msg.Text = "All standard pages have been added successfully.";
            }
            catch (Exception ex)
            {
                l_msg.Text = "Error: Standard Pages could not all be added. The state of the database is currently unknown. Please manually delete the database and start again.";
            }
        } // b_db_Click

        private void InsertRedirectPlaceholder(CmsPage targetPage, int identifier, string targetUrl)
        {
            HatCMS.Placeholders.PageRedirectDb redirectDB = new HatCMS.Placeholders.PageRedirectDb();
            foreach (CmsLanguage lang in CmsConfig.Languages)
            {
                redirectDB.createNewPageRedirect(targetPage, identifier, lang.shortCode, targetUrl);
            }

        }


	}
}
