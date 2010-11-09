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
            if (page.ID < 0 || identifier < 0)
                return new JobPostingDetailsData();

            string sql = "";
            sql = "select * from jobdetails s ";
            sql += " where s.pageid = " + page.ID.ToString() + " and s.identifier = " + identifier.ToString() + " and langShortCode = '" + dbEncode(language.shortCode) + "' and s.deleted is null;";

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
            sql += page.ID.ToString() + "," + identifier.ToString() + ",";
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
            if (page.ID < 0 || identifier < 0)
                return new JobPostingAggregatorData();

            string sql = "";
            sql = "select * from jobsummary s ";
            sql += " where s.pageid = " + page.ID.ToString() + " and s.identifier = " + identifier.ToString() + " and langShortCode = '" + dbEncode(language.shortCode) + "' and s.deleted is null;";

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
            sql += page.ID.ToString() + "," + identifier.ToString() + ",";
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

#if IncludeOldJobDatabasePlaceholder
	/// <summary>
	/// Summary description for ProjectDatabaseDb.
	/// </summary>
	public class JobDatabaseDb: PlaceholderDb
	{

        public JobSummaryData getJobSummary(CmsPage page, int identifier, CmsLanguage language, bool createNewIfDoesNotExist)
		{
			if (page.ID < 0 || identifier < 0)
				return new JobSummaryData();
			
			string sql = "";
            sql = "select * from jobsummary s ";			
			sql += " where s.pageid = "+page.ID.ToString()+" and s.identifier = "+identifier.ToString()+ " and langShortCode = '" + language.shortCode + "' and s.deleted is null;";
			
			DataSet ds = this.RunSelectQuery(sql);
			if (this.hasSingleRow(ds))
			{
				JobSummaryData data = new JobSummaryData();
				DataRow dr = ds.Tables[0].Rows[0];
				
				data.JobSummaryId = Convert.ToInt32(dr["JobSummaryId"]);
                data.LocationToDisplay = (dr["LocationToDisplay"].ToString());
                data.HtmlHeader = (dr["HtmlHeader"].ToString());

				return data;
			}
			else
			{				
				if(createNewIfDoesNotExist)
				{
					JobSummaryData data = new JobSummaryData();
					bool b = createNewJobSummary(page,identifier, language, data );	
					
					if (!b)
					{
                        throw new Exception("getJobSummary database error: Error creating new placeholder");
					}
					else
					{
						return data;
					}
				}
				else
				{
                    throw new Exception("getJobSummary database error: placeholder does not exist");
				}
			}
			
		} // getProjectSummaries

        public bool createNewJobSummary(CmsPage page, int identifier, CmsLanguage language, JobSummaryData data)
		{
            string sql = "insert into jobsummary (pageid, identifier, langShortCode, LocationToDisplay, HtmlHeader) values (";
			sql = sql +page.ID.ToString()+","+identifier.ToString()+",";
            sql += "'" + dbEncode(language.shortCode) + "', ";
			sql += "'"+dbEncode(data.LocationToDisplay)+"', ";
            sql += "'" + dbEncode(data.HtmlHeader) + "' ";			
			sql += "); ";
			
			int newId = this.RunInsertQuery(sql);
			if (newId > -1)
			{
				data.JobSummaryId = newId;
				return page.setLastUpdatedDateTimeToNow();
			}
			else
				return false;
		}  // createNewProjectSummary

		public bool saveUpdatedJobSummary(JobSummaryData data)
		{
            string sql = "update jobsummary set ";
            sql += " LocationToDisplay = '" + dbEncode(data.LocationToDisplay) + "', ";
            sql += " HtmlHeader = '" + dbEncode(data.HtmlHeader) + "' ";
            sql += " where JobSummaryId = " + data.JobSummaryId.ToString();

			int numAffected = this.RunUpdateQuery(sql);
			return (numAffected > 0);
		} 

		public bool saveUpdatedJobDetails(JobDetailsData data)
		{
			bool doUpdate = false;
            string existsSql = "select JobId from jobdetails where JobId = " + data.JobDetailsId.ToString() + ";";
			DataSet d = this.RunSelectQuery(existsSql);
            if (this.hasSingleRow(d) && d.Tables[0].Rows[0]["JobId"] != System.DBNull.Value)
			{
				doUpdate = true;
			}

			if (doUpdate)
			{
                string sql = "update jobdetails set ";
				sql += " Title = '"+dbEncode(data.Title)+"', ";
                sql += " Location = '" + dbEncode(data.Location) + "', ";
                sql += " HtmlJobDescription = '" + dbEncode(data.HtmlJobDescription) + "', ";
                sql += " RemoveAnonAccessAt = " + dbEncode(data.RemoveAnonAccessAt) + ", ";
                sql += " LastUpdatedDateTime = " + dbEncode(DateTime.Now) + " ";
                sql += " where JobId = " + data.JobDetailsId.ToString();

				int numAffected = this.RunUpdateQuery(sql);
				return (numAffected > 0);
			}
			else
			{
                string sql = "INSERT into jobdetails (JobId, Title, Location, HtmlJobDescription, RemoveAnonAccessAt, LastUpdatedDateTime) VALUES ";
				sql += " ( ";
                sql += data.JobDetailsId.ToString() + ", ";
				sql += "'"+dbEncode(data.Title)+"', ";
                sql += "'" + dbEncode(data.Location) + "', ";
                sql += "'" + dbEncode(data.HtmlJobDescription) + "', ";
                sql += "" + dbEncode(data.RemoveAnonAccessAt) + ", ";
                sql += "" + dbEncode(DateTime.Now) + " ";
				sql += " ); ";

				int newId = this.RunInsertQuery(sql);
                if (newId >= 0)
                {
                    data.JobDetailsId = newId;
                    return true;
                }
				return false;
			}
		} 

		public bool deleteJobDetails(int jobDetailsId)
		{
            if (jobDetailsId < 0)
				return false;

            string sql = "update jobdetails set Deleted = NOW() ";
            sql += " where JobId = " + jobDetailsId.ToString() + ";";

			int numAffected = this.RunUpdateQuery(sql);
			return (numAffected > 0);
		}

		private JobDetailsData JobDetailsDataFromDataRow(DataRow dr)
		{
			JobDetailsData ret = new JobDetailsData();

            ret.JobDetailsId = Convert.ToInt32(dr["JobId"]);
			ret.Title = dr["Title"].ToString();
            ret.Location = dr["Location"].ToString();
            ret.HtmlJobDescription = dr["HtmlJobDescription"].ToString();
            ret.RemoveAnonAccessAt = Convert.ToDateTime(dr["RemoveAnonAccessAt"]);
            ret.LastUpdatedDateTime = Convert.ToDateTime(dr["LastUpdatedDateTime"]);

			return ret;
		}

		

		/// <summary>
        /// if location = "", return all jobs in all locations.
		/// </summary>
		/// <param name="year"></param>
		/// <returns></returns>
        public JobDetailsData[] getJobDetailsByLocation(string location, bool includeExpired)
		{            
			ArrayList where = new ArrayList();
            if (location != "" && location != JobDatabase.AllLocationsDisplayText)
            {
                where.Add(" (p.Location) like ('" + dbEncode(location) + "') ");
            }

            if (! includeExpired)
            {
                where.Add(" p.RemoveAnonAccessAt >= " + dbEncode(DateTime.Now) + " ");
            }
                        
            where.Add(" p.Deleted is null ");

			string whereClause = String.Join(" AND ",(string[])where.ToArray(typeof(string)));

			ArrayList jobs = new ArrayList();

            string sql = "SELECT p.* FROM jobdetails p where ";
			sql += whereClause;

            sql += " ORDER BY Location, RemoveAnonAccessAt asc; ";

			DataSet ds = this.RunSelectQuery(sql);
			if (this.hasRows(ds))
			{
				foreach(DataRow dr in ds.Tables[0].Rows)
				{
                    jobs.Add(JobDetailsDataFromDataRow(dr));
				}
			}

            return (JobDetailsData[])jobs.ToArray(typeof(JobDetailsData));
        } // getJobDetailsByLocation

		

		public JobDetailsData getJobDetails(int jobId, bool createNewIfDoesNotExist)
		{
            if (jobId == -1)
				return new JobDetailsData();

			ArrayList jobs = new ArrayList();

            string sql = "SELECT * from jobdetails p where ";
            sql += " p.JobId = " + jobId + " AND ";
			sql += " p.Deleted is null ";            

			DataSet ds = this.RunSelectQuery(sql);
            if (this.hasSingleRow(ds))
            {
                return JobDetailsDataFromDataRow(ds.Tables[0].Rows[0]);
            }
            else
            {
                if (createNewIfDoesNotExist)
                {
                    JobDetailsData job = new JobDetailsData();
                    job.JobDetailsId = jobId;
                    if (saveUpdatedJobDetails(job))
                        return job;
                }
            }
			return new JobDetailsData();
        } // getJobDetails

		public int getNextJobDetailsId()
		{
            string sql = "SELECT max(JobId)+1 as nextId FROM jobdetails p;";
			DataSet ds = this.RunSelectQuery(sql);
			if (this.hasSingleRow(ds))
			{
				DataRow dr = ds.Tables[0].Rows[0];
				if (dr["nextId"] != System.DBNull.Value)
					return Convert.ToInt32(dr["nextId"]);
			}
			return 1;
        } // getNextJobDetailsId	
				
	}
#endif
}
