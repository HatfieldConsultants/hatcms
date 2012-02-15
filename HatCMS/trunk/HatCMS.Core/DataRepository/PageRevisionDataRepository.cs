using System.Collections.Generic;
using NHibernate;
using System.Reflection;
using SharpArch.Core;
using SharpArch.Core.PersistenceSupport;
using NHibernate.Criterion;
using System.Collections.Specialized;
using System;
using SharpArch.Core.PersistenceSupport.NHibernate;
using SharpArch.Core.DomainModel;
using SharpArch.Data.NHibernate;
using HatCMS.Core.DataInterface;

namespace HatCMS.Core.DataRepository
{
    public class PageRevisionDataRepository : NHibernateRepository<CmsPageRevisionData>, IPageRevisionDataRepository
    {
        #region IPageRevisionDataRepository Members

        public IList<CmsPageRevisionData> FetchAllRevisionDataofPage(CmsPage page)
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageRevisionData))
                  .Add(Expression.Eq("PageId", page.Id))
                  .AddOrder(new Order("ModificationDate", false));
                  
            IList<CmsPageRevisionData> pagelist = criteria.List<CmsPageRevisionData>();
            return pagelist;
        }

        #endregion
    }
}
