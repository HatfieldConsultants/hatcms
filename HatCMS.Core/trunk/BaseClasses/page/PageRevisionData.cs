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
using System.Collections.Generic;
using Hatfield.Web.Portal;
using SharpArch.Core.DomainModel;
using NHibernate.Validator.Constraints;
using HatCMS.Core.DataRepository;

namespace HatCMS
{
    /// <summary>
    /// PageRevisionData class stores information about a <see cref="CmsPage"/>'s revions. Use <c>CmsPage.getAllRevisionData()</c> to fill these objects for a page.
    /// </summary>
    public class CmsPageRevisionData : Entity
    {
        #region domain model
        private int pageid = -1;
        [DomainSignature]
        public virtual int PageId
        {
            get { return pageid; }
            set { pageid = value; }
        }
        private int revisionnumber = -1;
        [DomainSignature]
        public virtual int RevisionNumber
        {
            get { return revisionnumber; }
            set { revisionnumber = value; }
        }
        private DateTime revisionsavedat = DateTime.MinValue;

        public virtual DateTime ModificationDate
        {
            get { return revisionsavedat; }
            set { revisionsavedat = value; }
        }
        private string revisionsavedbyusername = "";

        public virtual string ModifiedBy
        {
            get { return revisionsavedbyusername; }
            set { revisionsavedbyusername = value; }
        }
        #endregion domain model
        
        public CmsPageRevisionData()
        { }//constructor

        public CmsPageRevisionData(int pageid, int revisionnumber, DateTime modificationdate, string modifiedby)
        {
            this.pageid = pageid;
            this.revisionnumber = revisionnumber;
            this.revisionsavedat = modificationdate;
            this.revisionsavedbyusername = modifiedby;
        
        }//constructor

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
                    return DateTime.Compare((x as CmsPageRevisionData).ModificationDate, (y as CmsPageRevisionData).ModificationDate);
                else
                    return DateTime.Compare((y as CmsPageRevisionData).ModificationDate, (x as CmsPageRevisionData).ModificationDate);
            }
        } // LastUpdatedDateComparer
    }
}
