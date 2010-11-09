using System;
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
	public partial class InlineLinkBrowser : System.Web.UI.Page
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
            if (!CmsContext.currentUserCanAuthor)
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
            // -- add onclick events to all links
            /*
            js.Append(" var links = document.getElementsByTagName('a');" + Environment.NewLine);
            js.Append(" for(var i=0; i< links.length; i++) {" + Environment.NewLine);
            js.Append("     if(links[i].href.indexOf('ToggleNode') >= 0) { continue; } " + Environment.NewLine);
            js.Append("     links[i].addEventListener('click', function(){"+Environment.NewLine);            
            // js.Append("alert(this.href);" + Environment.NewLine);
            js.Append("         var links = document.getElementsByTagName('a');" + Environment.NewLine);
            js.Append("         for(var i=0; i< links.length; i++) {" + Environment.NewLine);
            js.Append("             if (links[i].getAttribute('rel')) { links[i].removeAttribute('rel'); links[i].style.borderStyle = 'none'; break; }" + Environment.NewLine);
            js.Append("         }" + Environment.NewLine);
            js.Append("         this.rel = '1';" + Environment.NewLine);
            js.Append("         this.style.border = '2px solid red';" + Environment.NewLine);

            js.Append("     }, false );" + Environment.NewLine);
            js.Append(" }" + Environment.NewLine);
             */
            
            js.Append("}" + Environment.NewLine + Environment.NewLine);

            js.Append("function ckSelLink(url, linkEl) {" + Environment.NewLine);
            js.Append(" window.parent.CKEDITOR.dialog.getCurrent().setValueOf('info', 'url', encodeURI(url));" + Environment.NewLine);
            js.Append(" window.parent.CKEDITOR.dialog.getCurrent().setValueOf('target', 'linkTargetType', '_self');" + Environment.NewLine);
            js.Append("}" + Environment.NewLine + Environment.NewLine);



            js.Append(CmsPageHeadSection.getOnloadJavascript("ScrollToSelectedTreeNode"));


            Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ScrollToSelectedTreeNode", js.ToString(), true);
        }

        private TreeNode createNodeForPage(CmsPage page)
        {
            TreeNode n = new TreeNode();            
            n.PopulateOnDemand = true;
            string title = page.MenuTitle;
            if (title.Trim() == "")
                title = page.Title;
            n.Text = title;
            n.Value = page.Path;
            string navUrl = "javascript:parent.selectLink('" + page.Title + "','" + page.Url + "');";
            if (IsCKEditor)
            {
                navUrl = "javascript:ckSelLink('" + page.Url.Replace("'", "\\'") + "');";
            }
            n.NavigateUrl = navUrl;
            return n;
        }

        private void fillInitialTree()
        {
            CmsPage homePage = CmsContext.HomePage;
            TreeNode homeNode = createNodeForPage(homePage);
            foreach(CmsPage p in homePage.ChildPages)
            {
                if (p.isVisibleForCurrentUser)
                {
                    TreeNode n = createNodeForPage(p);
                    homeNode.ChildNodes.Add(n);
                }
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
            CmsPage rootPage = CmsContext.getPageByPath(rootPagePath);
            
            e.Node.ChildNodes.Clear();

            foreach (CmsPage p in rootPage.ChildPages)
            {
                if (p.isVisibleForCurrentUser)
                {
                    TreeNode n = createNodeForPage(p);
                    e.Node.ChildNodes.Add(n);
                }
            } // foreach
        } // tv_Pages_TreeNodePopulate
	}
}
