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
using MySql.Data;

namespace HatCMS.Domain.Test.PageSecurityZoneUserRoleTest
{
    using NUnit.Framework;
    public class pagezoneTest : DatabaseRepositoryTestsBase
    {
        PageRepository pagerepository = new PageRepository();
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            ServiceLocatorInitializer.Init();
        }

        [Test]
        public void CanGetAllObject()
        {

            //CmsPageSecurityZoneUserRole role = new CmsPageSecurityZoneUserRole();
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            IList<CmsPageSecurityZone> rolelist = repository.GetAll();
            Assert.That(rolelist.Count, Is.EqualTo(2));
        }

        [Test]
        public void CanFecthByZoneId()
        {
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            CmsPageSecurityZoneDb dboperation = new CmsPageSecurityZoneDb();
            CmsPageSecurityZone zone = dboperation.fetch(2);
            Assert.That(zone.StartingPage.Id, Is.EqualTo(3));        
        }

        [Test]
        public void CanFetchAll()
        {
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            Assert.That(repository.fetchAll().Count, Is.EqualTo(2));
        }

        [Test]
        public void CanDelete()
        {
            CmsPageSecurityZoneDb repository = new CmsPageSecurityZoneDb();
            CmsPageSecurityZone zone = repository.fetch(1);
            repository.delete(zone);
        }

        [Test]
        public void CanGetPage()
        {
            IRepository<CmsPage> repository = new Repository<CmsPage>();
            IList<CmsPage> pagelist = repository.GetAll();
            Assert.That(pagelist, Is.Not.Null);
        
        }

        [Test]
        public void CanFetchByPage()
        {
            CmsPage page = pagerepository.Get(34);
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            CmsPageSecurityZone zone = repository.fetchByPage(page, true);
            Assert.That(zone.Id, Is.EqualTo(1));
        
        }

        [Test]
        public void CanGetAllPageLangInfo()
        { 
            IRepository<CmsPageLanguageInfo> repository = new Repository<CmsPageLanguageInfo>();
            IList<CmsPageLanguageInfo> pageinfolist = repository.GetAll();
            Assert.That(pageinfolist.Count, Is.EqualTo(25));
        }

        [Test]
        public void CanGetAllRevisionData()
        {
            IRepository<CmsPageRevisionData> repository = new Repository<CmsPageRevisionData>();
            IList<CmsPageRevisionData> pageinfolist = repository.GetAll();
            Assert.That(pageinfolist.Count, Is.EqualTo(6));
        }

        [Test]
        public void CanGetAllPageLockData()
        {
            IRepository<CmsPageLockData> repository = new Repository<CmsPageLockData>();
            IList<CmsPageLockData> pageinfolist = repository.GetAll();
            Assert.That(pageinfolist.Count, Is.EqualTo(0));
        }

    }
}
