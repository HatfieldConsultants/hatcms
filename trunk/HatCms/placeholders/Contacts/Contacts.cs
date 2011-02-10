using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class Contacts : BaseCmsPlaceholder
    {

        public enum PlaceholderDisplayMode { SingleContact, MultipleContacts }

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency("contacts", 
                new string[] { "ContactsId", "PageId", "Identifier", 
                    "numColumnsToShow", "nameDisplayMode", "forceFilterToCategoryId", 
                    "allowFilterByCategory", "allowFilterByCompany", 
                    "accessLevelToEditContacts", "accessLevelToAddContacts", "deleted" }));
            ret.Add(new CmsDatabaseTableDependency("contactdata"));
            ret.Add(new CmsDatabaseTableDependency("contactdatacategory"));
            ret.Add(new CmsDatabaseTableDependency("contactlinktocategory"));

            ret.Add(CmsFileDependency.UnderAppPath("js/_system/jquery/jquery-1.4.1.min.js"));
            return ret.ToArray();
        }


        public override RevertToRevisionResult revertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            if (CmsConfig.Languages.Length > 1)
                throw new Exception("Error: Contacts placeholder can not be used in multilingual sites yet!");
            PlaceholderDisplayMode mode = currentViewRenderMode;
            switch (mode)
            {
                case PlaceholderDisplayMode.MultipleContacts:
                    RenderViewSummary(writer, page, identifier, langToRenderFor, paramList);
                    break;
                case PlaceholderDisplayMode.SingleContact:
                    RenderViewIndividual(writer, page, identifier, langToRenderFor, paramList);
                    break;
                default:
                    throw new ArgumentException("invalid PlaceholderDisplayMode");
            }
        }

        public static PlaceholderDisplayMode currentViewRenderMode
        {
            get
            {
                int currentId = PageUtils.getFromForm(CurrentContactIdFormName, -1);
                if (currentId >= 0)
                    return PlaceholderDisplayMode.SingleContact;

                return PlaceholderDisplayMode.MultipleContacts;
            }
        }

        public static bool isContactsPage(CmsPage page)
        {
            return (StringUtils.IndexOf(page.getAllPlaceholderNames(), "Contacts", StringComparison.CurrentCultureIgnoreCase) > -1);
        }

        /// <summary>
        /// if not found, returns a new ContactData object with ID = -1.
        /// </summary>
        /// <returns></returns>
        public static ContactData getCurrentContactData()
        {
            int currentId = PageUtils.getFromForm(CurrentContactIdFormName, -1);
            if (currentId > -1)
            {
                return ContactData.getContact(currentId);                
            }
            return new ContactData();
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string ControlId = "Contacts_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode;

            ContactsDb db = new ContactsDb();
            ContactPlaceholderData data = new ContactPlaceholderData();
            data = db.getContactPlaceholderData(page, identifier, true);

            string action = PageUtils.getFromForm(ControlId + "_action", "");
            if (String.Compare(action, "saveNewValues", true) == 0)
            {                
                data.numColumnsToShow = PageUtils.getFromForm(ControlId + "numColumnsToShow", data.numColumnsToShow);
                data.forceFilterToCategoryId = PageUtils.getFromForm(ControlId + "forceFilterToCategoryId", data.forceFilterToCategoryId);
                data.nameDisplayMode = (ContactPlaceholderData.ContactNameDisplayMode)PageUtils.getFromForm(ControlId + "nameDisplayMode", typeof(ContactPlaceholderData.ContactNameDisplayMode), data.nameDisplayMode);
                
                data.allowFilterByCategory = PageUtils.getFromForm(ControlId + "allowFilterByCategory", false);
                data.allowFilterByCompany = PageUtils.getFromForm(ControlId + "allowFilterByCompany", false);

                data.accessLevelToAddContacts = (BaseCmsPlaceholder.AccessLevel)PageUtils.getFromForm(ControlId + "accessLevelToAddContacts", typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToAddContacts);
                data.accessLevelToEditContacts = (BaseCmsPlaceholder.AccessLevel)PageUtils.getFromForm(ControlId + "accessLevelToEditContacts", typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToEditContacts);
                db.saveUpdatedContactPlaceholderData(page, identifier, data);
            }


            StringBuilder html = new StringBuilder();

            html.Append("Contacts Display Configuration:");

            
            html.Append("<table>");
            
            html.Append("<tr>");
            html.Append("<td>Force Category filter to display: </td>");
            html.Append("<td>");
            ContactDataCategory[] allCats = ContactDataCategory.getAllContactCategories();
            NameValueCollection options = new NameValueCollection();
            options.Add("-1", "do not force category to filter");
            foreach (ContactDataCategory cat in allCats)
            {
                options.Add(cat.CategoryId.ToString(), cat.Title);
            } // foreach
            html.Append(PageUtils.getDropDownHtml(ControlId + "forceFilterToCategoryId", ControlId + "forceFilterToCategoryId",options, data.forceFilterToCategoryId.ToString()));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Allow filter by: </td>");
            html.Append("<td>");            
            html.Append(PageUtils.getCheckboxHtml("Category", ControlId + "allowFilterByCategory", ControlId + "allowFilterByCategory", true.ToString(), data.allowFilterByCompany));
            html.Append("<br>");
            html.Append(PageUtils.getCheckboxHtml("Company Name", ControlId + "allowFilterByCompany", ControlId + "allowFilterByCompany", true.ToString(), data.allowFilterByCompany));
            html.Append("</td>");
            html.Append("</tr>");


            html.Append("<tr>");
            html.Append("<td>Number of columns: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getInputTextHtml(ControlId + "numColumnsToShow", ControlId + "numColumnsToShow", data.numColumnsToShow.ToString(), 3, 5));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Name display format: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getDropDownHtml(ControlId + "nameDisplayMode", ControlId + "nameDisplayMode", Enum.GetNames(typeof(ContactPlaceholderData.ContactNameDisplayMode)), Enum.GetName(typeof(ContactPlaceholderData.ContactNameDisplayMode), data.nameDisplayMode)));
            html.Append("</td>");
            html.Append("</tr>");


            html.Append("<tr>");
            html.Append("<td>Access needed to add contacts: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getDropDownHtml(ControlId + "accessLevelToAddContacts", ControlId + "accessLevelToAddContacts", Enum.GetNames(typeof(BaseCmsPlaceholder.AccessLevel)), Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToAddContacts)));
            html.Append("</td>");
            html.Append("</tr>");


            html.Append("<tr>");
            html.Append("<td>Access needed to edit contacts: </td>");
            html.Append("<td>");
            html.Append(PageUtils.getDropDownHtml(ControlId + "accessLevelToEditContacts", ControlId + "accessLevelToEditContacts", Enum.GetNames(typeof(BaseCmsPlaceholder.AccessLevel)), Enum.GetName(typeof(BaseCmsPlaceholder.AccessLevel), data.accessLevelToEditContacts)));
            html.Append("</td>");
            html.Append("</tr>");

            html.Append("</table>");

            html.Append(PageUtils.getHiddenInputHtml(ControlId + "_action", "saveNewValues"));

            writer.Write(html.ToString());
        } // RenderEditSummary

        public int[] getCategoryIdsToDisplay(ContactPlaceholderData data)
        {
            if (data.forceFilterToCategoryId >= 0)
                return new int[] { data.forceFilterToCategoryId };

            string[] ss = PageUtils.getFromForm("contactCat");
            return StringUtils.ToIntArray(ss);
        }

        public string[] getOrgNamesToDisplay()
        {
            return PageUtils.getFromForm("contactOrg");            
        }

        public void RenderViewSummary(HtmlTextWriter writer, CmsPage page, int identifier,CmsLanguage langToRenderFor, string[] paramList)
        {
            string ControlId = "Contacts_" + page.ID.ToString() + "_" + identifier.ToString()+langToRenderFor.shortCode;

            ContactsDb db = new ContactsDb();
            ContactPlaceholderData data = db.getContactPlaceholderData(page, identifier, true);

            // -- before getContacts, process the form
            string addFormHtml = "";
            if (currentUserCanAddContact(data))
            {
                addFormHtml = getAddEditContactForm(data, new ContactData(), page, identifier, langToRenderFor);
            }

            int[] categoryIdsToDisplay = getCategoryIdsToDisplay(data);
            string[] orgsToDisplay = getOrgNamesToDisplay();
            ContactData[] contacts = ContactData.getContacts(data, categoryIdsToDisplay, orgsToDisplay);            

            StringBuilder html = new StringBuilder();
            html.Append("<table width=\"100%\"><tr>");
            html.Append("<td valign=\"top\">");
            html.Append(getSummaryDisplay(data, contacts, langToRenderFor, page));
            html.Append("</td>");
            html.Append("<td valign=\"top\">");
            if (contacts.Length > 1)
            {
                html.Append(getSummaryDisplayFilterForm(data, contacts, page, identifier));
            }
            html.Append("</td>");
            html.Append("</tr>");
            html.Append("</table>");
            if (currentUserCanAddContact(data))
            {
                html.Append("<p>" + addFormHtml + "</p>");
            }

            writer.Write(html.ToString());
        } // RenderViewSummary

        private string getSummaryDisplay(ContactPlaceholderData data, ContactData[] contacts,CmsLanguage langToRenderFor, CmsPage page)
        {
            StringBuilder html = new StringBuilder();

            if (contacts.Length < 1)
            {
                html.Append("<p><em>there are no contacts to display</em></p>");
            }
            else
            {
                html.Append("<table class=\"ContactsSummaryList\"><tr>");

                int numContactsPerCol = Convert.ToInt32(Math.Ceiling((double)contacts.Length / (double)data.numColumnsToShow));
                if (numContactsPerCol == 1 && (contacts.Length > data.numColumnsToShow))
                    numContactsPerCol++;
                if (numContactsPerCol < 1)
                    numContactsPerCol = contacts.Length;

                bool internalTableStarted = false;
                for (int contactCount = 0; contactCount < contacts.Length; contactCount++)
                {
                    if ((contactCount % numContactsPerCol) == 0)
                    {
                        if (internalTableStarted)
                            html.Append("</table></td>");
                        html.Append("<td valign=\"top\"><table>");
                        internalTableStarted = true;
                    }

                    ContactData contact = contacts[contactCount];
                    string detailsUrl = getContactDetailsDisplayUrl(contact, page);
                    html.Append("<tr><td class=\"ContactInformationDisplay\">");
                    html.Append("<strong><a href=\"" + detailsUrl + "\">" + getNameDisplayOutput(data, contact) + "</a></strong>");
                    string details = getContactDetailsSummaryDisplay(contact);
                    if (details != "")
                        html.Append("<br />" + details);
                    html.Append("</td></tr>");
                } // foreach
                if (internalTableStarted)
                    html.Append("</table></td>");

                html.Append("</tr></table>");
            }

            return html.ToString();
        }

        public string getSummaryDisplayFilterForm(ContactPlaceholderData data, ContactData[] contacts, CmsPage page, int identifier )
        {
            if (!data.allowFilterByCategory && !data.allowFilterByCompany)
                return "";

            string ControlId = "Contacts_" + page.ID.ToString() + "_" + identifier.ToString();

            string[] allOrganizations = ContactData.getAllOrganizationNames(contacts);
            ContactDataCategory[] allCategories = ContactDataCategory.getAllContactCategories();
            if (allOrganizations.Length < 1 && allCategories.Length < 1)
                return "";

            StringBuilder html = new StringBuilder();
            int cbid = 0;

            html.Append("<div class=\"ContactsFilterForm\">");
            string formId = ControlId+"filterContacts";
            html.Append(page.getFormStartHtml(formId));
            
            html.Append("<strong>Filter contacts</strong><br />");
            if (data.allowFilterByCategory)
            {
                if (allCategories.Length > 1)
                {
                    html.Append(" <em> by category:</em><br />");

                    int[] catsChecked = getCategoryIdsToDisplay(data);
                    
                    foreach (ContactDataCategory cat in allCategories)
                    {
                        bool check = (catsChecked.Length == 0 || (Array.IndexOf(catsChecked, cat.CategoryId) > -1));
                        string displayUrl = getContactSummaryDisplayUrl(page, new int[] { cat.CategoryId }, new string[0]);
                        string display = cat.Title;
                        // string link = " <a title=\"view only contacts in '" + cat.Title + "'\" href=\"" + displayUrl + "\">(view)</a>";
                        string cb = PageUtils.getCheckboxHtml(display, "contactCat", ControlId + "category" + cbid.ToString(), cat.CategoryId.ToString(), check);
                        html.Append(cb + "<br />");
                        cbid++;
                    } // foreach
                }
            }

            if (data.allowFilterByCompany)
            {
                if (allOrganizations.Length > 1)
                {
                    html.Append(" <em> by organization:</em><br />");

                    string[] orgsChecked = getOrgNamesToDisplay();
                    
                    foreach (string org in allOrganizations)
                    {
                        bool check = (orgsChecked.Length == 0 || (Array.IndexOf(orgsChecked, org) > -1));
                        string displayUrl = getContactSummaryDisplayUrl(page, new int[0], new string[] { org });
                        string display = org;
                        // string link = " <a title=\"view only contacts belonging to '" + org + "'\" href=\"" + displayUrl + "\">(view)</a>";

                        string cb = PageUtils.getCheckboxHtml(display, "contactOrg", ControlId + "orgName" + cbid.ToString(), org, check);
                        html.Append(cb + "<br />");
                        cbid++;
                    } // foreach
                }
            }
            if (cbid > 0)
            {
                html.Append("<input type=\"submit\" value=\"filter\">");

                string checkAllFnName = ControlId + "Check";
                string checkNoneFnName = ControlId + "CheckNone";

                StringBuilder js = new StringBuilder();
                js.Append("function " + checkAllFnName + "() {" + Environment.NewLine);
                js.Append(" $('#" + formId + " input[type=checkbox]:not(:checked)' ).attr('checked', true);" + Environment.NewLine);
                js.Append("} " + Environment.NewLine);

                js.Append("function " + checkNoneFnName + "() {" + Environment.NewLine);
                js.Append(" $('#" + formId + " input[type=checkbox]:checked' ).attr('checked', false);" + Environment.NewLine);
                js.Append("} " + Environment.NewLine);

                page.HeadSection.AddJSStatements(js.ToString());
                page.HeadSection.AddJavascriptFile("js/_system/jquery/jquery-1.4.1.min.js");

                html.Append("<br>Check: <a href=\"#\" onclick=\"" + checkAllFnName + "(); return false;\">all</a> | <a href=\"#\" onclick=\"" + checkNoneFnName + "(); return false;\">none</a>");

            }
            html.Append(page.getFormCloseHtml(formId));
            html.Append("</div>");
            return html.ToString();
        }


        public static string CurrentContactIdFormName
        {
            get
            {
                return "contact";
            }
        }

        private string getContactSummaryDisplayUrl(CmsPage page, int[] categoriesToDisplay, string[] orgNames)
        {
            NameValueCollection pageParams = new NameValueCollection();
            if (categoriesToDisplay.Length > 0)
                pageParams.Add("contactCat", StringUtils.Join(",", categoriesToDisplay));

            if (orgNames.Length > 0)
                pageParams.Add("contactOrg", StringUtils.JoinNonBlanks(",", orgNames));

            string url = CmsContext.getUrlByPagePath(page.Path, pageParams);
            return url;
        }

        private string getContactDetailsDisplayUrl(ContactData contactData, CmsPage page)
        {
            NameValueCollection pageParams = new NameValueCollection();
            pageParams.Add(CurrentContactIdFormName, contactData.contactId.ToString());

            string url = CmsContext.getUrlByPagePath(page.Path, pageParams);
            return url;
        }

        private string getNameDisplayOutput(ContactPlaceholderData data, ContactData contact)
        {
            string ret = "";
            switch (data.nameDisplayMode)
            {
                case ContactPlaceholderData.ContactNameDisplayMode.FirstnameLastname:
                    ret = contact.firstName + " " + contact.lastName;
                    break;
                case ContactPlaceholderData.ContactNameDisplayMode.LastnameFirstname:
                    ret =  contact.lastName + ", " + contact.firstName;
                    break;
                default:
                    throw new ArgumentException("invalid ContactNameDisplayMode");
            }
            return ret;
        }

        private string getContactDetailsSummaryDisplay(ContactData contact)
        {
            List<string> lines = new List<string>();
            if (contact.title.Trim() != "")
                lines.Add(contact.title);

            if (contact.organizationName.Trim() != "")
                lines.Add(contact.organizationName);
            
            if (contact.address1.Trim() != "")
                lines.Add(contact.address1);
            if (contact.address2.Trim() != "")
                lines.Add(contact.address2);

            string cityLine = "";
            if (contact.city.Trim() != "")
                cityLine = contact.city + ", ";
            if (contact.provinceState.Trim() != "")
                cityLine += contact.provinceState + " ";
            if (contact.postalZipCode.Trim() != "")
                cityLine += contact.postalZipCode;
            if (cityLine.Trim() != "")
                lines.Add(cityLine);

            if (contact.phoneNumber1.Trim() != "")
                lines.Add("Phone: "+contact.phoneNumber1);
            if (contact.phoneNumber2.Trim() != "")
                lines.Add("Phone: "+contact.phoneNumber2);
            if (contact.mobileNumber.Trim() != "")
                lines.Add("Mobile: "+contact.mobileNumber);
            if (contact.faxNumber.Trim() != "")
                lines.Add("Fax: "+contact.faxNumber);
            if (contact.emailAddress.Trim() != "")
                lines.Add("E-mail: <a href=\"mailto:" + contact.SpamEncodedEmailAddress + "\">" + contact.SpamEncodedEmailAddress + "</a>");

            string ret = String.Join("<br />", lines.ToArray());
            return ret;
        }

        private bool currentUserCanEditContact(ContactPlaceholderData data, ContactData contactData)
        {
            if (CmsContext.currentUserIsSuperAdmin) // SuperAdmin can always edit
                return true;
            
            // file creator can always edit
            /* -- contacts don't have a creator!
            if (eventData.CreatedBy != "" && CmsContext.currentWebPortalUser != null && String.Compare(CmsContext.currentWebPortalUser.UserName, eventData.CreatedBy, true) == 0)
                return true;
            */
            bool allowEdit = false;
            switch (data.accessLevelToEditContacts)
            {
                case BaseCmsPlaceholder.AccessLevel.Anonymous:
                    allowEdit = true;
                    break;
                case BaseCmsPlaceholder.AccessLevel.CmsAuthor:
                    if (CmsContext.currentUserCanAuthor)
                        allowEdit = true;
                    break;
                case BaseCmsPlaceholder.AccessLevel.LoggedInUser:
                    if (CmsContext.currentUserIsLoggedIn)
                        allowEdit = true;
                    break;
                default:
                    throw new ArgumentException("invalid PageFilesData.AccessLevel");
            }

            return allowEdit;
        }

        private bool currentUserCanAddContact(ContactPlaceholderData data)
        {
            if (CmsContext.currentUserIsSuperAdmin) // SuperAdmin can always upload
                return true;

            bool allowUpload = false;
            switch (data.accessLevelToAddContacts)
            {
                case BaseCmsPlaceholder.AccessLevel.Anonymous:
                    allowUpload = true;
                    break;
                case BaseCmsPlaceholder.AccessLevel.CmsAuthor:
                    if (CmsContext.currentUserCanAuthor)
                        allowUpload = true;
                    break;
                case BaseCmsPlaceholder.AccessLevel.LoggedInUser:
                    if (CmsContext.currentUserIsLoggedIn)
                        allowUpload = true;
                    break;
                default:
                    throw new ArgumentException("invalid PageFilesData.AccessLevel");
            }

            return allowUpload;
        }

        private void RenderViewIndividual(HtmlTextWriter writer, CmsPage page, int identifier,CmsLanguage langToRenderFor, string[] paramList)
        {
            ContactsDb db = new ContactsDb();
            ContactPlaceholderData data = db.getContactPlaceholderData(page, identifier, true);

            int contactId = PageUtils.getFromForm(CurrentContactIdFormName, -1);

            ContactData contactToView = ContactData.getContact(contactId);

            bool canEdit = currentUserCanEditContact(data, contactToView);

            string backUrl = page.Url;
            writer.Write("<p><a href=\"" + backUrl + "\">&#171; back to contact listing</a></p>");
            // -- begin output
            if (canEdit)
            {
                writer.Write(getAddEditContactForm(data, contactToView, page, identifier, langToRenderFor));
            }
            else
            {
                StringBuilder html = new StringBuilder();
                html.Append("<table border=\"0\">");

                string dividerLineHtml = getDividerLineHtml(2);

                html.Append(dividerLineHtml);

                html.Append("<tr>");                
                html.Append("<td colspan=\"2\" align=\"center\"><h2>" + StringUtils.JoinNonBlanks(" ", new string[] {contactToView.firstName, contactToView.lastName}) + "</h2></td>");

                html.Append(dividerLineHtml);

                string colspan = "1";

                html.Append("<td>Categories:</td>");
                ContactDataCategory[] allCategories = ContactDataCategory.getAllContactCategories();
                html.Append("<td colspan=\"" + colspan + "\">");
                int cbid = 0;
                foreach (ContactDataCategory cat in allCategories)
                {
                    bool check = (contactToView.contactCategoryIds.IndexOf(cat.CategoryId) > -1);
                    string cb = PageUtils.getCheckboxHtml(cat.Title, "category", "category" + cbid.ToString(), cat.CategoryId.ToString(), check);
                    html.Append(cb + "<br />");
                    cbid++;
                } // foreach
                html.Append("</td>");

                html.Append(dividerLineHtml);

                html.Append("<tr>");
                html.Append("<td>Title:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.title + "</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td>Organization:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.organizationName + "</td>");
                html.Append("</tr>");

                html.Append(dividerLineHtml);

                html.Append("<tr>");
                html.Append("<td>Address 1:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.address1 + "</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td>Address 2:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.address2 + "</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td>City:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.city + "</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td>Province/State:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.provinceState + "</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td>Postal/Zip Code:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.postalZipCode + "</td>");
                html.Append("</tr>");

                html.Append(dividerLineHtml);

                html.Append("<tr>");
                html.Append("<td>Phone Number 1:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.phoneNumber1 + "</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td>Phone Number 2:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.phoneNumber2 + "</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td>Fax Number:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.faxNumber + "</td>");
                html.Append("</tr>");

                html.Append("<tr>");
                html.Append("<td>Mobile Number:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + contactToView.mobileNumber + "</td>");
                html.Append("</tr>");

                html.Append(dividerLineHtml);

                string emailDisplay = "";
                if (contactToView.emailAddress.Trim() != "")
                    emailDisplay = "<a href=\"mailto:" + contactToView.SpamEncodedEmailAddress + "\">" + contactToView.SpamEncodedEmailAddress + "</a>";

                html.Append("<tr>");
                html.Append("<td>Email Address:</td>");
                html.Append("<td colspan=\"" + colspan + "\">" + emailDisplay + "</td>");
                html.Append("</tr>");

                html.Append("</table>");

                writer.Write(html.ToString());
            }
        }

        private string getDividerLineHtml(int colspan)
        {
            return "<tr><td colspan=\"" + colspan.ToString() + "\" style=\"border-bottom: 1px dashed #CCCCCC; font-size: 5px;\">&nbsp;</td></tr>";
        }

        private string getAddEditContactForm(ContactPlaceholderData data, ContactData contactToEdit, CmsPage page, int identifier, CmsLanguage langToRenderFor)
        {
            string ControlId = "Contacts_" + page.ID.ToString() + "_" + identifier.ToString() + langToRenderFor.shortCode;
            bool editing = (contactToEdit.contactId > -1);

            // -- process form actions
            string action = PageUtils.getFromForm(ControlId + "action", "");
            string _userErrorMessage = "";
            string _userMessage = "";
            bool _showContactDetails = true;
            if (editing && String.Compare(action, "deleteContact", true) == 0)
            {                
                bool b = ContactData.DeleteContact(contactToEdit);
                if (b)
                {
                    _userMessage = "The contact \"" + getNameDisplayOutput(data, contactToEdit) + "\" has been deleted";
                    _showContactDetails = false;
                }
                else
                {
                    _userErrorMessage = "Error: could not delete contact. There was a database error";
                }
            }
            else if (String.Compare(action, "addNewContact", true) == 0)
            {
                if (!editing)
                    contactToEdit = new ContactData();

                contactToEdit.firstName = PageUtils.getFromForm(ControlId + "firstName", contactToEdit.firstName);
                contactToEdit.lastName = PageUtils.getFromForm(ControlId + "lastName", contactToEdit.lastName);
                contactToEdit.title = PageUtils.getFromForm(ControlId + "title", contactToEdit.title);
                contactToEdit.organizationName = PageUtils.getFromForm(ControlId + "organizationName", contactToEdit.organizationName);
                contactToEdit.address1 = PageUtils.getFromForm(ControlId + "address1", contactToEdit.address1);
                contactToEdit.address2 = PageUtils.getFromForm(ControlId + "address2", contactToEdit.address2);
                contactToEdit.city = PageUtils.getFromForm(ControlId + "city", contactToEdit.city);
                contactToEdit.provinceState = PageUtils.getFromForm(ControlId + "provinceState", contactToEdit.provinceState);
                contactToEdit.postalZipCode = PageUtils.getFromForm(ControlId + "postalZipCode", contactToEdit.postalZipCode);
                contactToEdit.phoneNumber1 = PageUtils.getFromForm(ControlId + "phoneNumber1", contactToEdit.phoneNumber1);
                contactToEdit.phoneNumber2 = PageUtils.getFromForm(ControlId + "phoneNumber2", contactToEdit.phoneNumber2);
                contactToEdit.faxNumber = PageUtils.getFromForm(ControlId + "faxNumber", contactToEdit.faxNumber);
                contactToEdit.mobileNumber = PageUtils.getFromForm(ControlId + "mobileNumber", contactToEdit.mobileNumber);
                contactToEdit.emailAddress = PageUtils.getFromForm(ControlId + "emailAddress", contactToEdit.emailAddress);

                string[] s_catIds = PageUtils.getFromForm(ControlId + "category");
                int[] catIds = StringUtils.ToIntArray(s_catIds);
                contactToEdit.contactCategoryIds.Clear();
                contactToEdit.contactCategoryIds.AddRange(catIds);

                if (contactToEdit.firstName.Trim() == "")
                    _userErrorMessage = "Please enter the contact's first name";
                else if (contactToEdit.lastName.Trim() == "")
                    _userErrorMessage = "Please enter the contact's last name";
                else if (contactToEdit.contactCategoryIds.Count < 1)
                    _userErrorMessage = "Please select at least one category for the contact";
                else
                {
                    bool b = contactToEdit.SaveToDatabase();
                    if (!b)
                        _userErrorMessage = "There was a problem saving the contact to the database";
                    else
                    {
                        if (editing)
                        {
                            string nameDisplay = getNameDisplayOutput(data, contactToEdit);
                            _userMessage = "The changes to \"" + nameDisplay + "\" have been saved.";
                        }
                        else
                        {
                            string nameDisplay = getNameDisplayOutput(data, contactToEdit);
                            _userMessage = "The contact \"" + nameDisplay + "\" has been added.";
                            contactToEdit = new ContactData(); // remove all previously submitted values
                            editing = false;
                        }
                    }
                }
            } // if process 

            StringBuilder html = new StringBuilder();
            
            if (editing)
            {
                string nameDisplay = getNameDisplayOutput(data, contactToEdit);
                html.Append("<h2>Edit contact \"" + nameDisplay + "\":</h2>");
            }
            else
                html.Append("<h2>Add a new contact:</h2>");

            if (_userErrorMessage != "")
            {
                html.Append("<p style=\"color: red\">Error: " + _userErrorMessage + "</p>");
            }
            if (_userMessage != "")
            {
                html.Append("<p style=\"color: green\">" + _userMessage + "</p>");
            }

            if (!_showContactDetails)
                return html.ToString();

            string formId = "editContact";
            html.Append(page.getFormStartHtml(formId));
            html.Append("<table border=\"0\">");

            string dividerLineHtml = getDividerLineHtml(4); 

            html.Append(dividerLineHtml);

            html.Append("<tr>");
            html.Append("<td>First Name:</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(ControlId + "firstName", ControlId + "firstName", contactToEdit.firstName, 20, 255) + "</td>");

            
            html.Append("<td>Last Name:</td>");
            html.Append("<td>" + PageUtils.getInputTextHtml(ControlId + "lastName", ControlId + "lastName", contactToEdit.lastName, 20, 255) + "</td>");
            html.Append("</tr>");

            html.Append(dividerLineHtml);
            
            string colspan = "3";

            html.Append("<td>Categories:</td>");
            ContactDataCategory[] allCategories = ContactDataCategory.getAllContactCategories();
            html.Append("<td colspan=\"" + colspan + "\">");
            int cbid = 0;
            foreach (ContactDataCategory cat in allCategories)
            {
                bool check = (contactToEdit.contactCategoryIds.IndexOf(cat.CategoryId) > -1);
                string cb = PageUtils.getCheckboxHtml(cat.Title, ControlId + "category", ControlId + "category" + cbid.ToString(), cat.CategoryId.ToString(), check);
                html.Append(cb+"<br />");
                cbid++;
             } // foreach
            html.Append("</td>");

            html.Append(dividerLineHtml);
            
            html.Append("<tr>");
            html.Append("<td>Title:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "title", ControlId + "title", contactToEdit.title, 40, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Organization:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "organizationName", ControlId + "organizationName", contactToEdit.organizationName, 40, 255) + "</td>");
            html.Append("</tr>");

            html.Append(dividerLineHtml);
            
            html.Append("<tr>");
            html.Append("<td>Address 1:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "address1", ControlId + "address1", contactToEdit.address1, 40, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Address 2:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "address2", ControlId + "address2", contactToEdit.address2, 40, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>City:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "city", ControlId + "city", contactToEdit.city, 40, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Province/State:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "provinceState", ControlId + "provinceState", contactToEdit.provinceState, 20, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Postal/Zip Code:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "postalZipCode", ControlId + "postalZipCode", contactToEdit.postalZipCode, 10, 255) + "</td>");
            html.Append("</tr>");

            html.Append(dividerLineHtml);
            
            html.Append("<tr>");
            html.Append("<td>Phone Number 1:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "phoneNumber1", ControlId + "phoneNumber1", contactToEdit.phoneNumber1, 20, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Phone Number 2:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "phoneNumber2", ControlId + "phoneNumber2", contactToEdit.phoneNumber2, 20, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Fax Number:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "faxNumber", ControlId + "faxNumber", contactToEdit.faxNumber, 20, 255) + "</td>");
            html.Append("</tr>");

            html.Append("<tr>");
            html.Append("<td>Mobile Number:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "mobileNumber", ControlId + "mobileNumber", contactToEdit.mobileNumber, 20, 255) + "</td>");
            html.Append("</tr>");

            html.Append(dividerLineHtml);

            html.Append("<tr>");
            html.Append("<td>Email Address:</td>");
            html.Append("<td colspan=\"" + colspan + "\">" + PageUtils.getInputTextHtml(ControlId + "emailAddress", ControlId + "emailAddress", contactToEdit.emailAddress, 40, 255) + "</td>");
            html.Append("</tr>");

            html.Append("</table>");

            html.Append(PageUtils.getHiddenInputHtml(ControlId + "action", "addNewContact"));
                                        

            if (editing)
            {
                html.Append(PageUtils.getHiddenInputHtml(CurrentContactIdFormName, contactToEdit.contactId.ToString()));
                html.Append("<input type=\"submit\" value=\"save changes\">");
            }
            else
                html.Append("<input type=\"submit\" value=\"add contact\">");


            html.Append(page.getFormCloseHtml(formId));

            if (editing)
            {
                formId = formId + "_Delete";
                html.Append(page.getFormStartHtml(formId));
                html.Append("<p align=\"right\">Delete:");
                html.Append(PageUtils.getHiddenInputHtml(CurrentContactIdFormName, contactToEdit.contactId.ToString()));
                html.Append(PageUtils.getHiddenInputHtml(ControlId + "action", "deleteContact"));
                html.Append("<input type=\"submit\" value=\"delete contact\">");
                html.Append("</p>");
                html.Append(page.getFormCloseHtml(formId));
            }

            return html.ToString();

        } // getAddEditContactForm
    }
}

