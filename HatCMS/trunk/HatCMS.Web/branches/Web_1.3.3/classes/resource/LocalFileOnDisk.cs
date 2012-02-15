using System;
using System.Web;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Data;

namespace HatCMS
{
	// Note: look at the Orchard IStorageProvider interface as being a nicer way to implement this: http://orchard.codeplex.com/SourceControl/changeset/view/48ba0ed3ac80#src%2fOrchard%2fFileSystems%2fMedia%2fIStorageProvider.cs
	public class CmsLocalFileOnDisk
    {
        public int autoincid;        

        protected int resourceid;
        /// <summary>
        /// the unique identifier for this File.
        /// </summary>
        public int ResourceId
        {
            get { return resourceid; }
            set { resourceid = value; }
        }

        protected int revisionnumber;
        public int RevisionNumber
        {
            get { return revisionnumber; }
            set { revisionnumber = value; }
        }

        protected string filename;
        /// <summary>
        /// the name of the file (does NOT contain the full path of the file!!)
        /// </summary>
        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }

        protected string filepath;
        /// <summary>
        /// the full file path (including drive name, path, and filename) of the resource on disk. Eg: "C:\Inetpub\wwwroot\hatCms\UserFiles\Image\(Evening Grosbeak)  J Elser.jpg"
        /// </summary>
        public string FilePath
        {
            get { return filepath; }
            set { filepath = value; }
        }

        protected string fileDirectory;
        /// <summary>
        /// the directory part of the file path of the resource on disk
        /// Eg: "C:\Inetpub\wwwroot\hatCms\UserFiles\Image".
        /// </summary>
        public string FileDirectory
        {
            get { return fileDirectory; }
            set { fileDirectory = value; }
        }

        protected long filesize;
        /// <summary>
        /// the size of the file, in bytes
        /// </summary>
        public long FileSize
        {
            get { return filesize; }
            set { filesize = value; }
        }

        protected DateTime filetimestamp;
        /// <summary>
        /// the timestamp that the file was last modified at
        /// </summary>
        public DateTime FileTimestamp
        {
            get { return filetimestamp; }
            set { filetimestamp = value; }
        }

        protected string mimetype;
        public string MimeType
        {
            get { return mimetype; }
            set { mimetype = value; }
        }

        protected int modifiedby;
        /// <summary>
        /// the username that last modified this record
        /// </summary>
        public int ModifiedByUserId
        {
            get { return modifiedby; }
            set { modifiedby = value; }
        }

        protected DateTime modificationdate;
        /// <summary>
        /// the date that this database record was last modified (not the date that the file on disk was last modified)
        /// </summary>
        public DateTime ModificationDate
        {
            get { return modificationdate; }
            set { modificationdate = value; }
        }

        protected List<CmsLocalFileOnDiskMetaItem> metaData;
        public CmsLocalFileOnDiskMetaItem[] MetaData
        {
            get { return metaData.ToArray(); }
            set { metaData.Clear(); metaData.AddRange(value); }
        }

        public CmsLocalFileOnDisk()
        {
            autoincid = -1;
            resourceid = -1;
            revisionnumber = -1;
            filename = "";
            filepath = "";
            fileDirectory = "";
            filesize = -1;
            filetimestamp = DateTime.MinValue;
            mimetype = "";
            modifiedby = -1;
            modificationdate = DateTime.MinValue;
            metaData = new List<CmsLocalFileOnDiskMetaItem>();
        } // constructor

        public string getUrl()
        {
            return getUrl(System.Web.HttpContext.Current);
        }

        /// <summary>
        /// derive the file storage folder path
        /// </summary>
        /// <param name="aggregatorPage"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static string getDMSStorageFolderUrl(CmsPage fileDetailsPage, int identifier, CmsLanguage language)
        {
            string DMSFileStorageFolderUrl = CmsConfig.getConfigValue("DMSFileStorageFolderUrl", "");

            DMSFileStorageFolderUrl = VirtualPathUtility.ToAbsolute(DMSFileStorageFolderUrl);
            DMSFileStorageFolderUrl = VirtualPathUtility.AppendTrailingSlash(DMSFileStorageFolderUrl);

            string subDir = "";
            if (CmsConfig.getConfigValue("DMSFileStorageLocationVersion", "V1") == "V2")
                subDir = fileDetailsPage.ID.ToString() + identifier.ToString() + language.shortCode.ToLower() + "/";

            return DMSFileStorageFolderUrl + subDir;
        }

