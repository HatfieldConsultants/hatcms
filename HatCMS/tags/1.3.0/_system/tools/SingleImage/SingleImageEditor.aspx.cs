using System;
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
using HatCMS.Placeholders;

namespace HatCMS.FCKHelpers
{
    public partial class SingleImageEditor : System.Web.UI.Page
    {
        public SingleImageData currentSingleImage;

        /// <summary>
        /// return null if not found
        /// </summary>
        /// <returns></returns>
        public SingleImageData getSingleImageData()
        {
            int id = PageUtils.getFromForm("i", -1);
            if (id < 0)
                return null;

            SingleImageData d = (new SingleImageDb()).getSingleImage(id);            
            return d;
            
        }

        public string formName
        {
            get
            {
                return PageUtils.getFromForm("formName","");                
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
            }
            
            if (formName.Trim() == "")
            {
                Response.Write("Error: no FormName specified");
                Response.End();
            }
            currentSingleImage = getSingleImageData();                        
        }

        public void OutputDeleteFileLink()
        {
            StringBuilder html = new StringBuilder();

            if (CmsContext.currentUserIsSuperAdmin)
            {
                string onclick = "var url = '" + CmsContext.ApplicationPath + "_system/tools/FCKHelpers/DeleteResourcePopup.aspx?FileUrl='+document.getElementById('selectedImageUrl').value; window.open(url,'delResourcePopup','width=400,height=400,resizable=1,scrollbars=1'); return false;";
                html.Append(" <input type=\"button\" onclick=\"" + onclick + "\" value=\"delete selected file\" />");
            }

            Response.Write(html.ToString());
        }
    }
}
