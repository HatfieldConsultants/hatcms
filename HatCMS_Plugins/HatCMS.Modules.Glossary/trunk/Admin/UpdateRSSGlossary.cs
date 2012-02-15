using System;
using System.Text;
using System.Collections.Generic;
using System.Text;
using HatCMS;
using HatCMS.Admin;

namespace HatCMS.Modules.Glossary
{
    public class UpdateRSSGlossary : BaseCmsAdminTool
    {
        public override string Render()
        {
            StringBuilder html = new StringBuilder();
            if (GlossaryPlaceholderData.DataSource != GlossaryPlaceholderData.GlossaryDataSource.RssFeed)
            {
                html.Append(base.formatErrorMsg("The Glossary data is currently hosted from the Local Database. To update the glossary, edit the page with the glossary placeholder in it"));
            }
            else
            {
                html.Append("<p>");
                string url = GlossaryPlaceholderData.getRssDataSourceUrl();
                html.Append(base.formatNormalMsg("Fetching updated glossary from <a href=\"" + url + "\" target=\"_blank\">" + url + "</a>"));

                try
                {
                    // -- run the background task in the foreground thread to update the database.
                    HatCMS.Modules.Glossary.BackgroundTasks.FetchUpdatedRSSGlossaryItems backgroundFetcher = new HatCMS.Modules.Glossary.BackgroundTasks.FetchUpdatedRSSGlossaryItems();
                    if (backgroundFetcher.FetchAndSaveRemoteRSSGlossaryData())
                    {
                        // -- fetch the data from the database.
                        GlossaryDb db = new GlossaryDb();
                        GlossaryData[] items = db.FetchRssFeedGlossaryDataFromDatabase();
                        html.Append(base.formatNormalMsg( items.Length + " glossary entries are now available."));
                    }
                    else
                    {
                        html.Append(base.formatErrorMsg("Exception: could not update the glossary from the external URL."));
                    }
                }
                catch(Exception ex)
                {
                    html.Append(base.formatErrorMsg("Exception: could not update the glossary from the external URL: " + ex.Message));
                }
                
                html.Append("</p>");
            }

            return html.ToString();
        }

        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Tool_Utility, AdminMenuTab.Tools, "Refresh External Glossary Data");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            return ret.ToArray();
        }

        public override System.Web.UI.WebControls.GridView RenderToGridViewForOutputToExcelFile()
        {
            return null; // not implemented
        }
    }
}
