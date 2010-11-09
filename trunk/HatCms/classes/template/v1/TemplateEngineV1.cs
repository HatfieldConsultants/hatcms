using System;
using System.Web.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using HatCMS.Placeholders;

using Hatfield.Web.Portal;

namespace HatCMS.TemplateEngine
{
	/// <summary>
	/// the TemplateExecutor class executes (ie Renders) a template
	/// </summary>
	public class TemplateEngineV1: CmsTemplateEngine
	{
		protected CmsPage _page;
		string _templateName;
		/// <summary>
		/// the template extension (with a beginning period ('.')))
		/// </summary>
		private const string TEMPLATE_EXTENSION = ".htm";
		
		/// <summary>
        /// the template sub directory (no beginning slash; must have trailing slash ('/'))
		/// </summary>
        private const string TEMPLATE_SUBDIR = "templates/";
		
		/// <summary>
		/// the controls sub directory (no beginning slash; must have trailing slash ('/'))
		/// </summary>
        private const string CONTROLS_SUBDIR = "controls/";

        private const string COMMAND_DELIMITER = "##";

        private const string PARAMLIST_ITEM_DELIMITER = ",";

		/// <summary>
		/// Creates a new Template Engine class.
		/// </summary>
		/// <param name="templateName">the name of the template to execute</param>
		/// <param name="page">the CmsPage that will be rendered using the template</param>
        public TemplateEngineV1(string templateName, CmsPage page)
		{
			_templateName = templateName;
			_page = page;
		}

        private bool hasParentControl(string parentControlId, Control control)
        {
            Control currentControl = control;
            while (currentControl != null)
            {
                if (String.Compare(parentControlId, currentControl.ID, true) == 0)
                    return true;

                currentControl = currentControl.Parent;
            }
            return false;
        }

        private enum ControlRenderMode { UpToEmbeddedPage, AfterEmbeddedPage, FullPage }

        bool hasReachedEmbeddedPageMarker = false;
        private void AddControlToPage(Control controlToAdd, ControlRenderMode controlRenderMode)
        {            
            switch (controlRenderMode)
            {
                case ControlRenderMode.UpToEmbeddedPage:
                    if (!hasReachedEmbeddedPageMarker)
                        _page.Controls.Add(controlToAdd);
                    break;
                case ControlRenderMode.AfterEmbeddedPage:
                    if (hasReachedEmbeddedPageMarker)
                        _page.Controls.Add(controlToAdd);
                    break;
                case ControlRenderMode.FullPage:
                    _page.Controls.Add(controlToAdd);
                    break;
            }
        }

        private string getTemplateFilenameOnDisk()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            // get the templateFilename
            string templateUrl = CmsContext.ApplicationPath + TEMPLATE_SUBDIR + _templateName + TEMPLATE_EXTENSION;
            string templateFN_ondisk = context.Server.MapPath(templateUrl);

            return templateFN_ondisk;
        }

        private string getTemplateFileContents()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string templateFN_ondisk = getTemplateFilenameOnDisk();

