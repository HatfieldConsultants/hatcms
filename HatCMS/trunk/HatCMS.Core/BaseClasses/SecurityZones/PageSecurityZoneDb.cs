using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Data;
using HatCMS.Core.DataRepository;
using MySql.Data;


namespace HatCMS
{
    /// <summary>
    /// DB object for `Zone`
    /// </summary>
    public class CmsPageSecurityZoneDb
    {
        public CmsPageSecurityZoneDb()
        { }

        public CmsPageSecurityZone fetch(int zoneId)
        {
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            return repository.fetch(zoneId);
        }

        /// <summary>
        /// Find out the zone where the current page is located.  If the current page is
        /// not defined in ZoneManagement, search the parent page.  Repeat until a record
        /// is defined in ZoneManagement.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public CmsPageSecurityZone fetchByPage(CmsPage page)
        {
            PageSecurityZoneRepository repostitory = new PageSecurityZoneRepository();
            return repostitory.fetchByPage(page);
        }

        /// <summary>
        /// Recursive is T: see what CmsZone a page is.
        /// Recursive is F: select the exact zone record given a cms page (i.e. boundary page).
        /// </summary>
        /// <param name="page"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        public CmsPageSecurityZone fetchByPage(CmsPage page, bool recursive)
        {
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            return repository.fetchByPage(page, recursive);
        }

        /// <summary>
        /// Select all zone records order by their names
        /// </summary>
        /// <returns></returns>
        public List<CmsPageSecurityZone> fetchAll()
        {
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            return repository.fetchAll();
        }

        /// <summary>
        /// Insert into `zone`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insert(CmsPageSecurityZone entity)
        {
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            if (repository.SaveOrUpdate(entity).Id > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Update `Zone`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool update(CmsPageSecurityZone entity)
        {
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            if (repository.SaveOrUpdate(entity).Id > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Delete from `Zone`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool delete(CmsPageSecurityZone entity)
        {
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            return repository.delete(entity);
        }

    }
}
