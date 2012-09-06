using System;
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
using System.IO;
using System.Text;
using Hatfield.Web.Portal;

namespace HatCMS.WebEditor.Helpers
{
    public partial class InlineImageBrowser2 : System.Web.UI.Page
    {

        #region Static functions used by other classes
        
        public static string[] ImageFileFilters
        {
            get
            {
                return new string[] { "*.jpg", "*.gif", "*.png", "*.jpe", "*.jpeg" };
            }
        }
        

        public static FileInfo[] GetImageFiles(DirectoryInfo di)
        {
            List<FileInfo> ret = new List<FileInfo>();

            foreach (string filter in ImageFileFilters)
            {
                ret.AddRange(di.GetFiles(filter));
            } // foreach

            return ret.ToArray();
        }

        public static bool DirHasImageFiles(DirectoryInfo di)
        {
            foreach (string filter in ImageFileFilters)
            {
                if (di.GetFiles(filter).Length > 0)
                    return true;
            }
            return false;
        }

        public static string ReverseMapPath(string PhysicalFilePath)
        {            
            string rootPath = System.Web.Hosting.HostingEnvironment.MapPath(CmsContext.ApplicationPath);
            string url = PathUtils.RelativePathTo(rootPath, PhysicalFilePath);

            if (url.StartsWith("..\\"))
                url = url.Substring(2); // remove ".."

            url = url.Replace("\\", "/");
            return url;
        }

        /// <summary>
        /// on error, returns an 2 item array with both items set to -1 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static int[] getImageDimensions(string filename)
        {
            try
            {
                System.Drawing.Image img = System.Drawing.Bitmap.FromFile(filename);
                int w = img.Width;
                int h = img.Height;
                img.Dispose();
                return new int[] { w, h };
            }
            catch (Exception ex)
            { Console.Write(ex.Message); }

            return new int[] { -1, -1 };

        }

        public static string FolderOpenIconUrl
        {
            get
            {
                return CmsContext.ApplicationPath + "js/_system/FCKEditor/editor/filemanager/browser/default/images/FolderOpened.gif";
            }
        }

        public static string DefaultFileIconUrl
        {
            get
            {
                return CmsContext.ApplicationPath + "js/_system/FCKEditor/editor/filemanager/browser/default/images/icons/default.icon.gif";
            }
        }

        #endregion

        public bool IsCKEditor
        {
            get
            {
                if (Request.QueryString["ck"] != null && Request.QueryString["ck"].ToString() == "1")
                    return true;
                else
                    return false;
            }
        }

