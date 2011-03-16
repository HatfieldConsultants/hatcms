using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS
{
    public class CmsLocalImageOnDisk: CmsLocalFileOnDisk
    {
        public CmsLocalImageOnDisk(): base ()
        { } // Constructor

        public CmsLocalImageOnDisk(CmsLocalFileOnDisk baseFile)
        {
            autoincid = baseFile.autoincid;
            resourceid = baseFile.ResourceId;
            revisionnumber = baseFile.RevisionNumber;
            filename = baseFile.FileName;
            filepath =baseFile.FilePath;
            fileDirectory = baseFile.FileDirectory;
            filesize = baseFile.FileSize;
            filetimestamp = baseFile.FileTimestamp;
            mimetype = baseFile.MimeType;
            modifiedby = baseFile.ModifiedByUserId.ToString();
            modificationdate = baseFile.ModificationDate;
            MetaData = baseFile.MetaData;
        }

        public string getThumbnailMetaDataNameRoot(int displayBoxWidth, int displayBoxHeight)
        {
            if (displayBoxWidth <= 0)
                displayBoxWidth = -1;
            if (displayBoxHeight <= 0)
                displayBoxHeight = -1;

            return "IMAGEThumb(" + displayBoxWidth + "," + displayBoxHeight + ")";
        }


        /// <summary>
        /// returns String.Empty if display URL is not set
        /// </summary>
        /// <param name="displayBoxWidth"></param>
        /// <param name="displayBoxHeight"></param>
        /// <returns></returns>
        public string getThumbDisplayUrl(int displayBoxWidth, int displayBoxHeight)
        {
            string metaName = getThumbnailMetaDataNameRoot(displayBoxWidth, displayBoxHeight);
            metaName = metaName + "_URL";
            CmsLocalFileOnDiskMetaItem[] metaItems = getMetaDataByName(metaName);
            if (metaItems.Length > 0)
                return metaItems[0].ItemValue;
            else
                return String.Empty;
        }

        /// <summary>
        /// returns TRUE if metaData set. Note: does NOT update the database!
        /// </summary>
        /// <param name="displayBoxWidth"></param>
        /// <param name="displayBoxHeight"></param>
        /// <param name="thumbUrl"></param>
        /// <returns></returns>
        public bool setThumbDisplayUrl(int displayBoxWidth, int displayBoxHeight, string thumbUrl)
        {
            if (thumbUrl.Trim() == "")
                return false;
            string metaName = getThumbnailMetaDataNameRoot(displayBoxWidth, displayBoxHeight);
            metaName = metaName + "_URL";

            setMetaDataValue(metaName, thumbUrl);

            return true;
        }


        /// <summary>
        /// returns string.empty if no caption found
        /// </summary>
        /// <returns></returns>
        public string getImageCaption()
        {
            return getMetaDataValue("Xmp:Description", string.Empty);

        }

        /// <summary>
        /// returns TRUE if metaAta set. Note: does NOT update the database!
        /// </summary>
        /// <param name="newCaption"></param>
        /// <returns></returns>
        public bool setImageCaption(string newCaption)
        {
            setMetaDataValue("Xmp:Description", newCaption);

            return true;
        }

        /// <summary>
        /// returns a zero length array on error or failure
        /// </summary>
        /// <returns></returns>
        public int[] getImageDimensions()
        {
            try
            {
                CmsLocalFileOnDiskMetaItem[] widthItems = getMetaDataByName("IMAGE:Width");
                CmsLocalFileOnDiskMetaItem[] heightItems = getMetaDataByName("IMAGE:Height");
                if (widthItems.Length >= 1 && heightItems.Length >= 1)
                {
                    return new int[] { Convert.ToInt32(widthItems[0].ItemValue), Convert.ToInt32(heightItems[0].ItemValue) };
                }
            }
            catch
            { }
            return new int[0];
        }

        /// <summary>
        /// Note: does not save the image dimensions to the database.
        /// </summary>
        /// <param name="dimensions"></param>
        /// <returns></returns>
        public bool setImageDimensions(int[] dimensions)
        {
            if (dimensions.Length != 2)
                return false;


            removeAllMetaDataWithName("IMAGE:Width");
            removeAllMetaDataWithName("IMAGE:Height");

            metaData.Add(new CmsLocalFileOnDiskMetaItem(this, "IMAGE:Width", dimensions[0].ToString()));
            metaData.Add(new CmsLocalFileOnDiskMetaItem(this, "IMAGE:Height", dimensions[1].ToString()));

            return true;
        }

        /// <summary>
        /// gets the &ltimg&gt; tag for this image.
        /// Note: has the side-effect of updating the database if the thumbUrl isn't previously stored.
        /// </summary>
        /// <param name="displayWidth"></param>
        /// <param name="displayHeight"></param>
        /// <param name="CssClassName"></param>
        /// <returns></returns>
        public string getImageHtmlTag(int displayWidth, int displayHeight, string CssClassName)
        {

            if (displayWidth <= 0)
                displayWidth = -1;
            if (displayHeight <= 0)
                displayHeight = -1;

            bool updateResource = false;

            // -- create the image's display Url	            
            string thumbUrl = this.getThumbDisplayUrl(displayWidth, displayHeight);
            if (thumbUrl == "")
            {
                thumbUrl = showThumbPage.getThumbDisplayUrl(this, displayWidth, displayHeight);
                if (thumbUrl.ToLower().IndexOf(".aspx") < 0)
                {
                    this.setThumbDisplayUrl(displayWidth, displayHeight, thumbUrl);
                    updateResource = true;
                }

            }

            string imgId = "file_" + this.ResourceId.ToString();
            // int[] dimensions = InlineImageBrowser2.getImageDimensions(fi.FullName);

            System.Drawing.Size sz = showThumbPage.getDisplayWidthAndHeight(this, displayWidth, displayHeight);
            string html = "";
            if (sz.IsEmpty)
            {
                html = ("<img id=\"" + imgId + "\" class=\"" + CssClassName + "\" src=\"" + thumbUrl + "\" />");
            }
            else
            {
                html = ("<img id=\"" + imgId + "\" class=\"" + CssClassName + "\" width=\"" + sz.Width + "\" height=\"" + sz.Height + "\" src=\"" + thumbUrl + "\" />");
            }


            if (updateResource)
            {
                bool b = this.SaveToDatabase();
                if (!b)
                    Console.Write("resource failed to update.");
            }

            return html;
        }

        /// <summary>
        /// creates a new CmsResource object from an image file
        /// </summary>
        /// <param name="imageFilename"></param>
        /// <returns></returns>
        public static CmsLocalImageOnDisk CreateFomImageFile(string imageFilename)
        {
            CmsLocalImageOnDisk ret = new CmsLocalImageOnDisk();

            ret.FileName = Path.GetFileName(imageFilename);
            ret.FilePath = imageFilename;
            ret.FileDirectory = Path.GetDirectoryName(imageFilename);
            ret.MimeType = PageUtils.MimeTypeLookup(Path.GetExtension(imageFilename));

            if (File.Exists(imageFilename))
            {
                FileInfo fi = new FileInfo(imageFilename);

                ret.FileSize = fi.Length;
                ret.FileTimestamp = fi.LastWriteTime;

                MetaDataItem[] metaData = MetaDataUtils.GetFromImageFile(imageFilename);
                ret.MetaData = CmsLocalFileOnDiskMetaItem.FromMetaDataItems(ret, metaData);

            }

            return ret;
        } // CreateFromImageFile

        public static CmsLocalImageOnDisk[] FetchAllImagesInDirectory(string directoryPath, string[] fileExtensions)
        {
            CmsLocalFileOnDisk[] fileArray = CmsLocalFileOnDisk.FetchAllFilesInDirectory(directoryPath, fileExtensions);
            CmsLocalImageOnDisk[] imgArray = new CmsLocalImageOnDisk[fileArray.Length];
            for(int x= 0; x< fileArray.Length; x++)
                imgArray[x] = (CmsLocalImageOnDisk)(fileArray[x]);
            return imgArray;
        }

        public static CmsLocalImageOnDisk[] FetchAllImagesInDirectory(string directoryPath)
        {
            CmsLocalFileOnDisk[] fileArray = CmsLocalFileOnDisk.FetchAllFilesInDirectory(directoryPath);
            CmsLocalImageOnDisk[] imgArray = new CmsLocalImageOnDisk[fileArray.Length];
            for (int x = 0; x < fileArray.Length; x++)
                imgArray[x] = (CmsLocalImageOnDisk)(fileArray[x]);
            return imgArray;
        }


        public static int DeleteAllCachedThumbnailUrls()
        {
            return (new CmsImageResourceDB()).DeleteAllCachedThumbnailUrls();
        }

        public static CmsLocalImageOnDisk[] FromFileArray(CmsLocalFileOnDisk[] fileArray)
        {
            Converter<CmsLocalFileOnDisk, CmsLocalImageOnDisk> func = new Converter<CmsLocalFileOnDisk, CmsLocalImageOnDisk>(ConvertFileToImageObj);
            return Array.ConvertAll(fileArray, func);
        }

        public static CmsLocalImageOnDisk ConvertFileToImageObj(CmsLocalFileOnDisk file)
        {
            return new CmsLocalImageOnDisk(file);            
        }


        public static new CmsLocalImageOnDisk[] UpdateFolderInDatabase(DirectoryInfo di)
        {
            CmsLocalFileOnDisk[] arr = CmsLocalFileOnDisk.UpdateFolderInDatabase(di);
            return FromFileArray(arr);
        }

#region CmsImageResourceDB
        private class CmsImageResourceDB : Hatfield.Web.Portal.Data.MySqlDbObject
        {
            public CmsImageResourceDB()
                : base(ConfigUtils.getConfigValue("ConnectionString", ""))
            { }


            public int DeleteAllCachedThumbnailUrls()
            {
                int width = 200;
                int height = 400;
                string name = (new CmsLocalImageOnDisk()).getThumbnailMetaDataNameRoot(width, height);
                name = name.Replace(width.ToString(), "%");
                name = name.Replace(height.ToString(), "%");
                name += "%";

                string sql = "update resourceitemmetadata set deleted = NOW() where `name` like '" + dbEncode(name) + "'; ";
                int numUpdated = RunUpdateQuery(sql);
                return numUpdated;

            }
        }
#endregion

    }
}
