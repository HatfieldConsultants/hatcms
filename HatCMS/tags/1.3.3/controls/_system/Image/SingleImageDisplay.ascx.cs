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
using Hatfield.Web.Portal;
using HatCMS.Placeholders;

namespace HatCMS.Controls.Image
{
    public partial class SingleImageDisplay : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected override void Render(HtmlTextWriter writer)
        {
            int ImageId = PageUtils.getFromForm("i", -1);
            if (ImageId < 0)
            {
                writer.Write("Error: invalid image specified");
                return;
            }

            SingleImageDb db = new SingleImageDb();
            SingleImageData image = db.getSingleImage(ImageId);
            if (image == null)
            {
                writer.Write("Error: invalid image specified");
                return;
            }

            int imageBoxWidth = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayWidth", -1);
            int imageBoxHeight = CmsConfig.getConfigValue("SingleImage.FullSizeDisplayHeight", -1);            

            // -- prepare the output
            string largeImageUrl = showThumbPage.getThumbDisplayUrl(image.ImagePath, imageBoxWidth, imageBoxHeight);
            System.Drawing.Size LargeImageSize = showThumbPage.getDisplayWidthAndHeight(image.ImagePath, imageBoxWidth, imageBoxHeight);
                        
            string width = "";
            string height = "";
            if (!LargeImageSize.IsEmpty)
            {
                width = " width=\"" + LargeImageSize.Width + "\"";
                height = " height=\"" + LargeImageSize.Height.ToString() + "\"";
            }

            string EOL = Environment.NewLine;
            System.Text.StringBuilder html = new System.Text.StringBuilder();

            // -- do the output

            html.Append("<div class=\"SingleImageDisplay\">" + EOL);
            html.Append("<img src=\"" + largeImageUrl + "\"" + width + "" + height + ">" + EOL);
            html.Append("<div class=\"Caption\">");
            html.Append(image.Caption);
            html.Append("</div>" + EOL);
            if (image.Credits.Trim() != "")
            {
                html.Append("<div class=\"credits\">");
                string creditsPrefix = CmsConfig.getConfigValue("SingleImage.CreditsPrefix", "");
                html.Append(creditsPrefix + image.Credits);
                html.Append("</div>"); // credits
            }

            html.Append("</div>");

            writer.Write(html.ToString());
        }
    }
}