        public static string getDMSStorageFilenameOnDisk(CmsPage pageLinkedToFile, int identifier, CmsLanguage language, string userFilename)
        {
            string prependToFilename = "";
            if (CmsConfig.getConfigValue("DMSFileStorageLocationVersion", "V1") == "V1")
                prependToFilename = pageLinkedToFile.ID.ToString() + identifier.ToString();

            string baseUrl = getDMSStorageFolderUrl(pageLinkedToFile, identifier, language);

            string fn = baseUrl + prependToFilename + userFilename;
            string fnOnDisk = System.Web.Hosting.HostingEnvironment.MapPath(fn);
            return fnOnDisk;
        }

        public static string getDMSDownloadUrl(CmsPage pageLinkedToFile, int identifier, CmsLanguage language, string fileName)
        {
            string baseUrl = getDMSStorageFolderUrl(pageLinkedToFile, identifier, language);
            string url = baseUrl + fileName;
            return url;
        }

        public string getUrl(System.Web.HttpContext context)
        {
            string rootPath = System.Web.Hosting.HostingEnvironment.MapPath(CmsContext.ApplicationPath);
            string url = PathUtils.RelativePathTo(rootPath, this.FilePath);

            if (url.StartsWith("..\\"))
                url = url.Substring(2); // remove ".."

            url = url.Replace("\\", "/");
            return url;

        }

        



        public void setMetaDataValue(string metaName, string newValue)
        {
            removeAllMetaDataWithName(metaName);

            metaData.Add(new CmsLocalFileOnDiskMetaItem(this, metaName, newValue));

        }

        public void setMetaDataValue(string metaName, int newValue)
        {
            removeAllMetaDataWithName(metaName);

            metaData.Add(new CmsLocalFileOnDiskMetaItem(this, metaName, newValue.ToString()));
        }

        public void setMetaDataValue(string metaName, double newValue)
        {
            removeAllMetaDataWithName(metaName);

            metaData.Add(new CmsLocalFileOnDiskMetaItem(this, metaName, newValue.ToString()));
        }

        public void setMetaDataValue(string metaName, bool newValue)
        {
            removeAllMetaDataWithName(metaName);

            metaData.Add(new CmsLocalFileOnDiskMetaItem(this, metaName, newValue.ToString()));
        }


        public string getMetaDataValue(string metaDataKey, string valueToReturnOnNotFoundOrError)
        {
            CmsLocalFileOnDiskMetaItem[] arr = getMetaDataByName(metaDataKey);
            if (arr.Length == 1)
                return arr[0].ItemValue;
            return valueToReturnOnNotFoundOrError;
        }

        public int getMetaDataValue(string metaDataKey, int valueToReturnOnNotFoundOrError)
        {
            CmsLocalFileOnDiskMetaItem[] arr = getMetaDataByName(metaDataKey);
            if (arr.Length == 1)
            {
                try
                {
                    return Convert.ToInt32(arr[0].ItemValue);
                }
                catch
                { }
            }
            return valueToReturnOnNotFoundOrError;
        }

        public double getMetaDataValue(string metaDataKey, double valueToReturnOnNotFoundOrError)
        {
            CmsLocalFileOnDiskMetaItem[] arr = getMetaDataByName(metaDataKey);
            if (arr.Length == 1)
            {
                try
                {
                    return Convert.ToDouble(arr[0].ItemValue);
                }
                catch
                { }
            }
            return valueToReturnOnNotFoundOrError;
        }

        public bool getMetaDataValue(string metaDataKey, bool valueToReturnOnNotFoundOrError)
        {
            CmsLocalFileOnDiskMetaItem[] arr = getMetaDataByName(metaDataKey);
            if (arr.Length == 1)
            {
                try
                {
                    return Convert.ToBoolean(arr[0].ItemValue);
                }
                catch
                { }
            }
            return valueToReturnOnNotFoundOrError;
        }

        public CmsLocalFileOnDiskMetaItem[] getMetaDataByName(string metaName)
        {
            List<CmsLocalFileOnDiskMetaItem> ret = new List<CmsLocalFileOnDiskMetaItem>();
            foreach (CmsLocalFileOnDiskMetaItem mi in MetaData)
            {
                if (String.Compare(metaName, mi.Name, true) == 0)
                    ret.Add(mi);

            }
            return ret.ToArray();
        }

        public void removeAllMetaDataWithName(string metaName)
        {
            List<CmsLocalFileOnDiskMetaItem> itemsToRemove = new List<CmsLocalFileOnDiskMetaItem>();
            foreach (CmsLocalFileOnDiskMetaItem mi in MetaData)
            {
                if (String.Compare(metaName, mi.Name, true) == 0)
                    itemsToRemove.Add(mi);

            }
            foreach (CmsLocalFileOnDiskMetaItem mi in itemsToRemove)
                metaData.Remove(mi);
        }

