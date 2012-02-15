using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Placeholders
{
    public class ContactsDb : PlaceholderDb
    {
        public ContactPlaceholderData getContactPlaceholderData(CmsPage page, int identifier, bool createNewIfDoesNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new ContactPlaceholderData();

            string sql = "select numColumnsToShow, nameDisplayMode, forceFilterToCategoryId, allowFilterByCategory, allowFilterByCompany, accessLevelToEditContacts, accessLevelToAddContacts from contacts c where c.pageid = " + page.ID.ToString() + " and c.identifier = " + identifier.ToString() + " and deleted is null;";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                ContactPlaceholderData data = new ContactPlaceholderData();
                data.numColumnsToShow = Convert.ToInt32(dr["numColumnsToShow"]);
                
                data.nameDisplayMode = (ContactPlaceholderData.ContactNameDisplayMode)Enum.Parse(typeof(ContactPlaceholderData.ContactNameDisplayMode), dr["nameDisplayMode"].ToString());
                data.forceFilterToCategoryId = Convert.ToInt32(dr["forceFilterToCategoryId"]);
                data.allowFilterByCategory = Convert.ToBoolean(dr["allowFilterByCategory"]);
                data.allowFilterByCompany = Convert.ToBoolean(dr["allowFilterByCompany"]);
                data.accessLevelToEditContacts = (BaseCmsPlaceholder.AccessLevel)Enum.Parse(typeof(BaseCmsPlaceholder.AccessLevel), dr["accessLevelToEditContacts"].ToString());
                data.accessLevelToAddContacts = (BaseCmsPlaceholder.AccessLevel)Enum.Parse(typeof(BaseCmsPlaceholder.AccessLevel), dr["accessLevelToAddContacts"].ToString());
                return data;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    return createNewContactPlaceholderData(page, identifier);
                }
                else
                {
                    throw new Exception("getContactPlaceholderData database error: placeholder does not exist");
                }
            }
            return new ContactPlaceholderData();
        } // getFlashObject

        public ContactPlaceholderData createNewContactPlaceholderData(CmsPage page, int identifier)
        {
            ContactPlaceholderData data = new ContactPlaceholderData();

            string sql = "insert into contacts (pageid, identifier, numColumnsToShow, nameDisplayMode,forceFilterToCategoryId, allowFilterByCategory, allowFilterByCompany, accessLevelToEditContacts, accessLevelToAddContacts) values (";
            sql = sql + page.ID.ToString() + "," + identifier.ToString() + ",";
            sql = sql + "" + data.numColumnsToShow.ToString() + ", ";            
            sql = sql + "'" + Enum.GetName(typeof(ContactPlaceholderData.ContactNameDisplayMode), data.nameDisplayMode) + "', ";
            sql = sql + "" + Convert.ToInt32(data.forceFilterToCategoryId).ToString() + ", ";
            sql = sql + "" + Convert.ToInt32(data.allowFilterByCategory).ToString() + ", ";
            sql = sql + "" + Convert.ToInt32(data.allowFilterByCompany).ToString() + ", ";            
            sql = sql + "'" + Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToEditContacts) + "', ";
            sql = sql + "'" + Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToAddContacts) + "' ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                page.setLastUpdatedDateTimeToNow();
            }

            return data;

        } // createNewPageFilesData

        public bool saveUpdatedContactPlaceholderData(CmsPage page, int identifier, ContactPlaceholderData data)
        {
            string sql = "update contacts set ";
            sql += " numColumnsToShow = " + data.numColumnsToShow.ToString() + ", ";            
            sql += " nameDisplayMode = '" + Enum.GetName(typeof(ContactPlaceholderData.ContactNameDisplayMode), data.nameDisplayMode) + "', ";
            sql += " forceFilterToCategoryId = " + Convert.ToInt32(data.forceFilterToCategoryId).ToString() + ", ";
            sql += " allowFilterByCategory = " + Convert.ToInt32(data.allowFilterByCategory).ToString() + ", ";
            sql += " allowFilterByCompany = " + Convert.ToInt32(data.allowFilterByCompany).ToString() + ", ";
            sql += " accessLevelToEditContacts = '" + Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToEditContacts) + "', ";
            sql += " accessLevelToAddContacts = '" + Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToAddContacts) + "' ";
            sql += " where pageid = " + page.ID.ToString();
            sql += " AND identifier = " + identifier.ToString() + "; ";

            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;

        } // saveUpdatedContactPlaceholderData

        

    } // class ContactsDb
}
