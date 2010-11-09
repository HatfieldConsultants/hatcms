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
    public class SingleImageGalleryPlaceholderData
    {        
        public int PageIdToGatherImagesFrom = Int32.MinValue;

        public bool RecursiveGatherImages = false;
        
        /// <summary>
        /// values less than 0 means to not take the width into consideration;
        /// if both ThumbnailDisplayBoxWidth and ThumbnailDisplayBoxHeight are less than 0, the full-sized image will be rendered
        /// </summary>
        public int ThumbImageDisplayBoxWidth = -1;

        /// <summary>
        /// values less than 0 means to not take the width into consideration;
        /// if both ThumbnailDisplayBoxWidth and ThumbnailDisplayBoxHeight are less than 0, the full-sized image will be rendered
        /// </summary>
        public int ThumbImageDisplayBoxHeight = -1;        

        public bool OverrideFullDisplayBoxSize = false;

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

        private List<string> tagsImagesMustHave = new List<string>();
        /// <summary>
        /// The tags that images displayed in this placeholder must have
        /// </summary>
        public string[] TagsImagesMustHave
        {
            get
            {
                return tagsImagesMustHave.ToArray();
            }

            set
            {
                tagsImagesMustHave.Clear();
                tagsImagesMustHave.AddRange(value);
            }
        }
    }
}
