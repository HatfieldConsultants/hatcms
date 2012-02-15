using Migrator.Framework;
using System.Data;
using System;

[Migration(4)]
public class AddIdToPageLanguageInfoTable : Migration
{
    override public void Up()
    {
        Database.BeginTransaction();
        try
        {
            Database.RemoveConstraint("pagelanginfo", "PRIMARY");
            Database.ExecuteNonQuery("ALTER TABLE `pagelanginfo` ADD COLUMN `pagelangId` INT NOT NULL AUTO_INCREMENT  FIRST, ADD PRIMARY KEY (`pagelangId`)");

            string[] uniquecolumns = { "pageId", "langCode" };
            Database.AddUniqueConstraint("pageid_langcode_unique_constraint", "pagelanginfo", uniquecolumns);
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
            Database.RemoveConstraint("pagelanginfo", "pageid_langcode_unique_constraint");
            Database.RemoveColumn("pagelanginfo", "pagelangId");
            string[] primarykeycolumn = { "pageId", "langCode" };
            Database.AddPrimaryKey("", "pagelanginfo", primarykeycolumn);
            Database.Commit();
        }
        catch (Exception)
        {
            Database.Rollback();
        }
    }
}