        /// <summary>
        /// get a list of all valid revision numbers for this resource, sorted so that the oldest Rev# is first ([0])
        /// </summary>
        /// <returns></returns>
        public int[] getAllRevisionNumbers()
        {
            CmsResourceDB db = new CmsResourceDB();
            int[] revNums = db.getAllRevisionNumbers(this);
            List<int> ret = new List<int>(revNums);
            ret.Sort();
            return ret.ToArray();
        }

        public bool SaveToDatabase()
        {
            if (autoincid < 0)
            {
                return (new CmsResourceDB()).Insert(this);
            }
            else
            {
                return (new CmsResourceDB()).InsertNewRevision(this);
            }
        } // SaveToDatabase

        public static int getLastExistingRevisionNumber(int ResourceId)
        {
            return (new CmsResourceDB()).getLastExistingRevisionNumber(ResourceId);
        }

        /// <summary>
        /// returns a blank (newly created) CmsResource if not found
        /// </summary>
        /// <param name="ResourceId"></param>
        /// <returns></returns>
        public static CmsLocalFileOnDisk FetchLastRevision(int ResourceId)
        {
            return (new CmsResourceDB()).GetLastRevision(ResourceId);
        } // get

        public static CmsLocalFileOnDisk Fetch(int ResourceId, int RevisionNumber)
        {
            return (new CmsResourceDB()).Get(ResourceId, RevisionNumber);
        } // get
               

        public static bool Delete(CmsLocalFileOnDisk resource, bool deletePhysicalFiles)
        {
            return (new CmsResourceDB()).Delete(resource.resourceid, resource.revisionnumber, deletePhysicalFiles);
        } // Delete

        


        /// <summary>
        /// creates a new CmsResource object from an OLE file (ie old-style Word, Excel, PPT files)
        /// </summary>
        /// <param name="imageFilename"></param>
        /// <returns></returns>
        public static CmsLocalFileOnDisk CreateFomOLEDocument(string oleFilename)
        {
            CmsLocalFileOnDisk ret = new CmsLocalFileOnDisk();

            ret.filename = Path.GetFileName(oleFilename);
            ret.filepath = oleFilename;
            ret.fileDirectory = Path.GetDirectoryName(oleFilename);
            ret.MimeType = PageUtils.MimeTypeLookup(Path.GetExtension(oleFilename));

            if (File.Exists(oleFilename))
            {
                FileInfo fi = new FileInfo(oleFilename);


                ret.FileSize = fi.Length;
                ret.FileTimestamp = fi.LastWriteTime;

                MetaDataItem[] metaData = MetaDataUtils.GetFromOLEDocument(oleFilename);
                ret.MetaData = CmsLocalFileOnDiskMetaItem.FromMetaDataItems(ret, metaData);

            }

            return ret;
        }

        public static CmsLocalFileOnDisk CreateFromFile(string filenameOnDisk)
        {
            CmsLocalFileOnDisk ret = new CmsLocalFileOnDisk();

            ret.filename = Path.GetFileName(filenameOnDisk);
            ret.filepath = filenameOnDisk;
            ret.fileDirectory = Path.GetDirectoryName(filenameOnDisk);
            ret.MimeType = PageUtils.MimeTypeLookup(Path.GetExtension(filenameOnDisk));

            if (File.Exists(filenameOnDisk))
            {
                FileInfo fi = new FileInfo(filenameOnDisk);


                ret.FileSize = fi.Length;
                ret.FileTimestamp = fi.LastWriteTime;

                MetaDataItem[] metaData = new MetaDataItem[0];
                if (MetaDataUtils.CanExtractImageMetaData(filenameOnDisk))
                    metaData = MetaDataUtils.GetFromImageFile(filenameOnDisk);
                else if (MetaDataUtils.CanExtractOLEMetaData(filenameOnDisk))
                    metaData = MetaDataUtils.GetFromOLEDocument(filenameOnDisk);
                else if (MetaDataUtils.CanExtractXmpData(filenameOnDisk))
                    metaData = MetaDataUtils.GetXmpData(filenameOnDisk);

                ret.MetaData = CmsLocalFileOnDiskMetaItem.FromMetaDataItems(ret, metaData);

            }

            return ret;
        }

