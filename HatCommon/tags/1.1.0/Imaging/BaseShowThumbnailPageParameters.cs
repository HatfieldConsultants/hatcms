using System;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal.Imaging
{
    public class BaseShowThumbnailPageParameters
    {
        public bool useMemoryCache = true;
        public bool useDiskCache = true;
        public string diskCacheStorageDirectoryPath = "";
        public string showThumbDisplayPageUrl = "";
        public string fullSizeImageStorageDir = "";

        public BaseShowThumbnailPageParameters(string ShowThumbDisplayPageUrl, bool UseMemoryCache, bool UseDiskCache, string FullSizeImageStorageDir, string DiskCacheStorageDirectoryPath)
        {
            showThumbDisplayPageUrl = ShowThumbDisplayPageUrl;
            useMemoryCache = UseMemoryCache;
            useDiskCache = UseDiskCache;
            diskCacheStorageDirectoryPath = DiskCacheStorageDirectoryPath;
            fullSizeImageStorageDir = FullSizeImageStorageDir;

        }

    }
}
