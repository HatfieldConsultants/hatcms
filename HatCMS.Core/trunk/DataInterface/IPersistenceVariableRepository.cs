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

namespace HatCMS.Core.DataInterface
{
    public interface IPersistenceVariableRepository : IRepository<CmsPersistentVariable>
    {
        CmsPersistentVariable FetchbyName(string name);

        CmsPersistentVariable[] FetchAllWithNamePrefix(string namePrefix);

    }
}