            string fullText = "";
            // -- see if the template's contents are in the memory cache
            string cacheKey = "template_" + templateFN_ondisk;
            if (context.Cache[cacheKey] != null)
            {
                fullText = context.Cache[cacheKey] as string;
            }
            else
            {
                // -- the contents are not in the cache - let's read it from the file.
                if (!File.Exists(templateFN_ondisk))
                {
                    throw new TemplateExecutionException(new Exception(), templateFN_ondisk, "Could not read template file contents: The template file '" + templateFN_ondisk + "' for page '" + this._page.Path + "' was not found on disk!");
                }
                // -- read the template file			
                StreamReader sr = new StreamReader(templateFN_ondisk);

                try
                {
                    fullText = sr.ReadToEnd();
                    context.Cache.Insert(cacheKey, fullText, new System.Web.Caching.CacheDependency(templateFN_ondisk));
                }
                finally
                {
                    sr.Close();
                }
            }
            return fullText;
        }


		/// <summary>
		/// Renders a template with all its controls and placeholders, and add these controls and placeholders to the CmsPage.
		/// Note: do not use Render to HtmlWriter - Control events are not called properly.
		/// </summary>
		public override void CreateChildControls()
		{            
            ControlRenderMode controlRenderMode = ControlRenderMode.FullPage;
            if (hasParentControl("BeforeEmbeddedPage", _page))
                controlRenderMode = ControlRenderMode.UpToEmbeddedPage;
            else if (hasParentControl("AfterEmbeddedPage",_page))
                controlRenderMode = ControlRenderMode.AfterEmbeddedPage;

            string fullText = getTemplateFileContents();
            string templateFN_ondisk = getTemplateFilenameOnDisk();

			// -- parse the template line-by-line
			foreach(string line in fullText.Split(new char[] {'\n'}))
			{
				if (line.IndexOf(COMMAND_DELIMITER) >= 0)
				{
					string tmpLine = line;
					while(tmpLine.IndexOf(COMMAND_DELIMITER) >= 0)
					{
						int commandStart = tmpLine.IndexOf(COMMAND_DELIMITER);
						int commandEnd = tmpLine.IndexOf(COMMAND_DELIMITER,commandStart+1)+COMMAND_DELIMITER.Length;

						string preCommand = tmpLine.Substring(0, commandStart);
						System.Web.UI.LiteralControl literal = new LiteralControl(preCommand);
						_page.Controls.Add(literal);

						string command = tmpLine.Substring(commandStart, commandEnd - commandStart);

                        renderCommand(templateFN_ondisk, command, controlRenderMode);
					
						tmpLine = tmpLine.Substring(preCommand.Length+command.Length); // get whatever is left
						
					}
					if (tmpLine != "")
					{
						// -- render what's left over after the command statements
                        AddControlToPage(new LiteralControl(tmpLine), controlRenderMode);
					}
				}
				else
				{					
					System.Web.UI.LiteralControl literal = new LiteralControl(line);
					AddControlToPage(literal, controlRenderMode);
				}
			
			} // foreach line
            
		} // CreateChildControls

        public override string renderControlToString(string controlPath)
        {
            string Control_VirtualPath = CmsContext.ApplicationPath + CONTROLS_SUBDIR + controlPath + ".ascx";

            string html = Hatfield.Web.Portal.PageUtils.RenderUserControl(System.Web.HttpContext.Current, Control_VirtualPath);
            return html;
        }

        private string[] getParamArray(string paramList)
        {
            if (paramList.IndexOf(PARAMLIST_ITEM_DELIMITER) == -1)
                return new string[] { paramList };
            
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            MemoryStream ms = StringUtils.WriteTextToMemoryStream(paramList,enc);

            Hatfield.Web.Portal.Data.Depricated.CSVReader2 csv = new Hatfield.Web.Portal.Data.Depricated.CSVReader2(ms, enc);
            return csv.GetCSVLine();
        }


        private void renderCommand(string templateFilename, string line, ControlRenderMode controlRenderMode)
		{
			System.Web.HttpContext context = System.Web.HttpContext.Current;
			
			int start = line.IndexOf(COMMAND_DELIMITER)+COMMAND_DELIMITER.Length;
			int end = line.LastIndexOf(COMMAND_DELIMITER);
			string command = line.Substring(start,end-start);
			start = command.IndexOf("(")+"(".Length;
			end = command.IndexOf(")");
            if (start < 2 || end < 1)
            {
                throw new TemplateExecutionException(new ArgumentException(), templateFilename, "The template command \"" + command + "\" needs to have brackets \"()\" after it!");
            }
			string paramList = command.Substring(start, end-start);
            string[] paramArray = getParamArray(paramList);

			string commandFunction = command.Replace("("+paramList+")","");;

			if (String.Compare(commandFunction,"rendercontrol",true)==0)
			{								
				// -- try to dynamically load the control onto the page from the ASCX file.
				//    if the ASCX file is not found, throw an Exception
                string Control_VirtualPath = CmsContext.ApplicationPath + CONTROLS_SUBDIR + paramArray[0] + ".ascx";
				string Control_FilenameOnDisk = context.Server.MapPath(Control_VirtualPath);
				if (File.Exists(Control_FilenameOnDisk))
				{
					Control control = _page.LoadControl(Control_VirtualPath);
                    // set the parameters for the control. Note: use CmsContext.getControlParameters() to get the list of parameters
                    control.ID = StringUtils.JoinNonBlanks(PARAMLIST_ITEM_DELIMITER, paramArray);

                    AddControlToPage(control, controlRenderMode);
				}
				else
				{
					string ControlNotFoundMessage = "Could not load UserControl: ASCX file not found: "+Control_VirtualPath+"";
					throw new TemplateExecutionException(new Exception(),templateFilename, ControlNotFoundMessage);
					/*
					System.Web.UI.LiteralControl errorMessage = new LiteralControl(ControlNotFoundMessage);
					_page.Controls.Add(control);
					*/
				}
			}
			else if (String.Compare(commandFunction ,"placeholder",true)==0)
			{
                renderPlaceholder(templateFilename, paramArray, controlRenderMode, CmsContext.currentLanguage);
			}
            else if (String.Compare(commandFunction, "EmbeddedASPXPage", true) == 0)
            {
                hasReachedEmbeddedPageMarker = true;
            }
			
		} // renderCommand

		private void renderPlaceholder(string templateFilename, string[] paramArray, ControlRenderMode controlRenderMode, CmsLanguage renderedLanguage)
		{
            System.Web.HttpContext context = System.Web.HttpContext.Current;
                       
            // two parameters: type, identifier	
			if (paramArray.Length < 2)
			{
				throw new TemplateExecutionException(new Exception(), templateFilename, "PlaceHolder must have at least 2 parameters!");				
			}
			/// TODO: this could work dynamically by loading PlaceHolder classes dynamically:
			/// http://www.west-wind.com/presentations/dynamicCode/DynamicCode.htm
			
			// -- set up the parameters
			int identifier = -1;
			try
			{
				identifier = Convert.ToInt32(paramArray[1]);
			}
			catch
			{
				identifier = -1;
			}
			ArrayList subParams = new ArrayList();
			for(int i = 2; i < paramArray.Length; i++)
			{
				subParams.Add(paramArray[i]);
			}

			string[] subParamsArray = new string[subParams.Count];
			subParams.CopyTo(subParamsArray);

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb));
			string PlaceholderType = paramArray[0].Trim().ToLower();

			// dynamically load the Placeholder class and call its Render method            
            switch (CmsContext.currentEditMode)
            {
                case CmsEditMode.Edit:
                    PlaceholderUtils.RenderInEditMode(PlaceholderType, writer, _page, identifier, renderedLanguage, subParamsArray, templateFilename);
                    break;
                case CmsEditMode.View:
                    PlaceholderUtils.RenderInViewMode(PlaceholderType, writer, _page, identifier, renderedLanguage, subParamsArray, templateFilename);
                    break;
            }
                            
						
			string txt = sb.ToString();
			LiteralControl literal = new LiteralControl(txt);
            AddControlToPage(literal, controlRenderMode);
		} // renderPlaceholder

        

        public override bool templateExists(string templatePath)
        {
            if (templatePath.EndsWith(TEMPLATE_EXTENSION, StringComparison.CurrentCultureIgnoreCase))
            {
                templatePath = templatePath.Substring(0, templatePath.Length - (TEMPLATE_EXTENSION.Length));
            }
            string templateUrl = CmsContext.ApplicationPath + TEMPLATE_SUBDIR + templatePath + TEMPLATE_EXTENSION;
            string templateFN_ondisk = System.Web.HttpContext.Current.Server.MapPath(templateUrl);
            return File.Exists(templateFN_ondisk);                
        }

        public override bool controlExists(string controlPath)
        {
            if (controlPath.EndsWith(".ascx", StringComparison.CurrentCultureIgnoreCase))
            {
                controlPath = controlPath.Substring(0, controlPath.Length - (".ascx".Length));
            }
            string fullPath = System.Web.HttpContext.Current.Server.MapPath(CmsContext.ApplicationPath + CONTROLS_SUBDIR + controlPath + ".ascx");
            return File.Exists(fullPath);                
        }

        public override DateTime getControlLastModifiedDate(string controlPath)
        {
            if (controlPath.EndsWith(".ascx", StringComparison.CurrentCultureIgnoreCase))
            {
                controlPath = controlPath.Substring(0, controlPath.Length - (".ascx".Length));
            }
            string fullPath = System.Web.HttpContext.Current.Server.MapPath(CmsContext.ApplicationPath + CONTROLS_SUBDIR + controlPath + ".ascx");

            FileInfo fi = new FileInfo(fullPath);
            return fi.LastWriteTime;
        }

        /// <summary>
        /// gets all parameters for the control, including the 
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        private string[] getControlParameters(System.Web.UI.UserControl control)
        {
            string paramList = control.ID; // set in TemplateExecutor.renderCommand()
            if (paramList.IndexOf(PARAMLIST_ITEM_DELIMITER) == -1)
                return new string[] { paramList };

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            System.IO.MemoryStream ms = StringUtils.WriteTextToMemoryStream(paramList, enc);

            Hatfield.Web.Portal.Data.Depricated.CSVReader2 csv = new Hatfield.Web.Portal.Data.Depricated.CSVReader2(ms, enc);
            return csv.GetCSVLine();
        }

        public override int getControlParameterKeyValue(System.Web.UI.UserControl control, string key, int defaultValue)
        {
            string val = getControlParameterKeyValue(control, key, defaultValue.ToString());
            try
            {
                return Convert.ToInt32(val);
            }
            catch
            { }
            return defaultValue;
        }

        public override bool getControlParameterKeyValue(System.Web.UI.UserControl control, string key, bool defaultValue)
        {
            string val = getControlParameterKeyValue(control, key, defaultValue.ToString());
            try
            {
                return Convert.ToBoolean(val);
            }
            catch
            { }
            return defaultValue;
        } // getControlParameterKeyValue

        public override string getControlParameterKeyValue(System.Web.UI.UserControl control, string key, string defaultValue)
        {
            try
            {
                string[] controlParameters = getControlParameters(control);
                foreach (string cmd in controlParameters)
                {
                    int eqIndex = cmd.IndexOf('=');
                    if (eqIndex < 0)
                        eqIndex = cmd.Length;

                    string beforeEquals = cmd.Substring(0, eqIndex).Trim();

                    if (String.Compare(beforeEquals, key, true) == 0)
                    {
                        if (eqIndex < 0)
                            return defaultValue;

                        string afterEquals = cmd.Substring(eqIndex + 1); // +1 for '=' character

                        afterEquals = afterEquals.Trim();
                        return afterEquals;

                    }
                } // foreach
            }
            catch { }
            return defaultValue;
        }

        public override string[] getControlParameterKeys(System.Web.UI.UserControl control)
        {
            List<string> ret = new List<string>();
            try
            {
                string[] controlParameters = getControlParameters(control);
                bool first = true;
                foreach (string cmd in controlParameters)
                {
                    if (first)
                    {
                        first = false; // skip the first one which is the control name
                        continue;
                    }
                    int eqIndex = cmd.IndexOf('=');
                    if (eqIndex < 0)
                        eqIndex = cmd.Length;

                    string beforeEquals = cmd.Substring(0, eqIndex).Trim();

                    ret.Add(beforeEquals);

                } // foreach
            }
            catch { }
            return ret.ToArray();
        } // getControlParameterKeys

        public override CmsDependency[] getControlDependencies(string controlPath)
        {
            return HatCMS.CmsControlUtils.getControlDependencies(controlPath);
        }


        public override string[] getAllControlPaths()
        {            
            List<string> ret = new List<string>();
            string fullText = getTemplateFileContents();

            string[] fileParts = fullText.Split(new string[] { COMMAND_DELIMITER }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in fileParts)
            {
                string part_lower = part.ToLower();
                if (part_lower.StartsWith("rendercontrol"))
                {
                    Console.Write(part);
                    int start = part.IndexOf("(") + "(".Length;
                    int end = part.IndexOf(")");
                    if (start < 2 || end < 1)
                    {
                        throw new TemplateExecutionException(new ArgumentException(), this._templateName, "The template command \"Placeholder\" needs to have brackets \"()\" after it!");
                    }
                    string paramList = part.Substring(start, end - start);
                    string[] paramArray = getParamArray(paramList);
                    if (paramArray.Length >= 1 && paramArray[0].Trim().ToLower() != "")
                    {
                        ret.Add(paramArray[0].Trim());
                    }
                } // if
            } // foreach
            return ret.ToArray();
        }

        /// <summary>
        /// returns an array of CmsPlaceholderDefinition parsed from the template file.
        /// </summary>
        /// <returns></returns>
        public override CmsPlaceholderDefinition[] getAllPlaceholderDefinitions()
        {

            List<CmsPlaceholderDefinition> ret = new List<CmsPlaceholderDefinition>();

            string fullText = getTemplateFileContents();

            string[] fileParts = fullText.Split(new string[] { COMMAND_DELIMITER }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in fileParts)
            {
                string part_lower = part.ToLower();
                if (part_lower.StartsWith("placeholder"))
                {
                    Console.Write(part);
                    int start = part.IndexOf("(") + "(".Length;
                    int end = part.IndexOf(")");
                    if (start < 2 || end < 1)
                    {
                        throw new TemplateExecutionException(new ArgumentException(), this._templateName, "The template command \"Placeholder\" needs to have brackets \"()\" after it!");
                    }
                    string paramList = part.Substring(start, end - start);
                    string[] paramArray = getParamArray(paramList);
                    if (paramArray.Length >= 2 && paramArray[0].Trim().ToLower() != "" && paramArray[1].Trim() != "")
                    {
                        try
                        {
                            string placeholderType = paramArray[0].Trim().ToLower();
                            int identifier = Convert.ToInt32(paramArray[1].Trim());
                            CmsPlaceholderDefinition def = new CmsPlaceholderDefinition(placeholderType, identifier, paramArray);
                            ret.Add(def);
                        }
                        catch
                        { }
                    } // if
                }// if
            } // foreach

            return ret.ToArray();
        } // getAllPlaceholderDefinitions


        /// <summary>
        /// gets the names of the currently available templates for the current user.
        /// </summary>
        public override string[] getTemplateNamesForCurrentUser()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string path = context.Server.MapPath(CmsContext.ApplicationPath + TEMPLATE_SUBDIR);
            ArrayList ret = new ArrayList();
            getRecursiveTemplateList(ret, path, "");
            return (string[])ret.ToArray(typeof(string));
        }

        private void getRecursiveTemplateList(ArrayList fileList, string FullDirectoryPath, string appendToTemplateName)
        {
            string[] files = System.IO.Directory.GetFiles(FullDirectoryPath, "*" + TEMPLATE_EXTENSION);
            foreach (string f in files)
            {
                string templateName = appendToTemplateName + System.IO.Path.GetFileNameWithoutExtension(f);

                // -- hide templates that start with "_" from non-admin users
                if (CmsContext.currentUserIsSuperAdmin)
                {
                    fileList.Add(templateName);
                }
                else
                {
                    if (!templateName.StartsWith("_") && !System.IO.Path.GetFileName(f).StartsWith("_"))
                    {
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

                getRecursiveTemplateList(fileList, dir, append);
            }

        } // getRecursiveTemplateList

        
	} // class

	
}
