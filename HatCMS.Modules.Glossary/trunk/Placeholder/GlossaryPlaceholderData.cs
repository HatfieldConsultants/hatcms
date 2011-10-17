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
    public class GlossaryPlaceholderData
    {
        public enum GlossarySortOrder { byId, byDescription, byWord }
        public enum GlossaryViewMode { SinglePageWithJumpList, PagePerLetter }

        public int GlossaryId = Int32.MinValue;
        public GlossarySortOrder SortOrder = GlossarySortOrder.byWord;
        public GlossaryViewMode ViewMode = GlossaryViewMode.PagePerLetter;
    }
}
