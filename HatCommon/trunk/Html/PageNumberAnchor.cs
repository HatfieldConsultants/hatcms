using System;
using System.Collections.Generic;
using System.Text;

namespace Hatfield.Web.Portal.Html
{
    public class PageNumberAnchor
    {
        private string href = "";
        public string Href
        {
            get { return href; }
            set { href = value; }
        }

        private int pageNum = -1;
        public int PageNum
        {
            get { return pageNum; }
            set { pageNum = value; }
        }

        private string cssClass = "";
        public string CssClass
        {
            get { return cssClass; }
            set { cssClass = value; }
        }

        public PageNumberAnchor()
        {
        }

        public PageNumberAnchor(string href, int pageNum, string cssClass)
        {
            Href = href;
            PageNum = pageNum;
            CssClass = cssClass;
        }
    }
}
