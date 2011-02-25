using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Imaging;
using System.Web;

namespace HatCMS
{
	/// <summary>
	/// An .ASPX page that either shows the thumbnail for an image, or directs the client to the cached version of the thumbnail. Thumbnail images can be cached in memory, and on disk.
	/// </summary>
    public partial class showThumbPage : Hatfield.Web.Portal.Imaging.BaseShowThumbnailPage2 
	{
        /// <summary>
        /// the full path on-disk to the directory to store image thumbnails
        /// </summary>
        public static string ThumbImageCacheDirectory
        {
            get
            {                
                string relPath = CmsConfig.getConfigValue("ThumbImageCacheDirectory", "~/_system/writable/ThumbnailCache/");
                string absPath = VirtualPathUtility.ToAbsolute(relPath);
                string pathOnDisk = HttpContext.Current.Server.MapPath(absPath);

                if (!pathOnDisk.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    pathOnDisk += Path.DirectorySeparatorChar.ToString();
                return pathOnDisk;
            }
        } // ThumbImageCacheDirectory

        
        private static BaseShowThumbnailPageParameters getBaseShowThumbnailPageParameters()
        {
            string FullSizeImageStorageDir = System.Web.HttpContext.Current.Server.MapPath(System.Web.HttpContext.Current.Request.ApplicationPath);
            if (!FullSizeImageStorageDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                FullSizeImageStorageDir = FullSizeImageStorageDir + Path.DirectorySeparatorChar.ToString();

            string ThumbDiskCacheStorageDir = ThumbImageCacheDirectory;

            BaseShowThumbnailPageParameters ret = new BaseShowThumbnailPageParameters(
                CmsContext.ApplicationPath + "_system/showThumb.aspx", /* this page's url */
                true, /* use Memory Cache */
                true, /* use Disk Cache */
                FullSizeImageStorageDir,
                ThumbDiskCacheStorageDir);

            return ret;
        } // getBaseShowThumbnailPageParameters


        public static string getThumbDisplayUrl(string imgPath, System.Drawing.Size displayBoxSize)
        {
            return BaseShowThumbnailPage2.getThumbDisplayUrl(
                getBaseShowThumbnailPageParameters(),
                imgPath,
                displayBoxSize.Width,
                displayBoxSize.Height);

        }

        public static string getThumbDisplayUrl(string imgPath, int displayBoxWidth, int displayBoxHeight)
        {
            return BaseShowThumbnailPage2.getThumbDisplayUrl(
                getBaseShowThumbnailPageParameters(),
                imgPath,
                displayBoxWidth,
                displayBoxHeight);

        }

        public static string getThumbDisplayUrl(CmsResource resource, int displayBoxWidth, int displayBoxHeight)
        {
            string url = resource.getThumbDisplayUrl(displayBoxWidth, displayBoxHeight);
            if (url != "")
                return url;

            url = BaseShowThumbnailPage2.getThumbDisplayUrl(
                getBaseShowThumbnailPageParameters(),
                resource.getUrl(System.Web.HttpContext.Current),
                displayBoxWidth,
                displayBoxHeight);

            if (url.ToLower().IndexOf(".aspx") == -1)
                resource.setThumbDisplayUrl(displayBoxWidth, displayBoxHeight, url);
            
            return url;

        }

        /// <summary>
        /// if width and height can not be determined, returns a new size with isEmpty = true.
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="displayBoxWidth"></param>
        /// <param name="displayBoxHeight"></param>
        /// <returns></returns>
        public static System.Drawing.Size getDisplayWidthAndHeight(string fileUrl, System.Drawing.Size displayBox)
        {
            return BaseShowThumbnailPage2.getDisplayWidthAndHeight(
                 getBaseShowThumbnailPageParameters(),
                 fileUrl,
                 displayBox.Width,
                 displayBox.Height);
        }

        /// <summary>
        /// if width and height can not be determined, returns a new size with isEmpty = true.
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="displayBoxWidth"></param>
        /// <param name="displayBoxHeight"></param>
        /// <returns></returns>
        public static System.Drawing.Size getDisplayWidthAndHeight(string fileUrl, int displayBoxWidth, int displayBoxHeight)
        {
            return BaseShowThumbnailPage2.getDisplayWidthAndHeight(
                 getBaseShowThumbnailPageParameters(),
                 fileUrl,
                 displayBoxWidth,
                 displayBoxHeight);
        }

        /// <summary>
        /// if width and height can not be determined, returns a new size with isEmpty = true.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="displayBoxWidth"></param>
        /// <param name="displayBoxHeight"></param>
        /// <returns></returns>
        public static System.Drawing.Size getDisplayWidthAndHeight(CmsResource resource, int displayBoxWidth, int displayBoxHeight)
        {
            int[] actualSize = resource.getImageDimensions();
            if (actualSize.Length == 2)
            {
                System.Drawing.Size ret = Thumbnail2.calculateDisplayWidthAndHeight(actualSize, displayBoxWidth, displayBoxHeight);
                if (!ret.IsEmpty)
                    return ret;
            }
            
            return BaseShowThumbnailPage2.getDisplayWidthAndHeight(
                 getBaseShowThumbnailPageParameters(),
                 resource.getUrl(System.Web.HttpContext.Current),
                 displayBoxWidth,
                 displayBoxHeight);
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            base.Process_Page_Load(getBaseShowThumbnailPageParameters());
        } // Page_Load

		

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
	}
}
