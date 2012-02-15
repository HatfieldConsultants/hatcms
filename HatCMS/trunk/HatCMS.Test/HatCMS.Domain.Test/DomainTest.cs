using System;
using System.Collections.Generic;
using System.Text;
using SharpArch.Core.CommonValidator;
using SharpArch.Testing.NUnit.NHibernate;
using SharpArch.Data.NHibernate;
using SharpArch.Core.PersistenceSupport;
using HatCMS;
using HatCMS.Core.DataRepository;

namespace HatCMS.Domain.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class DomainTest : DatabaseRepositoryTestsBase
    {

        [Test]
        public void CanCreatePersistenVariable()
        {
            HatCMS.CmsPersistentVariable persistentvariable = new HatCMS.CmsPersistentVariable();
            persistentvariable.Name = "testname";
            Assert.IsNotNull(persistentvariable);
            Assert.That(persistentvariable.Name, Is.EqualTo("testname"));
        }

        [Test]
        public void CanQueryDatabase()
        {
            ServiceLocatorInitializer.Init();
            IRepository<CmsPersistentVariable> repository = new Repository<CmsPersistentVariable>();
            IList<CmsPersistentVariable> persistentvariables = repository.GetAll();
            Assert.IsNotNull(persistentvariables);
            Assert.That(persistentvariables, Is.Not.Empty);        
        }

        [Test]
        public void CanInsertLongBlobtoDatabase()
        {
            ServiceLocatorInitializer.Init();
            IRepository<CmsPersistentVariable> repository = new Repository<CmsPersistentVariable>();
            IList<CmsPersistentVariable> persistentvariables = repository.GetAll();
            Assert.IsNotNull(persistentvariables);
            Assert.That(persistentvariables, Is.Not.Empty);

            CmsPersistentVariable insertvariable = new CmsPersistentVariable();
            
            insertvariable.Name = "This is a test variable for long blob type";
            insertvariable.PersistedValue = persistentvariables[0].PersistedValue;
            Assert.That(insertvariable, Is.Not.Null);

            CmsPersistentVariable returnvariable = repository.SaveOrUpdate(insertvariable);

            IList<CmsPersistentVariable> newvariables = repository.GetAll();

            Assert.That(newvariables.Count, Is.EqualTo(4));
        }

        [Test]
        public void CanCustomRepository()
        {
            PersistentVariableRepository persistencevariableRepository = new PersistentVariableRepository();
            HatCMS.CmsPersistentVariable variable = persistencevariableRepository.FetchbyName("LastPeriodicTaskStartTime_HatCms.Admin.BackgroundTasks.EmailConfigErrors");
            Assert.That(variable.Name, Is.EqualTo("LastPeriodicTaskStartTime_HatCms.Admin.BackgroundTasks.EmailConfigErrors"));
        
        }

        [Test]
        public void CanQueryByPrefix()
        {
            PersistentVariableRepository persistencevariableRepository = new PersistentVariableRepository();
            CmsPersistentVariable[] variableArray = persistencevariableRepository.FetchAllWithNamePrefix("LastPeriodicTaskStartTime_HatCms.Admin");
            Assert.That(variableArray.Length, Is.EqualTo(3));
        }

        
    }
}
