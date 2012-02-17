using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Migrator.Framework;
using Migrator;

namespace HatCMS.Core.Migration
{
    public class HatCMSMigrator : IMigrator
    {
        #region IMigrator Members
        private Migrator.Migrator migrator;
 
        public HatCMSMigrator(string provider, string connectionString, string pathtomigrationAssembly)
        {
            Assembly migrationAssembly = Assembly.LoadFrom(pathtomigrationAssembly);
            migrator = new Migrator.Migrator(provider, connectionString, migrationAssembly);
        
        }

        public void Migrate()
        {
            migrator.MigrateToLastVersion();
        }

        public IList<long> GetAvailableVersions()
        {
            return migrator.AppliedMigrations;
        }

        #endregion
    }
}
