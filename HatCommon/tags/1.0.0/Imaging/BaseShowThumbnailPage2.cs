using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal.Imaging
{
    public class BaseShowThumbnailPage2 : System.Web.UI.Page
    {


#if DEBUG
        private const int MemoryCacheLengthMinutes = 0; // do not cache
#else
		        private const int MemoryCacheLengthMinutes = (60*24*5); // 5 days
#endif

        /// <summary>
        /// returns NULL if not in the memory cache
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        protected byte[] CheckMemoryCache(string cacheKey)
        {
            if (System.Web.Hosting.HostingEnvironment.Cache[cacheKey] != null && MemoryCacheLengthMinutes > 0)
            {
                return System.Web.Hosting.HostingEnvironment.Cache[cacheKey] as byte[];
            }

            return null;

        }


        protected void InsertIntoMemoryCache(string cacheKey, byte[] data)
        {
            System.Web.Caching.Cache c = System.Web.Hosting.HostingEnvironment.Cache;
            c.Insert(cacheKey, data, null, DateTime.MaxValue, TimeSpan.FromMinutes(MemoryCacheLengthMinutes));
        }


        protected static string getImageCacheKey(string FullSizeImageFilenameOnDisk, int displayBoxWidth, int displayBoxHeight)
        {
            //-- algorithm note: nothing called here should access the disk or database; everything should be memory based.             
            string PathUnderAppRoot = PathUtils.RelativePathTo(System.Web.Hosting.HostingEnvironment.MapPath("~/"), FullSizeImageFilenameOnDisk);

            string ret = PathUnderAppRoot;


            byte[] bytes = System.Text.Encoding.UTF32.GetBytes(ret);
            long val = long.MaxValue;
            foreach (byte b in bytes)
                val = val - b;

            ret = StringUtils.Base36Encode(val);

            string fn = Path.GetFileNameWithoutExtension(PathUnderAppRoot);
            ret = ret + fn;

            foreach (char c in Path.GetInvalidPathChars())
            {
                ret = ret.Replace(c, '.');
            }

            ret = ret.Replace(Path.DirectorySeparatorChar.ToString(), "");
            ret = ret.Replace(Path.AltDirectorySeparatorChar.ToString(), "");
            ret = ret.Replace(Path.VolumeSeparatorChar.ToString(), "");
            ret = ret.Replace(Path.PathSeparator.ToString(), "");
            ret = ret.Replace("#", "");
            ret = ret.Replace("%", "");
            ret = ret.Replace(" ", ".");
            ret = ret.Replace("~", "."); // -- note: when using mono/linux, ~ has special significance            

            // -- HTTrack has problems with unicode characters in filenames
            //      logged here: http://forum.httrack.com/readmsg/18923/index.html
            //      let's replace some common chars, but leave the rest to the for loop before.
            //      We could use the list of Unicode chars: http://en.wikipedia.org/wiki/List_of_Unicode_characters
            ret = ret.Replace("ä", "a");
            ret = ret.Replace("á", "a");
            ret = ret.Replace("á", "a");

            ret = ret.Replace("Ä", "A");
            ret = ret.Replace("Ã", "A");

            ret = ret.Replace("ç", "c");
            ret = ret.Replace("Č", "C");
            ret = ret.Replace("č", "c");

            ret = ret.Replace("ě", "e");
            ret = ret.Replace("é", "e");

            ret = ret.Replace("í", "i");

            ret = ret.Replace("ñ", "n");
            ret = ret.Replace("Ñ", "N");

            ret = ret.Replace("ř", "r");


            ret = ret.Replace("ß", "ss");

            ret = ret.Replace("ö", "o");
            ret = ret.Replace("Ö", "O");
            ret = ret.Replace("Ó", "O");
            ret = ret.Replace("ó", "o");

            ret = ret.Replace("ü", "u");
            ret = ret.Replace("Ü", "U");

            ret = ret.Replace("ý", "y");
            ret = ret.Replace("ž", "z");


            string noUnicode = ret;
            foreach (char c in ret)
            {
                if (c > '\u0080')
                {
                    noUnicode = noUnicode.Replace(c.ToString(), Convert.ToInt32(c).ToString());
                }
            } // foreach
            ret = noUnicode;

            // -- check the length of the string to ensure it will fit on the file system.

            if (ret.Length >= 245)
            {
                ret = ret.Substring(ret.Length - 245);
            }

            // -- add the displayBoxHeight and displayBoxWidth 
            if (displayBoxHeight <= 0)
                displayBoxHeight = -1;

            if (displayBoxWidth <= 0)
                displayBoxWidth = -1;

            ret = ret + "." + displayBoxWidth.ToString() + "." + displayBoxHeight.ToString() + ".";

            return ret;
        }

        /// <summary>
        /// if width and height can not be determined, returns a new size with isEmpty = true.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="filename"></param>
        /// <param name="longestSidePixels"></param>
        /// <param name="shortestSidePixels"></param>
        /// <returns></returns>
        protected static System.Drawing.Size getDisplayWidthAndHeight(BaseShowThumbnailPageParameters config, string filename, int displayBoxWidth, int displayBoxHeight)
        {
            string FullSizeImageFilenameOnDisk = config.fullSizeImageStorageDir + filename;
            string cacheKey = getImageCacheKey(FullSizeImageFilenameOnDisk, displayBoxWidth, displayBoxHeight);

            ImageDiskCache diskCache = new ImageDiskCache(config.diskCacheStorageDirectoryPath);
            if (config.useDiskCache && diskCache.ExistsInCache(cacheKey, Path.GetExtension(FullSizeImageFilenameOnDisk)))
            {
                string fn = diskCache.CacheKeyToFilename(cacheKey, Path.GetExtension(FullSizeImageFilenameOnDisk));
                return Thumbnail2.getDisplayWidthAndHeight(fn, displayBoxWidth, displayBoxHeight);
            }
            else if (File.Exists(FullSizeImageFilenameOnDisk))
            {
                return Thumbnail2.getDisplayWidthAndHeight(FullSizeImageFilenameOnDisk, displayBoxWidth, displayBoxHeight);
            }
            else return new System.Drawing.Size();
        }


        protected static string getThumbDisplayUrl(BaseShowThumbnailPageParameters config, string fileUrl, int displayBoxWidth, int displayBoxHeight)
        {
            // -- get the AppPath
            string appPath = PageUtils.ApplicationPath;

            // -- remove appPath
            if (fileUrl.StartsWith(appPath))
            {
                fileUrl = fileUrl.Substring(appPath.Length);
            }

            if (config.useDiskCache && config.diskCacheStorageDirectoryPath != "")
            {

                string FullSizeImageFilenameOnDisk = config.fullSizeImageStorageDir + fileUrl;
                string cacheUrl = getCachedThumbnailUrl(config, FullSizeImageFilenameOnDisk, displayBoxWidth, displayBoxHeight);
                if (cacheUrl != String.Empty)
                    return cacheUrl;

            } // if use disk cache


            string displayUrl = config.showThumbDisplayPageUrl + "?file=" + fileUrl;
            if (displayBoxWidth > 0)
            {
                displayUrl += "&w=" + displayBoxWidth.ToString();
            }
            if (displayBoxHeight > 0)
            {
                displayUrl += "&h=" + displayBoxHeight.ToString();
            }
            return displayUrl;
        }


        /// <summary>
        /// returns String.Empty if not found in the cache.
        /// </summary>
        /// <param name="FullSizeImageFilenameOnDisk"></param>
        /// <param name="longestSidePixels"></param>
        /// <param name="shortestSidePixels"></param>
        /// <returns></returns>
        protected static string getCachedThumbnailUrl(BaseShowThumbnailPageParameters config, string FullSizeImageFilenameOnDisk, int displayBoxWidth, int displayBoxHeight)
        {
            ImageDiskCache diskCache = new ImageDiskCache(config.diskCacheStorageDirectoryPath);

            string cacheKey = getImageCacheKey(FullSizeImageFilenameOnDisk, displayBoxWidth, displayBoxHeight);
            string imgExtension = Path.GetExtension(FullSizeImageFilenameOnDisk);
            if (diskCache.ExistsInCache(cacheKey, imgExtension))
            {
                string url = diskCache.CacheKeyToUrl(cacheKey, imgExtension);
                return url;
            }


            return String.Empty;
        }

        protected void Process_Page_Load(BaseShowThumbnailPageParameters config)
        {

            string requestFile = PageUtils.getFromForm("file", "");
            int displayBoxWidth = PageUtils.getFromForm("w", 0);
            int displayBoxHeight = PageUtils.getFromForm("h", 0);

            // -- get the full sized image filename on disk
            string FullSizeImageFilenameOnDisk = "";
            // requestFile could have the Application path in it, and the config.fullSizeImageStorageDir could also making the file impossible to find
            // so let's remove it from requestFile
            string appPath = Request.ApplicationPath;
            if (!appPath.EndsWith("/"))
                appPath += "/";
            if (requestFile.StartsWith(appPath))
                requestFile = requestFile.Substring(appPath.Length);

            FullSizeImageFilenameOnDisk = config.fullSizeImageStorageDir + requestFile;

            if (FullSizeImageFilenameOnDisk == "" || !File.Exists(FullSizeImageFilenameOnDisk))
            {
                return;
            }

            string imgExtension = Path.GetExtension(FullSizeImageFilenameOnDisk);

            // -- check the cache for the image key
            string cacheKey = getImageCacheKey(FullSizeImageFilenameOnDisk, displayBoxWidth, displayBoxHeight);
            byte[] imageContent = null;
            if (config.useMemoryCache)
                imageContent = CheckMemoryCache(cacheKey);

            if (imageContent == null && config.useDiskCache && config.diskCacheStorageDirectoryPath != "" && Directory.Exists(config.diskCacheStorageDirectoryPath))
            {
                ImageDiskCache diskCache = new ImageDiskCache(config.diskCacheStorageDirectoryPath);
                imageContent = diskCache.getFromCache(cacheKey, imgExtension);
            }

            if (imageContent == null)
            {
                imageContent = Thumbnail2.CreateThumbnail(FullSizeImageFilenameOnDisk, displayBoxWidth, displayBoxHeight);
            }

            if (imageContent != null)
            {
                // -- save to cache
                if (config.useMemoryCache)
                    InsertIntoMemoryCache(cacheKey, imageContent);
                if (config.useDiskCache)
                {
                    ImageDiskCache diskCache = new ImageDiskCache(config.diskCacheStorageDirectoryPath);
                    diskCache.addToCache(cacheKey, imgExtension, imageContent);
                }

                // -- serve the image binary data
                serveImageContent(imgExtension, imageContent);


            }

        } // Process_Page_Load

        private void serveImageContent(string imgExtension, byte[] imageContent)
        {
            Response.Clear();
            Response.Buffer = true; // this is needed if we also send the content-length header.                        
            Response.Cache.SetExpires(DateTime.Now.AddDays(30));
            Response.Cache.SetMaxAge(TimeSpan.FromDays(30)); // cache on the client for 30 days.
            Response.AddHeader("Content-Length", imageContent.Length.ToString());
            Response.ContentType = PageUtils.MimeTypeLookup(imgExtension);
            Response.BinaryWrite(imageContent);
            Response.Flush();
            Response.End();
        }
    } // BaseShowThumbnailPage

}
