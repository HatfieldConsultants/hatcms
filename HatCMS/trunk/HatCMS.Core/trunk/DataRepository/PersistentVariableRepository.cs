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
    public class PersistentVariableRepository : NHibernateRepository<CmsPersistentVariable>, IPersistenceVariableRepository
    {
        public CmsPersistentVariable FetchbyName(string name)
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPersistentVariable))
                .Add(Expression.Eq("Name", name));
            IList<CmsPersistentVariable> variablelsit = criteria.List<CmsPersistentVariable>();
            if (variablelsit.Count > 0)
            {
                return variablelsit[0];
            }
            else
                throw new Exception("No Persisten Variable with the provided name exists in the database.");
        }

        public CmsPersistentVariable[] FetchAllWithNamePrefix(string namePrefix)
        {
            ICriteria criteria = NHibernateSession.Current.CreateCriteria(typeof(CmsPersistentVariable))
                .Add(Expression.Like("Name", namePrefix, MatchMode.Start));

            List<CmsPersistentVariable> variablelsit = criteria.List<HatCMS.CmsPersistentVariable>() as List<CmsPersistentVariable>;
            return variablelsit.ToArray();
        }
    }
}
