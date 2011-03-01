using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    public class Glossary : BaseCmsPlaceholder
    {
        public override CmsDependency[] getDependencies()
        {
            return new CmsDependency[]{
                CmsFileDependency.UnderAppPath("js/_system/GlossaryEditor.js", new DateTime(2010,4,22)),
                CmsFileDependency.UnderAppPath("js/_system/json2.js"),
                CmsWritableDirectoryDependency.UnderAppPath("_system/writable/Glossary"),
                new CmsDatabaseTableDependency("glossary"),
                new CmsDatabaseTableDependency("glossarydata")
            };
        }

 
        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }
                

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string ControlId = "Glossary_" + page.ID.ToString() + "_" + identifier.ToString() + "_" + langToRenderFor.shortCode;
            string renderEditorToDivId = ControlId + "RenderArea";
            string renderJsonToFormId = ControlId + "JsonFormData";
            string jsonDataVarName = ControlId + "_GlossaryJsonData";


            GlossaryDb db = new GlossaryDb();
            GlossaryPlaceholderData placeholderData = db.getGlossary(page, identifier, langToRenderFor, true);

            string action = PageUtils.getFromForm(ControlId + "_action", "");
            if (String.Compare(action, "updateGlossary", true) == 0)
            {
                string receivedJSONData = PageUtils.getFromForm(renderJsonToFormId, "");
                
                GlossaryData[] newData = fromJSON(receivedJSONData);

                placeholderData.SortOrder = (GlossaryPlaceholderData.GlossarySortOrder)PageUtils.getFromForm(ControlId + "SortOrder", typeof(GlossaryPlaceholderData.GlossarySortOrder), placeholderData.SortOrder);
                placeholderData.ViewMode = (GlossaryPlaceholderData.GlossaryViewMode)PageUtils.getFromForm(ControlId + "ViewMode", typeof(GlossaryPlaceholderData.GlossaryViewMode), placeholderData.ViewMode);
                bool b = db.saveUpdatedGlossary(page, identifier, langToRenderFor, placeholderData, newData);
                if (b)
                {
                    GlossaryData[] dataToSave = db.getGlossaryData(placeholderData, "");
                    string jsonToSave = getJSVariableStatement(jsonDataVarName, dataToSave);
                    try
                    {
                        string jsonFilename = System.Web.HttpContext.Current.Server.MapPath(CmsContext.ApplicationPath + "_system/writable/Glossary/" + jsonDataVarName + ".js");
                        System.IO.File.Delete(jsonFilename);
                        System.IO.File.WriteAllText(jsonFilename, jsonToSave);
                    }
                    catch
                    { }
                }

            } // if updateGlossary

            StringBuilder html = new StringBuilder();
            html.Append(PageUtils.getHiddenInputHtml(ControlId + "_action", "updateGlossary"));
            string EOL = Environment.NewLine;

            html.Append("<p><strong>Glossary Display Configuration:</strong></p>");

            html.Append("<table>");
            if (CmsConfig.Languages.Length > 1)
            {
                html.Append("<tr>");
                html.Append("<td>Language:</td><td>"+langToRenderFor.shortCode+"</td>");
                html.Append("</tr>");
            }
            html.Append("<tr>");
            html.Append("<td>Display Mode: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getDropDownHtml(ControlId + "ViewMode", ControlId + "ViewMode", Enum.GetNames(typeof(GlossaryPlaceholderData.GlossaryViewMode)), Enum.GetName(typeof(GlossaryPlaceholderData.GlossaryViewMode), placeholderData.ViewMode)));
            html.Append("</td>");
            html.Append("</tr>");            
            html.Append("<tr>");
            html.Append("<td>Output Sorting: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getDropDownHtml(ControlId + "SortOrder", ControlId + "SortOrder", Enum.GetNames(typeof(GlossaryPlaceholderData.GlossarySortOrder)), Enum.GetName(typeof(GlossaryPlaceholderData.GlossarySortOrder), placeholderData.SortOrder)));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("</table>" + EOL);

            // -- glossary data
            // note: the form is linked to GlossaryEditor.js
            GlossaryData[] items = db.getGlossaryData(placeholderData, "");
            html.Append("<p><strong>Glossary Entries:</strong></p>");
            

            html.Append("<div id=\"" + renderEditorToDivId + "\"></div>");

            html.Append("<input type=\"button\" onclick=\"AddGlossaryElement('" + renderEditorToDivId + "');\" value=\"add new glossary entry\">" + Environment.NewLine);
            
            // -- the JSON data is passed to the server through this hidden form element
            html.Append(PageUtils.getHiddenInputHtml(renderJsonToFormId, renderJsonToFormId, ""));
                                    

            page.HeadSection.AddJavascriptFile("js/_system/json2.js");
            page.HeadSection.AddJavascriptFile("js/_system/GlossaryEditor.js");

            page.HeadSection.AddJSStatements(getJSVariableStatement(jsonDataVarName, items));


            page.HeadSection.AddJSOnReady("var " + ControlId + "Instance = new GlossaryEditor('" + renderEditorToDivId + "', '" + renderJsonToFormId + "', JSON.parse(" + jsonDataVarName + ")); ");
            page.HeadSection.AddJSOnReady("GlossaryEditorInstances[GlossaryEditorInstances.length] = " + ControlId + "Instance; ");
            page.HeadSection.AddJSOnReady("" + ControlId + "Instance.updateDisplay();");
            
            
            

            writer.Write(html.ToString());

        } // RenderEdit

        private GlossaryData[] fromJSON(string jsonString)
        {
            JsonFx.Json.JsonReader r = new JsonFx.Json.JsonReader(jsonString);
            object o = r.Deserialize();

            if (o is Dictionary<string, object>)
            {
                List<GlossaryData> ret = new List<GlossaryData>();
                Dictionary<string, object> dict = (o as Dictionary<string, object>);
                foreach (string dictKey in dict.Keys)
                {
                    if (dict[dictKey] is Dictionary<string, object>)
                    {
                        Dictionary<string, object> dictObj = (dict[dictKey] as Dictionary<string, object>);
                        GlossaryData newGData = new GlossaryData();
                        foreach (string objParam in dictObj.Keys)
                        {
                            if (string.Compare(objParam, "word", true) == 0)
                                newGData.word = dictObj[objParam].ToString();
                            else if (string.Compare(objParam, "text", true) == 0)
                                newGData.description = dictObj[objParam].ToString();
                            else if (string.Compare(objParam, "isAcronym", true) == 0)
                            {
                                newGData.isAcronym = Convert.ToBoolean( dictObj[objParam].ToString());
                            }
                        } // foreach
                        ret.Add(newGData);
                    }

                } // foreach
                return ret.ToArray();
            }

            return new GlossaryData[0];
        }

        private string jsonEncode(string s)
        {
            string ret = s.Replace(Environment.NewLine, " ");
            ret = ret.Replace("\n", " ");
            ret = ret.Replace("\r", " ");

            ret = ret.Replace("\"", @"\\"+"\"");
            ret = ret.Replace(@"'", @"\'");

            return ret;
        }

        private string jsonEncode(bool b)
        {
            if (b)
                return "true";
            else
                return "false";
        }
        
        private string getJSVariableStatement(string jsonDataVarName, GlossaryData[] items)
        {
            List<string> objJsons = new List<string>();
            // int numOutput = 0;
            foreach (GlossaryData g in items)
            {
                string objJson = " \"" + g.Id + "\": {\"id\": " + g.Id + ", \"word\": \"" + jsonEncode(g.word) + "\", \"isAcronym\": " + jsonEncode(g.isAcronym) + ", \"text\": \"" + jsonEncode(g.description) + "\"}";
                objJsons.Add(objJson);                
            }

            StringBuilder json = new StringBuilder();
            json.Append("var " + jsonDataVarName + " = '{");
            json.Append(String.Join(",", objJsons.ToArray()) );
            json.Append("}';");

            return json.ToString() + Environment.NewLine;
        }

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            GlossaryDb db = new GlossaryDb();
            GlossaryPlaceholderData placeholderData = db.getGlossary(page, identifier, langToRenderFor, true);
            string letterToDisplay = "";
            if (placeholderData.ViewMode == GlossaryPlaceholderData.GlossaryViewMode.PagePerLetter)
            {
                string l = PageUtils.getFromForm("l", "");
                if (l.Length == 1)
                    letterToDisplay = l[0].ToString();
                else
                {
                    // placeholderData.ViewMode = GlossaryPlaceholderData.GlossaryViewMode.SinglePageWithJumpList;
                }
            }
            
            GlossaryData[] items = db.getGlossaryData(placeholderData, letterToDisplay);

            StringBuilder html = new StringBuilder();
            html.Append("<div class=\"Glossary\">"+Environment.NewLine);
            // -- output JumpLinks
            string[] charactersWithData = db.getAllCharactersWithData(placeholderData);
            html.Append("<div class=\"JumpLinks\">");
            string JumpLinksSeperator = " | ";
            List<string> jumpLinks = new List<string>();
            if (placeholderData.ViewMode == GlossaryPlaceholderData.GlossaryViewMode.PagePerLetter && letterToDisplay != "")
            {
                jumpLinks.Add("<a href=\"" + page.Url + "\" title=\"view entire glossary\">[all]</a>");                
            }


            char startChar = 'A';
            char lastChar = 'Z'; lastChar++;
            char currentChar = startChar;
            do
            {
                string link = "";
                bool outputA = false;

                if (StringUtils.IndexOf(charactersWithData, currentChar.ToString(), StringComparison.CurrentCultureIgnoreCase) > -1)
                {
                    outputA = true;

                    string url = "";
                    if (placeholderData.ViewMode == GlossaryPlaceholderData.GlossaryViewMode.PagePerLetter)
                    {
                        NameValueCollection pageParams = new NameValueCollection();
                        pageParams.Add("l", currentChar.ToString());
                        url = CmsContext.getUrlByPagePath(page.Path, pageParams);
                    }
                    else if (placeholderData.ViewMode == GlossaryPlaceholderData.GlossaryViewMode.SinglePageWithJumpList)
                    {
                        url = "#letter_" + currentChar.ToString();
                    }
                        
                    link += "<a href=\""+url+"\">";
                }
                link += currentChar.ToString();

                if (outputA)
                    link += "</a>";
                
                jumpLinks.Add(link);
                currentChar++;
            }
            while (currentChar != lastChar);

            string jumpLinksHtml = String.Join(JumpLinksSeperator, jumpLinks.ToArray());

            html.Append(jumpLinksHtml);
            html.Append("</div>"); // JumpLinks

            // -- output terms
            switch (placeholderData.ViewMode)
            {
                case GlossaryPlaceholderData.GlossaryViewMode.PagePerLetter:
                    if (letterToDisplay == "")
                    {
                        startChar = 'A';
                        lastChar = 'Z'; lastChar++;
                    }
                    else
                    {
                        startChar = letterToDisplay[0];
                        lastChar = letterToDisplay[0]; lastChar++;
                    }
                    break;
                case GlossaryPlaceholderData.GlossaryViewMode.SinglePageWithJumpList:
                    startChar = 'A';
                    lastChar = 'Z'; lastChar++;
                    break;
                default:
                    throw new ArgumentException("invalid GlossaryViewMode");
            } // switch viewmode


            currentChar = startChar;
            html.Append("<table class=\"glossaryitems\">");
            do
            {
                html.Append("<tr><td class=\"LetterHeading\" colspan=\"2\">");
                html.Append("<a name=\"letter_" + currentChar.ToString() + "\"></a>");
                html.Append("<h2>" + currentChar + "</h2>");
                html.Append("</td></tr>" + Environment.NewLine);

                GlossaryData[] itemsToDisplay = new GlossaryData[0];
                if (placeholderData.ViewMode == GlossaryPlaceholderData.GlossaryViewMode.SinglePageWithJumpList || letterToDisplay == "")
                    itemsToDisplay = GlossaryData.getItemsStartingWithChar(items, currentChar);
                else if (placeholderData.ViewMode == GlossaryPlaceholderData.GlossaryViewMode.PagePerLetter)
                    itemsToDisplay = items;

                
                bool oddRow = true;
                foreach (GlossaryData item in itemsToDisplay)
                {
                    string cssClass = "even";
                    if (oddRow)
                        cssClass = "odd";
                    html.Append("<tr class=\"" + cssClass + "\">");
                    html.Append("<td class=\"word " + cssClass + "\">" + item.word + "</td>");
                    html.Append("<td class=\"description " + cssClass + "\">" + item.description + "</td>");
                    html.Append("</tr>"+Environment.NewLine);
                    oddRow = ! oddRow;
                } // foreach glossarydata item
                
                currentChar++;
            } while (currentChar != lastChar);

            html.Append("</table>");
            html.Append("</div>"); // glossary
            writer.Write(html.ToString());

        } // RenderView

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            GlossaryDb db = new GlossaryDb();
            GlossaryPlaceholderData placeholderData = db.getGlossary(page, placeholderDefinition.Identifier, langToRenderFor, true);
            GlossaryPlaceholderData.GlossaryViewMode origViewMode = placeholderData.ViewMode;
            
            // -- gets all glossary items (regardless of the ViewMode)
            GlossaryData[] items = db.getGlossaryData(placeholderData.GlossaryId);
            
            // -- each glossary item gets its own rssItem
            List<Rss.RssItem> ret = new List<Rss.RssItem>();
            foreach (GlossaryData glData in items)
            {
                Rss.RssItem rssItem = new Rss.RssItem();
                rssItem = base.InitRssItem(rssItem, page, langToRenderFor);

                rssItem.Description = glData.description;
                // -- setup the proper link
                switch (placeholderData.ViewMode)
                {
                    case GlossaryPlaceholderData.GlossaryViewMode.PagePerLetter:

                        Dictionary<string, string> urlParams = new Dictionary<string, string>();
                        urlParams.Add("l", glData.word.ToUpper()[0].ToString());
                        rssItem.Link = new Uri(page.getUrl(urlParams, langToRenderFor));
                        break;
                    case GlossaryPlaceholderData.GlossaryViewMode.SinglePageWithJumpList:
                        // nothing to do
                        break;
                    default:
                        throw new Exception("Error: invalid GlossaryViewMode");
                } // switch
            }
            return ret.ToArray();
        }

    } // Glossary placeholder class
}

