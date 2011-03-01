using System;

namespace HatCMS
{
	
	/// <summary>
	/// 404 Not Found Exception thrown by placeholders or CmsContext if the page or AlternateDisplay could not be found.
	/// </summary>
	public class CmsPageNotFoundException: Exception
	{
	}

    /// <summary>
    /// Thrown by CmsContext if no pages (including the HomePage) could be loaded from the database.
    /// </summary>
    public class CmsNoPagesFoundException : Exception
    {
        public CmsNoPagesFoundException(string message)
            : base(message)
        { }
    }
    
    /// <summary>
    /// An exception that is thrown when the requested page revision was not found
    /// </summary>
    public class RevisionNotFoundException : CmsPageNotFoundException
    {
    }

    /// <summary>
    /// An exception that is thrown when a placeholder wants the user to be redirected. Note that due to the execution model of placeholders that Response.Redirect() doesn't work.
    /// </summary>
    public class CmsPlaceholderNeedsRedirectionException : Exception
    {
        public string TargetUrl = "";
        public CmsPlaceholderNeedsRedirectionException(string targetUrl)
        {
            TargetUrl = targetUrl;
        }
    }


}
