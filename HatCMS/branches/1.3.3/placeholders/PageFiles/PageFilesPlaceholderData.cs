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
    public class PageFilesPlaceholderData
    {
        public enum SortDirection { Ascending, Descending }
        public enum SortColumn { NoSorting, Filename, Title, FileSize, DateLastModified }
        public enum TabularDisplayLinkMode { LinkToFile, LinkToDetails }

        public SortDirection sortDirection = SortDirection.Descending;
        public SortColumn sortColumn = SortColumn.NoSorting;
        public int numFilesToShowPerPage = -1;
        public BaseCmsPlaceholder.AccessLevel accessLevelToAddFiles = BaseCmsPlaceholder.AccessLevel.CmsAuthor;
        public BaseCmsPlaceholder.AccessLevel accessLevelToEditFiles = BaseCmsPlaceholder.AccessLevel.CmsAuthor;
        public TabularDisplayLinkMode tabularDisplayLinkMode = TabularDisplayLinkMode.LinkToDetails;
    }

}
