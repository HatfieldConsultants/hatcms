using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Placeholders
{
    public class UserImageGalleryPlaceholderData
    {
        public static string[] ImageExtensionsToDisplay = new string[] { ".jpg", ".jpeg", ".jpe", ".gif", ".png" };
        
        public enum CaptionDisplayLocation { Top, Bottom, TopAndBottom, NoCaptionDisplay };
        public enum FullSizeImageDisplayMode { Lightbox, Popup, IndividualPage };


        public int PageId = -1;
        public int Identifier = -1;
        public CmsLanguage Lang = CmsConfig.Languages[0];
        
        /// <summary>
        /// values less than 0 means to not take the width into consideration;
        /// if both ThumbnailDisplayBoxWidth and ThumbnailDisplayBoxHeight are less than 0, the full-sized image will be rendered
        /// </summary>
        public int ThumbnailDisplayBoxWidth = -1;

        /// <summary>
        /// values less than 0 means to not take the width into consideration;
        /// if both ThumbnailDisplayBoxWidth and ThumbnailDisplayBoxHeight are less than 0, the full-sized image will be rendered
        /// </summary>
        public int ThumbnailDisplayBoxHeight = -1;


        public FullSizeImageDisplayMode FullSizeLinkMode = FullSizeImageDisplayMode.IndividualPage;

        public CaptionDisplayLocation CaptionLocation = CaptionDisplayLocation.Bottom;


        /// <summary>
        /// values less than 0 means to not take the width into consideration;
        /// if both FullSizeDisplayBoxWidth and FullSizeDisplayBoxHeight are less than 0, the full-sized image will be rendered
        /// </summary>
        public int FullSizeDisplayBoxWidth = -1;

        /// <summary>
        /// values less than 0 means to not take the width into consideration;
        /// if both FullSizeDisplayBoxWidth and FullSizeDisplayBoxHeight are less than 0, the full-sized image will be rendered
        /// </summary>
        public int FullSizeDisplayBoxHeight = -1;

        public int NumThumbsPerRow = 4;

        public int NumThumbsPerPage = 20;


        public bool createImageStorageDirectory(CmsPage page)
        {
            try
            {
                string dir = getImageStorageDirectory(page);
                if (dir != string.Empty)
                {
                    Directory.CreateDirectory(dir);
                    return true;
                }
            }
            catch
            { }
            return false;
        }

        /// <summary>
        /// Gets the full path name for where images should be stored. returns String.Empty on error
        /// </summary>
        /// <returns></returns>
        public string getImageStorageDirectory(CmsPage page)
        {
            try
            {
                if (page.ID >= 0)
                {
                    string dir = System.Web.HttpContext.Current.Server.MapPath(CmsContext.ApplicationPath + "UserFiles" + Path.DirectorySeparatorChar + "ImageGalleries" + Path.DirectorySeparatorChar + page.ID.ToString() + Path.DirectorySeparatorChar);
                    if (!dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                        dir = dir + Path.DirectorySeparatorChar;                    

                    return dir;
                }
            }
            catch
            { }

            return string.Empty;
        }

        public static UserImageGalleryPlaceholderData CreateWithDefaults()
        {
            UserImageGalleryPlaceholderData ret = new UserImageGalleryPlaceholderData();

            ret.ThumbnailDisplayBoxWidth = 200;
            ret.ThumbnailDisplayBoxHeight = -1;
            ret.FullSizeDisplayBoxWidth = 600;
            ret.FullSizeDisplayBoxHeight = -1;
            ret.CaptionLocation = CaptionDisplayLocation.Bottom;
            ret.NumThumbsPerPage = 20;
            ret.NumThumbsPerRow = 4;

            ret.FullSizeLinkMode = FullSizeImageDisplayMode.IndividualPage;

            return ret;
        }

    }
}

