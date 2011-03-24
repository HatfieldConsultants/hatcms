using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using HatCMS.Placeholders;
using System.Text;
using System.Collections.Generic;

namespace HatCMS.Placeholders.RegisterProject
{
    public class RegisterProjectDb : PlaceholderDb
    {
        protected string TableNameRegisterProject = "registerproject";

        public static string GetSqlColumns()
        {
            return "Name,Location,Description,ContactPerson,Email,Telephone,Cellphone,Website,FundingSource,CreatedDateTime,ClientIP";
        }

        public static string GetSqlColumnsWithId()
        {
            return "ProjectId," + GetSqlColumns();
        }

        public class RegisterProjectData
        {
            private int id = -1;
            public int Id
            {
                get { return id; }
                set { id = value; }
            }

            private string name = "";
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            private string location = "";
            public string Location
            {
                get { return location; }
                set { location = value; }
            }

            private string description = "";
            public string Description
            {
                get { return description; }
                set { description = value; }
            }

            private string contactPerson = "";
            public string ContactPerson
            {
                get { return contactPerson; }
                set { contactPerson = value; }
            }

            private string email = "";
            public string Email
            {
                get { return email; }
                set { email = value; }
            }

            private string telephone = "";
            public string Telephone
            {
                get { return telephone; }
                set { telephone = value; }
            }

            private string cellphone = "";
            public string Cellphone
            {
                get { return cellphone; }
                set { cellphone = value; }
            }

            private string website = "";
            public string Website
            {
                get { return website; }
                set { website = value; }
            }

            private string fundingSource = "";
            public string FundingSource
            {
                get { return fundingSource; }
                set { fundingSource = value; }
            }

            private DateTime createdDateTime = DateTime.MinValue;
            public DateTime CreatedDateTime
            {
                get { return createdDateTime; }
                set { createdDateTime = value; }
            }

            private string clientIp = "";
            public string ClientIp
            {
                get { return clientIp; }
                set { clientIp = value; }
            }
        }

        public bool insertData(RegisterProjectData entity)
        {
            StringBuilder sql = new StringBuilder("INSERT INTO ");
            sql.Append(TableNameRegisterProject);
            sql.Append(" (" + GetSqlColumns() + ") VALUES ('");
            sql.Append(dbEncode(entity.Name) + "','");
            sql.Append(dbEncode(entity.Location) + "','");
            sql.Append(dbEncode(entity.Description) + "','");
            sql.Append(dbEncode(entity.ContactPerson) + "','");
            sql.Append(dbEncode(entity.Email) + "','");
            sql.Append(dbEncode(entity.Telephone) + "','");
            sql.Append(dbEncode(entity.Cellphone) + "','");
            sql.Append(dbEncode(entity.Website) + "','");
            sql.Append(dbEncode(entity.FundingSource) + "',");
            sql.Append(dbEncode(entity.CreatedDateTime) + ",'");
            sql.Append(dbEncode(entity.ClientIp) + "');");

            int newId = this.RunInsertQuery(sql.ToString());
            if (newId > -1)
            {
                entity.Id = newId;
                return true;
            }
            return false;
        }

        protected DataSet fetchAllAsDataSet()
        {
            StringBuilder sql = new StringBuilder("SELECT " + GetSqlColumnsWithId() + " FROM ");
            sql.Append(TableNameRegisterProject);
            sql.Append(" ORDER BY ProjectId DESC;");
            return this.RunSelectQuery(sql.ToString());
        }

        public List<RegisterProjectData> fetchAll()
        {
            List<RegisterProjectData> list = new List<RegisterProjectData>();
            DataSet ds = fetchAllAsDataSet();
            if (this.hasRows(ds) == false)
                return list;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                RegisterProjectData entity = new RegisterProjectData();
                rowToData(dr, entity);
                list.Add(entity);
            }
            return list;
        }

        public GridView fetchAllAsGrid()
        {
            StringBuilder sql = new StringBuilder("SELECT " + GetSqlColumns() + " FROM ");
            sql.Append(TableNameRegisterProject);
            sql.Append(" ORDER BY ProjectId DESC;");
            DataSet ds = this.RunSelectQuery(sql.ToString());

            GridView gv = new GridView();
            gv.DataSource = ds;
            gv.DataBind();
            return gv;
        }

        protected void rowToData(DataRow dr, RegisterProjectData entity)
        {
            entity.Id = Convert.ToInt32(dr["ProjectId"]);
            entity.Name = dr["Name"].ToString();
            entity.Location = dr["Location"].ToString();
            entity.Description = dr["Description"].ToString();
            entity.ContactPerson = dr["ContactPerson"].ToString();
            entity.Email = dr["Email"].ToString();
            entity.Telephone = dr["Telephone"].ToString();
            entity.Cellphone = dr["Cellphone"].ToString();
            entity.Website = dr["Website"].ToString();
            entity.FundingSource = dr["FundingSource"].ToString();
            entity.CreatedDateTime = Convert.ToDateTime(dr["CreatedDateTime"]);
            entity.ClientIp = dr["ClientIp"].ToString();
        }
    }
}
