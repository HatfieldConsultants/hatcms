using System;
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

namespace HatCMS.Placeholders
{
    public class JobPostingLocation
    {
        public int JobLocationId;
        private string LocationText; // multi-lingual
        public bool IsAllLocations;
        public int SortOrdinal;

        public string getLocationText(CmsLanguage forLanguage)
        {
            if (CmsConfig.Languages.Length < 2)
                return LocationText;

            int index = CmsLanguage.IndexOf(forLanguage.shortCode, CmsConfig.Languages);
            if (index < 0)
                throw new ArgumentException("Error: the joblocations table needs to be updated to make all LocationTexts multi-lingual");

            string[] langParts = LocationText.Split(new char[] { CmsConfig.PerLanguageConfigSplitter });
            if (index > langParts.Length-1)
                throw new ArgumentException("Error: the joblocations table needs to be updated to make all LocationTexts multi-lingual");

            return langParts[index];
        }

        public static NameValueCollection ToNameValueCollection(JobPostingLocation[] locations, CmsLanguage displayLanguage, bool includeAllLocations)
        {
            NameValueCollection ret = new NameValueCollection();
            foreach (JobPostingLocation loc in locations)
            {
                if (includeAllLocations || (!includeAllLocations && !loc.IsAllLocations))
                    ret.Add(loc.JobLocationId.ToString(), loc.getLocationText(displayLanguage));
            } // foreach
            return ret;
        }

        /// <summary>
        /// Returns a new JobPostingLocation object (with id &lt 0) if a location does not have .IsAllLocations set inside of the haystack.
        /// </summary>
        /// <param name="forLanguage"></param>
        /// <param name="haystack"></param>
        /// <returns></returns>
        public static JobPostingLocation getAllLocations(JobPostingLocation[] haystack)
        {
            foreach (JobPostingLocation loc in haystack)
            {
                if (loc.IsAllLocations)
                    return loc;
            }
            return new JobPostingLocation();
        }

        public JobPostingLocation()
        {
            JobLocationId = -1;
            LocationText = "";
            IsAllLocations = false;
            SortOrdinal = 0;
        }
        public bool SaveToDatabase()
        {
            if (this.JobLocationId < 0)
            {
                return (new JobPostingLocationDb()).Insert(this);
            }
            else
            {
                return (new JobPostingLocationDb()).Update(this);
            }
        } // SaveToDatabase

        public static JobPostingLocation Fetch(int JobLocationId)
        {
            return (new JobPostingLocationDb()).Fetch(JobLocationId);
        } // Fetch

        public static JobPostingLocation[] FetchAll()
        {
            return (new JobPostingLocationDb()).FetchAll();
        } // FetchAll

        private static int CompareBySortOrdinal(JobPostingLocation x, JobPostingLocation y)
        {
            return x.SortOrdinal.CompareTo(y.SortOrdinal);
        }

        public static JobPostingLocation[] SortBySortOrdinal(JobPostingLocation[] haystack)
        {
            List<JobPostingLocation> ret = new List<JobPostingLocation>(haystack);
            ret.Sort(CompareBySortOrdinal);
            return ret.ToArray();
        }

        private class JobPostingLocationDb : PlaceholderDb
        {
            public bool Insert(JobPostingLocation item)
            {
                string sql = "INSERT INTO joblocations ";
                sql += "(LocationText, IsAllLocations, SortOrdinal)";
                sql += " VALUES ( ";
                sql += "'" + dbEncode(item.LocationText) + "'" + ", ";
                sql += Convert.ToInt32(item.IsAllLocations).ToString() + ", ";
                sql += item.SortOrdinal.ToString() + " ";
                sql += " ); ";

                int newId = this.RunInsertQuery(sql);
                if (newId > -1)
                {
                    item.JobLocationId = newId;
                    return true;
                }
                return false;

            } // Insert

            public bool Update(JobPostingLocation item)
            {
                string sql = "UPDATE joblocations SET ";
                sql += "LocationText = " + "'" + dbEncode(item.LocationText) + "'" + ", ";
                sql += "IsAllLocations = " + Convert.ToInt32(item.IsAllLocations).ToString() + ", ";
                sql += "SortOrdinal = " + item.SortOrdinal.ToString() + " ";
                sql += " WHERE JobLocationId = " + item.JobLocationId.ToString();
                sql += " ; ";

                int numAffected = this.RunUpdateQuery(sql);
                if (numAffected < 0)
                {
                    return false;
                }
                return true;

            } // Update

            private JobPostingLocation GetFromRow(DataRow dr)
            {
                JobPostingLocation item = new JobPostingLocation();
                item.JobLocationId = Convert.ToInt32(dr["JobLocationId"]);

                item.LocationText = (dr["LocationText"]).ToString();

                item.IsAllLocations = Convert.ToBoolean(dr["IsAllLocations"]);

                item.SortOrdinal = Convert.ToInt32(dr["SortOrdinal"]);

                return item;
            } // GetFromRow

            public JobPostingLocation Fetch(int JobLocationId)
            {
                if (JobLocationId < 0)
                    return new JobPostingLocation();
                string sql = "SELECT JobLocationId, LocationText, IsAllLocations, SortOrdinal from joblocations ";
                sql += " WHERE JobLocationId = " + JobLocationId.ToString();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasSingleRow(ds))
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    return GetFromRow(dr);
                }
                return new JobPostingLocation();
            } // Fetch

            public JobPostingLocation[] FetchAll()
            {
                string sql = "SELECT JobLocationId, LocationText, IsAllLocations, SortOrdinal from joblocations order by SortOrdinal";
                

                List<JobPostingLocation> arrayList = new List<JobPostingLocation>();
                DataSet ds = this.RunSelectQuery(sql);
                if (this.hasRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        arrayList.Add(GetFromRow(dr));
                    } // foreach row
                } // if there is data

                return arrayList.ToArray();
            } // FetchAll

        } // database class
    }
}
