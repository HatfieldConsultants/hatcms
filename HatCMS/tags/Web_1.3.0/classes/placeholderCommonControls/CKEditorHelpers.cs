using System;
using System.Data;
using System.Configuration;
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
    /// A helper class that assists controls and placeholders to use the CKEditor in a consistent manner.
    /// </summary>
    public class CKEditorHelpers
    {
        public static CmsDependency[] CKEditorDependencies
        {
            get
            {
                return new CmsDependency[] {
                CmsFileDependency.UnderAppPath("js/_system/ckeditor/ckeditor.js", new DateTime(2011,5,9)),
                    CmsFileDependency.UnderAppPath("js/_system/ckeditor/config.js", new DateTime(2010,4,29)),
                    CmsFileDependency.UnderAppPath("js/_system/ckeditor/styles.js"),
                    CmsFileDependency.UnderAppPath("js/_system/ckeditor/contents.css"),                    

                    // -- ckeditor plugin enhancements
                    CmsFileDependency.UnderAppPath("js/_system/ckeditor/plugins/hatCms/plugin.js", new DateTime(2011,03,02)),
                    CmsFileDependency.UnderAppPath("js/_system/ckeditor/plugins/hatCms_styles/plugin.js", new DateTime(2011,6,3)),
                    CmsFileDependency.UnderAppPath("_system/tools/FCKHelpers/DeleteResourcePopup.aspx"),
                    CmsFileDependency.UnderAppPath("_system/tools/FCKHelpers/InlineImageBrowser2.aspx"),
                    CmsFileDependency.UnderAppPath("_system/tools/ckhelpers/dhtmlxFiles_xml.ashx"),
                    CmsFileDependency.UnderAppPath("_system/tools/ckhelpers/dhtmlxPages_xml.ashx"),
                    CmsFileDependency.UnderAppPath("_system/tools/ckhelpers/InlineFileBrowser.aspx", new DateTime(2011,3,21)),
                    CmsFileDependency.UnderAppPath("_system/tools/ckhelpers/InlinePageBrowser.aspx", new DateTime(2011,3,21)),

                    //-- dhtmlxTree dependencies
                    CmsFileDependency.UnderAppPath("js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxtree.css"),
                    CmsFileDependency.UnderAppPath("js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxcommon.js"),
                    CmsFileDependency.UnderAppPath("js/_system/dhtmlx/dhtmlxTree/codebase/dhtmlxtree.js"),
                    CmsFileDependency.UnderAppPath("js/_system/dhtmlx/dhtmlxTree/codebase/imgs/folderClosed.gif"),

                    // -- FCKEditor has been removed. Make sure it is gone.
                    CmsDirectoryDoesNotExistDependency.UnderAppPath("js/_system/FCKeditor")                    
                };
            } 
        } // CKEditorDependencies

        public static void AddPageJavascriptStatements(CmsPage page, string editorId, string renderWidth, string renderHeight, CmsLanguage language)
        {
            page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "js/_system/ckeditor/ckeditor.js");

            string lang = "";
            if (language.isValidLanguage && CmsConfig.Languages.Length > 1)
                lang = "language: \"" + language.shortCode.ToLower() + "\", ";

            page.HeadSection.AddJSOnReady("CKEDITOR.replace( '" + editorId + "', {width:\"" + renderWidth + "\", height:\"" + renderHeight + "\", " + lang + "stylesCombo_stylesSet: 'site_styles:" + PageUtils.ApplicationPath + "js/_system/ckeditor/styles.js', stylesSet: 'site_styles:" + PageUtils.ApplicationPath + "js/_system/ckeditor/styles.js', scayt_autoStartup: false} );");
        }


    }
}
