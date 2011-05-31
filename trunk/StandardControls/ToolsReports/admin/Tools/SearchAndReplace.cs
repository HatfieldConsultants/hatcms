using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using Hatfield.Web.Portal;
using System.Collections.Specialized;
using System.Collections.Generic;
using HatCMS.Placeholders;

namespace HatCMS.Admin
{
    public class SearchAndReplace : BaseCmsAdminTool
    {
        public override CmsAdminToolInfo getToolInfo()
        {
            return new CmsAdminToolInfo(CmsAdminToolCategory.Tool_Search, AdminMenuTab.Tools, "Global Search &amp; Replace");
        }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-1.4.1.min.js"));
            return ret.ToArray();
        }

        #region SearchAndReplace
        public override string Render()
        {
            string searchForFormName = "searchfor";
            string replaceWithFormName = "replacewith";

            string searchFor = PageUtils.getFromForm(searchForFormName, "");
            string replaceWith = PageUtils.getFromForm(replaceWithFormName, "");

            //@@TODO: add an option to only replace text (not HTML); use this: http://gallery.msdn.microsoft.com/ScriptJunkie/en-us/jQuery-replaceText-String-ce62ea13

            // -- because javascript regular expressions are used for the replacement, some search expressions will have unintended consequences 
            //    note some of these are now escaped using EscapeJSRegex so can be used. "[", "]", "+", "*", "?", "$", "^",".",
            //     ref: http://www.w3schools.com/jsref/jsref_obj_regexp.asp
            string[] invalidSearchItems = new string[] { "{", "}", "\\w", "\\W", "\\d", "\\D", "\\s", "\\S", "\\b", "\\B", "\\0", "\\n", "\\f", "\\r", "\\t", "\\v", "\\x", "\\u" };
            string _errorMessage = "";
            if (searchFor.Trim() != "")
            {
                foreach (string s in invalidSearchItems)
                {
                    if (searchFor.IndexOf(s) > -1)
                    {
                        _errorMessage = "Error: searches can not contain \"" + s + "\".";
                    }
                } // foreach
            }

            if (searchFor.Trim() != "" && replaceWith.Trim() != "" && _errorMessage == "")
            {
                StringBuilder script = new StringBuilder();
                script.Append("var langCode = '" + CmsContext.currentLanguage.shortCode + "'; " + Environment.NewLine);
                script.Append("var buttonId = ''; " + Environment.NewLine);
                script.Append("function go(url, lcode, bId){" + Environment.NewLine);
                script.Append(" opener.location.href = url;" + Environment.NewLine);
                script.Append(" langCode = lcode;" + Environment.NewLine);
                script.Append(" buttonId = bId;" + Environment.NewLine);

                script.Append(" setTimeout(poll, 1000);" + Environment.NewLine);

                // http://plugins.jquery.com/project/popupready
                script.Append("function poll() {" + Environment.NewLine);
                script.Append("if (jQuery(\"body *\", opener.document).length == 0 ) {" + Environment.NewLine);
                script.Append(" setTimeout(poll, 1000);" + Environment.NewLine);
                script.Append("}" + Environment.NewLine);
                script.Append("else {" + Environment.NewLine);
                script.Append(" $(opener.document).focus(); " + Environment.NewLine);
                script.Append(" setTimeout(doReplace, 1000); " + Environment.NewLine);
                script.Append("}" + Environment.NewLine);
                script.Append("}// poll" + Environment.NewLine);


                script.Append("} // go" + Environment.NewLine);

                script.Append("function numOccurrences(txt){" + Environment.NewLine);
                script.Append(" var r = txt.match(new RegExp('" + EscapeJSRegex(searchFor) + "','gi')); " + Environment.NewLine);
                script.Append(" if (r) return r.length; " + Environment.NewLine);
                script.Append(" return 0;" + Environment.NewLine);
                script.Append("}" + Environment.NewLine);


                script.Append("function setMsg(bodyEl, startNum, endNum, el){" + Environment.NewLine);
                script.Append(" var pos = el.offset(); " + Environment.NewLine);
                script.Append(" var e = $( opener.document.createElement('div') ); var nm =''; var n =1; " + Environment.NewLine);
                script.Append(" if (endNum - startNum > 1){ nm = (startNum+1) + ' - '+(endNum); n = endNum - (startNum); } else { nm= (endNum); }" + Environment.NewLine);
                script.Append(" e.html(nm+\": Replaced '" + searchFor + "' with '" + replaceWith + "' \"+n+\" times \");" + Environment.NewLine);
                script.Append(" e.css({'padding': '5px','font-size':'8pt', 'display':'block','z-index':'5000', 'font-weight':'bold', 'position':'absolute', 'top': pos.top, 'left': pos.left, 'background-color':'yellow', 'border': '2px solid red'});" + Environment.NewLine);

                script.Append(" bodyEl.append(e);" + Environment.NewLine);
                // script.Append(" alert($(document).scrollTop());" + Environment.NewLine);
                // Note: there is a bug in JQuery.offset() function that uses the document's scrollTop, not the context's scrollTop
                //       bug report: http://dev.jquery.com/ticket/6539
                script.Append(" e.css({'top': pos.top - 20 -  $(document).scrollTop() });" + Environment.NewLine);
                script.Append("}" + Environment.NewLine);



                script.Append("function doReplace(){" + Environment.NewLine);
                script.Append(" var bodyEl = $('#lang_'+langCode, opener.document);" + Environment.NewLine);

                script.Append(" var numChanges = 0;" + Environment.NewLine);

                script.Append(" $('#lang_'+langCode+' input:text,#lang_'+langCode+' textarea', opener.document).each(function(){" + Environment.NewLine);

                script.Append("     var ths = $(this); " + Environment.NewLine);
                // script.Append("     alert(ths.val());" + Environment.NewLine);
                script.Append("     if (ths.is(':visible') &&  ths.val().trim() != '' && ths.val().search(new RegExp('" + EscapeJSRegex(searchFor) + "','gi')) > -1 ) {" + Environment.NewLine);
                script.Append("         var startNum = numChanges; " + Environment.NewLine);
                script.Append("         numChanges+= numOccurrences(ths.val());" + Environment.NewLine);
                script.Append("         setMsg(bodyEl, startNum, numChanges, ths);" + Environment.NewLine);
                script.Append("         var v = ths.val().replace(new RegExp('" + EscapeJSRegex(searchFor) + "','gi'), '" + replaceWith + "');" + Environment.NewLine);
                script.Append("         ths.val(v);" + Environment.NewLine);
                script.Append("     }// if visible" + Environment.NewLine);
                script.Append(" });" + Environment.NewLine);

                script.Append(" if(opener.CKEDITOR && (opener.CKEDITOR.status == 'basic_loaded' || opener.CKEDITOR.status == 'basic_ready' || opener.CKEDITOR.status == 'ready') ){ " + Environment.NewLine);
                script.Append("     for(var edName in opener.CKEDITOR.instances) { " + Environment.NewLine);
                script.Append("         if ($('#'+edName, opener.document).closest('#lang_'+langCode, opener.document).is(':visible')) {" + Environment.NewLine);
                script.Append("             var d = opener.CKEDITOR.instances[edName].getData();" + Environment.NewLine);
                script.Append("             var numD = numOccurrences(d); " + Environment.NewLine);
                script.Append("             if (numD > 0) {" + Environment.NewLine);
                script.Append("                 var d2 = d.replace(new RegExp('" + EscapeJSRegex(searchFor) + "','gi'), '" + replaceWith + "'); " + Environment.NewLine);
                script.Append("                 var nStart = numChanges; numChanges += numD;" + Environment.NewLine);
                script.Append("                 setMsg(bodyEl, nStart, numChanges, $('#'+opener.CKEDITOR.instances[edName].container.$.id, opener.document));" + Environment.NewLine);
                script.Append("                 opener.CKEDITOR.instances[edName].setData(d2);" + Environment.NewLine);
                script.Append("             } // if " + Environment.NewLine);
                script.Append("         } // if " + Environment.NewLine);
                // script.Append("         alert('parent: '+$('#'+edName, opener.document).closest('#lang_en', opener.document).is(':visible'));" + Environment.NewLine);
                // window.CKEDITOR.instances['htmlcontent_1_1_en'].getData()
                script.Append("     }// for each editor" + Environment.NewLine);
                script.Append(" }" + Environment.NewLine);


                script.Append("$('#'+buttonId).val(numChanges+' replacements made');" + Environment.NewLine);
                script.Append(" alert('The text on this page has been updated ('+numChanges+' replacements made).\\nPlease save the page to continue.');" + Environment.NewLine);
                script.Append("}" + Environment.NewLine);
                // another $(opener.document).ready way: http://plugins.jquery.com/taxonomy/term/1219

                CmsContext.currentPage.HeadSection.AddJSStatements(script.ToString());

                CmsContext.currentPage.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");
            }

            StringBuilder html = new StringBuilder();
            html.Append(CmsContext.currentPage.getFormStartHtml("SearchReplaceForm"));
            html.Append("<table>");
            html.Append("<tr>");
            html.Append("<td>Search for:</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(searchForFormName, searchForFormName, searchFor, 30, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Replace with:</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(replaceWithFormName, replaceWithFormName, replaceWith, 30, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td colspan=\"2\"><input type=\"submit\" value=\"Search in all pages\" /> (no replacement of page contents will be done)</td>");
            html.Append("</tr>");

            html.Append("</table>");
            html.Append("<em>Warning! This searches and replaces the raw HTML for a page, so be careful what you search for! The search is not case-sensitive, and the replacement is done exactly.</em>");

            if (_errorMessage != "")
            {
                html.Append("<p style=\"color: red; font-weight: bold;\">" + _errorMessage + "</p>");
            }

            html.Append(PageUtils.getHiddenInputHtml("AdminTool", this.GetType().Name ));            

            html.Append(CmsContext.currentPage.getFormCloseHtml("SearchReplaceForm"));
            // -- do the search
            if (searchFor.Trim() != "" && replaceWith.Trim() != "" && _errorMessage == "")
            {
                html.Append("<table border=\"1\">");
                PageSearchResults[] searchResults = getAllPagesContainingText(searchFor);
                if (searchResults.Length == 0)
                {
                    html.Append("<p><strong>No pages were found that contained \"" + searchFor + "\"</strong></p>");
                }
                else
                {
                    int i = 1;
                    foreach (PageSearchResults searchResult in searchResults)
                    {
                        CmsPage p = searchResult.Page;

                        html.Append("<tr>");
                        html.Append("<td>" + p.getTitle(searchResult.Language) + " (" + searchResult.Language.shortCode + ")</td>");

                        NameValueCollection paramList = new NameValueCollection();
                        paramList.Add("target", p.ID.ToString());
                        string openEditUrl = CmsContext.getUrlByPagePath(CmsConfig.getConfigValue("GotoEditModePath", "/_admin/action/gotoEdit"), paramList, searchResult.Language);

                        string buttonId = "searchReplaceb_" + i.ToString();
                        i++;

                        html.Append("<td>");
                        html.Append("<input id=\"" + buttonId + "\" type=\"button\" onclick=\"go('" + openEditUrl + "', '" + searchResult.Language.shortCode + "','" + buttonId + "'); return false;\" value=\"open page &amp; replace\">");

                        html.Append("</td>");

                        html.Append("</tr>");
                    } // foreach page Id
                }
                html.Append("</table>");
            }
            return html.ToString();
        }

