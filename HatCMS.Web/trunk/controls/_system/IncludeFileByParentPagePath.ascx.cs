using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Controls
{
    /// <summary>
    /// example: ##RenderControl(IncludeFileByParentPagePath,/=~/UserFiles/File/InteractiveFeatures/home.txt,/ramp=~/UserFiles/File/InteractiveFeatures/ramp.txt,/river=~/UserFiles/File/InteractiveFeatures/river.txt,/people=~/UserFiles/File/InteractiveFeatures/people.txt,/resources=~/UserFiles/File/InteractiveFeatures/resources.txt,/management=~/UserFiles/File/InteractiveFeatures/management.txt)##
    /// </summary>
    public partial class IncludeFileByParentPagePath : System.Web.UI.UserControl
    {
        private const string RandomFilePathKeyValue = "{*}";

        
        /// <summary>
        /// crawls up the page path to see if anything has been configured for that page.
        /// if nothing found, returns String.Empty.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private string getFilePathByPagePath(CmsPage page)
        {
            CmsPage currPage = page;
            string notFoundValue = Guid.NewGuid().ToString();            
            while (currPage.Id != -1)
            {
                string PagePath = currPage.Path;
                string keyVal = CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, PagePath, notFoundValue);
                if (keyVal != notFoundValue)
                {
                    return keyVal;
                }

                currPage = currPage.ParentPage;
            } // while

            return String.Empty;
        } // getFilePathByPagePath

        /// <summary>
        /// if nothing found, returns String.Empty
        /// </summary>
        /// <returns></returns>
        private string getRandomFilePath()
        {
            // -- get all the configured image vals
            string[] configKeys = CmsControlUtils.getControlParameterKeys(CmsContext.currentPage, this);
            List<string> filePaths = new List<string>();
            string notFoundValue = Guid.NewGuid().ToString(); 
            foreach (string key in configKeys)
            {
                string val = CmsControlUtils.getControlParameterKeyValue(CmsContext.currentPage, this, key, notFoundValue);
                if (val != "" && val != notFoundValue && String.Compare(val, RandomFilePathKeyValue, true) != 0)
                    filePaths.Add(val);
            } // foreach

            // -- select the image at random
            if (filePaths.Count > 0)
            {
                int randomFilePathIndex = (new System.Random()).Next(0, filePaths.Count - 1);
                return filePaths[randomFilePathIndex].ToString();
            }
            return String.Empty;

        } // getRandomImage
        

        protected override void Render(HtmlTextWriter writer)
        {
            throw new Exception("Error: the IncludeFileByParentPagePath control needs to be updated to use updated HatCMS code!");

            string filePath = getFilePathByPagePath(CmsContext.currentPage);
            if (filePath != String.Empty)
            {
                if (String.Compare(filePath.Trim(), RandomFilePathKeyValue, true) == 0)
                {
                    filePath = getRandomFilePath();
                }

                if (filePath != String.Empty)
                {
                    string fileContents = "";
                    string cacheKey = filePath.ToLower();
                    if (Cache[cacheKey] != null)
                        fileContents = Cache[cacheKey].ToString();
                    else
                    {
                        try
                        {
                            string filenameOnDisk = System.Web.Hosting.HostingEnvironment.MapPath(filePath);
                            fileContents = System.IO.File.ReadAllText(filenameOnDisk);
                            // cache for 12 hours
                            Cache.Insert(cacheKey, fileContents, new System.Web.Caching.CacheDependency(filenameOnDisk), System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromHours(12));
                        }
                        catch(Exception ex)
                        {
                            if (CmsContext.currentPage.currentUserCanWrite)
                                fileContents = "<p>Template Error found with IncludeFileByParentPagePath control: " + ex.Message + "</p>";
                            else
                                fileContents = ""; // handle errors silently if user is not an author
                        }
                    } // else

                    writer.Write(fileContents);
                } // if
            } // if
        } // Render
    }
}