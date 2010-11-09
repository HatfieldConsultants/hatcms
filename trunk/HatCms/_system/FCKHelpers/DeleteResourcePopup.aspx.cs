using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS.FCKHelpers
{
    public partial class DeleteResourcePopup : System.Web.UI.Page
    {
        public bool ResourceHasBeenDeleted = false;

        public string FileUrl
        {
            get
            {
                return PageUtils.getFromForm("FileUrl", "");
            }
        }

        public bool DoPageSearch
        {
            get
            {
                return PageUtils.getFromForm("DoPageSearch", false);
            }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CmsContext.currentUserIsSuperAdmin)
            {
                Response.Write("Access denied");
                Response.End();
                return;
            }

            if (FileUrl == "")
            {
                Response.Write("No file specified");
                Response.End();
                return;
            }
            
            
        }

        protected void b_DoDelete_Click(object sender, EventArgs e)
        {
            string filename = Server.MapPath(FileUrl);

            CmsResource resources = CmsResource.GetResourceByFilename(filename);
            bool resDeleted = false;
            if (CmsResource.Delete(resources, true))
                resDeleted = true;


            if (!resDeleted)
            {
                try
                {
                    System.IO.File.Delete(filename);
                    ResourceHasBeenDeleted = true;
                }
                catch
                { }
                ResourceHasBeenDeleted = false;
            }
            else
                ResourceHasBeenDeleted = true;


        }

        public void OutputPageLinks()
        {
            bool forcePageSearch = true;



            StringBuilder html = new StringBuilder();

            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            if (!forcePageSearch && !DoPageSearch)
            {
                html.Append("<p><a onclick=\"document.getElementById('spinnerImg').display='block'; return true;\" href=\"DeleteResourcePopup.aspx?DoPageSearch=true&FileUrl=" + FileUrl + "\">Search entire site (" + allPages.Keys.Count + " pages) for this link</a> (slow!)");
                html.Append(" <img style=\"display: none\" id=\"spinnerImg\" src=\"" + CmsContext.ApplicationPath + "images/_system/ajax-loader_16x16.gif\">");
                html.Append("</p>");
            }
            else
            {
                int numPagesFound = 0;
                
                html.Append("<strong>This link has been found on the following pages:</strong>");
                html.Append("<ul>");
                foreach (CmsPage pageToSearch in allPages.Values)
                {
                    foreach (CmsLanguage lang  in CmsConfig.Languages)
                    {                        
                        string[] linksToFind = new string[] { FileUrl };
                        string phContent = pageToSearch.renderAllPlaceholdersToString(lang);
                        string[] linksInPage = ContentUtils.FindFileLinksInHtml(phContent, linksToFind);

                        if (linksInPage.Length > 0)
                        {
                            numPagesFound++;
                            html.Append("<li><a href=\"" + pageToSearch.getUrl(CmsUrlFormat.FullIncludingProtocolAndDomainName, lang) + "\" target=\"_blank\">" + pageToSearch.getTitle(lang) + "</a> (" + pageToSearch.getPath(lang) + ")</li>" + Environment.NewLine);
                        }
                    } // foreach language
                } // foreach page
                
                html.Append("</ul>");

                if (numPagesFound == 0)
                {                    
                    html.Append("<p><strong>" + allPages.Keys.Count + " pages have been searched, and this link has not been found</strong></p>");
                }
                else
                {
                    html.Append("<p><strong>" + allPages.Keys.Count + " pages have been searched, and this link has been found on " + numPagesFound + " pages</strong></p>");
                }

            }
            Response.Write(html.ToString());
        }
    }
}
