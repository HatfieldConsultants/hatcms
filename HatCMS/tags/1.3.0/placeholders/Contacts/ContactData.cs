using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// <summary>
    /// data storage for a single contact
    /// </summary>
    public class ContactData
    {
        public int contactId = -1;
        public string firstName = "";
        public string lastName = "";
        public string title = "";
        public string organizationName = "";
        public string address1 = "";
        public string address2 = "";
        public string city = "";
        public string provinceState = "";
        public string postalZipCode = "";
        public string phoneNumber1 = "";
        public string phoneNumber2 = "";
        public string faxNumber = "";
        public string mobileNumber = "";
        public string emailAddress = "";
        public List<int> contactCategoryIds = new List<int>();


        /// <summary>
        /// ASCII encoded Admin Email Address.
        /// Source: http://www.cogworks.co.uk
        /// Only to be used for web-side display, not for data use.
        /// </summary>
        public string SpamEncodedEmailAddress
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (char c in emailAddress)
                {
                    sb.AppendFormat("&#{0}", Convert.ToInt32(c));
                }
                return sb.ToString();
            }
        }

        public ContactDataCategory[] getCategories()
        {
            return ContactDataCategory.getContactCategories(contactCategoryIds.ToArray());
        }


        public bool SaveToDatabase()
        {
            if (contactId < 0)
                return new ContactDataDb().Insert(this);
            else
                return new ContactDataDb().Update(this);
        }

        public static string[] getAllOrganizationNames(ContactData[] contacts)
        {
            List<string> orgNames = new List<string>();
            foreach (ContactData contact in contacts)
            {
                if (orgNames.IndexOf(contact.organizationName) < 0)
                    orgNames.Add(contact.organizationName);
            } // foreach

            orgNames.Sort();
            return orgNames.ToArray();
        }

        public static string[] getAllOrganizationNames()
        {
            return new ContactDataDb().getAllOrganizationNames();
        }

        public static ContactData getContact(int contactId)
        {
            return new ContactDataDb().getContact(contactId);
        } // getContacts

        public static ContactData[] getContacts(ContactPlaceholderData data, int[] categoryIds, string[] orgNamesToDisplay)
        {
            return new ContactDataDb().getContacts(data, categoryIds, orgNamesToDisplay);
        } // getContacts

        public static bool DeleteContact(ContactData contactToDelete)
        {
            return new ContactDataDb().Delete(contactToDelete.contactId);
        }

        #region Contact Data Database
        private class ContactDataDb : PlaceholderDb
        {
            public bool Insert(ContactData item)
            {
                string sql = "INSERT INTO contactdata ";
                sql += "(firstName, lastName, title, organizationName, address1, address2, city, provinceState, postalZipCode, phoneNumber1, phoneNumber2, faxNumber, mobileNumber, emailAddress)";
                sql += " VALUES ( ";
                sql += "'" + dbEncode(item.firstName) + "'" + ", ";
                sql += "'" + dbEncode(item.lastName) + "'" + ", ";
                sql += "'" + dbEncode(item.title) + "'" + ", ";
                sql += "'" + dbEncode(item.organizationName) + "'" + ", ";
                sql += "'" + dbEncode(item.address1) + "'" + ", ";
                sql += "'" + dbEncode(item.address2) + "'" + ", ";
                sql += "'" + dbEncode(item.city) + "'" + ", ";
                sql += "'" + dbEncode(item.provinceState) + "'" + ", ";
                sql += "'" + dbEncode(item.postalZipCode) + "'" + ", ";
                sql += "'" + dbEncode(item.phoneNumber1) + "'" + ", ";
                sql += "'" + dbEncode(item.phoneNumber2) + "'" + ", ";
                sql += "'" + dbEncode(item.faxNumber) + "'" + ", ";
                sql += "'" + dbEncode(item.mobileNumber) + "'" + ", ";
                sql += "'" + dbEncode(item.emailAddress) + "'" + " ";                
                sql += " ); ";

                int newId = this.RunInsertQuery(sql);
                if (newId > -1)
                {
                    item.contactId = newId;
                    return updateCategoryIds(item);
                }
                return false;
            }

            public bool Update(ContactData item)
            {
                string sql = "UPDATE contactdata SET ";
                sql += "firstName = " + "'" + dbEncode(item.firstName) + "'" + ", ";
                sql += "lastName = " + "'" + dbEncode(item.lastName) + "'" + ", ";
                sql += "title = " + "'" + dbEncode(item.title) + "'" + ", ";
                sql += "organizationName = " + "'" + dbEncode(item.organizationName) + "'" + ", ";
                sql += "address1 = " + "'" + dbEncode(item.address1) + "'" + ", ";
                sql += "address2 = " + "'" + dbEncode(item.address2) + "'" + ", ";
                sql += "city = " + "'" + dbEncode(item.city) + "'" + ", ";
                sql += "provinceState = " + "'" + dbEncode(item.provinceState) + "'" + ", ";
                sql += "postalZipCode = " + "'" + dbEncode(item.postalZipCode) + "'" + ", ";
                sql += "phoneNumber1 = " + "'" + dbEncode(item.phoneNumber1) + "'" + ", ";
                sql += "phoneNumber2 = " + "'" + dbEncode(item.phoneNumber2) + "'" + ", ";
                sql += "faxNumber = " + "'" + dbEncode(item.faxNumber) + "'" + ", ";
                sql += "mobileNumber = " + "'" + dbEncode(item.mobileNumber) + "'" + ", ";
                sql += "emailAddress = " + "'" + dbEncode(item.emailAddress) + "'" + " ";                
                sql += " WHERE ContactId = " + item.contactId.ToString();
                sql += " ; ";

                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected < 0)
                {
                    return false;
                }
                return updateCategoryIds(item);
            }

            private bool updateCategoryIds(ContactData item)
            {
                string delSql = "delete from contactlinktocategory where ContactId = "+item.contactId;

                int numDeleted = this.RunUpdateQuery(delSql);

                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO contactlinktocategory ");
                sql.Append("(ContactId, CategoryId)");
                sql.Append(" VALUES ");
                foreach (int catId in item.contactCategoryIds)
                {
                    sql.Append(" ( ");
                    sql.Append(item.contactId.ToString() + ", ");
                    sql.Append(catId.ToString() + " ");
                    sql.Append(" ),");
                } // foreach

                // remove trailing comma
                string s = sql.ToString().Substring(0, sql.ToString().Length - 1);
                int numInserted = this.RunUpdateQuery(s); // do not use RunInsertQuery
                if (numInserted == item.contactCategoryIds.Count)
                    return true;

                return false;
            } // updateCategoryIds

            public bool Delete(int ContactId)
            {
                string sql = "UPDATE contactdata ";
                sql += " set deleted = " + dbEncode(DateTime.Now) + " ";
                sql += " WHERE ContactId = " + ContactId.ToString();
                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected < 0)
                {
                    return false;
                }
                return true;
            }

            public string[] getAllOrganizationNames()
            {
                string sql = "select distinct TRIM(organizationName) as organizationName from contactdata where organizationName != '' order by organizationName;";
                DataSet ds = this.RunSelectQuery(sql);
                List<string> ret = new List<string>();
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ret.Add(dr["organizationName"].ToString());
                    } // foreach
                }

                return ret.ToArray();
            }

            public ContactData getContact(int contactId)
            {
                string sql = "SELECT c.ContactId, c.firstName, c.lastName, c.title, c.organizationName, c.address1, c.address2, c.city, c.provinceState, c.postalZipCode, c.phoneNumber1, c.phoneNumber2, c.faxNumber, c.mobileNumber, c.emailAddress, l.CategoryId ";
                sql += " from contactdata c left join ContactLinkToCategory l on (c.ContactId = l.ContactId) ";
                sql += " WHERE " + DBDialect.isNull("c.Deleted") + " ";

                sql += " AND C.ContactId = " + contactId + "; ";

                Dictionary<int, ContactData> list = new Dictionary<int, ContactData>();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ContactData item = new ContactData();
                        item.contactId = Convert.ToInt32(dr["ContactId"]);
                        if (list.ContainsKey(item.contactId))
                        {
                            item = list[item.contactId];
                        }
                        else
                        {
                            item.firstName = (dr["firstName"]).ToString();
                            item.lastName = (dr["lastName"]).ToString();
                            item.title = (dr["title"]).ToString();
                            item.organizationName = (dr["organizationName"]).ToString();
                            item.address1 = (dr["address1"]).ToString();
                            item.address2 = (dr["address2"]).ToString();
                            item.city = (dr["city"]).ToString();
                            item.provinceState = (dr["provinceState"]).ToString();
                            item.postalZipCode = (dr["postalZipCode"]).ToString();
                            item.phoneNumber1 = (dr["phoneNumber1"]).ToString();
                            item.phoneNumber2 = (dr["phoneNumber2"]).ToString();
                            item.faxNumber = (dr["faxNumber"]).ToString();
                            item.mobileNumber = (dr["mobileNumber"]).ToString();
                            item.emailAddress = (dr["emailAddress"]).ToString();
                        }

                        int CategoryId = getPossiblyNullValue(dr, "CategoryId", Int32.MinValue);
                        if (CategoryId >= 0)
                            item.contactCategoryIds.Add(CategoryId);

                        if (!list.ContainsKey(item.contactId))
                        {
                            list.Add(item.contactId, item);
                        }

                    } // foreach row
                } // if there is data                

                List<ContactData> ret = new List<ContactData>(list.Values);
                if (ret.Count == 1)
                {
                    ContactData[] arr = ret.ToArray();
                    return arr[0];
                }
                return new ContactData();
            } // getContact

            public ContactData[] getContacts(ContactPlaceholderData data, int[] categoryIds, string[] orgNamesToDisplay)
            {
                string sql = "SELECT c.ContactId, c.firstName, c.lastName, c.title, c.organizationName, c.address1, c.address2, c.city, c.provinceState, c.postalZipCode, c.phoneNumber1, c.phoneNumber2, c.faxNumber, c.mobileNumber, c.emailAddress, l.CategoryId ";
                sql += " from contactdata c left join ContactLinkToCategory l on (c.ContactId = l.ContactId) ";
                sql += " WHERE " + DBDialect.isNull("c.Deleted") + " ";

                if (categoryIds.Length > 0)
                {
                    sql += " AND l.CategoryId in (" + StringUtils.Join(",", categoryIds) + ") ";
                }

                if (orgNamesToDisplay.Length > 0)
                {
                    foreach (string org in orgNamesToDisplay)
                    {
                        sql += " AND c.organizationName like '" + dbEncode(org) + "' ";
                    }
                }

                switch (data.nameDisplayMode)
                {
                    case ContactPlaceholderData.ContactNameDisplayMode.FirstnameLastname:
                        sql += " ORDER BY c.firstName ";
                        break;
                    case ContactPlaceholderData.ContactNameDisplayMode.LastnameFirstname:
                        sql += " ORDER BY c.lastName ";
                        break;
                    default:
                        throw new ArgumentException("invalid ContactNameDisplayMode");
                }// switch

                
                Dictionary<int, ContactData> list = new Dictionary<int, ContactData>();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ContactData item = new ContactData();
                        item.contactId = Convert.ToInt32(dr["ContactId"]);
                        if (list.ContainsKey(item.contactId))
                        {
                            item = list[item.contactId];
                        }
                        else
                        {
                            item.firstName = (dr["firstName"]).ToString();
                            item.lastName = (dr["lastName"]).ToString();
                            item.title = (dr["title"]).ToString();
                            item.organizationName = (dr["organizationName"]).ToString();
                            item.address1 = (dr["address1"]).ToString();
                            item.address2 = (dr["address2"]).ToString();
                            item.city = (dr["city"]).ToString();
                            item.provinceState = (dr["provinceState"]).ToString();
                            item.postalZipCode = (dr["postalZipCode"]).ToString();
                            item.phoneNumber1 = (dr["phoneNumber1"]).ToString();
                            item.phoneNumber2 = (dr["phoneNumber2"]).ToString();
                            item.faxNumber = (dr["faxNumber"]).ToString();
                            item.mobileNumber = (dr["mobileNumber"]).ToString();
                            item.emailAddress = (dr["emailAddress"]).ToString();
                        }

                        int CategoryId = getPossiblyNullValue(dr, "CategoryId", Int32.MinValue);
                        if (CategoryId >= 0)
                            item.contactCategoryIds.Add(CategoryId);

                        if (!list.ContainsKey(item.contactId))
                        {
                            list.Add(item.contactId, item);
                        }

                    } // foreach row
                } // if there is data

                List<ContactData> ret = new List<ContactData>(list.Values);
                return ret.ToArray();
            } // getContacts
        } // ContactDataDb class
        #endregion
    }
}
