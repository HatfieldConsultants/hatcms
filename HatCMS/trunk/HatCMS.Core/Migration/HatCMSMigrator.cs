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

        public HatCMSMigrator()
        {
            string assemblypath = System.Web.Hosting.HostingEnvironment.MapPath("~/Deploy/MyMigrations.dll");
            Assembly migrationAssembly = Assembly.LoadFrom(assemblypath);
            string provider = "MySql";
            string connectionString = "Data Source=localhost;Database=hatcms_test;User Id=hatcms;Password=hatcms";
            migrator = new Migrator.Migrator(provider, connectionString, migrationAssembly);
        }

        public int currentVersion()
        {
            return migrator.AppliedMigrations.Count;
        }

        public bool IsOutDated()
        {
            return migrator.MigrationsTypes.Count > migrator.AppliedMigrations.Count;

        }

        public void Migrate()
        {
            migrator.MigrateToLastVersion();
        }

        #endregion
    }
}
