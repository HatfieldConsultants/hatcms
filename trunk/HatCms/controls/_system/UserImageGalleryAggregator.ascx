<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserImageGalleryAggregator.ascx.cs" Inherits="HatCMS.Controls._system.UserImageGalleryAggregator" %>
<%@ Import Namespace="HatCMS"%> 
<%@ Import Namespace="HatCMS.Placeholders"%> 
<%@ Import Namespace="System.Text"%>
<%@ Import Namespace="System.IO"%>
<%@ Import Namespace="System.Collections.Generic"%>
<%
    CmsLanguage lang = CmsContext.currentLanguage;
    string imgText = CmsConfig.getConfigValue("UserImageGallery.ImageText", "Number of images", lang);
    string noGalleryText = CmsConfig.getConfigValue("UserImageGallery.NoGalleryText", "No image galleries are currently available", lang);
	CmsPage galleryPage = CmsContext.currentPage;
	int numGalleriesOutput = 0;
	StringBuilder html = new StringBuilder();
    foreach (CmsPage g in galleryPage.ChildPages)
    {
        if (g.ShowInMenu)
        {


            UserImageGalleryDb db = new UserImageGalleryDb();
            UserImageGalleryPlaceholderData data = new UserImageGalleryPlaceholderData();
            data = db.getUserImageGalleryPlaceholderData(g, 1, CmsContext.currentLanguage, true);

            string dirOnDiskToView = data.getImageStorageDirectory(g);

            if (Directory.Exists(dirOnDiskToView))
            {
                DirectoryInfo di = new DirectoryInfo(dir);

                int update = Hatfield.Web.Portal.PageUtils.getFromForm("update", 0);
                if (update == 1 && g.currentUserCanWrite)
                {
                    CmsLocalImageOnDisk[] updates = CmsLocalImageOnDisk.UpdateFolderInDatabase(di);
                }

                CmsLocalImageOnDisk[] allResources = CmsLocalImageOnDisk.FetchAllImagesInDirectory(di, UserImageGalleryPlaceholderData.ImageExtensionsToDisplay);                

                if (allResources.Length >= 1)
                {
                    numGalleriesOutput++;

                    string thumbUrl = showThumbPage.getThumbDisplayUrl(allResources[0], 100, 100); ;
                    html.Append("<tr><td><a href=\"" + g.Url + "\"><img border=\"0\" src=\"" + thumbUrl + "\"></a></td><td><a href=\"" + g.Url + "\">" + g.Title + "</a><br />(" + allResources.Length + " images)</td></tr>");
                }
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