        /// <summary>
        /// creates a new CmsResource object from an Adobe file (is PDF, PSD, AI)
        /// </summary>
        /// <param name="imageFilename"></param>
        /// <returns></returns>
        public static CmsLocalFileOnDisk CreateFomAdobeDocument(string adobeFilename)
        {
            CmsLocalFileOnDisk ret = new CmsLocalFileOnDisk();

            ret.filename = Path.GetFileName(adobeFilename);
            ret.filepath = adobeFilename;
            ret.fileDirectory = Path.GetDirectoryName(adobeFilename);
            ret.MimeType = PageUtils.MimeTypeLookup(Path.GetExtension(adobeFilename));

            if (File.Exists(adobeFilename))
            {
                FileInfo fi = new FileInfo(adobeFilename);


                ret.FileSize = fi.Length;
                ret.FileTimestamp = fi.LastWriteTime;

                MetaDataItem[] metaData = MetaDataUtils.GetXmpData(adobeFilename);
                ret.MetaData = CmsLocalFileOnDiskMetaItem.FromMetaDataItems(ret, metaData);

            }

            return ret;
        }

        public static bool FileWithFilenameExists(string filename)
        {
            CmsLocalFileOnDisk res = FetchByFilename(filename);
            if (res.FilePath == filename)
                return true;
            return false;
        }

        /// <summary>
        /// returns a blank (newly created) CmsResource object if not found
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="resourcesToSearchIn"></param>
        /// <returns></returns>
        public static CmsLocalFileOnDisk GetByFilename(string filename, CmsLocalFileOnDisk[] haystack)
        {
            foreach (CmsLocalFileOnDisk res in haystack)
            {
                if (String.Compare(filename, res.FilePath, true) == 0)
                    return res;
            }

            return new CmsLocalFileOnDisk();
        }

        /// <summary>
        /// returns a blank (newly created) CmsResource object if not found
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static CmsLocalFileOnDisk FetchByFilename(string filename)
        {
            return (new CmsResourceDB()).GetResourceByFilename(filename);
        }

        /// <summary>
        /// does an "AND" query for all metaDataNamesAndValuesToMatch. To do an "OR" query, use FetchAllFilesWithAnyMetaData
        /// </summary>
        /// <param name="metaDataNamesAndValuesToMatch"></param>
        /// <returns></returns>
        public static CmsLocalFileOnDisk[] FetchAllFilesByMetaData(Dictionary<string, string> metaDataNamesAndValuesToMatch)
        {
            return (new CmsResourceDB()).FetchAllFilesByMetaData(metaDataNamesAndValuesToMatch);
        }

        /// <summary>
        /// does an "OR" query for all metaDataNamesAndValuesToPossiblyMatch. To do an "AND" query, use FetchAllFilesWithMetaData
        /// </summary>
        /// <param name="metaDataNamesAndValuesToPossiblyMatch"></param>
        /// <returns></returns>
        public static CmsLocalFileOnDisk[] FetchAllFilesByAnyMetaData(Dictionary<string, string> metaDataNamesAndValuesToPossiblyMatch)
        {
            return (new CmsResourceDB()).FetchAllFilesByAnyMetaData(metaDataNamesAndValuesToPossiblyMatch);
        }

        public static CmsLocalFileOnDisk[] FetchAllFilesInDirectory(string directoryPath, string[] fileExtensions)
        {
            return (new CmsResourceDB()).FetchFilesInDirectory(directoryPath, fileExtensions);
        }

        public static CmsLocalFileOnDisk[] FetchAllFilesInDirectory(string directoryPath)
        {
            return (new CmsResourceDB()).FetchFilesInDirectory(directoryPath, new string[0]);
        }

