using System;
using System.Web.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using HatCMS.Placeholders;

namespace HatCMS.TemplateEngine
{
    /// <summary>
    /// the TemplateEngineV2 class executes (ie Renders) a template that has a TemplateLayout file.
    /// 
    /// in v2 templates, the overall page layout is defined in a seperate file - the TemplateLayout file.
    /// The TemplateLayout file defines the main html layout, including placement of controls
    /// and PlaceholderRegions. TemplateLayout files do not contain Placeholders. The content of
    /// PlaceholderAreas is defined in the template file.
    /// 
    /// In this system, TemplateLayout files are similar to ASP.Net MasterPages, while
    /// Template files are like .ASPX content pages with a MasterPage defined.
    /// 
    /// TemplateLayout files can contain the following commands:
    /// ##RenderControl("ControlPath" param="value" key="value")##
    /// ##PlaceholderRegion("RegionName")##
    /// 
    /// note: placeholders can not be defined in TemplateLayout files.
    /// 
    /// Template files can contain the following commands:
    /// ##TemplateLayout("LayoutFilePath")##
    /// ##StartPlaceholderRegion("RegionName")##
    ///     ##RenderControl("ControlPath" param="value" key="value")##
    ///     ##Placeholder("PlaceholderName" id="#" param0="value" param2="value" param3="quoted: \"escaped quotes\"" param4="single quotes: 'no escape needed.' ")##
    /// ##EndPlaceholderRegion("RegionName")##
    /// 
    /// note: nested PlaceholderRegions are not possible. Control or placeholder param keys can not have spaces.
    /// A template file MUST have one (and only one) ##TemplateLayout()## defined.
    /// </summary>
    public class TemplateEngineV2 : CmsTemplateEngine
    {
        private string templateName;
        private CmsPage page;


        private int currentLangIndex = 0;
        private string templateLayoutFileContents = "";
        private string templateFileContents = "";

        /// <summary>
        /// the template extension (with a beginning period ('.')))
        /// </summary>
        public const string TEMPLATE_EXTENSION = ".template";

        /// <summary>
        /// the template layout file extension (with a beginning period ('.')))
        /// </summary>
        public const string TEMPLATE_LAYOUT_EXTENSION = ".htm";

        /// <summary>
        /// the template sub directory (no beginning slash; must have trailing slash ('/'))
        /// </summary>
        private const string TEMPLATE_SUBDIR = "templates/";

        /// <summary>
        /// the template layout sub directory (no beginning slash; must have trailing slash ('/'))
        /// </summary>
        private const string TEMPLATE_LAYOUT_SUBDIR = "templates/";

        /// <summary>
        /// the controls sub directory (no beginning slash; must have trailing slash ('/'))
        /// </summary>
        public const string CONTROLS_SUBDIR = "controls/";

        private const string COMMAND_DELIMITER = "##";      
  
        /// <summary>
        /// If a placeholder or a control has this parameter name, and it's value is set to "false", and CmsContext.currentUserIsRequestingPrintFriendlyVersion,
        /// the control or placeholder will NOT be output.
        /// </summary>
        private const string PRINTER_FRIENDLY_VERSION_OUTPUT_CONTROL_PARAMETERNAME = "includeinprintfriendlyversion"; // note: must be all lower-case

        /// <summary>
        /// If a placeholder or a control has this parameter name, and it's value is set to "false", and CmsContext.currentUserIsMakingOfflineVersion,
        /// the control or placeholder will NOT be output.
        /// </summary>
        private const string OFFLINE_VERSION_OUTPUT_CONTROL_PARAMETERNAME = "includeinofflineversion"; // note: must be all lower-case
                

        public TemplateEngineV2(string templateName, CmsPage page)
        {
            this.templateName = templateName;
            this.page = page;
        } // constructor


        private Dictionary<string, System.Reflection.Assembly> getAllTemplatesInAssemblies()
        {
            string cacheKey = "getTemplatesInAssemblies" + TEMPLATE_EXTENSION + TEMPLATE_LAYOUT_EXTENSION;

            if (Hatfield.Web.Portal.PerRequestCache.CacheContains(cacheKey))
            {
                return (Dictionary<string, System.Reflection.Assembly>)Hatfield.Web.Portal.PerRequestCache.GetFromCache(cacheKey, new Dictionary<string, System.Reflection.Assembly>());
            }

            string[] extensions = new string[] { TEMPLATE_EXTENSION, TEMPLATE_LAYOUT_EXTENSION };

            Dictionary<string, System.Reflection.Assembly> ret = Hatfield.Web.Portal.AssemblyHelpers.LoadAllAssembliesAndGetAllEmbeddedResourcesWithExtensions(extensions);
            Hatfield.Web.Portal.PerRequestCache.AddToCache(cacheKey, ret);

            return ret;
            
        }

