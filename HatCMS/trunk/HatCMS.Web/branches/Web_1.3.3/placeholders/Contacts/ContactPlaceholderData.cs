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
    public class ContactPlaceholderData
    {
        public enum ContactNameDisplayMode { FirstnameLastname, LastnameFirstname }

        public int numColumnsToShow = 4;


        public int forceFilterToCategoryId = -1;
        public bool allowFilterByCategory = true;
        public bool allowFilterByCompany = true;

        public ContactNameDisplayMode nameDisplayMode = ContactNameDisplayMode.LastnameFirstname;
        public BaseCmsPlaceholder.AccessLevel accessLevelToEditContacts = BaseCmsPlaceholder.AccessLevel.CmsAuthor;
        public BaseCmsPlaceholder.AccessLevel accessLevelToAddContacts = BaseCmsPlaceholder.AccessLevel.CmsAuthor;

    }
}
