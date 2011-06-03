using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS
{
    /// <summary>
    /// group number serves as a weight: the markup for loading a stylesheet within a lower weight group is output to the page before the markup for loading a stylesheet within a higher weight group, so CSS within higher weight groups take precendence over CSS within lower weight groups.
    /// <list type="bullet">
    /// <item>CSSGroup.Library: Any system-layer CSS (ie for jquery UI). Loaded first.</item>
    /// <item>CSSGroup.ControlOrPlaceholder: Any module-layer CSS (ie for a placeholder or control). Loaded second.</item>
    /// <item>CSSGroup.FrontEnd: Any theme-layer CSS (ie for front-end development). Loaded third.</item>
    /// <item>CSSGroup.Override: CSS that is loaded after everything else (fourth).</item>
    /// </list>
    /// </summary>
    public enum CSSGroup { Library = -100, ControlOrPlaceholder = 0, FrontEnd = 100, Override = 1000 };

    /// <summary>
    /// group number serves as a weight: the markup for loading a stylesheet within a lower weight group is output to the page before the markup for loading a stylesheet within a higher weight group, so CSS within higher weight groups take precendence over CSS within lower weight groups.
    /// <list type="bullet">
    /// <item>JavascriptGroup.Library: Any system-layer JS (ie for Jquery). Loaded first.</item>
    /// <item>JavascriptGroup.ControlOrPlaceholder: Any module-layer JS (ie for a placeholder or control). Loaded second</item>
    /// <item>JavascriptGroup.FrontEnd: Any theme-layer JS (for front-end development). Loaded third.</item>
    /// <item>JavascriptGroup.Override: Javascript that is after everything else.</item>
    /// </list>
    /// </summary>
    public enum JavascriptGroup { Library = -100, ControlOrPlaceholder = 0, FrontEnd = 100, Override = 1000 };

    
    /// <summary>
    /// Handles the common output of a CmsPage's &gt;head&lt;&gt;/head&lt; through the use of some useful functions.
    /// <para>The items tracked in this class are output using a <see cref="CmsOutputFilter"/> at the CmsOutputFilterScope.PageHtmlOutput level.</para>
    /// </summary>
    public class CmsPageHeadSection: BaseCmsOutputFilter
    {
        // used by private jqueryIsIncluded function
        string[] JQueryLibraryFilenames = new string[] {
            "jquery-1.4.1.min.js", "jquery-1.4.1.js", "jquery.js", "jquery.min.js"
        };

        string[] MooToolsLibraryFilenames = new string[] {
            "mootools.v1.11.compressed.js", "mootools.v1.11.uncompressed.js", "mootools-1.2.4-core-yc.js", "mootools-1.2.4-core-jm.js", "mootools-1.2.4-core-nc.js", "mootools-1.2.4-core.js"
        };
        
        private Dictionary<JavascriptGroup, List<string>> jsFilePaths;
        private List<string> jsOnReadyStatements;
        private List<string> jsStatements;
        private Dictionary<CSSGroup, List<string>> cssFilePaths;
        private List<string> styleStatements;
        private List<string> registeredBlockNames;

        private CmsPage _page;

        public CmsPageHeadSection(CmsPage owningPage)
        {
            _page = owningPage;

            jsFilePaths = new Dictionary<JavascriptGroup, List<string>>();
            foreach (JavascriptGroup jsGroup in Enum.GetValues(typeof(JavascriptGroup)))
                jsFilePaths[jsGroup] = new List<string>();

            jsOnReadyStatements = new List<string>();
            jsStatements = new List<string>();
            cssFilePaths = new Dictionary<CSSGroup, List<string>>();
            foreach (CSSGroup cssGroup in Enum.GetValues(typeof(JavascriptGroup)))
                cssFilePaths[cssGroup] = new List<string>();

            styleStatements = new List<string>();
            registeredBlockNames = new List<string>();
        }

        /// <summary>
        /// for the output filter to work, a parameterless constructor is needed.
        /// </summary>
        public CmsPageHeadSection()
        {            
        }

        public override CmsOutputFilterInfo getOutputFilterInfo()
        {
            return new CmsOutputFilterInfo(CmsOutputFilterScope.PageHtmlOutput, _runPageFilter);
        }
        

        private string _runPageFilter(CmsPage pageBeingFiltered, string htmlToFilter)
        {
            return StringUtils.Replace(htmlToFilter, "</head>", pageBeingFiltered.HeadSection._OutputForPageFilter(), true);
        }
        

        /// <summary>
        /// Checks to see if a named head-section block is registered for output.
        /// The only way to register a head-section block is using registerBlockForOutput().
        /// Note: blockNames are case in-sensitive.
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public bool isBlockRegisteredForOutput(string blockName)
        {
            foreach(string b in registeredBlockNames)
            {
                if (string.Compare(b, blockName, true) == 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// registers a head-section block for output.
        /// Use isBlockRegisteredForOutput() to check if the block is already registered.
        /// Note: blockNames are case in-sensitive.
        /// </summary>
        /// <param name="scriptName"></param>
        /// <returns></returns>
        public void registerBlockForOutput(string blockName)
        {
            registeredBlockNames.Add(blockName);
        }
                      

        private string removeBeginningSlash(string s)
        {
            if (s.StartsWith("/"))
                s = s.Substring(1);
            return s;
        }

        private bool jqueryIsIncluded()
        {
            foreach (JavascriptGroup jsGroup in jsFilePaths.Keys)
            {
                foreach (string js in jsFilePaths[jsGroup])
                {
                    foreach (string jqFn in JQueryLibraryFilenames)
                    {
                        if (js.EndsWith(jqFn, StringComparison.CurrentCultureIgnoreCase))
                            return true;
                    } // foreach
                } // foreach
            } // foreach
            return false;
        }

        private bool mooToolsIsIncluded()
        {
            foreach (JavascriptGroup jsGroup in jsFilePaths.Keys)
            {
                foreach (string js in jsFilePaths[jsGroup])
                {
                    foreach (string jqFn in MooToolsLibraryFilenames)
                    {
                        if (js.EndsWith(jqFn, StringComparison.CurrentCultureIgnoreCase))
                            return true;
                    } // foreach
                } // foreach
            }
            return false;
        }

        private JavascriptGroup? getGroupForJavascriptFile(string jsFile)
        {
            foreach (JavascriptGroup jsGroup in jsFilePaths.Keys)
            {
                foreach (string js in jsFilePaths[jsGroup])
                {
                    if (string.Compare(js, jsFile, true) == 0) 
                        return jsGroup;

                } // foreach
            }
            return null;
        }

        private CSSGroup? getGroupForCSSFile(string cssFile)
        {
            foreach (CSSGroup cssGroup in cssFilePaths.Keys)
            {
                foreach (string css in cssFilePaths[cssGroup])
                {
                    if (string.Compare(css, cssFile, true) == 0)
                        return cssGroup;

                } // foreach
            }
            return null;
        }

        private bool isExternallyHosted(string name)
        {
            if (name.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }
        
        /// <summary>
        /// Adds a javascript file statement to the head section
        /// </summary>
        /// <param name="pathToJSFileUnderAppPath"></param>
        public void AddJavascriptFile(JavascriptGroup jsGroup, string pathToJSFileUnderAppPath)
        {
            // -- add only unique items
            string jsPath = pathToJSFileUnderAppPath.Trim();
            if (!isExternallyHosted(jsPath))
                jsPath = removeBeginningSlash(jsPath);

            JavascriptGroup? existingGroup = getGroupForJavascriptFile(jsPath);
            if (existingGroup == null)
                jsFilePaths[jsGroup].Add(jsPath);
            else if (existingGroup != null && existingGroup != jsGroup)
                throw new ArgumentException(pathToJSFileUnderAppPath+" has already been added to the "+existingGroup.ToString()+" javascript group");            
                
        } // Add JavascriptFile

        /// <summary>
        /// Adds some javascript statements to the OnReady event handler. Do NOT include &lt;script&gt;&lt;/script&gt; tags!
        /// </summary>
        /// <param name="jsStatements"></param>
        public void AddJSOnReady(StringBuilder jsStatements)
        {
            jsOnReadyStatements.Add(jsStatements.ToString());
        }

        /// <summary>
        /// Adds some javascript statements to the OnReady event handler. do NOT include &lt;script&gt;&lt;/script&gt; tags!
        /// </summary>
        /// <param name="jsStatements"></param>
        public void AddJSOnReady(string jsStatements)
        {
            jsOnReadyStatements.Add(jsStatements);
        }

        /// <summary>
        /// Adds some javascript statements to the head section. do NOT include &lt;script&gt;&lt;/script&gt; tags!
        /// </summary>
        /// <param name="jsStatement"></param>
        public void AddJSStatements(string jsStatement)
        {
            jsStatements.Add(jsStatement);
        }

        /// <summary>
        /// Adds some CSS statements to the page head's &lt;style&gt;&lt;/style&gt section. Do NOT include &lt;style&gt;&lt;/style&gt; tags!
        /// </summary>
        /// <param name="cssStyleStatements"></param>
        public void AddCSSStyleStatements(string cssStyleStatements)
        {
            styleStatements.Add(cssStyleStatements);
        }

        /// <summary>
        /// Adds a CSS file reference to the page's head section.
        /// </summary>
        /// <param name="pathToCSSFileUnderAppPath"></param>
        public void AddCSSFile(CSSGroup cssGroup, string pathToCSSFileUnderAppPath)
        {
            // -- add only unique items
            string cssPath = pathToCSSFileUnderAppPath.Trim();
            if (!isExternallyHosted(cssPath))
                cssPath = removeBeginningSlash(cssPath);

            CSSGroup? existingGroup = getGroupForCSSFile(cssPath);
            if (existingGroup == null)
                cssFilePaths[cssGroup].Add(cssPath);
            else if (existingGroup != null && existingGroup != cssGroup)
                throw new ArgumentException(cssPath + " has already been added to the " + existingGroup.ToString() + " CSS group");            

        } // Add AddCSSFile


        private string getOutputUrl(string pathToFileUnderAppPath, string cacheTimestamp)
        {
            StringBuilder url = new StringBuilder();
            if (!isExternallyHosted(pathToFileUnderAppPath))
                url.Append(CmsContext.ApplicationPath);

            url.Append(pathToFileUnderAppPath);
            if (url.ToString().IndexOf("?") < 0)
                url.Append("?" + cacheTimestamp);
            else
                url.Append("&" + cacheTimestamp);

            return url.ToString();
        }        


        private string getOutputCacheTimestamp()
        {
            // -- if authoring, always force cache to bust. Otherwise base the timestamp from when the assembly was created.
            long ticks = DateTime.Now.Ticks;
            if (CmsContext.currentPage.currentUserCanWrite)
                ticks = DateTime.Now.Ticks;
            else
            {
                try
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    ticks = fi.CreationTime.Ticks;
                }
                catch
                { }
            }

            string ret = StringUtils.Base36Encode(ticks);            
            return ret;
        }


        

        public string _OutputForPageFilter()
        {
            string cacheTimestamp = getOutputCacheTimestamp();
            
            StringBuilder html = new StringBuilder();
            string EOL = Environment.NewLine;
            // A: put CSS files and style statements first so that scripts can reference them: http://code.google.com/speed/page-speed/docs/rendering.html#PutCSSInHead
            // -- 1: CSS Files
            //       note: do NOT use @import rule
            foreach (CSSGroup cssGroup in Enum.GetValues(typeof(CSSGroup)))
            {
                foreach (string css in cssFilePaths[cssGroup])
                {
                    html.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + getOutputUrl(css, cacheTimestamp) + "\" />" + EOL);
                }
            }

            // -- 2: Style statements
            if (styleStatements.Count > 0)
            {
                html.Append("<style type=\"text/css\">" + EOL);
                foreach (string s in styleStatements)
                {
                    html.Append(s + EOL);
                }
                html.Append(EOL + "</style>" + EOL);
            }

            // B: Javascript files should be placed before javascript statements (http://code.google.com/speed/page-speed/docs/rtt.html#PutStylesBeforeScripts)
            // -- 3: Javascript files
            foreach (JavascriptGroup jsGroup in Enum.GetValues(typeof(JavascriptGroup)))
            {
                foreach (string js in jsFilePaths[jsGroup])
                {
                    html.Append("<script src=\"" + getOutputUrl(js, cacheTimestamp) + "\" type=\"text/javascript\"></script>" + EOL);
                }
            }

            bool scriptTagStarted = false;
            // -- 4: Javascript Statements
            if (jsStatements.Count > 0)
            {
                html.Append("<script type=\"text/javascript\">" + EOL);
                scriptTagStarted = true;
                foreach (string js in jsStatements)
                {
                    html.Append(js + EOL + EOL);
                } // foreach
            }

            // -- 5: Javascript On Ready
            if (jqueryIsIncluded() && jsOnReadyStatements.Count > 0)
            {
                if (!scriptTagStarted)
                {
                    html.Append("<script type=\"text/javascript\">" + EOL);
                    scriptTagStarted = true;
                }
                html.Append("$(document).ready(function() {"+EOL);
                foreach(string js in jsOnReadyStatements)
                {
                    html.Append(js + EOL + EOL);
                } // foreach
                html.Append("});"+EOL);                
            }
            else if (mooToolsIsIncluded() && jsOnReadyStatements.Count > 0)
            {
                // 	window.addEvent('domready', function(){
                if (!scriptTagStarted)
                {
                    html.Append("<script type=\"text/javascript\">" + EOL);
                    scriptTagStarted = true;
                }
                html.Append("window.addEvent('domready', function(){" + EOL);
                foreach (string js in jsOnReadyStatements)
                {
                    html.Append(js + EOL + EOL);
                } // foreach
                html.Append("});" + EOL);                
            }
            else if (jsOnReadyStatements.Count > 0)
            {
                string onLoadFunctionName = "hatCms_pageLoad";
                if (!scriptTagStarted)
                {
                    html.Append("<script type=\"text/javascript\">" + EOL);
                    scriptTagStarted = true;
                }
                html.Append("function " + onLoadFunctionName + "() {" + EOL);
                foreach (string js in jsOnReadyStatements)
                {
                    html.Append(js + EOL + EOL);
                } // foreach
                html.Append("} // " + onLoadFunctionName + "()" + EOL);
                html.Append(CmsPageHeadSection.getOnloadJavascript(onLoadFunctionName));
                html.Append(EOL);

            }
            if (scriptTagStarted)
            {
                html.Append("</script>" + EOL);
            }
         
            html.Append("</head>" + EOL);
            return html.ToString();

        } // ToHtml


        public static string getOnloadJavascript(string onLoadJSFunctionName)
        {
            StringBuilder js = new StringBuilder();
            string EOL = Environment.NewLine;
            js.Append("if( window.addEventListener ) {" + EOL);
            js.Append("	window.addEventListener( 'load', " + onLoadJSFunctionName + ", false );" + EOL);
            js.Append("} else if( document.addEventListener ) {" + EOL);
            js.Append("	document.addEventListener('load' , " + onLoadJSFunctionName + ", false );" + EOL);
            js.Append("} else if( window.attachEvent ) {" + EOL);
            js.Append(" window.attachEvent( 'onload', " + onLoadJSFunctionName + " );" + EOL);
            js.Append(" } else {" + EOL);
            js.Append("	window.onload = " + onLoadJSFunctionName + ";" + EOL);
            js.Append("}" + EOL);
            return js.ToString();
        } // getOnloadJavascript


    }
}