        /// <summary>
        /// Escape characters in a search string so that they can be used in a RexExp object as plain text (not their special meta-character values)
        /// </summary>
        /// <param name="searchFor"></param>
        /// <returns></returns>
        private string EscapeJSRegex(string searchFor)
        {
            string doubleSlash = @"\\";

            // "+", "*", "?", "$", "^"
            StringBuilder ret = new StringBuilder(searchFor);

            ret.Replace("[", doubleSlash + "[");
            ret.Replace("]", doubleSlash + "]");
            ret.Replace(".", doubleSlash + ".");
            ret.Replace("?", doubleSlash + "?");
            ret.Replace("$", doubleSlash + "$");
            ret.Replace("^", doubleSlash + "^");
            ret.Replace("+", doubleSlash + "+");
            ret.Replace("*", doubleSlash + "*");


            return ret.ToString();
        }

        private class PageSearchResults
        {
            public CmsLanguage Language;
            public CmsPage Page;

            public PageSearchResults(CmsLanguage lang, CmsPage page)
            {
                Language = lang;
                Page = page;
            }
        }


        private PageSearchResults[] getAllPagesContainingText(string searchFor)
        {
            List<PageSearchResults> ret = new List<PageSearchResults>();
            Dictionary<int, CmsPage> allPages = CmsContext.HomePage.getLinearizedPages();
            foreach (int pageId in allPages.Keys)
            {
                CmsPage p = allPages[pageId];

                CmsPlaceholderDefinition[] phDefs = p.getAllPlaceholderDefinitions();
                foreach (CmsLanguage lang in CmsConfig.Languages)
                {
                    bool foundInPage = false;
                    foreach (CmsPlaceholderDefinition phDef in phDefs)
                    {
                        if (foundInPage)
                            break;

                        string phVal = PlaceholderUtils.renderPlaceholderToString(p, lang, phDef);
                        if (phVal.IndexOf(searchFor, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            foundInPage = true;
                            break;
                        }

                    } // foreach placeholder type

                    if (foundInPage)
                    {
                        ret.Add(new PageSearchResults(lang, p));
                    }
                } // foreach language

            } // foreach page

            return ret.ToArray();
        }

        #endregion
    }
}
