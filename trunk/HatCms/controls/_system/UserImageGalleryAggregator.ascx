<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserImageGalleryAggregator.ascx.cs" Inherits="HatCMS.Controls._system.UserImageGalleryAggregator" %>
<%@ Import Namespace="HatCMS" %>
<%@ Import Namespace="HatCMS.Placeholders" %>
<%
    CmsLanguage lang = CmsContext.currentLanguage;
    string imgText = CmsConfig.getConfigValue("UserImageGallery.ImageText", "Number of images", lang);
    string noGalleryText = CmsConfig.getConfigValue("UserImageGallery.NoGalleryText", "No image galleries are currently available", lang);
	CmsPage galleryPage = CmsContext.currentPage;
	int numGalleriesOutput = 0;
	StringBuilder html = new StringBuilder();
	foreach(CmsPage g in galleryPage.ChildPages)
	{
		if (g.ShowInMenu)
		{
			
			UserImageGalleryDb db = new UserImageGalleryDb();
			UserImageGalleryPlaceholderData data = new UserImageGalleryPlaceholderData();
			data = db.getUserImageGalleryPlaceholderData(g, 1, lang, true);

			string dirOnDiskToView = data.getImageStorageDirectory(g);

			CmsResource[] allResources = CmsResource.GetResourcesInDirectory(dirOnDiskToView, UserImageGalleryPlaceholderData.ImageExtensionsToDisplay);

			if (allResources.Length >= 1)
			{
				numGalleriesOutput ++;

				string thumbUrl = showThumbPage.getThumbDisplayUrl(allResources[0], 100, 100);;
                html.Append("<tr><td><a href=\"" + g.Url + "\"><img border=\"0\" src=\"" + thumbUrl + "\"></a></td><td><a href=\"" + g.Url + "\">" + g.Title + "</a><br />[ " + imgText + ": " + allResources.Length + " ]</td></tr>");
			}
			
		}
	} // foreach

	if (numGalleriesOutput > 0)
	{
	    %>
	    <table cellspacing="10">
	    <%= html.ToString() %>
	    </table>
	    <%
	}
	else
	{
    	%><em><%= noGalleryText %></em><%
	}
%>
<p>&nbsp;</p>