        public static string DeletedFileFilenamePrefix = "..DELETED..";


        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <returns></returns>
        public static CmsLocalFileOnDisk[] UpdateFolderInDatabase(DirectoryInfo directoryInfo)
        {
            List<CmsLocalFileOnDisk> existingResources = new List<CmsLocalFileOnDisk>(FetchAllFilesInDirectory(directoryInfo.FullName));

            List<CmsLocalFileOnDisk> ret = new List<CmsLocalFileOnDisk>();

            foreach (FileInfo fi in directoryInfo.GetFiles())
            {
                if (fi.Name.StartsWith(DeletedFileFilenamePrefix)) // skip deleted files
                    continue;

                CmsLocalFileOnDisk res = GetByFilename(fi.FullName, existingResources.ToArray());
                bool doSave = false;
                if (res.FilePath == fi.FullName)
                {
                    if (res.FileSize != fi.Length || res.FileTimestamp.ToString("yyyy-MM-dd HH:mm:ss") != fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"))
                    {
                        // update file
                        CmsLocalFileOnDisk resBak = res;
                        res = CreateFromFile(fi.FullName);

                        res.autoincid = resBak.autoincid;
                        res.resourceid = resBak.resourceid;
                        res.revisionnumber = resBak.revisionnumber;

                        doSave = true;
                    }
                    else
                        doSave = false; // filetime has not changed
                }
                else
                {
                    // insert
                    res = CreateFromFile(fi.FullName);
                    doSave = true;
                }


                if (doSave)
                {
                    if (!res.SaveToDatabase())
                        return new CmsLocalFileOnDisk[0];
                }
                else
                {
                    int idToRemove = -1;
                    for (int i = 0; i < existingResources.Count; i++)
                    {
                        if (String.Compare(existingResources[i].FilePath, res.FilePath, true) == 0)
                        {
                            idToRemove = i;
                            break;
                        }
                    } // for
                    if (idToRemove >= 0)
                        existingResources.RemoveAt(idToRemove);
                }
                ret.Add(res);
            } // foreach

            // -- remove other existingResources that no longer exist on disk
            foreach (CmsLocalFileOnDisk res in existingResources)
            {
                CmsLocalFileOnDisk.Delete(res, false);
            }

            
            return ret.ToArray();
        }


        #region CmsResourceDB
        protected class CmsResourceDB : Hatfield.Web.Portal.Data.MySqlDbObject
        {
            public CmsResourceDB()
                : base(ConfigUtils.getConfigValue("ConnectionString", ""))
            { }

            

            public bool Insert(CmsLocalFileOnDisk item)
            {
                if (item.ResourceId < 0)
                {
                    int lastResId = getLastExistingResourceId();
                    if (lastResId < 0)
                        item.ResourceId = 0;
                    else
                        item.ResourceId = lastResId+1;
                }
                
                if (item.revisionnumber < 0)
                    item.revisionnumber = 1;

                int lastModifiedBy = -1;
                if (CmsContext.currentUserIsLoggedIn)
                    lastModifiedBy = CmsContext.currentWebPortalUser.uid;

                
                string sql = "INSERT INTO resourceitems ";
                sql += "(ResourceId, RevisionNumber, Filename, FilePath, FileDirectory, FileSize, FileTimestamp, MimeType, ModifiedBy, ModificationDate)";
                sql += " VALUES ( ";
                sql += item.resourceid.ToString() + ", ";
                sql += item.revisionnumber.ToString() + ", ";
                sql += "'" + dbEncode(item.filename) + "'" + ", ";
                sql += "'" + dbEncode(item.filepath) + "'" + ", ";
                sql += "'" + dbEncode(item.fileDirectory) + "'" + ", ";
                sql += item.filesize.ToString() + ", ";
                sql += "" + dbEncode(item.filetimestamp) + " " + ", "; 
                sql += "'" + dbEncode(item.mimetype) + "'" + ", ";
                sql += "'" + dbEncode(lastModifiedBy.ToString()) + "'" + ", ";
                sql += "" + dbEncode(DateTime.Now) + " " + " ";                

                sql += " ); ";

                int newId = this.RunInsertQuery(sql);
                if (newId > -1)
                {
                    item.autoincid = newId;

                    return CmsLocalFileOnDiskMetaItem.BulkInsert(item, item.MetaData);
                                        
                }
                return false;

            } // Insert
           

            public bool InsertNewRevision(CmsLocalFileOnDisk item)
            {
                int lastRevNum = getLastExistingRevisionNumber(item.ResourceId);
                int nextRevNum = 1;
                if (lastRevNum > 0)
                    nextRevNum = lastRevNum + 1;

                item.revisionnumber = nextRevNum;

                return Insert(item);

            } // Update


            public bool Delete(int ResourceId, int RevisionNumber, bool deletePhysicalFiles)
            {
                CmsLocalFileOnDisk resToDelete = Get(ResourceId, RevisionNumber);
                if (resToDelete.ResourceId != ResourceId)
                    return false;

                string renamedFilename = "";
                string renamedFilenameOnDisk = "";

                // remove all derivative thumbnail images
                if (deletePhysicalFiles)
                {
                    foreach (CmsLocalFileOnDiskMetaItem meta in resToDelete.MetaData)
                    {
                        if (meta.Name.StartsWith("IMAGEThumb") && meta.Name.EndsWith("URL") && meta.ItemValue.IndexOf(".aspx") == -1)
                        {

                            string thumbFilename = System.Web.Hosting.HostingEnvironment.MapPath(meta.ItemValue);
                            try
                            {
                                File.Delete(thumbFilename);
                            }
                            catch
                            { }
                        } // if
                    } // foreach

                    // -- rename the actual file on disk
                    renamedFilename = DeletedFileFilenamePrefix + DateTime.Now.ToString("yyyyMMdd.HH.mm.ss.") + resToDelete.FileName;
                    string currentFilenameOnDisk = resToDelete.FilePath;
                    renamedFilenameOnDisk = Path.GetDirectoryName(currentFilenameOnDisk) + Path.DirectorySeparatorChar + renamedFilename;

                    try
                    {
                        if (System.IO.File.Exists(currentFilenameOnDisk))
                        {
                            System.IO.File.Move(currentFilenameOnDisk, renamedFilenameOnDisk);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                        return false;
                    }
                } // if deletePhysicalFiles
                else
                {
                    renamedFilename = resToDelete.filename;
                    renamedFilenameOnDisk = resToDelete.filepath;
                }
                // update the database
                
                string sql = "UPDATE resourceitems ";
                sql += " set deleted = " + dbEncode(DateTime.Now) + ", Filename = '"+dbEncode(renamedFilename)+"', FilePath = '"+dbEncode(renamedFilenameOnDisk)+"' ";
                sql += " WHERE ResourceId = " + ResourceId.ToString() + " AND RevisionNumber = " + RevisionNumber.ToString() + " ";
                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected < 0)
                {
                    return false;
                }
                return true;
            }

            protected CmsLocalFileOnDisk ResourceFromDataRow(DataRow dr)
            {
                CmsLocalFileOnDisk item = new CmsLocalFileOnDisk();
                item.autoincid = Convert.ToInt32(dr["AutoIncId"]);

                item.resourceid = Convert.ToInt32(dr["ResourceId"]);

                item.revisionnumber = Convert.ToInt32(dr["RevisionNumber"]);

                item.filename = (dr["Filename"]).ToString();

                item.filepath = (dr["FilePath"]).ToString();

                item.fileDirectory = (dr["FileDirectory"]).ToString();

                item.filesize = Convert.ToInt64(dr["FileSize"]);
                
                item.filetimestamp = Convert.ToDateTime(dr["FileTimestamp"]);

                item.mimetype = (dr["MimeType"]).ToString();

                item.modifiedby = Convert.ToInt32(dr["ModifiedBy"]);

                item.modificationdate = Convert.ToDateTime(dr["ModificationDate"]);

                return item;
            }

            /// <summary>
            /// returns a blank CmsResource object if not found
            /// </summary>
            /// <param name="ResourceId"></param>
            /// <param name="RevisionNumber"></param>
            /// <returns></returns>
            public CmsLocalFileOnDisk Get(int ResourceId, int RevisionNumber)
            {
                string sql = "SELECT AutoIncId, ResourceId, RevisionNumber, Filename, FilePath, FileDirectory, FileSize, FileTimestamp, MimeType, ModifiedBy, ModificationDate from resourceitems r ";                
                sql += " WHERE ResourceId = " + ResourceId.ToString() + " AND RevisionNumber = " + RevisionNumber.ToString() + " ";
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasSingleRow(ds))
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    CmsLocalFileOnDisk res = ResourceFromDataRow(dr);
                    CmsLocalFileOnDiskMetaItem[] metaItems = CmsLocalFileOnDiskMetaItem.FetchAll(res);
                    res.MetaData = metaItems;
                    return res;
                }
                return new CmsLocalFileOnDisk();
            } // Get

