using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text;

using Hatfield.Web.Portal;

namespace HatCMS.WebEditor.Helpers
{
    public partial class PopupFlashObjectBrowser : System.Web.UI.Page
    {
        protected string JSCallbackFunctionName
        {
            get
            {
                return PageUtils.getFromForm("callback", "alert");
            }
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!CmsContext.currentUserIsLoggedIn)
            {
                Response.Write("Access denied");
                Response.End();
                return;
            }
            
            if (!this.IsPostBack)
            {
                TreeNode rootNode = new TreeNode("Flash Files",Server.MapPath(InlineImageBrowser2.UserFilesPath + "Flash/"));
                rootNode.Selected = true;
                rootNode.PopulateOnDemand = true;                
                FolderTreeView.Nodes.Add(rootNode);
                FolderTreeView_SelectedNodeChanged(this, null);
            }

            RegisterScrollToSelectedScript(FolderTreeView);
        }

        private void RegisterScrollToSelectedScript(TreeView treeView)
        {
            // http://www.developer.com/net/asp/article.php/3643956
            StringBuilder js = new StringBuilder();


            js.Append(" function ScrollToSelectedTreeNode()" + Environment.NewLine);
            js.Append("{ " + Environment.NewLine);
            js.Append("try " + Environment.NewLine);
            js.Append("{   " + Environment.NewLine);
            js.Append("var elem = document.getElementById('" + treeView.ClientID + "_SelectedNode');   " + Environment.NewLine);
            js.Append("if(elem != null )   " + Environment.NewLine);
            js.Append("{     " + Environment.NewLine);
            js.Append("var node = document.getElementById(elem.value);     " + Environment.NewLine);
            js.Append("if(node != null)     " + Environment.NewLine);
            js.Append("{       " + Environment.NewLine);
            js.Append("node.scrollIntoView(true); " + Environment.NewLine);
            // js.Append("Panel1.scrollLeft = 0;     "+Environment.NewLine);
            js.Append("}   " + Environment.NewLine);
            js.Append("} " + Environment.NewLine);
            js.Append("} " + Environment.NewLine);
            js.Append("catch(oException) " + Environment.NewLine);
            js.Append("{}" + Environment.NewLine);
            js.Append("}" + Environment.NewLine + Environment.NewLine);

            js.Append(CmsPageHeadSection.getOnloadJavascript("ScrollToSelectedTreeNode"));


            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ScrollToSelectedTreeNode", js.ToString(), true);
        }

        protected void FolderTreeView_TreeNodePopulate(object sender, TreeNodeEventArgs e)
        {
            string rootDir = e.Node.Value;
            DirectoryInfo rootDI = new System.IO.DirectoryInfo(rootDir);

            e.Node.ChildNodes.Clear();

            foreach (DirectoryInfo subDir in rootDI.GetDirectories())
            {

                if (CmsContext.currentUserIsSuperAdmin || !subDir.Name.StartsWith("_"))
                {
                    TreeNode node = new TreeNode(subDir.Name, subDir.FullName);
                    node.PopulateOnDemand = true;                    
                    e.Node.ChildNodes.Add(node);
                }
            } // foreach
            
        }

        public static string[] FlashFileFilters
        {
            get
            {
                return new string[] { "*.swf" };
            }
        }

        public static FileInfo[] GetFlashFiles(DirectoryInfo di)
        {
            ArrayList ret = new ArrayList();

            foreach (string filter in PopupFlashObjectBrowser.FlashFileFilters)
            {
                ret.AddRange(di.GetFiles(filter));
            } // foreach

            return (FileInfo[])ret.ToArray(typeof(FileInfo));
        }

        public static bool DirHasSWFFiles(DirectoryInfo di)
        {
            foreach (string filter in PopupFlashObjectBrowser.FlashFileFilters)
            {
                if (di.GetFiles(filter).Length > 0)
                    return true;
            }
            return false;
        }

        public static string getUrl(string JSCallbackFunctionName)
        {
            return CmsContext.ApplicationPath + "_system/tools/FlashObject/PopupFlashObjectBrowser.aspx?callback=" + System.Web.HttpContext.Current.Server.UrlPathEncode(JSCallbackFunctionName);
        }

        public static int PopupWidth = 500;
        public static int PopupHeight = 400;


