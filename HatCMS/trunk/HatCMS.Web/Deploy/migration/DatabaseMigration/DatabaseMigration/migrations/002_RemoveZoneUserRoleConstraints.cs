using Migrator.Framework;
using System.Data;
using System;

[Migration(2)]
public class RemoveZoneUserRoleConstraints : Migration
{
    override public void Up()
    {
        
        try
        {
            Database.BeginTransaction();
            if (!Database.ConstraintExists("zoneuserrole", "zoneuserrole_ibfk_1"))
                Database.ExecuteNonQuery("ALTER TABLE `zoneuserrole` ADD INDEX `zoneuserrole_ibfk_1` (`ZoneId` ASC)");
            Database.RemoveForeignKey("zoneuserrole", "zoneuserrole_ibfk_1"); 

            Database.RemoveConstraint("zoneuserrole", "PRIMARY");
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
            string[] primarykeycolumns = { "ZoneId", "UserRoleId" };
            Database.AddPrimaryKey("primarykey", "zoneuserrole", primarykeycolumns);            
            Database.AddForeignKey("zoneuserrole_ibfk_1", "zoneuserrole", "ZoneId", "zone", "ZoneId");            
            Database.Commit();
        }
        catch (Exception ex)
        {
            Database.Rollback();
        }
    }
}