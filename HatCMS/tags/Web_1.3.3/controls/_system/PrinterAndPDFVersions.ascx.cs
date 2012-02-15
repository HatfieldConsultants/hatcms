using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using HatCMS;

namespace HatCMS.controls._system
{
    public partial class PrinterAndPDFVersions : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        public CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();

            bool printerVer = CmsConfig.getConfigValue("PrinterAndPdfVer.printerVer", false);
            bool pdfVer = CmsConfig.getConfigValue("PrinterAndPdfVer.pdfVer", false);
            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.pdfVer"));
            if (pdfVer)
            {
                ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.pdfIcon"));
            }
            
            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.printerVer"));
            if (printerVer)
            {
                ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.printerIcon"));
            }

            ret.Add(new CmsConfigItemDependency("PrinterAndPdfVer.placeAfterDom", CmsDependency.ExistsMode.MustNotExist));

            // this file should not exist            
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/printerAndPdfVersion.js", CmsDependency.ExistsMode.MustNotExist ));
            return ret.ToArray();
        }


        protected override void Render(HtmlTextWriter writer)
        {
            bool printerVer = CmsConfig.getConfigValue("PrinterAndPdfVer.printerVer", false);
            bool pdfVer = CmsConfig.getConfigValue("PrinterAndPdfVer.pdfVer", false);
            if (printerVer == false && pdfVer == false)
                return;

            string _printerIcon = CmsConfig.getConfigValue("PrinterAndPdfVer.printerIcon", "").Replace("~", CmsContext.ApplicationPath);
            string _pdfIcon = CmsConfig.getConfigValue("PrinterAndPdfVer.pdfIcon", "").Replace("~", CmsContext.ApplicationPath);

            CmsPage currentPage = CmsContext.currentPage;
            

            if ( ! CmsContext.currentUserIsRequestingPrintFriendlyVersion) 
            {
                StringBuilder html = new StringBuilder();
                
                html.Append("<div class=\"PrinterPdfVersionLinks\">");
                if (printerVer)
                {
                    NameValueCollection printParams = new NameValueCollection();
                    printParams.Add(CmsContext.PrintFriendlyVersionFormName, "1");
                    string printUrl = currentPage.getUrl(printParams);

                    html.Append("<a style=\"margin-left: 5px;\" title=\"Printer-friendly version of this page\" target=\"_blank\" href=\"" + printUrl + "\" id=\"printerLinkButton\">");
                    html.Append("<img src=\""+_printerIcon+"\" border=\"0\">");
                    html.Append("</a>");
                }
                if (pdfVer)
                {
                    html.Append("<a style=\"margin-left: 5px;\" title=\"Download PDF of this page\" target=\"_blank\" href=\"/html2pdf/convert.php?URL=" + Server.UrlEncode(CmsContext.currentPage.Url) + "\" id=\"printerLinkButton\">");
                    html.Append("<img src=\""+_pdfIcon+"\" border=\"0\"></a>");
                    html.Append("</div>");
                }

                
                writer.Write(html.ToString());                
                                
            } 
        } // Render
    }
}