        protected void FolderTreeView_SelectedNodeChanged(object sender, EventArgs e)
        {
            FolderTreeView.SelectedNode.Expand();
            DirectoryInfo di = new DirectoryInfo(FolderTreeView.SelectedNode.Value);
            FileInfo[] flashFileInfos = GetFlashFiles(di);

            StringBuilder html = new StringBuilder();
            if (flashFileInfos.Length < 1)
            {
                html.Append("the selected folder does not contain any flash files. select another directory");
            }
            else
            {
                foreach (FileInfo fi in flashFileInfos)
                {
                    // -- create the file's Url	
                    string fileUrl = InlineImageBrowser2.ReverseMapPath(fi.FullName);

                    // string thumbUrl = showThumbPage.getThumbDisplayUrl(fileUrl, 120, true);                    

                    string fiName = fi.Name;
                    if (fiName.IndexOf("'") > -1)
                    {
                        fiName = fi.Name.Replace("'", "\\'");
                        fileUrl = fileUrl.Replace("'", "\\'");
                    }

                    if (fileUrl.StartsWith("UserFiles/"))
                        fileUrl = fileUrl.Substring("UserFiles/".Length);

                    string onclick = "if (opener) { opener." + JSCallbackFunctionName + "('" + fileUrl + "','UserFiles/" + fileUrl + "'); } else { alert('fatal error: window has no opener!'); } window.close();";
                    html.Append(" <a href=\"#\" onclick=\"" + onclick + "\">");
                    html.Append(StringUtils.compactPath(fi.Name, 30));
                    html.Append("</a> ");

                } // foreach
            }

            ph_ImagePanel.Controls.Add(new LiteralControl(html.ToString()));
        }

        protected void b_DoFileUpload_Click(object sender, EventArgs e)
        {
            string msg = "";
            if (FolderTreeView.SelectedNode == null)
            {
                msg = "no folder selected. please try upload again.";                                
            }
            else if (fileUpload.PostedFile.FileName == "")
            {
                msg = "no file selected. please try upload again.";                
            }
            else if (Array.IndexOf(PopupFlashObjectBrowser.FlashFileFilters, "*" + Path.GetExtension(fileUpload.PostedFile.FileName).ToLower()) < 0)
            {
                msg = "uploaded file is not a flash file.";
            }
            else
            {
                string dirName = FolderTreeView.SelectedNode.Value;
                if (!dirName.EndsWith("\\"))
                    dirName += "\\";
                string targetFilename = dirName + Path.GetFileName(fileUpload.PostedFile.FileName);
                if (System.IO.File.Exists(targetFilename))
                {
                    msg = "filename already exists!";
                }
                else
                {
                    try
                    {
                        fileUpload.PostedFile.SaveAs(targetFilename);
                    }
                    catch (Exception ex)
                    {
                        msg = "Error when saving file to webserver.";
                    }
                }
            }

            if (msg != "")
            {
                Response.Write(msg);
                return;
            }

            // -- success: update the display
            FolderTreeView_SelectedNodeChanged(sender, e);
        }

        protected void b_CreateSubFolder_Click(object sender, EventArgs e)
        {
            string folderName = tb_subFolder.Text.Trim();
            string msg = "";
            if (FolderTreeView.SelectedNode == null)
            {
                msg = "no folder selected. please select a parent folder and try again.";
            }
            else if (folderName == "")
            {
                msg = "no sub-folder name was specified. Please try again.";
            }
            else
            {
                string parentDirName = FolderTreeView.SelectedNode.Value;
                if (!parentDirName.EndsWith("\\"))
                    parentDirName += "\\";
                string newDirName = parentDirName + folderName + "\\";
                if (System.IO.Directory.Exists(newDirName))
                {
                    msg = "the folder with the given name already exists. Please try again.";
                }
                else
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(newDirName);
                    }
                    catch (Exception ex)
                    {
                        msg = "there was a problem creating the new folder: "+ex.Message;
                    }
                }
            }

            if (msg != "")
            {
                Response.Write(msg);
                return;
            }

            // -- success: update the display
            tb_subFolder.Text = "";
            FolderTreeView_SelectedNodeChanged(sender, e);
            TreeNodeEventArgs treeNodeEventArgs = new TreeNodeEventArgs(FolderTreeView.SelectedNode);
            FolderTreeView_TreeNodePopulate(sender, treeNodeEventArgs);
        } // page_load
    }
}