        public string SelImagePath
        {
            get
            {
                return PageUtils.getFromForm("SelImagePath", "");
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

            // -- if the user can't write, hide controls that allow files to be written
            if (!CmsContext.currentUserCanWriteToUserFilesOnDisk)
            {
                b_CreateSubFolder.Visible = false;
                tb_subFolder.Visible = false;
            }

            RegisterScrollToSelectedScript();
            RegisterCurrentlySelectedImageScript();

            if (!this.IsPostBack)
            {
                TreeNode rootNode = new TreeNode("Images",Server.MapPath(CmsConfig.UserFilesPath + "Image/"));
                rootNode.Selected = true;
                rootNode.PopulateOnDemand = true;                
                FolderTreeView.Nodes.Add(rootNode);
                rootNode.Expand();

                string currfile = PageUtils.getFromForm("SelImagePath", "");
                if (currfile.Trim() != "")
                {
                    if (currfile.StartsWith(CmsContext.ApplicationPath+"UserFiles/Image/"))
                        currfile = currfile.Substring((CmsContext.ApplicationPath + "UserFiles/Image/").Length);
                    else if (currfile.StartsWith("UserFiles/Image/"))
                        currfile = currfile.Substring("UserFiles/Image/".Length);

                    string[] currfile_parts = currfile.Split(new char[] { '/' });
                    TreeNode parentNode = rootNode;
                    for(int i=0; i< currfile_parts.Length-1; i++)                    
                    {
                        string dir = currfile_parts[i];
                        foreach (TreeNode childNode in parentNode.ChildNodes)
                        {
                            if (String.Compare(childNode.Text, dir, true) == 0)
                            {
                                parentNode = childNode;                                
                                parentNode.Expand();
                                parentNode.Selected = true;
                                break;
                            }
                        } // foreach
                    } // foreach
                    
                } // if
                // -- update the panel display
                FolderTreeView_SelectedNodeChanged(this, null);
            }
        }

        public string treeHeight()
        {
            string ret = "320px";
            /*
            if (IsCKEditor)
                ret = "250px";
             */
            return ret;
        }

        private void RegisterCurrentlySelectedImageScript()
        {
            StringBuilder js = new StringBuilder();
            js.Append(" var selImgId = '';" + Environment.NewLine);
            js.Append(" function doSelImg(newId) { " + Environment.NewLine);
            js.Append("  if (selImgId != '') { document.getElementById(selImgId).style.border = '2px solid white'; }" + Environment.NewLine);
            js.Append("  var newIdEl = document.getElementById(newId); " + Environment.NewLine);
            js.Append("  newIdEl.style.border = '2px solid red';" + Environment.NewLine);
            // js.Append("  newIdEl.scrollIntoView(false);" + Environment.NewLine);
            js.Append("  selImgId = newId; " + Environment.NewLine);
            js.Append("} " + Environment.NewLine);

            js.Append(" function ckSelImg(newId, imgUrl, w, h) { " + Environment.NewLine);
            js.Append("     doSelImg(newId); " + Environment.NewLine);
            js.Append("     window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'txtUrl', encodeURI(imgUrl));" + Environment.NewLine);
            js.Append("     window.parent.CKEDITOR.dialog.getCurrent().setValueOf('advanced','appPath','" + PageUtils.ApplicationPath + "');" + Environment.NewLine);

            int maxWidth = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayWidth", -1);
            int maxHeight = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayHeight", -1);            

            js.Append("     if (w > " + maxWidth + ") {" + Environment.NewLine);
            js.Append("         var i = new Image(); i.src=imgUrl; i.onload = function(){ "+Environment.NewLine);
            js.Append("             window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'txtWidth', '" + maxWidth + "');" + Environment.NewLine);
            js.Append("             window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'txtHeight', Math.round((" + maxWidth + "*h)/w));" + Environment.NewLine); 
            js.Append("         };"+Environment.NewLine);
            js.Append("     } else if (h > " + maxHeight + ") {" + Environment.NewLine);
            js.Append("         var i = new Image(); i.src=imgUrl; i.onload = function(){" + Environment.NewLine);
            js.Append("             window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'txtHeight', '" + maxHeight + "');" + Environment.NewLine);
            js.Append("             window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'txtWidth', Math.round((" + maxHeight + "*w)/h));" + Environment.NewLine); 
            js.Append("         };" + Environment.NewLine);
            js.Append("     }" + Environment.NewLine);
            js.Append("} " + Environment.NewLine);

            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "CurrentlySelectedImage", js.ToString(), true);
        }