            public CmsLocalFileOnDisk GetResourceByFilename(string filename)
            {
                string sql = "select AutoIncId, ResourceId, RevisionNumber, Filename, FilePath, FileDirectory, FileSize, FileTimestamp, MimeType, ModifiedBy, ModificationDate from resourceitems ";
                sql += " where LOWER(FilePath) = '" + dbEncode(filename.ToLower()) + "' AND deleted is null order by ResourceId, RevisionNumber DESC ; ";

                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        CmsLocalFileOnDisk res = ResourceFromDataRow(dr);
                        int lastExistingRevNum = getLastExistingRevisionNumber(res.ResourceId);

                        if (lastExistingRevNum == res.RevisionNumber)
                        {
                            res.MetaData = CmsLocalFileOnDiskMetaItem.FetchAll(res);
                            return res;
                        }

                    } // foreach
                }
                return new CmsLocalFileOnDisk();
            }


            public CmsLocalFileOnDisk[] FetchAllFilesByAnyMetaData(Dictionary<string, string> metaDataNamesAndValuesToMatch)
            {
                if (metaDataNamesAndValuesToMatch.Keys.Count == 0)
                    return new CmsLocalFileOnDisk[0];

                StringBuilder sql = new StringBuilder();
                sql.Append("select r.AutoIncId, r.ResourceId, r.RevisionNumber, r.Filename, r.FilePath, r.FileDirectory, r.FileSize, r.FileTimestamp, r.MimeType, r.ModifiedBy, r.ModificationDate, ");
                sql.Append(" m.AutoIncId as metaId, m.Name as metaName, m.`Value` as metaValue ");
                sql.Append(" from resourceitems r ");
                sql.Append(" left join resourceitemmetadata m on (m.ResourceId = r.ResourceId) ");
                sql.Append(" where ");
                sql.Append(" RevisionNumber = (select max(RevisionNumber) from resourceitems r2 where r2.resourceid = r.resourceid) ");
                sql.Append(" AND (m.ResourceRevisionNumber = r.RevisionNumber OR m.ResourceRevisionNumber is null) ");
                sql.Append(" AND r.deleted is null and m.deleted is null ");

                List<string> ORs = new List<string>();

                foreach (string metaName in metaDataNamesAndValuesToMatch.Keys)
                {
                    string metaVal = metaDataNamesAndValuesToMatch[metaName];
                    ORs.Add(" (m.Name = '" + dbEncode(metaName) + "' AND m.`Value` = '" + dbEncode(metaVal) + "') ");
                }

                sql.Append(" AND (" + string.Join(" OR ", ORs.ToArray()) + ")");

                sql.Append(" order by ResourceId; ");

                DataSet ds = this.RunSelectQuery(sql.ToString());

                if (this.hasRows(ds))
                {
                    return ResourcesWithMetaItemsFromDataRows(ds.Tables[0].Rows);
                }
                return new CmsLocalFileOnDisk[0];
            }


            public CmsLocalFileOnDisk[] FetchAllFilesByMetaData(Dictionary<string, string> metaDataNamesAndValuesToMatch)
            {
                if (metaDataNamesAndValuesToMatch.Keys.Count == 0)
                    return new CmsLocalFileOnDisk[0];

                StringBuilder sql = new StringBuilder();
                sql.Append("select r.AutoIncId, r.ResourceId, r.RevisionNumber, r.Filename, r.FilePath, r.FileDirectory, r.FileSize, r.FileTimestamp, r.MimeType, r.ModifiedBy, r.ModificationDate, ");
                sql.Append(" m.AutoIncId as metaId, m.Name as metaName, m.`Value` as metaValue ");
                sql.Append(" from resourceitems r ");
                sql.Append(" left join resourceitemmetadata m on (m.ResourceId = r.ResourceId) ");
                sql.Append(" where ");
                sql.Append(" RevisionNumber = (select max(RevisionNumber) from resourceitems r2 where r2.resourceid = r.resourceid) ");
                sql.Append(" AND (m.ResourceRevisionNumber = r.RevisionNumber OR m.ResourceRevisionNumber is null) ");
                sql.Append(" AND r.deleted is null and m.deleted is null ");

                foreach (string metaName in metaDataNamesAndValuesToMatch.Keys)
                {
                    string metaVal = metaDataNamesAndValuesToMatch[metaName];
                    sql.Append(" AND (m.Name = '" + dbEncode(metaName) + "' AND m.`Value` = '" + dbEncode(metaVal) + "') ");
                }

                sql.Append(" order by ResourceId; ");

                DataSet ds = this.RunSelectQuery(sql.ToString());

                if (this.hasRows(ds))
                {
                    return ResourcesWithMetaItemsFromDataRows(ds.Tables[0].Rows);
                }
                return new CmsLocalFileOnDisk[0];
            }

            protected CmsLocalFileOnDisk[] ResourcesWithMetaItemsFromDataRows(DataRowCollection rows)
            {
                Dictionary<int, CmsLocalFileOnDisk> resources = new Dictionary<int, CmsLocalFileOnDisk>();

                foreach (DataRow dr in rows)
                {
                    int resId = Convert.ToInt32(dr["ResourceId"]);
                    CmsLocalFileOnDisk item;
                    if (resources.ContainsKey(resId))
                        item = resources[resId];
                    else
                    {
                        item = new CmsLocalFileOnDisk();
                        item.autoincid = Convert.ToInt32(dr["AutoIncId"]);

                        item.resourceid = resId;

                        item.revisionnumber = Convert.ToInt32(dr["RevisionNumber"]);

                        item.filename = (dr["Filename"]).ToString();

                        item.filepath = (dr["FilePath"]).ToString();

                        item.fileDirectory = (dr["FileDirectory"]).ToString();

                        item.filesize = Convert.ToInt64(dr["FileSize"]);

                        item.filetimestamp = Convert.ToDateTime(dr["FileTimestamp"]);

                        item.mimetype = (dr["MimeType"]).ToString();

                        // -- note: if the following line causes an exception, flush the resourceitems and resourceitemmetadata tables.
                        //          ie: run the following SQL: "truncate table resourceitems; truncate table resourceitemmetadata;"
                        //          all captions will be lost and will need to be re-created.
                        item.modifiedby = Convert.ToInt32(dr["ModifiedBy"]);

                        item.modificationdate = Convert.ToDateTime(dr["ModificationDate"]);
                    }

                    if (dr["metaId"] != DBNull.Value)
                    {
                        CmsLocalFileOnDiskMetaItem metaItem = new CmsLocalFileOnDiskMetaItem();
                        metaItem.autoincid = Convert.ToInt32(dr["metaId"]);
                        metaItem.ResourceId = resId;
                        metaItem.ResourceRevisionNumber = item.RevisionNumber;
                        metaItem.Name = dr["metaName"].ToString();
                        metaItem.ItemValue = dr["metaValue"].ToString();

                        item.metaData.Add(metaItem);
                    }

                    if (!resources.ContainsKey(resId))
                        resources.Add(resId, item);

                } // foreach

                return (new List<CmsLocalFileOnDisk>(resources.Values)).ToArray();
            }

            public CmsLocalFileOnDisk[] FetchFilesInDirectory(string directoryPath, string[] fileExtensions)
            {
                // -- directoryPath should not have a trailing slash.
                string dir = directoryPath;
                if (dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    dir = dir.Substring(0, dir.Length - 1);

                string sql = "select r.AutoIncId, r.ResourceId, r.RevisionNumber, r.Filename, r.FilePath, r.FileDirectory, r.FileSize, r.FileTimestamp, r.MimeType, r.ModifiedBy, r.ModificationDate, ";
                sql += " m.AutoIncId as metaId, m.Name as metaName, m.`Value` as metaValue ";
                sql += " from resourceitems r ";
                sql += " left join resourceitemmetadata m on (m.ResourceId = r.ResourceId) ";
                sql += " where ";
                sql += " RevisionNumber = (select max(RevisionNumber) from resourceitems r2 where r2.resourceid = r.resourceid) ";
                sql += " AND (m.ResourceRevisionNumber = r.RevisionNumber OR m.ResourceRevisionNumber is null) ";
                sql += " AND FileDirectory = '" + dbEncode(dir) + "' AND r.deleted is null and m.deleted is null ";
                if (fileExtensions.Length > 0)
                {
                    sql += " AND ("+ StringUtils.Join(" OR ", fileExtensions, " r.Filename like '%", "'") + ") ";
                }
                sql += " order by ResourceId; ";                

                DataSet ds = this.RunSelectQuery(sql);

                if (this.hasRows(ds))
                {
                    return ResourcesWithMetaItemsFromDataRows(ds.Tables[0].Rows);
                }
                return new CmsLocalFileOnDisk[0];
            }

             
            /// <summary>
            /// returns a newly created (blank) CmsResource if not found
            /// </summary>
            /// <param name="ResourceId"></param>
            /// <returns></returns>
            public CmsLocalFileOnDisk GetLastRevision(int ResourceId)
            {
                int lastExistingRevNum = getLastExistingRevisionNumber(ResourceId);
                if (lastExistingRevNum > 0)
                    return Get(ResourceId, lastExistingRevNum);
                else
                    return new CmsLocalFileOnDisk();
            }

            public int[] getAllRevisionNumbers(CmsLocalFileOnDisk resource)
            {
                string sql = "select RevisionNumber from resourceitems where ResourceId = " + resource.ResourceId+ " and deleted is null order by RevisionNumber; ";
                List<int> ret = new List<int>();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ret.Add(Convert.ToInt32(dr["RevisionNumber"]));
                    } // foreach
                }
                return ret.ToArray();
            }

            /// <summary>
            /// returns int32.MinValue if resourceid not found
            /// </summary>
            /// <param name="ResourceId"></param>
            /// <returns></returns>
            public int getLastExistingRevisionNumber(int ResourceId)
            {
                string sql = "select max(RevisionNumber) as maxRevisionNumber from resourceitems where ResourceId = " + ResourceId + " ; ";
                int ret = Int32.MinValue;
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasSingleRow(ds))
                {
                    
                    return getPossiblyNullValue(ds.Tables[0].Rows[0],"maxRevisionNumber", Int32.MinValue);
                }
                return Int32.MinValue;

            }

            /// <summary>
            /// returns Int32.MinValue if failed.
            /// </summary>
            /// <returns></returns>
            protected int getLastExistingResourceId()
            {
                string sql = "select max(ResourceId) as maxResourceId from resourceitems ; "; // note: do not filter out deleted items!
                int ret = Int32.MinValue;
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasSingleRow(ds))
                {
                    return getPossiblyNullValue(ds.Tables[0].Rows[0],"maxResourceId", Int32.MinValue);
                }
                return Int32.MinValue;
            }
            

        } // class CmsResourceDB
        #endregion
    } // class CmsResource
} // namespace

