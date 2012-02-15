using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using SharpArch.Core.PersistenceSupport;
using NHibernate;
using SharpArch.Data.NHibernate;

namespace HatCMS.Core.DataPersitentHelper
{
    

    public class DataPersister
    {
        protected ISession session;
        protected IDbContext dbcontext;

        public DataPersister()
        {
            this.session = NHibernateSession.Current;
            this.dbcontext = new DbContext(SessionFactoryKeyHelper.GetKey());        
        }

        public IDbContext getDBContext()
        {
            return this.dbcontext;
        }

    }
}
