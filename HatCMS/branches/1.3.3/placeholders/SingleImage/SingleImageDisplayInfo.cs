using System;
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
    /// <summary>
    /// Use a SingleImageDisplayInfo object to determine how to display a SingleImage using SingleImage.getStandardHtmlView()
    /// note: there are no database methods associated with this class.
    /// </summary>
    public class SingleImageDisplayInfo
    {
        /// <summary>
        /// path to the image to display. The path should not include the ApplicationPath. 
        /// <example>
        /// Example: "UserFiles/Image/home/AboutPOPs.jpg"
        /// </example>
        /// </summary>
        public string ImagePath = "";

        public string FullImageDisplayUrl = "";

        public System.Drawing.Size ThumbImageDisplayBox = new System.Drawing.Size(-1, -1);

        public System.Drawing.Size FullImageDisplayBox = new System.Drawing.Size(-1, -1);

        public System.Drawing.Size PopupDisplayBox = new System.Drawing.Size(-1, -1);

        /// <summary>
        /// {0} = image thumbnail width
        /// {1} = image thumbnail height
        /// {2} = image thumbnail URL
        /// {3} = popup page width
        /// {4} = popup page height
        /// {5} = popup page URL
        /// {6} = Caption
        /// {7} = CreditsPrefix
        /// {8} = Credits
        /// {9} = ClickToEnlargeText
        /// To encode for use in web.config, copy your raw HTML to this page: http://www.tumuski.com/code/htmlencode/
        /// <example>
        /// -- Multibox:
        /// <a class="mb" href="{5}" rel="width:{3},height:{4}"><img src="{2}" width="{0}" height="{1}" /></a><div class="caption">{6}</div><div class="credits">{7}{8}</div><div class="clickToEnlarge">{9}</div>
        /// &lt;a class=&quot;mb&quot; href=&quot;{5}&quot; rel=&quot;width:{3},height:{4}&quot;&gt;&lt;img src=&quot;{2}&quot; width=&quot;{0}&quot; height=&quot;{1}&quot; /&gt;&lt;/a&gt;&lt;div class=&quot;caption&quot;&gt;{6}&lt;/div&gt;&lt;div class=&quot;credits&quot;&gt;{7}{8}&lt;/div&gt;&lt;div class=&quot;clickToEnlarge&quot;&gt;{9}&lt;/div&gt;
        ///</example>
        /// <example>
        /// Sub-modal:
        /// <a class="submodal-{3}-{4}" href="{5}"><img src="{2}" width="{0}" height="{1}" /></a>
        /// &lt;a class=&quot;submodal-{3}-{4}&quot; href=&quot;{5}&quot;&gt;&lt;img src=&quot;{2}&quot; width=&quot;{0}&quot; height=&quot;{1}&quot; /&gt;&lt;/a&gt;
        /// </example>
        /// <example>
        /// Popup:
        /// <a href="{5}" onclick="var w = window.open(this.href, 'popupLargeImage', 'toolbar=no,menubar=no,resizable=yes,scrollbars=yes,status=yes,height={3},width={4}'); "><img src="{2}" width="{0}" height="{1}" /></a>
        /// &lt;a href=&quot;{5}&quot; onclick=&quot;var w = window.open(this.href, 'popupLargeImage', 'toolbar=no,menubar=no,resizable=yes,scrollbars=yes,status=yes,height={3},width={4}'); &quot;&gt;&lt;img src=&quot;{2}&quot; width=&quot;{0}&quot; height=&quot;{1}&quot; /&gt;&lt;/a&gt;
        /// </example>
        /// </summary>
        public string ThumbDisplayWithLinkTemplate = "";
        
        public string ThumbDisplayWithoutLinkTemplate = "";

        public string Caption = "";
        public string CreditsPromptPrefix = "";
        public string Credits = "";
        public string ClickToEnlargeText = "";
        
    }
}
