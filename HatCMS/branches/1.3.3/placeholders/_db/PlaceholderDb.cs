using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// Summary description for PlaceholderDb.
	/// </summary>
	public class PlaceholderDb: Hatfield.Web.Portal.Data.MySqlDbObject
	{
		public PlaceholderDb(): base(ConfigurationManager.AppSettings["ConnectionString"])
		{ }
				 
	} // PlaceholderDb Class		

}
