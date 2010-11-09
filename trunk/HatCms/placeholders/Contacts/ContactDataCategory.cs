using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;
using Hatfield.Web.Portal.Data;

namespace HatCMS.Placeholders
{
    public class ContactDataCategory
    {
        private int categoryId;
        public int CategoryId
        {
            get { return categoryId; }
        }
        private string colourHex;
        public string ColourHex
        {
            get { return colourHex; }
            set { colourHex = value; }
        }
        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        private string description;
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public ContactDataCategory()
        {
            categoryId = -1;
            colourHex = "";
            title = "";
            description = "";
        }

        /// <summary>
        /// Saves this image to the database. Either inserts it, or updates it.
        /// </summary>
        /// <returns></returns>
        public bool SaveToDatabase()
        {
            ContactDataCategoryDb db = new ContactDataCategoryDb();
            if (categoryId < 0)
            {
                return db.Insert(this);
            }
            else
            {
                return db.Update(this);
            }
        } // SaveToDatabase

        public bool Delete()
        {
            ContactDataCategoryDb db = new ContactDataCategoryDb();
            return db.Delete(this);
        } // Delete


        public static ContactDataCategory getContactCategory(int categoryId)
        {
            ContactDataCategoryDb db = new ContactDataCategoryDb();
            return db.getCategory(categoryId);

        } // getContactCategory

        public static ContactDataCategory[] getAllContactCategories()
        {
            ContactDataCategoryDb db = new ContactDataCategoryDb();
            return db.getAllContactCategories();
        } // getAllContactCategories

        public static ContactDataCategory[] getContactCategories(int[] categoryIds)
        {
            ContactDataCategoryDb db = new ContactDataCategoryDb();
            return db.getContactCategories(categoryIds);
        } // getContactCategories



        #region ContactDataCategory Database Class
        private class ContactDataCategoryDb : PlaceholderDb
        {
            private ContactDataCategory fillFromDataRow(DataRow dr)
            {
                ContactDataCategory cat = new ContactDataCategory();
                cat.categoryId = Convert.ToInt32(dr["categoryId"]);
                cat.colourHex = dr["colourHex"].ToString();
                cat.title = dr["title"].ToString();
                cat.description = dr["description"].ToString();
                return cat;
            }

            public ContactDataCategory getCategory(int categoryId)
            {
                if (categoryId < 0)
                    return new ContactDataCategory();

                string sql = "select * from ContactDataCategory where categoryId = " + categoryId.ToString() + " ";
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasSingleRow(ds))
                {
                    DataRow dr = this.getSingleRow(ds);
                    ContactDataCategory cat = fillFromDataRow(dr);
                    return cat;
                }
                return new ContactDataCategory();
            } // getCategory

            public ContactDataCategory[] getAllContactCategories()
            {
                List<ContactDataCategory> tempList = new List<ContactDataCategory>();
                string sql = "select * from ContactDataCategory where " + DBDialect.isNull("Deleted") + " ";
                // sql += " ORDER BY title ";
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        tempList.Add(fillFromDataRow(dr));
                    } // foreach
                }

                // fill in return value

                return tempList.ToArray() ;

            } // getAllEventCategories

            public ContactDataCategory[] getContactCategories(int[] contactIds)
            {
                
                List<ContactDataCategory> tempList = new List<ContactDataCategory>();
                string sql = "select * from contactdatacategory where " + DBDialect.isNull("Deleted") + " ";
                sql += " AND categoryId in (" + StringUtils.Join(",", contactIds) + ") ";
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        tempList.Add(fillFromDataRow(dr));
                    } // foreach
                }

                // fill in return value

                return tempList.ToArray();

            } // getAllEventCategories

            public bool Insert(ContactDataCategory cat)
            {
                string sql = "INSERT into contactdatacategory (colourHex, title, description) VALUES ";
                sql += "(";
                sql += "'" + dbEncode(cat.colourHex) + "', ";
                sql += "'" + dbEncode(cat.title) + "', ";
                sql += "'" + dbEncode(cat.description) + "' ";
                sql += ") ";
                int newId = this.RunInsertQuery(sql);
                if (newId >= 0)
                {
                    cat.categoryId = newId;
                    return true;
                }
                return false;
            } // Insert

            public bool Update(ContactDataCategory cat)
            {
                try
                {
                    string sql = "UPDATE contactdatacategory SET ";
                    sql += "colourHex = '" + dbEncode(cat.colourHex) + "', ";
                    sql += "title = '" + dbEncode(cat.title) + "', ";
                    sql += "description = '" + dbEncode(cat.description) + "' ";

                    sql += "WHERE categoryId = " + cat.categoryId.ToString() + " ";
                    int numAffected = this.RunUpdateQuery(sql);
                    return (numAffected > 0);
                }
                catch
                { }
                return false;
            } // Update

            public bool Delete(ContactDataCategory cat)
            {
                try
                {
                    string sql = "UPDATE contactdatacategory C SET ";
                    sql += "C.DELETED = " + DBDialect.currentDateTime;

                    sql += " WHERE categoryId = " + cat.categoryId.ToString() + " ";
                    int numAffected = this.RunUpdateQuery(sql);
                    return (numAffected > 0);
                }
                catch
                { }
                return false;
            } // Update

        } // class ContactDataCategoryDb
        #endregion
    } // class ContactDataCategory
}