        private void RegisterScrollToSelectedScript()
        {
            // http://www.developer.com/net/asp/article.php/3643956
            StringBuilder js = new StringBuilder();


            js.Append(" function ScrollToSelectedTreeNode()" + Environment.NewLine);
            js.Append("{ " + Environment.NewLine);
            js.Append("try " + Environment.NewLine);
            js.Append("{   " + Environment.NewLine);
            js.Append("var elem = document.getElementById('" + FolderTreeView.ClientID + "_SelectedNode');   " + Environment.NewLine);
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

        CmsLocalImageOnDisk[] FilterOutNonImageFiles(CmsLocalImageOnDisk[] resources)
        {
            List<CmsLocalImageOnDisk> ret = new List<CmsLocalImageOnDisk>();

            List<string> allowedExtensions = new List<string>();
            foreach (string filter in ImageFileFilters)
            {
                string ext = filter.Replace("*.",".");
                allowedExtensions.Add(ext.ToLower());
            }
            
            foreach (CmsLocalImageOnDisk res in resources)
            {
                if (StringUtils.IndexOf(allowedExtensions.ToArray(), Path.GetExtension(res.FileName), StringComparison.CurrentCultureIgnoreCase) >= 0)
                    ret.Add(res);
            } // foreach
            return ret.ToArray();
        }

        protected void FolderTreeView_SelectedNodeChanged(object sender, EventArgs e)
        {
            int imagesPerRow = 2;
            
            FolderTreeView.SelectedNode.Expand();                     


            DirectoryInfo di = new DirectoryInfo(FolderTreeView.SelectedNode.Value);

            CmsLocalImageOnDisk[] dirResources = CmsLocalImageOnDisk.UpdateFolderInDatabase(di);
             // CmsResource.GetResourcesInDirectory(di.FullName);

            CmsLocalImageOnDisk[] imgResources = FilterOutNonImageFiles(dirResources);

            

            StringBuilder html = new StringBuilder();
            List<string> filenames = new List<string>();
            filenames.Add(di.FullName);
            int fileNum = 0; bool rowStarted = false;
            html.Append("<table cellpadding=\"2\" cellspacing=\"2\" width=\"100%\">");
            foreach (CmsLocalImageOnDisk res in imgResources)
            {
                bool updateResource = false;
                
                string imgHtmlTag = res.getImageHtmlTag(120, -1, "");
                // -- create the file's Url	
                string fileUrl = res.getUrl(Context);


                string fiName = res.FileName;
                if (fiName.IndexOf("'") > -1)
                {
                    fiName = fiName.Replace("'", "\\'");
                    fileUrl = fileUrl.Replace("'", "\\'");
                }


                if (!fileUrl.StartsWith(CmsContext.ApplicationPath))
                    fileUrl = CmsContext.ApplicationPath + fileUrl;


                int phThumbWidth = 999;
                int phThumbHeight = 222;
                string thumbUrlFormat = CmsContext.UserInterface.ShowThumbnailPage.getThumbDisplayUrl(res, phThumbWidth, phThumbHeight);
                thumbUrlFormat = thumbUrlFormat.Replace(phThumbWidth.ToString(), "{0}");
                thumbUrlFormat = thumbUrlFormat.Replace(phThumbHeight.ToString(), "{1}");

                if (fileNum % imagesPerRow == 0)
                {
                    if (rowStarted)
                        html.Append("</tr>");
                    html.Append("<tr>");
                }

                int[] dimensions = res.getImageDimensions();
                if (dimensions.Length != 2)
                {
                    dimensions = WebEditor.Helpers.InlineImageBrowser2.getImageDimensions(res.FilePath);
                    res.setImageDimensions(dimensions);
                    updateResource = true;
                }


                string imgId = "file_" + res.ResourceId.ToString();
                html.Append("<td align=\"center\">");
                string onclick = "doSelImg('" + imgId + "'); parent.selectImage('" + fiName + "','" + fileUrl + "'," + dimensions[0] + ", " + dimensions[1] + ", '" + thumbUrlFormat + "'); ";
                if (IsCKEditor)
                {
                    onclick = "ckSelImg('" + imgId + "', '" + fileUrl.Replace("'", "\\'") + "'," + dimensions[0] + ", " + dimensions[1] + ");";
                }
                html.Append(" <a href=\"#\" onclick=\""+onclick+" return false;\">");
                
                html.Append(imgHtmlTag);
                
                html.Append("</a> ");
                html.Append("<br /><div style=\"font-family: arial; font-size: 8pt; text-align: center; width: 120px; overflow: hidden;\">"+res.FileName+"</div>");                                

                if (SelImagePath != "" && fileUrl == SelImagePath)
                {
                    html.Append("<script>" + onclick + "</script>" + Environment.NewLine);
                }
                
                html.Append("</td>");

                filenames.Add(res.FilePath);

                fileNum++;

                if (updateResource)
                {
                    bool b = res.SaveToDatabase();
                    if (!b) 
                        Console.Write("resource failed to update.");
                }

            } // foreach
            if (rowStarted)
                html.Append("</tr>");
            html.Append("</table>");


            ph_ImagePanel.Controls.Add(new LiteralControl(html.ToString()));
        }
        
        protected void b_DoFileUpload_Click(object sender, EventArgs e)
        {
            string msg = "";
            if (!CmsContext.currentUserCanWriteToUserFilesOnDisk)
            {
                msg = "You are not authorized to upload images.";
                Response.Write(msg);
                return;
            }

            if (FolderTreeView.SelectedNode == null)
            {
                msg = "no folder selected. please try upload again.";
                Response.Write(msg);
                return;
            }

            if (fileUpload.PostedFile.FileName == "")
            {
                msg = "no file selected. please try upload again.";
                Response.Write(msg);
                return;
            }
            
            if (Array.IndexOf(InlineImageBrowser2.ImageFileFilters, "*"+Path.GetExtension(fileUpload.PostedFile.FileName).ToLower()) < 0)
            {
                msg = "uploaded file is not an image file.";
                Response.Write(msg);
                return;
            }
            string dirName = FolderTreeView.SelectedNode.Value;
            if (!dirName.EndsWith("\\"))
                dirName += "\\";
            string targetFilename = dirName + Path.GetFileName(fileUpload.PostedFile.FileName);
            if (System.IO.File.Exists(targetFilename))
            {
                msg = "filename already exists!";
                Response.Write(msg);
                return;
            }
            try
            {
                fileUpload.PostedFile.SaveAs(targetFilename);
            }
            catch(Exception ex)
            {
                msg = "Error when saving file to webserver.";
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
                        msg = "there was a problem creating the new folder: " + ex.Message;
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
