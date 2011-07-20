using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

namespace Hatfield.Web.Portal.Imaging
{
    /// <summary>
    /// note: this class works but has a bunch of issues. We really should use this: 
    /// http://msdn2.microsoft.com/en-us/library/ms954815.aspx 
    /// </summary>
    public class ImageDiskCache
    {

        public ImageDiskCache(string cacheDirectoryOnDisk)
        {
            ThumbImageCacheDirectory = cacheDirectoryOnDisk;
            if (!ThumbImageCacheDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
                ThumbImageCacheDirectory += Path.DirectorySeparatorChar.ToString();

        }

        private string ThumbImageCacheDirectory = "";


        public string CacheKeyToUrl(string cacheKey, string ImageFileExtension)
        {
            string filename = CacheKeyToFilename(cacheKey, ImageFileExtension);

            string appPath = PageUtils.ApplicationPath;

            string appPathOnDisk = System.Web.Hosting.HostingEnvironment.MapPath(appPath);
            if (!appPathOnDisk.EndsWith(Path.DirectorySeparatorChar.ToString()))
                appPathOnDisk = appPathOnDisk + Path.DirectorySeparatorChar.ToString();

            string relPath = Hatfield.Web.Portal.PathUtils.RelativePathTo(appPathOnDisk, filename);

            relPath = relPath.Replace("\\", "/");

            string url = appPath + relPath;

            return url;
        }

        public string CacheKeyToFilename(string cacheKey, string ImageFileExtension)
        {
            string pre = Path.GetFileName(cacheKey);
            string filename = pre + cacheKey.GetHashCode().ToString();

            if (!ImageFileExtension.StartsWith("."))
                ImageFileExtension = "." + ImageFileExtension;

            return ThumbImageCacheDirectory + filename + ImageFileExtension;
        }


        public bool ExistsInCache(string cacheKey, string ImageFileExtension)
        {
            string imgFilename = CacheKeyToFilename(cacheKey, ImageFileExtension);
            return File.Exists(imgFilename);
        }

        public byte[] getFromCache(string cacheKey, string ImageFileExtension)
        {
            if (ExistsInCache(cacheKey, ImageFileExtension))
            {
                string imgFilename = CacheKeyToFilename(cacheKey, ImageFileExtension);
                return File.ReadAllBytes(imgFilename);
            }
            return null;
        }

        public void addToCache(string cacheKey, string ImageFileExtension, byte[] imageData)
        {
            try
            {
                string imgFilename = CacheKeyToFilename(cacheKey, ImageFileExtension);

                File.WriteAllBytes(imgFilename, imageData);
            }
            catch
            { }
        }


    } // class
}
