using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS
{
    /// <summary>
    /// To define an output filter in a class, placeholder or control, create a public function <c>public CmsOutputFilter[] getOutputFilters(){};</c> That function will be magically called whenever filters are needed!
    /// </summary>
    public class CmsOutputFilterInfo
    {
        public CmsOutputFilterScope Scope;
        public string[] SpecificPlaceholderNamesOrControlPathsToFilter;
        private RunFilterDelegate _runFilterDelegate = null;

        public delegate string RunFilterDelegate(CmsPage pageBeingFiltered, string htmlToFilter);

        public CmsOutputFilterInfo(CmsOutputFilterScope scope, RunFilterDelegate filterDelegate)
        {
            Scope = scope;
            _runFilterDelegate = filterDelegate;
            SpecificPlaceholderNamesOrControlPathsToFilter = new string[0];
            if (scope == CmsOutputFilterScope.SpecifiedPlaceholderTypes)
                throw new ArgumentException("When specified controls or placeholders are needed, use another constructor");
        }

        public CmsOutputFilterInfo(CmsOutputFilterScope scope, string[] specificPlaceholdersOrControlsToFilter, RunFilterDelegate filterDelegate)
        {
            Scope = scope;
            _runFilterDelegate = filterDelegate;
            SpecificPlaceholderNamesOrControlPathsToFilter = specificPlaceholdersOrControlsToFilter;
            if (scope == CmsOutputFilterScope.AllPlaceholders || scope == CmsOutputFilterScope.PageHtmlOutput)
                throw new ArgumentException("When filtering all controls or placeholders, use another constructor");
        }

        public string RunFilter(CmsPage pageBeingFiltered, string htmlToFilter)
        {
            return _runFilterDelegate(pageBeingFiltered, htmlToFilter);
        }
    }
}
