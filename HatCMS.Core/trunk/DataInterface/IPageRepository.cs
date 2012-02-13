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
using Hatfield.Web.Portal;

namespace HatCMS.Core.DataInterface
{
    public interface IPageRepository : IRepository<CmsPage>
    {
        IList<CmsPage> fetallpage();
        IList<CmsPage> FetchPagesByTemplateName(string encodedTemplateName);
        int CreateNewRepository(ref CmsPage page, CmsPageRevisionData newrevision);
    }
}
