using System;
using System.IO;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace HatCMS.WebEditor.Helpers
{
	/// <summary>
	/// Summary description for InlineLinkBrowser.
	/// </summary>
	public partial class InlineUserFileBrowser : System.Web.UI.Page
	{

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
        
        protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
            if (!CmsContext.currentUserIsLoggedIn)
            {
                Response.Write("Access denied");
                Response.End();
                return;
            }

            RegisterScrollToSelectedScript(tv_Pages);
            if (!IsPostBack)
            {
                fillInitialTree();
            }
		}

        private string UserFilesPath
        {
            get
            {

                return InlineImageBrowser2.UserFilesPath;
            }
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

            js.Append(" var links = document.getElementsByTagName('a');" + Environment.NewLine);
            js.Append(" for(var i=0; i< links.length; i++) {" + Environment.NewLine);
            js.Append("     links[i].addEventListener('click', function(){" + Environment.NewLine);
            // js.Append("alert(this.href);" + Environment.NewLine);
            js.Append("         var links = document.getElementsByTagName('a');" + Environment.NewLine);
            js.Append("         for(var i=0; i< links.length; i++) {" + Environment.NewLine);
            js.Append("             if (links[i].getAttribute('rel')) { links[i].removeAttribute('rel'); links[i].style.borderStyle = 'none'; break; }" + Environment.NewLine);
            js.Append("         }" + Environment.NewLine);
            js.Append("         this.rel = '1';" + Environment.NewLine);
            js.Append("         this.style.border = '2px solid red';" + Environment.NewLine);

            js.Append("     }, false );" + Environment.NewLine);
            js.Append(" }" + Environment.NewLine);


            js.Append("}" + Environment.NewLine + Environment.NewLine);

            js.Append("function ckSelLink(url, linkEl) {" + Environment.NewLine);
            js.Append(" window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'url', encodeURI(url));" + Environment.NewLine);
            js.Append(" window.parent.CKEDITOR.dialog.getCurrent().setValueOf('target', 'linkTargetType', '_self');" + Environment.NewLine);
            js.Append("}" + Environment.NewLine + Environment.NewLine);

            js.Append(CmsPageHeadSection.getOnloadJavascript("ScrollToSelectedTreeNode"));


            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ScrollToSelectedTreeNode", js.ToString(), true);
        }

        private TreeNode createNodeForFile(FileInfo fi)
        {
            // -- create the file's Url					
            string rootUserFilesDir = Server.MapPath(UserFilesPath);

            string subDir = fi.FullName.Replace(rootUserFilesDir, "");

            subDir = UserFilesPath + subDir;
            if (!subDir.EndsWith("\\"))
                subDir += "\\";

            string fileUrl = subDir + fi.Name;

            fileUrl = fileUrl.Replace("\\", "/");

            fileUrl = fileUrl.Replace("//", "/");
            
            TreeNode n = new TreeNode();            
            n.PopulateOnDemand = false;
            
            n.Text = fi.Name;
            n.Value = fi.FullName;

            string navUrl = "javascript:parent.selectFile('" + fi.Name + "','" + fileUrl + "');";
            if (IsCKEditor)
            {
                navUrl = "javascript:ckSelLink('" + fileUrl.Replace("'", "\\'") + "');";
            }
            n.NavigateUrl = navUrl;
            return n;
        }

        private TreeNode createNodeForDirectory(DirectoryInfo di)
        {
            TreeNode n = new TreeNode();
            n.PopulateOnDemand = true;

            n.Text = di.Name;
            n.Value = di.FullName;            
            return n;
        }

        private bool listFile(FileInfo fi)
        {
            if ((fi.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden &&
                    (fi.Attributes & FileAttributes.System) != FileAttributes.System)
                return true;

            return false;
        }

        private void fillInitialTree()
        {
            string UserFilesDir = Server.MapPath(InlineImageBrowser2.UserFilesPath);
            DirectoryInfo di = new DirectoryInfo(UserFilesDir);

            TreeNode homeNode = createNodeForDirectory(di);
            homeNode.Text = "Site Files";
            foreach(FileInfo fi in di.GetFiles())
            {
                if (listFile(fi))
                {
                    TreeNode n = createNodeForFile(fi);
                    homeNode.ChildNodes.Add(n);
                }
            }

            foreach (DirectoryInfo subdir in di.GetDirectories())
            {
                TreeNode n = createNodeForDirectory(subdir);
                homeNode.ChildNodes.Add(n);
            }

            homeNode.PopulateOnDemand = false;
            tv_Pages.Nodes.Add(homeNode);
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

        protected void tv_Pages_TreeNodePopulate(object sender, TreeNodeEventArgs e)
        {
            string rootPagePath = e.Node.Value;
            if (Directory.Exists(rootPagePath))
            {
                DirectoryInfo di = new DirectoryInfo(rootPagePath);
                e.Node.ChildNodes.Clear();

                foreach (FileInfo fi in di.GetFiles())
                {
                    if (listFile(fi))
                    {
                        TreeNode n = createNodeForFile(fi);
                        e.Node.ChildNodes.Add(n);
                    }
                }

                foreach (DirectoryInfo subDir in di.GetDirectories())
                {
                    TreeNode n = createNodeForDirectory(subDir);
                    e.Node.ChildNodes.Add(n);
                }
                
            } // if
        } // tv_Pages_TreeNodePopulate
	}
}
