using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Data;
using HatCMS.Core.DataRepository;

namespace HatCMS
{
    /// <summary>
    /// DB object for `ZoneUserRole`
    /// </summary>
    public class CmsPageSecurityZoneUserRoleDb
    {
        PageSecurityZoneUserRoleRepository repository;
        public CmsPageSecurityZoneUserRoleDb(){ 
            repository = new PageSecurityZoneUserRoleRepository();
        }

        /// <summary>
        /// Select all the aurhority definitions by providing a zone (zone ID)
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public List<CmsPageSecurityZoneUserRole> fetchAllByZone(CmsPageSecurityZone z)
        {
            //PageSecurityZoneUserRoleRepository repository = new PageSecurityZoneUserRoleRepository();
            List<CmsPageSecurityZoneUserRole> list = repository.fetchAllByZone(z);            
            return list;
        }

        /// <summary>
        /// Count the number of roles ID defined.  If the count is zero, it indicates
        /// the WebPortalUser does not belong to the roles which are required to access
        /// the zone.
        /// </summary>
        /// <param name="z"></param>
        /// <param name="roleArray"></param>
        /// <returns></returns>
        public int fetchRoleMatchingCountForRead(CmsPageSecurityZone z, WebPortalUserRole[] roleArray)
        {
           // PageSecurityZoneUserRoleRepository repository = new PageSecurityZoneUserRoleRepository();
            return repository.fetchRoleMatchingCountForRead(z, roleArray);
        }

        /// <summary>
        /// Count the number of roles ID defined.  If the count is zero, it indicates
        /// the WebPortalUser does not belong to the roles which are required to access
        /// the zone.
        /// </summary>
        /// <param name="z"></param>
        /// <param name="roleArray"></param>
        /// <returns></returns>
        public int fetchRoleMatchingCountForWrite(CmsPageSecurityZone z, WebPortalUserRole[] roleArray)
        {
            //PageSecurityZoneUserRoleRepository repository = new PageSecurityZoneUserRoleRepository();
            return repository.fetchRoleMatchingCountForWrite(z, roleArray);
        }

        /// <summary>
        /// Insert into `ZoneUserRole`
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool insert(CmsPageSecurityZoneUserRole entity)
        {
            
            try
            {
                repository.Save(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Bulk insert into `ZoneUserRole`
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        public bool insert(List<CmsPageSecurityZoneUserRole> entityList)
        {
            foreach(CmsPageSecurityZoneUserRole entity in entityList)
            {
                if(this.insert(entity) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Delete records according to zone id
        /// </summary>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool deleteByZone(CmsPageSecurityZone z)
        {
            //PageSecurityZoneUserRoleRepository repository = new PageSecurityZoneUserRoleRepository();
            return repository.deleteByZone(z);
        }


    }
}
