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
using Hatfield.Web.Portal.Data;

namespace HatCMS.Placeholders
{
    public class UserFeedbackDb : PlaceholderDb
    {
        public UserFeedbackFormInfo getUserFeedbackFormInfo(CmsPage page, int identifier, CmsLanguage lang, bool createNewIfDoesNotExist)
        {
            if (page.ID < 0 || identifier < 0)
                return new UserFeedbackFormInfo();

            string sql = "select EmailAddressesToNotify, ThankyouMessage, FormFieldDisplayWidth, TextAreaQuestion from userfeedbackform c ";
            sql +=" where c.pageid = " + page.ID.ToString() + " and c.identifier = " + identifier.ToString() + " and c.LangCode = '" + dbEncode(lang.shortCode) + "';";
            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                DataRow dr = ds.Tables[0].Rows[0];
                UserFeedbackFormInfo info = new UserFeedbackFormInfo();
                info.EmailAddressesToNotify = dr["EmailAddressesToNotify"].ToString();
                info.ThankyouMessage = dr["ThankyouMessage"].ToString();
                info.FormFieldDisplayWidth = Convert.ToInt32(dr["FormFieldDisplayWidth"]);
                info.TextAreaQuestion = dr["TextAreaQuestion"].ToString();
                
                return info;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    return createUserFeedbackForm(page, identifier, lang);
                }
                else
                {
                    throw new Exception("getUserFeedbackFormInfo database error: placeholder does not exist");
                }
            }
            return new UserFeedbackFormInfo();
        } // getHtmlContent

        public UserFeedbackFormInfo createUserFeedbackForm(CmsPage page, int identifier, CmsLanguage lang)
        {
            UserFeedbackFormInfo d = new UserFeedbackFormInfo();
            string sql = "insert into userfeedbackform (pageid, identifier, LangCode, EmailAddressesToNotify, ThankyouMessage, FormFieldDisplayWidth, TextAreaQuestion) values (";
            sql = sql + page.ID.ToString() + "," + identifier.ToString() + ",'" + dbEncode(lang.shortCode) + "',";
            sql += "'"+dbEncode(d.EmailAddressesToNotify)+"', ";
            sql += "'" + dbEncode(d.ThankyouMessage) + "', ";
            sql += "" + d.FormFieldDisplayWidth.ToString() + ", ";
            sql += "'" + dbEncode(d.TextAreaQuestion) + "' ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                page.setLastUpdatedDateTimeToNow();
                             
                return d;
            }
            else
                return new UserFeedbackFormInfo();

        }

        public bool saveUpdatedUserFeedbackFormInfo(CmsPage page, int identifier, CmsLanguage lang, UserFeedbackFormInfo formInfo)
        {
            string sql = "update userfeedbackform set ";
            sql += " EmailAddressesToNotify = '" + dbEncode(formInfo.EmailAddressesToNotify) + "', ";
            sql += " ThankyouMessage = '" + dbEncode(formInfo.ThankyouMessage) + "', ";
            sql += " FormFieldDisplayWidth = " + formInfo.FormFieldDisplayWidth + ", ";
            sql += " TextAreaQuestion = '" + dbEncode(formInfo.TextAreaQuestion) + "' ";
            
            sql += " where pageid= " + page.ID.ToString();
            sql += " AND identifier = " + identifier.ToString();
            sql += " AND LangCode = '" + dbEncode(lang.shortCode) + "';";

            int numAffected = this.RunUpdateQuery(sql);
            if (numAffected > 0)
                return page.setLastUpdatedDateTimeToNow();
            else
                return false;

        }

        public bool saveUserFeedbackSubmittedData(UserFeedbackSubmittedData submittedData)
        {
            string sql = "insert into userfeedbacksubmitteddata (dateTimeSubmitted, Name, EmailAddress, Location, TextAreaQuestion, TextAreaValue, ReferringUrl ) VALUES (";
            sql += "NOW(), ";
            sql += "'" + dbEncode(submittedData.Name) + "', ";
            sql += "'" + dbEncode(submittedData.EmailAddress) + "', ";
            sql += "'" + dbEncode(submittedData.Location) + "', ";
            sql += "'" + dbEncode(submittedData.TextAreaQuestion) + "', ";
            sql += "'" + dbEncode(submittedData.TextAreaValue) + "', ";
            sql += "'" + dbEncode(submittedData.ReferringUrl) + "' ";
            sql += ");";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                return true;
            }
            return false;
        }

        public UserFeedbackSubmittedData[] FetchAllUserFeedbackSubmittedData()
        {
            List<UserFeedbackSubmittedData> ret = new List<UserFeedbackSubmittedData>();
            string sql = "select dateTimeSubmitted, Name, EmailAddress, Location, TextAreaQuestion, TextAreaValue, ReferringUrl ";
            sql += " from userfeedbacksubmitteddata order by dateTimeSubmitted ";
            DataSet ds = RunSelectQuery(sql);
            if (hasRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    UserFeedbackSubmittedData d = new UserFeedbackSubmittedData();
                    d.dateTimeSubmitted = Convert.ToDateTime(dr["dateTimeSubmitted"]);
                    d.Name = dr["Name"].ToString();
                    d.EmailAddress = dr["EmailAddress"].ToString();
                    d.Location = dr["Location"].ToString();
                    d.TextAreaQuestion = dr["TextAreaQuestion"].ToString();
                    d.TextAreaValue = dr["TextAreaValue"].ToString();
                    d.ReferringUrl = dr["ReferringUrl"].ToString();
                    ret.Add(d);
                } // foreach
            }
            return ret.ToArray();
        }

        public GridView FetchAllUserFeedbackSubmittedDataAsGrid()
        {
            string sql = "select dateTimeSubmitted, Name, EmailAddress, Location, TextAreaQuestion, TextAreaValue, ReferringUrl from userfeedbacksubmitteddata order by dateTimeSubmitted;";
            DataSet ds = this.RunSelectQuery(sql);

            GridView gv = new GridView();
            gv.DataSource = ds;
            gv.DataBind();
            return gv;
        }
    }
}
