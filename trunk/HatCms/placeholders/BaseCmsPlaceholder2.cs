using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace HatCMS.Placeholders
{
    /*
    public abstract class BaseCmsPlaceholder2
    {        

        public abstract CmsDependency[] getDependencies();

        // removed
        // public abstract string getPlaceholderValue(CmsPage page, int identifier, AppLanguage language);

        /// <summary>
        /// Tests to see if the specified links are on this page. The links parameter is modified to contain only the links actually found on the page.
        /// Returns TRUE if the links parameter was modified, otherwise returns FALSE
        /// </summary>
        /// <param name="links"></param>
        /// <returns>TRUE if the links parameter was modified, otherwise returns FALSE</returns>
        public abstract bool contentContainsLinks(CmsPage page, int[] identifiers, AppLanguage[] languages, ref string[] links);

        
        public abstract CmsAlternateView[] getAlternateViews(CmsPlaceholderParams placeholderParams);

        public abstract bool revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, AppLanguage language);

        // removed
        // public abstract void Render(HtmlTextWriter writer, CmsPage page, int identifier, AppLanguage langToRenderFor, string[] paramList);

        public abstract void RenderInViewMode(HtmlTextWriter writer, CmsPlaceholderParams placeholderParams);

        public abstract void RenderInEditMode(HtmlTextWriter writer, CmsPlaceholderParams placeholderParams);

        public abstract CmsValidateAndSaveResult ValidateAndSaveSubmittedEditModeValues(CmsPlaceholderParams placeholderParams);

        public abstract CmsValidateAndSaveResult ValidateAndSaveSubmittedViewModeValues(CmsPlaceholderParams placeholderParams);
                
      
    }

    /// <summary>
    /// The SimpleCmsPlaceholder type should be used for all standard Placeholders. These placeholders
    /// do not allow data to be submitted from non-logged-in users, and do not have 
    /// </summary>
    public abstract class SimpleCmsPlaceholder : BaseCmsPlaceholder2
    {        
        public override CmsAlternateView[] getAlternateViews(CmsPage page, int[] identifiers, AppLanguage pageLanguage)
        {
            return new CmsAlternateView[0];
        }

        public override CmsValidateAndSaveResult ValidateSubmittedViewModeValues(CmsPlaceholderParams placeholderParams)
        {
            return new CmsValidateAndSaveResult(true, new string[0]);
        }

    }

    /// <summary>
    /// The UserSubmittedDataCmsPlaceholder type should be used by placeholders that accept user submitted data (ie data that is submitted by users that are potentially not logged-in)
    /// </summary>
    public abstract class UserSubmittedDataCmsPlaceholder : BaseCmsPlaceholder2
    {        
        
    }

    
    public abstract class MasterDetailsUserSubmittedDataCmsPlaceholder : BaseCmsPlaceholder2
    {
        public override void RenderInViewMode(HtmlTextWriter writer, CmsPlaceholderParams placeholderParams) 
        { 
            PlaceholderDisplay display = getCurrentDisplayMode();
            if (display == PlaceholderDisplay.SelectedItem)
                return RenderDetailsInViewMode(writer, placeholderParams);
            else
                return RenderMasterInViewMode(writer, placeholderParams);
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPlaceholderParams placeholderParams) 
        {
            PlaceholderDisplay display = getCurrentDisplayMode();
            if (display == PlaceholderDisplay.SelectedItem)
                return RenderDetailsInEditMode(writer, placeholderParams);
            else
                return RenderMasterInEditMode(writer, placeholderParams);
        }

        public override CmsValidateAndSaveResult ValidateAndSaveSubmittedEditModeValues(CmsPlaceholderParams placeholderParams) 
        { 
        }

        public override CmsValidateAndSaveResult ValidateAndSaveSubmittedViewModeValues(CmsPlaceholderParams placeholderParams) 
        { 
        }

        public enum PlaceholderDisplay { MasterDisplay, SelectedItem };
        public abstract PlaceholderDisplay getCurrentDisplayMode();

        public abstract void RenderMasterInViewMode(HtmlTextWriter writer, CmsPlaceholderParams placeholderParams);

        public abstract void RenderDetailsInViewMode(HtmlTextWriter writer, CmsAlternateView detailsViewToRender);

        public abstract CmsValidateAndSaveResult ValidateAndSaveSubmittedMasterEditModeValues(CmsPlaceholderParams placeholderParams);

        public abstract CmsValidateAndSaveResult ValidateAndSaveSubmittedDetailsEditModeValues(CmsAlternateView currentView);

        public abstract CmsValidateAndSaveResult ValidateAndSaveSubmittedMasterViewModeValues(CmsPlaceholderParams placeholderParams);

        public abstract CmsValidateAndSaveResult ValidateAndSaveSubmittedDetailsViewModeValues(CmsAlternateView currentView);

    }


    public class CmsPlaceholderParams
    {
        public CmsPage Page;
        public int Identifier;
        public AppLanguage Language;
        public string[] TemplateParameters;

        public CmsPlaceholderParams(CmsPage page, int identifier, AppLanguage lang, string[] paramList)
        {
            Page = page;
            Identifier = identifier;
            Language = lang;
            TemplateParameters = paramList;
        }

        public string getPlaceholderFormNameBase(BaseCmsPlaceholder2 placeholderInstance)
        {
            return placeholderInstance.GetType().Name + Page.ID.ToString() + Identifier.ToString()+Language.shortCode;
        }


    }

    public class CmsValidateAndSaveResult
    {
        public bool CanContinue = false;
        public List<string> ErrorMessages = new List<string>();

        public CmsValidateAndSaveResult()
        {
            CanContinuePastValidation = false;
            ErrorMessages = new List<string>();
        }

        public CmsValidateAndSaveResult(bool canContinue, string[] errorMessages)
        {
            CanContinue = canContinue;
            ErrorMessages.AddRange(errorMessages);
        }
    }
*/
}