        private string cleanTemplateNameStoredInAssembly(string rawTemplateName)
        {
            string ret = rawTemplateName;
            // -- templates should be stored in a directory called "template" or "templates", so the name should come after that
            int index1 = ret.IndexOf(".templates.", StringComparison.CurrentCultureIgnoreCase);
            int index2 = ret.IndexOf(".template.", StringComparison.CurrentCultureIgnoreCase);
            if (index1 >= 0)
            {
                ret = ret.Substring(index1 + ".templates.".Length);
            }
            else if (index2 >= 0)
            {
                ret = ret.Substring(index2 + ".template.".Length);
            }

            // -- remove the template_extension
            if (String.Compare(Path.GetExtension(ret), TEMPLATE_EXTENSION, true) == 0)
                ret = Path.GetFileNameWithoutExtension(ret);

            return ret;
        }

        private string getTemplateFileContents()
        {
            // -- try to get the template on disk
            string fn = _getTemplateFilenameOnDisk();
            if (File.Exists(fn))
            {
                return _getCachedFileContents(fn);
            }
            else
            {
                // try to get the template from assemblies
                string toFind = templateName + TEMPLATE_EXTENSION;
                Dictionary<string, System.Reflection.Assembly> templatesInAssemblies = getAllTemplatesInAssemblies();
                foreach (string templateFn in templatesInAssemblies.Keys)
                {
                    if (templateFn.EndsWith(toFind, StringComparison.CurrentCultureIgnoreCase))
                    {
                        using (Stream stream = Hatfield.Web.Portal.AssemblyHelpers.GetEmbeddedResourceStream(templatesInAssemblies[templateFn], templateFn))
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }

                } // foreach
            }

            throw new TemplateExecutionException(new Exception(), templateName, "Could not read or find the template '" + templateName + "' for page '" + this.page.Path + "' was not found.");
        }

        private string getTemplateLayoutFileContents(string layoutPath)
        {
            // -- try to get the template on disk
            string fn = _getTemplateLayoutFilenameOnDisk(layoutPath);
            if (File.Exists(fn))
            {
                return _getCachedFileContents(fn);
            }
            else
            {
                // try to get the template from assemblies
                string toFind = layoutPath + TEMPLATE_LAYOUT_EXTENSION;
                Dictionary<string, System.Reflection.Assembly> templatesInAssemblies = getAllTemplatesInAssemblies();
                foreach (string templateFn in templatesInAssemblies.Keys)
                {
                    if (templateFn.EndsWith(toFind, StringComparison.CurrentCultureIgnoreCase))
                    {
                        using (Stream stream = Hatfield.Web.Portal.AssemblyHelpers.GetEmbeddedResourceStream(templatesInAssemblies[templateFn], templateFn))
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }

                } // foreach
            }

            throw new TemplateExecutionException(new Exception(), templateName, "Could not read or find the template layout '" + layoutPath + "' in template '" + templateName + "' for page '" + this.page.Path + "' was not found.");
        }


        private string _getTemplateFilenameOnDisk()
        {                        
            // get the templateFilename
            string templateUrl = "~/" + TEMPLATE_SUBDIR + templateName + TEMPLATE_EXTENSION;
            string templateFN_ondisk = System.Web.Hosting.HostingEnvironment.MapPath(templateUrl);

            return templateFN_ondisk;
        }

        private string _getTemplateLayoutFilenameOnDisk(string layoutPath)
        {            
            // get the template Layout Filename
            string templateUrl = "~/" + TEMPLATE_LAYOUT_SUBDIR + layoutPath + TEMPLATE_LAYOUT_EXTENSION;
            string templateFN_ondisk = System.Web.Hosting.HostingEnvironment.MapPath(templateUrl);

            return templateFN_ondisk;
        }

        private string _getCachedFileContents(string filenameOnDisk)
        {
            System.Web.Caching.Cache Cache = System.Web.Hosting.HostingEnvironment.Cache;

            string fullText = "";
            // -- see if the template's contents are in the memory cache
            string cacheKey = "template_" + filenameOnDisk;
            if (Cache[cacheKey] != null)
            {
                fullText = Cache[cacheKey] as string;
            }
            else
            {
                // -- the contents are not in the cache - let's read it from the file.
                if (!File.Exists(filenameOnDisk))
                {
                    throw new TemplateExecutionException(new Exception(), filenameOnDisk, "Could not read template file contents: The template file '" + filenameOnDisk + "' for page '" + this.page.Path + "' was not found on disk!");
                }
                // -- read the template file			
                StreamReader sr = new StreamReader(filenameOnDisk);

                try
                {
                    fullText = sr.ReadToEnd();
                    Cache.Insert(cacheKey, fullText, new System.Web.Caching.CacheDependency(filenameOnDisk));
                }
                finally
                {
                    sr.Close();
                }
            }
            return fullText;
        }

