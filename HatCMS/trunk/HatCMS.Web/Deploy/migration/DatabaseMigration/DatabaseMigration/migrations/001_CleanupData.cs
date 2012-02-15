using Migrator.Framework;
using System.Data;

[Migration(1)]
public class CleanupData : Migration
{
    override public void Up()
    {
        Database.BeginTransaction();
        string[] columns = new string[1];
        columns[0] = "parentPageId";
        string[] values = { "0" };
        int affectedRows = Database.Update("pages", columns, values, "pageId = 1");
        if (affectedRows == 1)
            Database.Commit();
        else
            Database.Rollback();

        Database.BeginTransaction();
        columns[0] = "name";
        values[0] = "";
        affectedRows = Database.Update("pagelanginfo", columns, values, "pageId = 1");
        if (affectedRows == 1)
            Database.Commit();
        else
            Database.Rollback();


    }
    override public void Down()
    {
        Database.BeginTransaction();
        string[] columns = new string[1];
        columns[0] = "parentPageId";
        string[] values = { "-1" };
        int affectedRows = Database.Update("pages", columns, values, "pageId = 1");
        if (affectedRows == 1)
            Database.Commit();
        else
            Database.Rollback();

        Database.BeginTransaction();
        columns[0] = "name";
        values[0] = null;
        affectedRows = Database.Update("pagelanginfo", columns, values, "pageId = 1");
        if (affectedRows == 1)
            Database.Commit();
        else
            Database.Rollback();
    }
}
