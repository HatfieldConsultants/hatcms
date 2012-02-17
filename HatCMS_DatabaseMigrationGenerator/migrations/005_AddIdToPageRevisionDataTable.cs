using Migrator.Framework;
using System.Data;
using System;

[Migration(5)]
public class AddIdToPageRevisionTable : Migration
{
    override public void Up()
    {
        Database.BeginTransaction();
        try
        {
            Database.RemoveConstraint("pagerevisiondata", "PRIMARY");
            Database.ExecuteNonQuery("ALTER TABLE `pagerevisiondata` ADD COLUMN `PageRevisionDataId` INT NOT NULL AUTO_INCREMENT  FIRST, ADD PRIMARY KEY (`PageRevisionDataId`)");

            string[] uniquecolumns = { "PageId", "RevisionNumber" };
            Database.AddUniqueConstraint("pageid_revisionnumber_unique_constraint", "pagerevisiondata", uniquecolumns);
            Database.Commit();
        }
        catch (Exception)
        {
            Database.Rollback();

        }


    }

    override public void Down()
    {

        Database.BeginTransaction();
        try
        {
            Database.RemoveConstraint("pagerevisiondata", "pageid_revisionnumber_unique_constraint");
            Database.RemoveColumn("pagerevisiondata", "PageRevisionDataId");
            string[] primarykeycolumn = { "PageId", "RevisionNumber" };
            Database.AddPrimaryKey("", "pagerevisiondata", primarykeycolumn);
            Database.Commit();
        }
        catch (Exception)
        {
            Database.Rollback();
        }
    }
}