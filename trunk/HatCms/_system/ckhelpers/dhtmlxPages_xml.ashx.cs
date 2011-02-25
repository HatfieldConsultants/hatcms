using System;
using System.Xml;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using HatCMS.Placeholders;

namespace HatCMS._system.ckhelpers
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class dhtmlxPages_xml : IHttpHandler
    {
        XmlAttribute getAttribute(string name, string val, XmlDocument doc)
        {
            XmlAttribute ret = doc.CreateAttribute(name);
            ret.Value = val;
            return ret;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!CmsContext.currentUserIsLoggedIn)
            {
                context.Response.Write("Access denied");
                return;
            }

            // -- parameters
            int pageLevelToExpand = 1;
            string selectedUrl = "";
            if (context.Request.QueryString["currurl"] != null)
                selectedUrl = context.Request.QueryString["currurl"].ToString().Trim();

            
            
            
            bool isSuperAdmin = CmsContext.currentUserIsSuperAdmin;
            
            // http://dhtmlx.com/docs/products/dhtmlxTree/doc/guide.html
            // <?xml version='1.0' encoding='iso-8859-1'?> <tree id="0">  <item text="My Computer" id="1" child="1" im0="my_cmp.gif" im1="my_cmp.gif" im2="my_cmp.gif" call="true" select="yes">  <userdata name="system">true</userdata>  <item text="Floppy (A:)" id="11" child="0" im0="flop.gif" im1="flop.gif" im2="flop.gif"/>  <item text="Local Disk (C:)" id="12" child="0" im0="drv.gif" im1="drv.gif" im2="drv.gif"/>  </item>  <item text="Recycle Bin" id="4" child="0" im0="recyc.gif" im1="recyc.gif" im2="recyc.gif"/> </tree> 
            XmlDocument doc = new XmlDocument();
            XmlElement rootTreeEl = doc.CreateElement("tree");
            rootTreeEl.Attributes.Append(getAttribute("id", "0", doc));

            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "iso-8859-1", String.Empty);
            doc.InsertBefore(declaration, doc.DocumentElement);

            // -- show a tree of pages per-language if more than one language is in use.
            if (CmsConfig.Languages.Length > 1)
            {
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    rootTreeEl.AppendChild(getLangXmlTree(lang, doc, pageLevelToExpand, selectedUrl, isSuperAdmin));
                } // foreach language
            }
            else
            {
                rootTreeEl.AppendChild(ToXmlRecursive(CmsContext.HomePage, CmsContext.currentLanguage, doc, pageLevelToExpand, selectedUrl, isSuperAdmin));
            }

            doc.AppendChild(rootTreeEl);

            context.Response.ContentType = "text/xml";
            context.Response.Write(doc.OuterXml);
        }

        private XmlElement getLangXmlTree(CmsLanguage pageLanguage, XmlDocument doc, int pageLevelToExpand, string selectedUrl, bool isSuperAdmin)
        {
            XmlElement ret = doc.CreateElement("item");
            ret.Attributes.Append(getAttribute("id", "lang_" + pageLanguage.shortCode, doc));
            ret.Attributes.Append(getAttribute("text", pageLanguage.shortCode, doc));
            ret.Attributes.Append(getAttribute("call", "1", doc));
            ret.Attributes.Append(getAttribute("open", "1", doc));

            ret.AppendChild(ToXmlRecursive(CmsContext.HomePage, pageLanguage, doc, pageLevelToExpand, selectedUrl, isSuperAdmin));

            return ret;
        }

        private XmlElement ToXmlRecursive(CmsPage p, CmsLanguage pageLanguage, XmlDocument doc, int pageLevelToExpand, string selectedUrl, bool isSuperAdmin)
        {
            XmlElement ret = doc.CreateElement("item");
            ret.Attributes.Append(getAttribute("id", pageLanguage.shortCode + p.ID.ToString(), doc));
            string title = p.getMenuTitle(pageLanguage);
            if (title.Trim() == "")
                title = p.getTitle(pageLanguage);

            ret.Attributes.Append(getAttribute("text", title, doc));
            ret.Attributes.Append(getAttribute("call","1", doc));
            if (p.Level <= pageLevelToExpand)
                ret.Attributes.Append(getAttribute("open", "1", doc));

            // -- use a url Macro for links. This macro is replaced by the page's HtmlLinkFilter
            string urlMacro = HtmlLinkMacroFilter.getLinkMacro(p, pageLanguage);
            if (String.Compare(selectedUrl, p.getUrl(pageLanguage), true) == 0 || String.Compare(selectedUrl, urlMacro, true) == 0)
                ret.Attributes.Append(getAttribute("select", "1", doc));

            XmlElement url = doc.CreateElement("userdata");
            url.Attributes.Append(getAttribute("name", "url", doc));            

            XmlText urlText = doc.CreateTextNode(urlMacro);
            url.AppendChild(urlText);
            ret.AppendChild(url);
                        

            if (p.ChildPages.Length > 0)
            {
                ret.Attributes.Append(getAttribute("child", "1", doc));
                foreach (CmsPage childPage in p.ChildPages)
                {
                    if (childPage.isVisibleForCurrentUser && (isSuperAdmin || childPage.ShowInMenu))
                    {
                        ret.AppendChild(ToXmlRecursive(childPage, pageLanguage, doc, pageLevelToExpand, selectedUrl, isSuperAdmin));
                    }
                } // foreach
            }

            return ret;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}

