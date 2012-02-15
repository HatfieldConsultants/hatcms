using System;
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
    public class SingleImageData
    {
        /// <summary>
        /// the page id that this image is found on
        /// </summary>
        public int PageId = -1;

        /// <summary>
        /// the placeholder identifier this image is associated with.
        /// </summary>
        public int PageIdentifier = -1;

        /// <summary>
        /// the unique identifier for this image
        /// </summary>
        public int SingleImageId = -1;

        public string ImagePath = "";


        public string Caption = "";
        public string Credits = "";

        private List<string> tags = new List<string>();
        /// <summary>
        /// note: duplicate tags are not allowed
        /// </summary>
        public string[] Tags
        {
            get
            {
                return tags.ToArray();
            }
            set
            {
                tags.Clear(); tags.AddRange(value);
            }
        }

        public bool containsTag(string tag)
        {
            foreach (string t in tags)
            {
                if (String.Compare(t, tag, true) == 0)
                    return true;
            }
            return false;
        }

        public bool removeTag(string tag)
        {
            return tags.Remove(tag);
        }

        /// <summary>
        /// note: duplicate tags are not added
        /// </summary>
        /// <param name="tag"></param>
        public void addTag(string tag)
        {
            // don't allow duplicates
            if (containsTag(tag))
                return;
            
            tags.Add(tag);
        }

        /// <summary>
        /// returns null if not found
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pagesToGatherImagesFrom"></param>
        /// <returns></returns>
        public CmsPage getPageContainingImage(CmsPage[] pagesToGatherImagesFrom)
        {
            if (this.PageId < 0)
                return null;
            foreach (CmsPage p in pagesToGatherImagesFrom)
            {
                if (this.PageId == p.Id)
                    return p;
            }

            return null;
        }

        public static string TagStorageSeperator = ";";

    } // class
}
