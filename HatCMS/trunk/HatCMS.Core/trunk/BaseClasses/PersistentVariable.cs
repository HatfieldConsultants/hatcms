using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Hatfield.Web.Portal;
using SharpArch.Core.DomainModel;
using NHibernate.Validator.Constraints;
using SharpArch.Core.PersistenceSupport;
using SharpArch.Data.NHibernate;
using HatCMS.Core.DataRepository;

namespace HatCMS
{
    public class CmsPersistentVariable : Entity
    {

        private string name;
        [DomainSignature]
        [Length(255)]
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        private object persistedvalue;

        public virtual object PersistedValue
        {
            get { return persistedvalue; }
            set { persistedvalue = value; }
        }

        public CmsPersistentVariable()
        { 
        
        }
        public CmsPersistentVariable(string name, ISerializable valueToPersist)
        {

            this.name = name;
            PersistedValue = valueToPersist;
        } // constructor

        public CmsPersistentVariable(string name, object valueToPersist)
        {

            this.name = name;
            PersistedValue = valueToPersist;
        } // constructor

        public virtual bool SaveToDatabase()
        {
            IRepository<CmsPersistentVariable> repository = new Repository<CmsPersistentVariable>();
            CmsPersistentVariable returnPersistenVariable = repository.SaveOrUpdate(this);
            if(returnPersistenVariable.Id > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            
        } // SaveToDatabase

        /// <summary>
        /// if the name wasn't found, returns a new CmsPersistentVariable object, with the .Name set to String.Empty
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CmsPersistentVariable Fetch(string name)
        {
            PersistentVariableRepository repository = new PersistentVariableRepository();            
            return repository.FetchbyName(name);
        } // Fetch

        public static CmsPersistentVariable[] FetchAll()
        {
            PersistentVariableRepository repository = new PersistentVariableRepository();
            List<CmsPersistentVariable> allVariables = repository.GetAll() as List<CmsPersistentVariable>;
            return allVariables.ToArray();
        } // FetchAll

        public static CmsPersistentVariable[] FetchAllWithNamePrefix(string namePrefix)
        {
            PersistentVariableRepository repository = new PersistentVariableRepository();

            return repository.FetchAllWithNamePrefix(namePrefix);
        }

        public static void Delete(CmsPersistentVariable persistentVariableToDelete)
        {

            PersistentVariableRepository repository = new PersistentVariableRepository();
            repository.Delete(persistentVariableToDelete);

        } // Delete

       
    }
}
