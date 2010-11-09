using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    #if useCmsAlternateView
    public abstract class CmsAlternateView
    {
        public Dictionary<string, string> viewPageParams;

        public int SortOrdinal;

        /// <summary>
        /// the user-friendly title of this page.        
        /// </summary>
        public string PageTitle;

        /// <summary>
        /// the user-friendly menu-title of this page.
        /// </summary>
        public string MenuTitle;

        public DateTime LastUpdatedDateTime = DateTime.MinValue;
        

        public CmsAlternateView()
        {
            viewPageParams = new Dictionary<string, string>();
            SortOrdinal = 0;

            PageTitle = "";
            MenuTitle = "";
        }

        public CmsAlternateView(string title, string param1Key, string param1Val)
        {
            viewPageParams = new Dictionary<string, string>();
            viewPageParams.Add(param1Key, param1Val);

            SortOrdinal = 0;
            PageTitle = title;
            MenuTitle = title;
        }

        public CmsAlternateView(string title, string paramKeyForAllVals, string[] paramVals)
        {
            viewPageParams = new Dictionary<string, string>();
            foreach (string v in paramVals)
            {
                viewPageParams.Add(paramKeyForAllVals, v);
            }

            SortOrdinal = 0;
            PageTitle = title;
            MenuTitle = title;
        }

        public void addViewParam(string key, string val)
        {
            viewPageParams.Add(key, val);
        }

        public string getUrl(CmsPage parentPage)
        {
            return parentPage.getUrl(viewPageParams);
        }

        private class SortOrdinalComparer : System.Collections.IComparer
        {
            int IComparer.Compare(Object x, Object y)
            {
                // Less than zero : x is less than y. 
                // Zero : x equals y. 
                // Greater than zero : x is greater than y. 

                if ((x as CmsAlternateView).SortOrdinal < (y as CmsAlternateView).SortOrdinal) return -1;
                if ((x as CmsAlternateView).SortOrdinal == (y as CmsAlternateView).SortOrdinal) return 0;
                return 1;
            }
        } // SortOrdinalComparer

        public static CmsAlternateView[] SortAlternateViewsBySortOrdinal(CmsAlternateView[] alternateViews)
        {
            ArrayList a = new ArrayList(alternateViews);
            SortOrdinalComparer comparer = new SortOrdinalComparer();
            a.Sort(comparer);
            return (CmsAlternateView[])a.ToArray(typeof(CmsAlternateView));

        } // SortPagesByLastModifiedDate 



    } // CmsAlternateView
#endif
}
