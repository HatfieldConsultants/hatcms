namespace HatCMS.Controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for header.
	/// </summary>
	public partial class header : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
			// Page_Load is NOT Called if Render() is overridden.            
		}

		protected string getRandomImageUrl()
		{
			string ConfUrls = CmsConfig.getConfigValue("RandomImageUrls","");

            if (ConfUrls.Trim() == "")
                return "";

			// -- split on | (pipe) character
			
			string[] urls = ConfUrls.Split(new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries);						
			
			int randomUrlIndex = (new System.Random()).Next(0,urls.Length-1);
				
			// -- let's try to actually rotate through the images (it's not random)
			if (Session["lastRandomImageIndex"] != null)
			{
				randomUrlIndex = Convert.ToInt32(Session["lastRandomImageIndex"]);
				randomUrlIndex ++;
			}

			if (randomUrlIndex >= urls.Length || randomUrlIndex < 0) // wrap
				randomUrlIndex = 0;

			
			string imgUrl = urls[randomUrlIndex];						

			Session["lastRandomImageIndex"] = randomUrlIndex;

			return imgUrl;
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
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}
		#endregion
	}
}
