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
    public class PageSecurityZoneRepository : NHibernateRepository<CmsPageSecurityZone>, IPageSecurityZoneRepository
    {
        #region IPageSecurityZoneRepository Members

        public CmsPageSecurityZone fetch(int zoneId)
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageSecurityZone))
                 .Add(Expression.Eq("Id", zoneId))
                 .Add(Expression.IsNull("Deleted"));
            IList<CmsPageSecurityZone> zonelist = criteria.List<CmsPageSecurityZone>();
            if (zonelist.Count > 0)
                return zonelist[0];
            else
                throw new Exception("Zone with the provided Id is not existed in the database");
        }

        public CmsPageSecurityZone fetchByPage(CmsPage page)
        {
            PageRepository pagerepository = new PageRepository();
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageSecurityZone))
                 .Add(Expression.Eq("StartingPage.Id", page.Id))
                 .Add(Expression.IsNull("Deleted"));
            IList<CmsPageSecurityZone> zonelist = criteria.List<CmsPageSecurityZone>();
            if (zonelist.Count > 0)
                return zonelist[0];
            else
            {
                ICriteria parentcriteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageSecurityZone))
                  .Add(Expression.Eq("StartingPage.Id", page.ParentID))
                  .Add(Expression.IsNull("Deleted"));
                IList<CmsPageSecurityZone> zoneparentlist = criteria.List<CmsPageSecurityZone>();
                if (zoneparentlist.Count > 0)
                    return zoneparentlist[0];
                else
                {
                    
                    CmsPageSecurityZone foundsecurityzone = fetchByPage(pagerepository.Get(page.ParentID));
                    if (foundsecurityzone.Id > 0)
                        return foundsecurityzone;

                }
                    
                    //throw new Exception("Zone with the provided page Id is not existed in the database");
            }
            throw new Exception("Zone with the provided page Id is not existed in the database");
                

        }

        public CmsPageSecurityZone fetchByPage(CmsPage page, bool recursive)
        {
            if (recursive)
                return this.fetchByPage(page);

            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageSecurityZone))
                 .Add(Expression.Eq("StartingPage.Id", page.Id))
                 .Add(Expression.IsNull("Deleted"));
            IList<CmsPageSecurityZone> zonelist = criteria.List<CmsPageSecurityZone>();
            if (zonelist.Count > 0)
                return zonelist[0];
            else
                throw new Exception("Zone with the provided page Id is not existed in the database");
        }

        public List<CmsPageSecurityZone> fetchAll()
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPageSecurityZone))
                 .Add(Expression.IsNull("Deleted"));
            IList<CmsPageSecurityZone> zonelist = criteria.List<CmsPageSecurityZone>();
            return zonelist as List<CmsPageSecurityZone>;

        }

        public bool delete(CmsPageSecurityZone entity)
        {
            entity.Deleted = DateTime.Now;
            if (this.Update(entity).Id > 0)
                return true;
            else
                return false;
        }

        #endregion
    }
}
