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
    public class pageTest : DatabaseRepositoryTestsBase
    {
        PageRepository pagerepository = new PageRepository();
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            ServiceLocatorInitializer.Init();
        }

        [Test]
        public void CanFetchByPage()
        {
            CmsPage page = pagerepository.Get(1);
            PageSecurityZoneRepository repository = new PageSecurityZoneRepository();
            CmsPageSecurityZone zone = repository.fetchByPage(page, false);

        }

        [Test]
        public void CanGetAllPage()
        { 
            PageRepository repository = new PageRepository();
            IList<CmsPage> pagelist = repository.FetchPagesByTemplateName("_login");
            Assert.That(pagelist, Is.Not.Null);
        }

        [Test]
        public void CanCreatepage()
        {
            PageRepository repository = new PageRepository();
            CmsPage newpage = pagerepository.Get(29);
            Assert.That(newpage, Is.Not.Null);
        }

        [Test]
        public void CanGetLockDataFromPageRepository()
        {
            PageRepository repository = new PageRepository();
            CmsPage newpage = pagerepository.Get(29);
            Assert.That(repository.getPageLockData(newpage), Is.Null);
        }

        [Test]
        public void CanUnlockDataofPage()
        {
            PageRepository repository = new PageRepository();
            CmsPage newpage = pagerepository.Get(29);
            repository.clearCurrentPageLock(newpage);        
        }

        [Test]
        public void CanLockDataforEditing()
        {
            PageRepository repository = new PageRepository();
            CmsPageLockData ret = new CmsPageLockData();
            ret.PageId = 1;
            ret.LockedByUsername = "zlu";
            ret.LockExpiresAt = new DateTime(DateTime.Now.Ticks + TimeSpan.FromMinutes(30).Ticks);
            CmsPageLockData insertedlock = repository.lockPageForEditing(ret);
            Assert.That(insertedlock.Id, Is.Not.LessThan(0));        
        }

        [Test]
        public void CanInsertPage()
        {
            PageRepository repository = new PageRepository();

            CmsPage targetPage = new CmsPage();
            targetPage.LastModifiedBy = "test";
            targetPage.LastUpdatedDateTime = DateTime.Now;
            targetPage.RevisionNumber = -1;
            targetPage.ShowInMenu = true;
            targetPage.SortOrdinal = 1;
            targetPage.TemplateName = "_login";
            targetPage.ParentID = 1;

            CmsPageLanguageInfo languagein = new CmsPageLanguageInfo();
            languagein.LanguageShortCode = "en";
            languagein.MenuTitle = "test insert";
            languagein.SearchEngineDescription = "description of test insert";
            languagein.Title = "test insert title";
            languagein.Page = targetPage;
            targetPage.LanguageInfo[0] = languagein;
            Assert.That(targetPage.LanguageInfo[0].LanguageShortCode, Is.EqualTo("en"));

            CmsPage returnpage = repository.Save(targetPage);
            Assert.That(returnpage, Is.Not.Null);
        
        }
    }
}
