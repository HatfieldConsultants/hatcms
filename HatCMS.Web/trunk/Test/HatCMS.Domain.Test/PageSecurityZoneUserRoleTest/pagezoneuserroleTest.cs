using System;
using System.Collections.Generic;
using System.Text;
using SharpArch.Core.CommonValidator;
using SharpArch.Testing.NUnit.NHibernate;
using SharpArch.Data.NHibernate;
using SharpArch.Core.PersistenceSupport;
using HatCMS;
using HatCMS.Core.DataRepository;
using Hatfield.Common;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Data;

namespace HatCMS.Domain.Test.PageSecurityZoneUserRoleTest
{
    using NUnit.Framework;

    [TestFixture]
    public class pagezoneuserroleTest : DatabaseRepositoryTestsBase
    {
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            ServiceLocatorInitializer.Init();
        }

        [Test]
        public void CanGetAllObject()
        {

            //CmsPageSecurityZoneUserRole role = new CmsPageSecurityZoneUserRole();
            PageSecurityZoneUserRoleRepository repository = new PageSecurityZoneUserRoleRepository();
            IList<CmsPageSecurityZoneUserRole> rolelist = repository.GetAll();
            Assert.That(rolelist.Count, Is.EqualTo(4));
        }

        [Test]
        public void CanInsertObject()
        {
            PageSecurityZoneUserRoleRepository repository = new PageSecurityZoneUserRoleRepository();
            CmsPageSecurityZoneUserRoleDb dboperation = new CmsPageSecurityZoneUserRoleDb();
            CmsPageSecurityZoneUserRole insertobject = new CmsPageSecurityZoneUserRole(1, 2, true, false);
            CmsPageSecurityZoneUserRole insertobject2 = new CmsPageSecurityZoneUserRole(2, 2, false, false);
            List<CmsPageSecurityZoneUserRole> objectlist = new List<CmsPageSecurityZoneUserRole>();
            objectlist.Add(insertobject);
            objectlist.Add(insertobject2);
            if (dboperation.insert(objectlist) == false)
            {
                
                throw new Exception("insert test fail");
            }
            //CmsPageSecurityZoneUserRole returnobject = repository.SaveOrUpdate(insertobject);
            //Assert.That(repository, Is.Not.Null);        
        }

        [Test]
        public void CanQueryByZoneID()
        {
            PageSecurityZoneUserRoleRepository repository = new PageSecurityZoneUserRoleRepository();
            CmsPageSecurityZoneUserRoleDb dboperation = new CmsPageSecurityZoneUserRoleDb();
            CmsPageSecurityZone z = new CmsPageSecurityZone(2);
            //z.Id = 2;
            List<CmsPageSecurityZoneUserRole> resultlist = dboperation.fetchAllByZone(z);
            Assert.That(resultlist.Count, Is.EqualTo(2));
        }

        [Test]
        public void CanDeleteByZone()
        {
            CmsPageSecurityZoneUserRoleDb dboperation = new CmsPageSecurityZoneUserRoleDb();
            CmsPageSecurityZone z = new CmsPageSecurityZone(2);
            //z.Id = 2;
            dboperation.deleteByZone(z);
            Assert.That(dboperation.fetchAllByZone(z).Count, Is.EqualTo(0));
        }

        [Test]
        public void CanFetchReadAccess()
        {
            CmsPageSecurityZoneUserRoleDb dboperation = new CmsPageSecurityZoneUserRoleDb();
            CmsPageSecurityZone z = new CmsPageSecurityZone(1);
            //z.Id = 1;
            WebPortalUserRole role1 = new WebPortalUserRole(1, "aa", "despri");
            WebPortalUserRole role2 = new WebPortalUserRole(-1, "aa", "despri");
            List<WebPortalUserRole> rolelist = new List<WebPortalUserRole>();
            rolelist.Add(role1);
            rolelist.Add(role2);

            Assert.That(dboperation.fetchRoleMatchingCountForRead(z, rolelist.ToArray()), Is.EqualTo(2));
            Assert.That(dboperation.fetchRoleMatchingCountForWrite(z, rolelist.ToArray()), Is.EqualTo(1));

        }
    }
}
