using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.setup;
using HatCMS.Core.Migration;


namespace HatCMS._system.tools
{
    public partial class UpgradePage : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            RequestState requeststate = new RequestState();
            HatCMSMigrator databasemigrator = new HatCMSMigrator();

            //this.updatecontent.InnerText = requeststate.Fetch("http://hatcms.googlecode.com/svn/HatCMS_DatabaseMigrationGenerator/version/version.txt");
            StringBuilder html = new StringBuilder();
            html.Append("<p style=\"color: red;\">The latest HatCMS database version is Version: ");
            html.Append(requeststate.Fetch("http://hatcms.googlecode.com/svn/HatCMS_DatabaseMigrationGenerator/version/version.txt"));
            html.Append("</p>");

            html.Append("<p style=\"color: red;\">The Current Database Version of the System is Version: ");
            html.Append(databasemigrator.currentVersion());
            html.Append("</p>");

            ph_ValidationErrors.Controls.Clear();
            ph_ValidationErrors.Controls.Add(new LiteralControl(html.ToString()));
        }

        private bool connectionStringMatches()
        {
            string configConnStr = ConfigurationManager.AppSettings["ConnectionString"].Trim();
            string enteredConnStr = tb_TestConnectionString.Text.Trim();

            if (string.Compare(configConnStr, enteredConnStr, true) == 0)
                return true;
            else
                return false;

        }

        protected void b_ValidateConfig_Click(object sender, EventArgs e)
        {
            StringBuilder html = new StringBuilder();
            if (!connectionStringMatches())
            {
                html.Append("<p style=\"color: red;\">You entered an incorrect Connection String.</p>");
            }
            else
            {
                CmsDependencyMessage[] msgs = setupPage.VerifyConfig();
                CmsDependencyMessage[] errorMessages = CmsDependencyMessage.GetAllMessagesByLevel(CmsDependencyMessage.MessageLevel.Error, msgs);
                if (errorMessages.Length == 0)
                {
                    html.Append("<p style=\"color: green;\">Configuration has been validated without errors</p>");
                }
                else
                {

                    html.Append("<div style=\"color: red;\">The following errors were found in your configuration: </div>");
                    html.Append("<ul>");

                    foreach (CmsDependencyMessage m in errorMessages)
                    {
                        html.Append("<li>" + m.Message + "</li>");
                    }
                    html.Append("</ul>");

                }
            }

            ph_ValidationErrors.Controls.Clear();
            ph_ValidationErrors.Controls.Add(new LiteralControl(html.ToString()));

        }

        protected void b_UpdateDatabase_Click(object sender, EventArgs e)
        {
            HatCMSMigrator databasemigrator = new HatCMSMigrator();
            StringBuilder html = new StringBuilder();
            if (databasemigrator.IsOutDated())
            {
                html.Append("<p style=\"color: red;\">Your database needs update.</p>");

            }
            else
            {
                html.Append("<p style=\"color: red;\">Your have the latest database.</p>");
            }

            ph_ValidationErrors.Controls.Clear();
            ph_ValidationErrors.Controls.Add(new LiteralControl(html.ToString()));


        }
    }
}
