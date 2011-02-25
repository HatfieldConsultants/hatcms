using System;
using System.IO;
using System.Xml;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using HatCMS.WebEditor.Helpers;

namespace HatCMS._system.ckhelpers
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class dhtmlxFiles_xml : IHttpHandler
    {
        XmlAttribute getAttribute(string name, string val, XmlDocument doc)
        {
            XmlAttribute ret = doc.CreateAttribute(name);
            ret.Value = val;
            return ret;
        }

        private bool listFileOrDir(FileAttributes attributes)
        {
            if ((attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                    (attributes & FileAttributes.System) != FileAttributes.System)
                return true;            
            return false;
        }

        public static string UserFilesPath
        {
            get
            {

                return InlineImageBrowser2.UserFilesPath;
            }
        }

        public static string getUrl(FileInfo fi, HttpContext context)
        {
            string rootUserFilesDir = context.Server.MapPath(UserFilesPath);

            string subDir = fi.Directory.FullName.Replace(rootUserFilesDir, "");

            subDir = UserFilesPath + subDir;
            if (!subDir.EndsWith("\\"))
                subDir += "\\";

            string fileUrl = subDir + fi.Name;

            fileUrl = fileUrl.Replace("\\", "/");

            fileUrl = fileUrl.Replace("//", "/");

            return fileUrl;
        }

        public static string getDirUrl(DirectoryInfo di, HttpContext context)
        {
            string rootUserFilesDir = context.Server.MapPath(UserFilesPath);

            string subDir = di.FullName.Replace(rootUserFilesDir, "");

            subDir = UserFilesPath + subDir;
            if (!subDir.EndsWith("\\"))
                subDir += "\\";

            string fileUrl = subDir;

            fileUrl = fileUrl.Replace("\\", "/");

            fileUrl = fileUrl.Replace("//", "/");

            return fileUrl;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (!CmsContext.currentUserIsLoggedIn)
            {
                context.Response.Write("Access denied");
                return;
            }

            // -- parameters
            int pageLevelToExpand = 0;
            string selectedUrl = "";
            if (context.Request.QueryString["currurl"] != null)
            {
                selectedUrl = context.Request.QueryString["currurl"].ToString().Trim();
                selectedUrl = context.Server.UrlDecode(selectedUrl);
            }

            bool isSuperAdmin = CmsContext.currentUserIsSuperAdmin;
            
            // http://dhtmlx.com/docs/products/dhtmlxTree/doc/guide.html
            // <?xml version='1.0' encoding='iso-8859-1'?> <tree id="0">  <item text="My Computer" id="1" child="1" im0="my_cmp.gif" im1="my_cmp.gif" im2="my_cmp.gif" call="true" select="yes">  <userdata name="system">true</userdata>  <item text="Floppy (A:)" id="11" child="0" im0="flop.gif" im1="flop.gif" im2="flop.gif"/>  <item text="Local Disk (C:)" id="12" child="0" im0="drv.gif" im1="drv.gif" im2="drv.gif"/>  </item>  <item text="Recycle Bin" id="4" child="0" im0="recyc.gif" im1="recyc.gif" im2="recyc.gif"/> </tree> 
            XmlDocument doc = new XmlDocument();
            XmlElement rootTreeEl = doc.CreateElement("tree");
            rootTreeEl.Attributes.Append(getAttribute("id", "0", doc));

            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "iso-8859-1", String.Empty);
            doc.InsertBefore(declaration, doc.DocumentElement);

            DirectoryInfo userDir = new DirectoryInfo(context.Server.MapPath(UserFilesPath));
            foreach(DirectoryInfo di in userDir.GetDirectories())
            {
                rootTreeEl.AppendChild(ToXmlRecursive(di,0, doc, pageLevelToExpand, selectedUrl, isSuperAdmin, context));
            }

            doc.AppendChild(rootTreeEl);

            context.Response.ContentType = "text/xml";
            context.Response.Write(doc.OuterXml);
        }

        private XmlElement ToXmlRecursive(DirectoryInfo dir, int level, XmlDocument doc, int pageLevelToExpand, string selectedUrl, bool isSuperAdmin, HttpContext context)
        {
            FileInfo[] files = dir.GetFiles();
            
            XmlElement ret = doc.CreateElement("item");
            ret.Attributes.Append(getAttribute("id", dir.GetHashCode().ToString(), doc));                     

            ret.Attributes.Append(getAttribute("text", dir.Name, doc));            
            if (level <= pageLevelToExpand)
                ret.Attributes.Append(getAttribute("open", "1", doc));

            string dirUrl = getDirUrl(dir, context);
            XmlElement dirPath = doc.CreateElement("userdata");
            dirPath.Attributes.Append(getAttribute("name", "dirurl", doc));
            XmlText pathText = doc.CreateTextNode(dirUrl);
            dirPath.AppendChild(pathText);
            ret.AppendChild(dirPath);

            if (String.Compare(selectedUrl, dirUrl, true) == 0)
                ret.Attributes.Append(getAttribute("select", "1", doc));

            // newly created folders appear a common nodes, so force folders to look like folders.
            ret.Attributes.Append(getAttribute("im0", "folderClosed.gif", doc));

            foreach (FileInfo fi in files)
            {
                if (listFileOrDir(fi.Attributes))
                {
                    ret.AppendChild(elForFile(fi, doc, selectedUrl, context));
                }
            } // foreach files
            
            

            DirectoryInfo[] subDirs = dir.GetDirectories();
            if (subDirs.Length > 0)
            {
                ret.Attributes.Append(getAttribute("child", "1", doc));
                foreach (DirectoryInfo subDir in subDirs)
                {
                    if (isSuperAdmin || !subDir.Name.StartsWith("_") && (listFileOrDir(subDir.Attributes)))
                    {
                        ret.AppendChild(ToXmlRecursive(subDir, level+1, doc, pageLevelToExpand, selectedUrl, isSuperAdmin, context));
                    }
                } // foreach
            }

            return ret;
        }

        private XmlElement elForFile(FileInfo fi, XmlDocument doc, string selectedUrl, HttpContext context)
        {
            XmlElement ret = doc.CreateElement("item");
            ret.Attributes.Append(getAttribute("id", fi.GetHashCode().ToString(), doc));            

            ret.Attributes.Append(getAttribute("text", fi.Name, doc));
            ret.Attributes.Append(getAttribute("call", "1", doc));

            string fileUrl = getUrl(fi, context);
            if (String.Compare(selectedUrl, fileUrl, true) == 0)
                ret.Attributes.Append(getAttribute("select", "1", doc));

            string dirUrl = getDirUrl(fi.Directory, context);

            XmlElement url = doc.CreateElement("userdata");
            url.Attributes.Append(getAttribute("name", "url", doc));
            XmlText urlText = doc.CreateTextNode(fileUrl);
            url.AppendChild(urlText);

            XmlElement dirPath = doc.CreateElement("userdata");
            dirPath.Attributes.Append(getAttribute("name", "dirurl", doc));
            XmlText pathText = doc.CreateTextNode(dirUrl);
            dirPath.AppendChild(pathText);

            ret.AppendChild(url);
            ret.AppendChild(dirPath);

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

