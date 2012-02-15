using System;
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
    /// <summary>
    /// PageRevisionData class stores information about a <see cref="CmsPage"/>'s revions. Use <c>CmsPage.getAllRevisionData()</c> to fill these objects for a page.
    /// </summary>
    public class CmsPageRevisionData
    {
        public int PageId = -1;
        public int RevisionNumber = -1;
        public DateTime RevisionSavedAt = DateTime.MinValue;
        public string RevisionSavedByUsername = "";

        public static CmsPageRevisionData[] SortMostRecentFirst(CmsPageRevisionData[] revData)
        {
            ArrayList al = new ArrayList(revData);
            al.Sort(new RevisionSavedAtDateComparer(false));
            return (CmsPageRevisionData[])al.ToArray(typeof(CmsPageRevisionData));
        }

        public static CmsPageRevisionData[] SortMostRecentLast(CmsPageRevisionData[] revData)
        {
            ArrayList al = new ArrayList(revData);
            al.Sort(new RevisionSavedAtDateComparer(true));
            return (CmsPageRevisionData[])al.ToArray(typeof(CmsPageRevisionData));
        }

        private class RevisionSavedAtDateComparer : System.Collections.IComparer
        {
            private bool _asc;
            public RevisionSavedAtDateComparer(bool ascending)
            {
                _asc = ascending;
            }
            int IComparer.Compare(Object x, Object y)
            {
                if (_asc)
                    return DateTime.Compare((x as CmsPageRevisionData).RevisionSavedAt, (y as CmsPageRevisionData).RevisionSavedAt);
                else
                    return DateTime.Compare((y as CmsPageRevisionData).RevisionSavedAt, (x as CmsPageRevisionData).RevisionSavedAt);
            }
        } // LastUpdatedDateComparer
    }
}
