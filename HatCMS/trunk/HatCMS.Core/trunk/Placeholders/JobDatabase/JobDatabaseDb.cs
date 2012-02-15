using System;
using System.Data;
using System.Collections;

namespace HatCMS.Placeholders
{

    /// <summary>
    /// The database calls for the JobPostingDetailsData and JobPostingAggregatorData classes.
    /// </summary>
    public class JobPostingDb : PlaceholderDb
    {
        public JobPostingDetailsData getJobPostingDetailsData(CmsPage page, int identifier, CmsLanguage language, bool createNewIfDoesNotExist)
        {
            if (page.Id < 0 || identifier < 0)
                return new JobPostingDetailsData();

            string sql = "";
            sql = "select * from jobdetails s ";
            sql += " where s.pageid = " + page.Id.ToString() + " and s.identifier = " + identifier.ToString() + " and langShortCode = '" + dbEncode(language.shortCode) + "' and s.deleted is null;";

            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                JobPostingDetailsData data = new JobPostingDetailsData();
                DataRow dr = ds.Tables[0].Rows[0];

                data.JobId = Convert.ToInt32(dr["JobId"]);
                data.LocationId = Convert.ToInt32(dr["JobLocationId"]);
                data.RemoveAnonAccessAt = Convert.ToDateTime(dr["RemoveAnonAccessAt"].ToString());                

                return data;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    JobPostingDetailsData data = new JobPostingDetailsData();
                    bool b = createNewJobPostingDetailsData(page, identifier, language, data);

                    if (!b)
                    {
                        throw new Exception("getJobPostingDetailsData database error: Error creating new placeholder");
                    }
                    else
                    {
                        return data;
                    }
                }
                else
                {
                    throw new Exception("getJobPostingDetailsData database error: placeholder does not exist");
                }
            }

        } // getJobPostingDetailsData

       

        public bool createNewJobPostingDetailsData(CmsPage page, int identifier, CmsLanguage language, JobPostingDetailsData data)
        {
            string sql = "insert into jobdetails (pageid, identifier, langShortCode, JobLocationId, RemoveAnonAccessAt) values (";
            sql += page.Id.ToString() + "," + identifier.ToString() + ",";
            sql += "'" + dbEncode(language.shortCode) + "', ";
            sql += "" + (data.LocationId.ToString()) + ", ";
            sql += "" + dbEncode(data.RemoveAnonAccessAt) + " ";
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                data.JobId = newId;                               
                
                return page.setLastUpdatedDateTimeToNow();
            }
            else
                return false;
        }  // createNewJobPostingDetailsData

        public bool saveUpdatedJobPostingDetailsData(JobPostingDetailsData data)
        {
            if (data.JobId < 0)
                return false;

            string sql = "update jobdetails set ";
            sql += " JobLocationId = " + data.LocationId.ToString() + ", ";            
            sql += " RemoveAnonAccessAt = " + dbEncode(data.RemoveAnonAccessAt) + " ";                        
            sql += " where JobId = " + data.JobId.ToString();

            int numAffected = this.RunUpdateQuery(sql);
            return (numAffected > 0);

        }


        public JobPostingAggregatorData getJobPostingAggregatorData(CmsPage page, int identifier, CmsLanguage language, bool createNewIfDoesNotExist)
        {
            if (page.Id < 0 || identifier < 0)
                return new JobPostingAggregatorData();

            string sql = "";
            sql = "select * from jobsummary s ";
            sql += " where s.pageid = " + page.Id.ToString() + " and s.identifier = " + identifier.ToString() + " and langShortCode = '" + dbEncode(language.shortCode) + "' and s.deleted is null;";

            DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                JobPostingAggregatorData data = new JobPostingAggregatorData();
                DataRow dr = ds.Tables[0].Rows[0];

                data.JobSummaryId = Convert.ToInt32(dr["JobSummaryId"]);
                data.LocationId = Convert.ToInt32(dr["locationId"]);                

                return data;
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    JobPostingAggregatorData data = new JobPostingAggregatorData();
                    bool b = createNewJobPostingAggregatorData(page, identifier, language, data);

                    if (!b)
                    {
                        throw new Exception("getJobPostingDetailsData database error: Error creating new placeholder");
                    }
                    else
                    {
                        return data;
                    }
                }
                else
                {
                    throw new Exception("getJobPostingDetailsData database error: placeholder does not exist");
                }
            }
        }
        

        public bool createNewJobPostingAggregatorData(CmsPage page, int identifier, CmsLanguage language, JobPostingAggregatorData data)
        {
            string sql = "insert into jobsummary (pageid, identifier, langShortCode, locationId) values (";
            sql += page.Id.ToString() + "," + identifier.ToString() + ",";
            sql += "'" + dbEncode(language.shortCode) + "', ";
            sql += "" + (data.LocationId.ToString()) + " ";            
            sql += "); ";

            int newId = this.RunInsertQuery(sql);
            if (newId > -1)
            {
                data.JobSummaryId = newId;                
                return page.setLastUpdatedDateTimeToNow();
            }
            else
                return false;
        }  // createNewJobPostingAggregatorData

        public bool saveUpdatedJobPostingAggregatorData(JobPostingAggregatorData data)
        {
            if (data.JobSummaryId < 0)
                return false;

            string sql = "update jobsummary set ";
            sql += " locationId = " + data.LocationId.ToString() + " ";            
            sql += " where JobSummaryId = " + data.JobSummaryId.ToString();

            int numAffected = this.RunUpdateQuery(sql);            
            return (numAffected > 0);

        }


    }

}
