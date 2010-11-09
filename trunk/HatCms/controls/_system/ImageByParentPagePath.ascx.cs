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

namespace HatCMS.Controls
{
    public partial class ImageByParentPagePath : System.Web.UI.UserControl
    {
        private const string RandomImageKeyValue = "{*}";

        
        /// <summary>
        /// crawls up the page path to see if anything has been configured for that page.
        /// if nothing found, returns String.Empty.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private string getImageByPagePath(CmsPage page)
        {
            CmsPage currPage = page;
            string notFoundValue = Guid.NewGuid().ToString();            
            while (currPage.ID != -1)
            {
                string PagePath = currPage.Path;
                string keyVal = CmsControlUtils.getControlParameterKeyValue(this, PagePath, notFoundValue);
                if (keyVal != notFoundValue)
                {
                    return keyVal;
                }

                currPage = currPage.ParentPage;
            } // while

            return String.Empty;
        } // getImageByPagePath

        /// <summary>
        /// if nothing found, returns String.Empty
        /// </summary>
        /// <returns></returns>
        private string getRandomImage()
        {
            // -- get all the configured image vals
            string[] configKeys = CmsControlUtils.getControlParameterKeys(this);
            ArrayList imgUrls = new ArrayList();
            string notFoundValue = Guid.NewGuid().ToString(); 
            foreach (string key in configKeys)
            {
                string val = CmsControlUtils.getControlParameterKeyValue(this, key, notFoundValue);
                if (val != "" && val != notFoundValue && String.Compare(val, RandomImageKeyValue, true) != 0)
                    imgUrls.Add(val);
            } // foreach

            // -- select the image at random
            if (imgUrls.Count > 0)
            {
                int randomUrlIndex = (new System.Random()).Next(0, imgUrls.Count - 1);
                return imgUrls[randomUrlIndex].ToString();
            }
            return String.Empty;

        } // getRandomImage
        

        protected override void Render(HtmlTextWriter writer)
        {
            string img = getImageByPagePath(CmsContext.currentPage);
            if (img != String.Empty)
            {
                if (String.Compare(img.Trim(), RandomImageKeyValue, true) == 0)
                {
                    img = getRandomImage();
                }

                if (img != String.Empty)
                {
                    string html = ("<img src=\"" + img + "\" />");
                    Response.Write(html.ToString());
                } // if
            } // if
        } // Render
    }
}