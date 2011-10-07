using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal
{
    public class FileUtils
    {
        public static void CopyDirectoryRecursive(string Src, string Dst)
        {
            // taken from http://www.codeproject.com/KB/files/copydirectoriesrecursive.aspx                        
            if (Dst[Dst.Length - 1] != Path.DirectorySeparatorChar)
                Dst += Path.DirectorySeparatorChar;

            if (!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            string[] Files = Directory.GetFileSystemEntries(Src);

            foreach (string Element in Files)
            {
                // Sub directories

                if (Directory.Exists(Element))
                    CopyDirectoryRecursive(Element, Dst + Path.GetFileName(Element));
                // Files in directory

                else
                    File.Copy(Element, Dst + Path.GetFileName(Element), true);
            }

        } // copyDirectoryRecursive

        public static string formatFileSize(long fileSizeBytes)
        {
            return StringUtils.formatFileSize(fileSizeBytes);
        }

        public static string formatFileSize(FileInfo file)
        {
            return StringUtils.formatFileSize(file.Length);
        }


        public static string getFileTypeDescription(FileInfo file)
        {
            return getFileTypeDescription(file.Extension);
        }

        public static string getFileTypeDescription(string fileExtension)
        {
            string ext = fileExtension.ToLower();
            if (ext.StartsWith("."))
                ext = ext.Substring(1);
            string ret = "";
            switch (ext)
            {
                // -- Word
                case "doc": ret = "Word Document"; break;
                case "docx": ret = "Word Document"; break;
                case "docm": ret = "Word Document"; break;
                case "dot": ret = "Word Template"; break;
                case "dotx": ret = "Word Template"; break;
                case "dotm": ret = "Word Template"; break;

                // -- Excel
                case "xls": ret = "Excel Workbook"; break;
                case "xlsx": ret = "Excel Workbook"; break;
                case "xlsm": ret = "Excel Workbook"; break;
                case "xlx": ret = "Excel Template"; break;
                case "xltx": ret = "Excel Template"; break;
                case "xltm": ret = "Excel Template"; break;
                case "xlsb": ret = "Excel Workbook"; break;
                case "xlam": ret = "Excel Add-In"; break;

                // -- PowerPoint
                case "ppt": ret = "PowerPoint Presentation"; break;
                case "pptx": ret = "PowerPoint Presentation"; break;
                case "pptm": ret = "PowerPoint Presentation"; break;
                case "potx": ret = "PowerPoint Template"; break;
                case "pot": ret = "PowerPoint Template"; break;
                case "potm": ret = "PowerPoint Template"; break;
                case "ppam": ret = "PowerPoint Add-In"; break;
                case "pps": ret = "PowerPoint Show"; break;
                case "ppsx": ret = "PowerPoint Show"; break;
                case "ppsm": ret = "PowerPoint Show"; break;

                // -- Access
                case "accdb": ret = "Access Database"; break;
                case "mdb": ret = "Access Database"; break;

                // -- compressed files
                case "zip": ret = "ZIP compressed file"; break;
                case "rar": ret = "RAR compressed file"; break;
                case "7z": ret = "7-Zip Compressed File"; break;
                case "gz": ret = "Gnu Zipped Archive"; break;

                // -- text
                case "txt": ret = "Text file"; break;

                // -- pdf
                case "pdf": ret = "Adode PDF file"; break;
                case "ps": ret = "Postscript file"; break;

                // -- exe
                case "exe": ret = "Executable file"; break;
                case "msi": ret = "Program Installation file"; break;

                case "png": ret = "PNG graphic"; break;
                case "gif": ret = "GIF graphic"; break;
                case "jpg": ret = "JPG graphic"; break;
                case "jpeg": ret = "JPG graphic"; break;
                case "tif": ret = "TIFF graphic"; break;

                // -- Google Earth
                case "kml": ret = "Google Earth Placemarks"; break;
                case "kmz": ret = "Google Earth Placemarks"; break;

                // -- Adobe
                case "ai": ret = "Adobe Illustrator File"; break;
                case "indd": ret = "Adobe InDesign File"; break;

                // -- Audio
                case "mp3": ret = "MP3 Audio File"; break;
                case "wav": ret = "WAVE Audio File"; break;
                case "wma": ret = "Windows Media Audio File"; break;

                // -- Movies
                case "mov": ret = "Quicktime Movie"; break;
                case "avi": ret = "Video Clip"; break;
                case "mp4": ret = "Video Clip"; break;
                case "wmv": ret = "Windows Media Video File"; break;
            } // switch

            return ret;
        }


        public static FileInfo[] SortByName(FileInfo[] filesToSort)
        {
            List<FileInfo> arr = new List<FileInfo>(filesToSort);
            arr.Sort(new FileInfoComparer(FileInfoComparer.SortBy.FileName));
            return arr.ToArray();
        }

        private class FileInfoComparer : IComparer<FileInfo>
        {
            public enum SortBy { FileName };
            public SortBy _sortBy;
            public FileInfoComparer(SortBy sortBy)
            {
                _sortBy = sortBy;
            }
            #region IComparer<FileInfo> Members

            public int Compare(FileInfo x, FileInfo y)
            {
                if (_sortBy == SortBy.FileName)
                    return string.Compare(x.Name, y.Name);
                else
                    return string.Compare(x.Name, y.Name);

            }

            #endregion
        }

        /// <summary>
        ///  Returns the URL of the file, relative to the current web-application's path.
        /// </summary>
        /// <param name="FullFilePath">the full file path (including drive name, path, and filename) of the resource on disk. Eg: "C:\Inetpub\wwwroot\hatCms\UserFiles\Image\(Evening Grosbeak)  J Elser.jpg"</param>
        /// <returns></returns>
        public static string getRelativeUrl(string FullFilePath)
        {
            return PathUtils.getRelativeUrl(FullFilePath);
        }

        /// <summary>
        ///  Returns the URL of the file, relative to the current web-application's path.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static string getRelativeUrl(FileInfo fileInfo)
        {
            return PathUtils.getRelativeUrl(fileInfo);
        }

    }
}
