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
    public interface IPageSecurityZoneRepository : IRepository<CmsPageSecurityZone>
    {

        CmsPageSecurityZone fetch(int zoneId);
        CmsPageSecurityZone fetchByPage(CmsPage page);
        CmsPageSecurityZone fetchByPage(CmsPage page, bool recursive);
        List<CmsPageSecurityZone> fetchAll();
        bool delete(CmsPageSecurityZone entity);
    }
}