        /// <summary>
        /// Renders a template with all its controls and placeholders, and add these controls and placeholders to the parentUserControl.
        /// Note: do not use Render to HtmlWriter - Control events are not called properly.
        /// </summary>
        public override void CreateChildControls(System.Web.UI.UserControl parentUserControl)
        {                        
            // -- get the template file contents
            string templateText = getTemplateFileContents();

            // -- get the TemplateLayout statement
            string[] layouts = getCommandStatementParameters("TemplateLayout", templateText);
            if (layouts.Length == 0)
                throw new TemplateExecutionException(templateName, "Template does not have a TemplateLayout statement.");
            else if (layouts.Length > 1)
                throw new TemplateExecutionException(templateName, "Template has more than one TemplateLayout statement - only one is allowed.");

            // -- read the template layout file
            string layoutText = getTemplateLayoutFileContents(layouts[0]);

            // -- start with the first language if we are editing a page.
            currentLangIndex = 0;
            if (CmsContext.currentEditMode == CmsEditMode.View)
                currentLangIndex = CmsLanguage.IndexOf(CmsContext.currentLanguage.shortCode, CmsConfig.Languages);

            // -- make sure that StartPageBody and EndPageBody commands are included                        
            int start = layoutText.IndexOf("StartPageBody", StringComparison.CurrentCultureIgnoreCase);
            if (start < 0)
                start = templateText.IndexOf("StartPageBody", StringComparison.CurrentCultureIgnoreCase);

            if (start < 0)
                throw new TemplateExecutionException(templateName, "You must include a StartPageBody and EndPageBody command in the template when multiple languages are used");

            int end = layoutText.IndexOf("EndPageBody", StringComparison.CurrentCultureIgnoreCase);
            if (end < 0)
                end = templateText.IndexOf("EndPageBody", StringComparison.CurrentCultureIgnoreCase);
            if (end < 0)
                throw new TemplateExecutionException(templateName, "You must include a StartPageBody and EndPageBody command in the template when multiple languages are used");
            

            string startEditFormCommand = COMMAND_DELIMITER+"RenderControl(StartEditForm)"+COMMAND_DELIMITER;
            string endEditFormCommand = COMMAND_DELIMITER+"RenderControl(EndEditForm)"+COMMAND_DELIMITER;
            if (layoutText.IndexOf(startEditFormCommand, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                templateText.IndexOf(startEditFormCommand, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                layoutText.IndexOf(endEditFormCommand, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                templateText.IndexOf(endEditFormCommand, StringComparison.CurrentCultureIgnoreCase)>= 0)
            {
                throw new TemplateExecutionException(templateName, "Do not include the StartEditForm or EndEditForm controls");
            }

            
            templateLayoutFileContents = layoutText;
            templateFileContents = templateText;

            // -- render the output
            RenderTextToPage(parentUserControl, templateLayoutFileContents);

        } // CreateChildControls

        private void RenderTextToPage(System.Web.UI.UserControl parentUserControl, string textToRender)
        {
            // -- parse the template layout file line-by-line
            foreach (string line in textToRender.Split(new char[] { '\n' }))
            {
                if (line.IndexOf(COMMAND_DELIMITER) >= 0)
                {
                    string tmpLine = line;
                    while (tmpLine.IndexOf(COMMAND_DELIMITER) >= 0)
                    {
                        int commandStart = tmpLine.IndexOf(COMMAND_DELIMITER);
                        int commandEnd = tmpLine.IndexOf(COMMAND_DELIMITER, commandStart + 1) + COMMAND_DELIMITER.Length;

                        string preCommand = tmpLine.Substring(0, commandStart);
                        System.Web.UI.LiteralControl literal = new LiteralControl(preCommand);
                        parentUserControl.Controls.Add(literal);

                        string command = tmpLine.Substring(commandStart, commandEnd - commandStart);

                        renderCommand(parentUserControl, command);

                        tmpLine = tmpLine.Substring(preCommand.Length + command.Length); // get whatever is left

                    }
                    if (tmpLine != "")
                    {
                        // -- render what's left over after the command statements
                        parentUserControl.Controls.Add(new LiteralControl(tmpLine));
                    }
                }
                else
                {
                    System.Web.UI.LiteralControl literal = new LiteralControl(line);
                    parentUserControl.Controls.Add(literal);
                }

            } // foreach line
        }
        

        private void renderCommand(System.Web.UI.UserControl parentUserControl, string command)
        {
            // command is the full command, such as ##Placeholder(HtmlContent id="1")## or ##RenderContro(_system/PageTitle)##.

            int parseFrom = command.IndexOf("(", StringComparison.CurrentCultureIgnoreCase);
            int parseTo = command.Length - ")".Length - COMMAND_DELIMITER.Length;
            string rawParameters = "";
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (command.IndexOf("StartPageBody", StringComparison.CurrentCultureIgnoreCase) < 0 && command.IndexOf("EndPageBody", StringComparison.CurrentCultureIgnoreCase) < 0)
            {
                if (parseFrom < 0 || parseTo < 0)
                    throw new TemplateExecutionException(templateName, "Template statement \"" + command + "\" is not formatted properly.");
                parseFrom += "(".Length;

                rawParameters = command.Substring(parseFrom, (parseTo - parseFrom));

                parameters = tokenizeCommandParameters(rawParameters);
            }

            string langShortCode = "";
            if (currentLangIndex < CmsConfig.Languages.Length)
                langShortCode = CmsConfig.Languages[currentLangIndex].shortCode.ToLower().Trim();
            
            string langDivId = "lang_" + langShortCode; // note: this divId is used (hard-coded) all over the place!!!
            
            // -- only output multiple languages if we are in Edit mode
            bool outputMultipleLanguages = (CmsContext.currentEditMode == CmsEditMode.Edit);
            
            if (outputMultipleLanguages && command.StartsWith(COMMAND_DELIMITER + "StartPageBody", StringComparison.CurrentCultureIgnoreCase))
            {
                // -- if the first StartPageBody, start the form - the same as in StartEditForm.ascx
                if (currentLangIndex == 0)
                {
                    RenderTextToPage(parentUserControl, COMMAND_DELIMITER + "RenderControl(StartEditForm)" + COMMAND_DELIMITER);                    
                }
                
                string cssStyle = "display: none;";
                CmsLanguage langToRender = CmsConfig.Languages[currentLangIndex];
                // -- default to view the current Language first
                if (langToRender == CmsContext.currentLanguage) 
                {
                    cssStyle = "display: block;";
                }

                parentUserControl.Controls.Add(new LiteralControl("<!-- Start Language " + langDivId + " --> "));
                parentUserControl.Controls.Add(new LiteralControl("<div id=\"" + langDivId + "\" class=\"" + langDivId + " PageLanguageBody\" style=\"" + cssStyle + "\">"));
            }
            else if (outputMultipleLanguages && command.StartsWith(COMMAND_DELIMITER + "EndPageBody", StringComparison.CurrentCultureIgnoreCase))
            {
                parentUserControl.Controls.Add(new LiteralControl("</div>"));
                parentUserControl.Controls.Add(new LiteralControl("<!-- End Language " + langDivId + " --> "));
                currentLangIndex++; // increment to the next language
                if (currentLangIndex < CmsConfig.Languages.Length)
                {
                    string pageBody = getPageBodyText();
                    RenderTextToPage(parentUserControl, pageBody);
                }
                else
                {
                    // -- the last EndPageBody, so close the edit form using the EndEditForm control
                    RenderTextToPage(parentUserControl, COMMAND_DELIMITER + "RenderControl(EndEditForm)" + COMMAND_DELIMITER);
                }
            }
            else if (command.StartsWith(COMMAND_DELIMITER + "PlaceholderRegion", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!parameters.ContainsKey("##commandname##"))
                    throw new TemplateExecutionException(templateName, "Template statement \"" + command + "\" must have at least one parameter!");

                string regionName = parameters["##commandname##"];

                string regionCommands = getTemplatePlaceholderRegionText(regionName);
                RenderTextToPage(parentUserControl, regionCommands);
            }
            else if (command.StartsWith(COMMAND_DELIMITER + "placeholder", StringComparison.CurrentCultureIgnoreCase))
            {
                // AddControlToPage(new LiteralControl("placeholder: " + rawParameters));

                if (!parameters.ContainsKey("##commandname##"))
                    throw new TemplateExecutionException(templateName, "Template statement \"" + command + "\" must have at least one parameter!");
                
                string placeholderName = parameters["##commandname##"];
                if (!parameters.ContainsKey("id"))
                    throw new TemplateExecutionException(templateName, "The placeholder statement must have an id attribute (\"" + command + "\").");

                int identifier = -1;
                try
                {
                    identifier = Convert.ToInt32(parameters["id"]);
                }
                catch
                {
                    throw new TemplateExecutionException(templateName, "The placeholder statement must have an integer id attribute (\"" + command + "\").");
                }


                // do not output if: 
                //  1) we are making a printer friendly version, and the placeholder has its printer friendly parameter name set to false.
                //  2) we are making an offline version, and the placeholder has its offline version parameter name set to false.
                bool doNotOutput = (
                    /* print friendly version: */
                                (CmsContext.currentUserIsRequestingPrintFriendlyVersion &&
                                parameters.ContainsKey(PRINTER_FRIENDLY_VERSION_OUTPUT_CONTROL_PARAMETERNAME) &&
                                String.Compare(parameters[PRINTER_FRIENDLY_VERSION_OUTPUT_CONTROL_PARAMETERNAME], "false", true) == 0) ||
                    /* offline version: */
                                (CmsContext.currentUserIsRequestingPrintFriendlyVersion &&
                                parameters.ContainsKey(OFFLINE_VERSION_OUTPUT_CONTROL_PARAMETERNAME) &&
                                String.Compare(parameters[OFFLINE_VERSION_OUTPUT_CONTROL_PARAMETERNAME], "false", true) == 0));

                if (!doNotOutput)
                {

                    // params[0] contains the rawParameters
                    string[] subParamsArray = new string[] { rawParameters };

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb));

                    CmsLanguage langToRender = CmsConfig.Languages[currentLangIndex]; // the currentLangIndex is incremented when the EndPageBody statement is found in the template 
                    // dynamically load the Placeholder class and call its Render method            
                    switch (CmsContext.currentEditMode)
                    {
                        case CmsEditMode.Edit:
                            PlaceholderUtils.RenderInEditMode(placeholderName, writer, page, identifier, langToRender, subParamsArray, templateName);
                            break;
                        case CmsEditMode.View:
                            PlaceholderUtils.RenderInViewMode(placeholderName, writer, page, identifier, langToRender, subParamsArray, templateName);
                            break;
                    }


                    string txt = sb.ToString();

                    // -- Run Placeholder Filters
                    txt = CmsOutputFilterUtils.RunPlaceholderFilters(placeholderName, page, txt);

                    LiteralControl literal = new LiteralControl(txt);
                    parentUserControl.Controls.Add(literal);
                }

            }
            else if (command.StartsWith(COMMAND_DELIMITER + "rendercontrol", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!parameters.ContainsKey("##commandname##"))
                    throw new TemplateExecutionException(templateName, "Template statement \"" + command + "\" must have at least one parameter!");
                
                string controlNameOrPath = parameters["##commandname##"];
                // -- try to dynamically load the control onto the page from the ASCX file.
                //    if the ASCX file is not found, we try to load the control as a class. If that fails, throw an Exception
                                
                if (CmsControlUtils.ControlExists(controlNameOrPath))
                {
                    // do not output if: 
                    //  1) we are making a printer friendly version, and the control has its printer friendly parameter name set to false.
                    //  2) we are making an offline version, and the control has its offline version parameter name set to false.
                    bool doNotOutput = (
                        /* print friendly version: */
                                    (CmsContext.currentUserIsRequestingPrintFriendlyVersion &&
                                    parameters.ContainsKey(PRINTER_FRIENDLY_VERSION_OUTPUT_CONTROL_PARAMETERNAME) &&
                                    String.Compare(parameters[PRINTER_FRIENDLY_VERSION_OUTPUT_CONTROL_PARAMETERNAME], "false", true) == 0) ||
                        /* offline version: */
                                    (CmsContext.currentUserIsRequestingPrintFriendlyVersion &&
                                    parameters.ContainsKey(OFFLINE_VERSION_OUTPUT_CONTROL_PARAMETERNAME) &&
                                    String.Compare(parameters[OFFLINE_VERSION_OUTPUT_CONTROL_PARAMETERNAME], "false", true) == 0));                


                    if (!doNotOutput)
                    {
                        int langIndex = currentLangIndex;
                        if (langIndex >= CmsConfig.Languages.Length)
                            langIndex = 0;
                        CmsLanguage langToRender = CmsConfig.Languages[langIndex]; // the currentLangIndex is incremented when the EndPageBody statement is found in the template 
                        CmsControlDefinition controlDef = new CmsControlDefinition(controlNameOrPath, rawParameters );
                        CmsControlUtils.AddControlToPage(controlNameOrPath, controlDef,parentUserControl , langToRender);                                               
                    }
                }
                else
                {                    
                    string ControlNotFoundMessage = "Could not find or load Control: \"" + controlNameOrPath + "\"";
                    throw new TemplateExecutionException(templateName, ControlNotFoundMessage);
                }
            } // renderRonctol
        }

        private string getPageBodyText()
        {            

            string startText = COMMAND_DELIMITER + "StartPageBody" + COMMAND_DELIMITER;            
            string endText = COMMAND_DELIMITER + "EndPageBody" + COMMAND_DELIMITER;

            string txt = this.templateLayoutFileContents;

            int startAt = txt.IndexOf(startText, StringComparison.CurrentCultureIgnoreCase);
            int endAt = txt.IndexOf(endText, StringComparison.CurrentCultureIgnoreCase);

            if (startAt < 0 || endAt < 0)
            {
                txt = this.templateFileContents;

                startAt = txt.IndexOf(startText, StringComparison.CurrentCultureIgnoreCase);
                endAt = txt.IndexOf(endText, StringComparison.CurrentCultureIgnoreCase);

                if (startAt < 0 || endAt < 0)
                    throw new TemplateExecutionException(templateName, "When multiple languages are available, the template must contain the StartPageBody and EndPageBody commands.");
            }


            endAt = endAt + endText.Length; // include the StartPageBody and EndPageBody commands

            string regionContent = txt.Substring(startAt, endAt - startAt);
            return regionContent;
        }

        private string getTemplatePlaceholderRegionText(string regionName)
        {
            string startText1 = COMMAND_DELIMITER + "StartPlaceholderRegion(" + regionName + ")" + COMMAND_DELIMITER;
            string startText2 = COMMAND_DELIMITER + "StartPlaceholderRegion(\"" + regionName + "\")" + COMMAND_DELIMITER;
            string endText1 = COMMAND_DELIMITER + "EndPlaceholderRegion(" + regionName + ")" + COMMAND_DELIMITER;
            string endText2 = COMMAND_DELIMITER + "EndPlaceholderRegion(\"" + regionName + "\")" + COMMAND_DELIMITER;

            int startAt = templateFileContents.IndexOf(startText1, StringComparison.CurrentCultureIgnoreCase);
            if (startAt < 0)
            {
                startAt = templateFileContents.IndexOf(startText2, StringComparison.CurrentCultureIgnoreCase);
                if (startAt >= 0)
                    startAt += startText2.Length;
            }
            else
                startAt += startText1.Length;

            int endAt = templateFileContents.IndexOf(endText1, StringComparison.CurrentCultureIgnoreCase);
            if (endAt < 0)
                endAt = templateFileContents.IndexOf(endText2, StringComparison.CurrentCultureIgnoreCase);

            if (startAt < 0 || endAt < 0)
                throw new TemplateExecutionException(templateName, "The placeholder region \""+regionName+"\" is not properly defined in the template file.");



            string regionContent = templateFileContents.Substring(startAt, endAt - startAt);
            return regionContent;
            
        }

        public static Dictionary<string, string> tokenizeCommandParameters(CmsControlDefinition controlDefinition)
        {
            // note: see getAllControlDefinitions(): the whole command is stored in RawTemplateParameters.            
            return tokenizeCommandParameters(controlDefinition.RawTemplateParameters);
        }

        public static Dictionary<string, string> tokenizeCommandParameters(string rawParameters)
        {
            //##Placeholder("PlaceholderName" id="#" param0="value" param2="value" param3="quoted: \"escaped quotes\"" param4="single quotes: 'no escape needed.' ")##

            Dictionary<string, string>  ret = new Dictionary<string,string>();
            
            bool inKey = false;
            bool inVal = true;
            string key = "##commandname##";
            string val = "";
            bool quoteStarted = false;
            char[] arr = rawParameters.ToCharArray();
            for(int i=0 ; i< arr.Length; i++)
            {
                char prev = '\0';
                if (i > 0)
                    prev = arr[i-1];
                char curr = arr[i];
                if ((curr == '"' && prev != '\\' && quoteStarted && inVal)
                    || (curr == ' ' && !quoteStarted && inVal))
                {
                    ret.Add(key.ToLower(),val);
                    quoteStarted = false;
                    inVal = false;
                    inKey = true;
                    key = "";
                    val = "";
                }
                else if (curr == '"' && prev != '\\' && ! quoteStarted)
                {
                    quoteStarted = true;
                }
                else if (curr == '=' && !quoteStarted && inKey)
                {
                    inKey = false;
                    inVal = true;
                }
                else if (curr == ' ' && !quoteStarted && inKey)
                {
                    // ignore spaces
                }
                else
                {
                    if (prev == '\\' && curr == '=')
                    {
                        if (inKey)
                            key = key.Substring(0, key.Length - 2);
                        if (inVal)
                            val = val.Substring(0, key.Length - 2);
                    }

                    if (inKey)
                        key += curr.ToString();
                    if (inVal)
                        val += curr.ToString();
                }
            } // foreach char

            if (key != "" || val != "")
                ret.Add(key.ToLower(), val);

            return ret;

        }
        

        /// <summary>
        /// gets the text between the brackets for command statements. For example
        /// getCommandStatements("Placeholder","##Placeholder(HtmlContent id=1)##")
        /// would return "HtmlContent id="1""
        /// </summary>
        /// <param name="command"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        private string[] getCommandStatementParameters(string command, string haystack)
        {
            List<string> ret = new List<string>();
            string[] parts = haystack.Split(new string[] { COMMAND_DELIMITER }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in parts)
            {
                if (s.StartsWith(command, StringComparison.CurrentCultureIgnoreCase))
                {
                    int parseFrom = s.IndexOf(command+"(", StringComparison.CurrentCultureIgnoreCase);
                    int parseTo = s.Length - 1;
                    // due to overlap between ##Placeholder and ##PlaceholderRegion and the .StartsWith function above, do not throw exception when parseFrom < 0
                    if (parseFrom >= 0)
                    {
                        if (parseTo < 0)
                            throw new TemplateExecutionException(templateName, "Command \"" + command + "\" is not formatted properly: \"" + s + "\".");

                        parseFrom += command.Length + "(".Length;

                        string statements = s.Substring(parseFrom, (parseTo - parseFrom));
                        ret.Add(statements);
                    }
                }
            } // foreach part

            return ret.ToArray();
        }


        public override string renderControlToString(string controlPath)
        {
            string Control_VirtualPath = CmsContext.ApplicationPath + CONTROLS_SUBDIR + controlPath + ".ascx";

            string html = Hatfield.Web.Portal.PageUtils.RenderUserControl(System.Web.HttpContext.Current, Control_VirtualPath);
            return html;
        }
                

        public override CmsDependency[] getControlDependencies(string controlPath)
        {
            return HatCMS.CmsControlUtils.getControlDependencies(controlPath);
            
        }


        /// <summary>
        /// returns an array of CmsControlDefinitions  for this template.
        /// note: all ControlPath entries are converted to lower case
        /// </summary>
        /// <returns></returns>
        public override CmsControlDefinition[] getAllControlDefinitions()
        {
            List<CmsControlDefinition> ret = new List<CmsControlDefinition>();
            
            // -- get the template file contents
            string templateText = getTemplateFileContents();

            string[] CommandParams = getCommandStatementParameters("rendercontrol", templateText);
            foreach (string rawStatement in CommandParams)
            {
                Dictionary<string, string> tokens = tokenizeCommandParameters(rawStatement);
                string controlNameOrPath = tokens["##commandname##"].ToLower();
                
                CmsControlDefinition def = new CmsControlDefinition(controlNameOrPath, rawStatement);
                ret.Add(def);

            }

            // -- get the TemplateLayout statement
            string[] layouts = getCommandStatementParameters("TemplateLayout", templateText);

            if (layouts.Length > 0)
            {
                string layoutText = getTemplateLayoutFileContents(layouts[0]);
                CommandParams = getCommandStatementParameters("rendercontrol", layoutText);
                foreach (string rawStatement in CommandParams)
                {
                    Dictionary<string, string> tokens = tokenizeCommandParameters(rawStatement);
                    string controlNameOrPath = tokens["##commandname##"].ToLower();

                    CmsControlDefinition def = new CmsControlDefinition(controlNameOrPath, rawStatement);
                    ret.Add(def);

                } // foreach
            }

            return ret.ToArray();


        } // getAllPlaceholderDefinitions

        /// <summary>
        /// returns an array of CmsPlaceholderDefinitions  for this template
        /// note: all placeholderType entries are lower case
        /// </summary>
        /// <returns></returns>
        public override CmsPlaceholderDefinition[] getAllPlaceholderDefinitions()
        {
            List<CmsPlaceholderDefinition> ret = new List<CmsPlaceholderDefinition>();

            // -- get the template file contents
            string templateText = getTemplateFileContents();

            string[] CommandParams = getCommandStatementParameters("placeholder", templateText);
            foreach (string c in CommandParams)
            {
                Dictionary<string, string> tokens = tokenizeCommandParameters(c);
                string phName = tokens["##commandname##"].ToLower();
                if (tokens.ContainsKey("id"))
                {
                    int phId = Convert.ToInt32(tokens["id"]);
                    string[] subParamsArray = new string[] { c };
                    CmsPlaceholderDefinition def = new CmsPlaceholderDefinition(phName, phId, subParamsArray); 
                    ret.Add(def);
                }                
            }

            // -- get the TemplateLayout statement
            string[] layouts = getCommandStatementParameters("TemplateLayout", templateText);

            if (layouts.Length > 0)
            {
                string layoutText = getTemplateLayoutFileContents(layouts[0]);
                CommandParams = getCommandStatementParameters("placeholder", layoutText);
                foreach (string c in CommandParams)
                {
                    Dictionary<string, string> tokens = tokenizeCommandParameters(c);
                    string phName = tokens["##commandname##"].ToLower();
                    if (tokens.ContainsKey("id"))
                    {
                        int phId = Convert.ToInt32(tokens["id"]);
                        string[] subParamsArray = new string[] { c };
                        CmsPlaceholderDefinition def = new CmsPlaceholderDefinition(phName, phId, subParamsArray);
                        ret.Add(def);
                    }
                } // foreach
            }            

            return ret.ToArray();


        } // getAllPlaceholderDefinitions

        public override bool templateExists(string templatePath)
        {
            string[] templates = getTemplateNamesForCurrentUser();
            if (Hatfield.Web.Portal.StringUtils.IndexOf(templates, templatePath, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;
            else
                return false;
        }

        public override bool controlExists(string controlNameOrPath)
        {
            return CmsControlUtils.ControlExists(controlNameOrPath);
        }

        public override DateTime getControlLastModifiedDate(string controlNameOrPath)
        {
            return CmsControlUtils.getControlLastModifiedDate(controlNameOrPath);
        }

        public override string[] getControlParameterKeys(System.Web.UI.UserControl control)
        {
            Dictionary<string, string> parameters = tokenizeCommandParameters(control.ID);
            List<string> keys = new List<string>(parameters.Keys);
            return keys.ToArray();
        }

        public override string[] getControlParameterKeys(CmsControlDefinition controlDefinition)
        {
            Dictionary<string, string> parameters = tokenizeCommandParameters(controlDefinition);
            List<string> keys = new List<string>(parameters.Keys);
            return keys.ToArray();
        }


        public override int getControlParameterKeyValue(System.Web.UI.UserControl control, string key, int defaultValue)
        {
            Dictionary<string, string> parameters = tokenizeCommandParameters(control.ID);
            if (parameters.ContainsKey(key))
            {
                try
                {
                    return Convert.ToInt32(parameters[key]);
                }
                catch { }
            }
            return defaultValue;
        }

        public override bool getControlParameterKeyValue(System.Web.UI.UserControl control, string key, bool defaultValue)
        {
            Dictionary<string, string> parameters = tokenizeCommandParameters(control.ID);
            if (parameters.ContainsKey(key))
            {
                try
                {
                    return Convert.ToBoolean(parameters[key]);
                }
                catch { }
            }
            return defaultValue;
        }

        public override string getControlParameterKeyValue(System.Web.UI.UserControl control, string key, string defaultValue)
        {
            Dictionary<string, string> parameters = tokenizeCommandParameters(control.ID);
            if (parameters.ContainsKey(key))
                return parameters[key];
            return defaultValue;
        }


        
        public override int getControlParameterKeyValue(CmsControlDefinition controlDefinition, string key, int defaultValue)
        {
            Dictionary<string, string> parameters = tokenizeCommandParameters(controlDefinition);
            if (parameters.ContainsKey(key))
            {
                try
                {
                    return Convert.ToInt32(parameters[key]);
                }
                catch { }
            }
            return defaultValue;
        }

        public override bool getControlParameterKeyValue(CmsControlDefinition controlDefinition, string key, bool defaultValue)
        {
            Dictionary<string, string> parameters = tokenizeCommandParameters(controlDefinition);
            if (parameters.ContainsKey(key))
            {
                try
                {
                    return Convert.ToBoolean(parameters[key]);
                }
                catch { }
            }
            return defaultValue;
        }

        public override string getControlParameterKeyValue(CmsControlDefinition controlDefinition, string key, string defaultValue)
        {
            Dictionary<string, string> parameters = tokenizeCommandParameters(controlDefinition);
            if (parameters.ContainsKey(key))
                return parameters[key];
            return defaultValue;
        }

        /// <summary>
        /// gets the names of the currently available templates for the current user.
        /// </summary>
        public override string[] getTemplateNamesForCurrentUser()
        {
            List<string> ret = new List<string>();
            
            // -- 1: add templates on disk

            string path = System.Web.Hosting.HostingEnvironment.MapPath(CmsContext.ApplicationPath + TEMPLATE_SUBDIR);
            
            getRecursiveTemplatesOnDisk(ret, path, "");

            // -- 2: add templates in embedded resources
            Dictionary<string, System.Reflection.Assembly> embeddedTemplates = getAllTemplatesInAssemblies();
            foreach (string templateName in embeddedTemplates.Keys)
            {
                // embeddedTemplates contains both templates and layout files. We only want template files (not layout ones).
                if (templateName.EndsWith(TEMPLATE_EXTENSION, StringComparison.CurrentCultureIgnoreCase))
                {
                    string cleanName = cleanTemplateNameStoredInAssembly(templateName);

                    if (ret.IndexOf(cleanName) >= 0)
                        throw new TemplateExecutionException(templateName, "There are multiple templates named '"+cleanName+"'");
                    ret.Add(cleanName);
                }
            } // foreach

            ret.Sort();
            return ret.ToArray();
        }

        private void getRecursiveTemplatesOnDisk(List<string> fileList, string FullDirectoryPath, string appendToTemplateName)
        {
            string[] files = System.IO.Directory.GetFiles(FullDirectoryPath, "*" + TEMPLATE_EXTENSION);
            foreach (string f in files)
            {
                string templateName = appendToTemplateName + System.IO.Path.GetFileNameWithoutExtension(f);

                // -- hide templates that start with "_" from non-admin users
                if (CmsContext.currentUserIsSuperAdmin)
                {
                    if (fileList.IndexOf(templateName) >= 0)
                        throw new TemplateExecutionException(templateName, "There are multiple templates named '" + templateName + "'");

                    fileList.Add(templateName);
                }
                else
                {
                    if (!templateName.StartsWith("_") && !System.IO.Path.GetFileName(f).StartsWith("_"))
                    {
                        if (fileList.IndexOf(templateName) >= 0)
                            throw new TemplateExecutionException(templateName, "There are multiple templates named '" + templateName + "'");

                        fileList.Add(templateName);
                    }
                }
            } // foreach file


            string[] dirs = System.IO.Directory.GetDirectories(FullDirectoryPath);
            foreach (string dir in dirs)
            {
                string append = appendToTemplateName;
                append += System.IO.Path.GetFileName(dir);
                append += "/";

                getRecursiveTemplatesOnDisk(fileList, dir, append);
            }

        } // getRecursiveTemplateList

    } // class
}
