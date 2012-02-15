using Migrator.Framework;
using System.Data;
using System;

[Migration(3)]
public class AddIdToZoneUserRoleTable : Migration
{
    override public void Up()
    {
        Database.BeginTransaction();
        try
        {
            Database.ExecuteNonQuery("ALTER TABLE `zoneuserrole` ADD COLUMN `zoneuserroleid` INT NOT NULL AUTO_INCREMENT  FIRST, ADD PRIMARY KEY (`zoneuserroleid`)");

            Database.AddForeignKey("zoneuserrole_zone_fk_1", "zoneuserrole", "zoneid", "zone", "zoneid", Migrator.Framework.ForeignKeyConstraint.Cascade);
            string[] uniquecolumns = { "zoneid", "userroleid" };
            Database.AddUniqueConstraint("zoneid_unique", "zoneuserrole", uniquecolumns);
            Database.Commit();
        }
        catch(Exception)
        {
            
            Database.Rollback();
        }
    }
    override public void Down()
    {
        Database.BeginTransaction();
        try
        {
            
            //step to remove index and keys
            //1. remove foreign key
            //2. remove unique constarint
            //3. remove columns if it is auto increment field
            //4. remove primary index
            
            if (!Database.ConstraintExists("zoneuserrole", "zoneuserrole_zone_fk_1"))
                Database.ExecuteNonQuery("ALTER TABLE `zoneuserrole` ADD INDEX `zoneuserrole_zone_fk_1` (`ZoneId` ASC)");
            Database.RemoveForeignKey("zoneuserrole", "zoneuserrole_zone_fk_1");
            Database.RemoveConstraint("zoneuserrole", "zoneid_unique");
            
            Database.RemoveColumn("zoneuserrole", "zoneuserroleid");
            Database.RemoveConstraint("zoneuserrole", "PRIMARY");
            Database.Commit();
        }
        catch (Exception)
        {
            Database.Rollback();
        }
    }
}
