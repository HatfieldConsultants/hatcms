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
    public class PageSecurityZoneUserRoleRepository : NHibernateRepository<CmsPageSecurityZoneUserRole>, IPageSecurityZoneUserRoleRepository
    {
        #region IPageSecurityZoneUserRoleRepository Members

        public List<CmsPageSecurityZoneUserRole> fetchAllByZone(CmsPageSecurityZone z)
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageSecurityZoneUserRole))
                 .Add(Expression.Eq("Zone.Id", z.Id));
            IList<CmsPageSecurityZoneUserRole> authoritylist = criteria.List<CmsPageSecurityZoneUserRole>();
            return authoritylist as List<CmsPageSecurityZoneUserRole>;
        }

        public int fetchRoleMatchingCountForRead(CmsPageSecurityZone z, Hatfield.Web.Portal.WebPortalUserRole[] roleArray)
        {
            List<int> userIdList = new List<int>();
            foreach(Hatfield.Web.Portal.WebPortalUserRole webportaluserrole in roleArray)
            {
                userIdList.Add(webportaluserrole.RoleID);
            }
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageSecurityZoneUserRole))
                 .Add(Expression.Eq("Zone.Id", z.Id))
                 .Add(Expression.Eq("ReadAccess", true))
                 .Add(Expression.In("UserRoleId", userIdList));

            return criteria.List<CmsPageSecurityZoneUserRole>().Count;
        }

        public int fetchRoleMatchingCountForWrite(CmsPageSecurityZone z, Hatfield.Web.Portal.WebPortalUserRole[] roleArray)
        {
            List<int> userIdList = new List<int>();
            foreach (Hatfield.Web.Portal.WebPortalUserRole webportaluserrole in roleArray)
            {
                userIdList.Add(webportaluserrole.RoleID);
            }
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageSecurityZoneUserRole))
                 .Add(Expression.Eq("Zone.Id", z.Id))
                 .Add(Expression.Eq("WriteAccess", true))
                 .Add(Expression.In("UserRoleId", userIdList));

            return criteria.List<CmsPageSecurityZoneUserRole>().Count;
        }

        public bool deleteByZone(CmsPageSecurityZone z)
        {
            List<CmsPageSecurityZoneUserRole> listfetchedbyzone = this.fetchAllByZone(z);
            foreach(CmsPageSecurityZoneUserRole entity in listfetchedbyzone)
            {
                try
                {
                    this.Delete(entity);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;

        }

        #endregion
    }
}
