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
    public interface IPageSecurityZoneUserRoleRepository : IRepository<CmsPageSecurityZoneUserRole>
    {
        List<CmsPageSecurityZoneUserRole> fetchAllByZone(CmsPageSecurityZone z);

        int fetchRoleMatchingCountForRead(CmsPageSecurityZone z, WebPortalUserRole[] roleArray);

        int fetchRoleMatchingCountForWrite(CmsPageSecurityZone z, WebPortalUserRole[] roleArray);

        bool deleteByZone(CmsPageSecurityZone z);


    }
}
