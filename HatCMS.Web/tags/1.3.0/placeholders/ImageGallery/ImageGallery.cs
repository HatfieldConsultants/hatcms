using System;
using System.IO;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
	
	#region ImageGallery data holding classes
	public class ImageGalleryData
	{
		public int ImageGalleryId;
		public string subDir;
		public int thumbSize;
		public int largeSize;
		public int numThumbsPerRow;

		private List<ImageGalleryImageData> imageData;
		public ImageGalleryImageData[] ImageData
		{
			get
			{
				return imageData.ToArray();
			}
		}

		public void addImage(ImageGalleryImageData img)
		{
			imageData.Add(img);
		}

		public ImageGalleryImageData getImageData(string imgFilename)
		{
			foreach(ImageGalleryImageData img in ImageData)
			{
				if (String.Compare(img.Filename, imgFilename, true) == 0)
					return img;
			}
			return new ImageGalleryImageData();
		}
			
		public ImageGalleryData()
		{
			ImageGalleryId = -1;
			subDir = "";
			thumbSize = -1;
			largeSize = -1;
			numThumbsPerRow = -1;
            imageData = new List<ImageGalleryImageData>();
		}
	} // ImageGalleryData

	public class ImageGalleryImageData
	{
		public int ImageGalleryImageId;
		public string Filename;
		public string Caption;
		public ImageGalleryImageData()
		{
			Filename = "";
			Caption = "";
			ImageGalleryImageId = -1;
		}
	}
	#endregion
	
	/// <summary>
	/// Summary description for ImageGallery.
	/// </summary>
	public class ImageGallery: BaseCmsPlaceholder
	{

        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `imagegallery` (
                  `ImageGalleryId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `subDir` varchar(255) NOT NULL,
                  `thumbSize` int(11) NOT NULL,
                  `largeSize` int(11) NOT NULL,
                  `numThumbsPerRow` int(11) NOT NULL,
                  `deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`ImageGalleryId`),
                  KEY `imagegallery_secondary` (`PageId`,`Identifier`,`deleted`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `imagegalleryimages` (
                  `ImageGalleryImageId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `Caption` varchar(255) NOT NULL,
                  `Filename` varchar(255) NOT NULL,
                  `ImageGalleryId` int(10) unsigned NOT NULL,
                  PRIMARY KEY (`ImageGalleryImageId`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;
            "));
            return ret.ToArray();
            
        }
        

        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented;; // this placeholder doesn't implement revisions
        }
        
        /// <summary>
        /// always has a trailing "/".
        /// </summary>
        private static string BaseDirectoryPathUrl
        {
            get
            {
                return CmsContext.ApplicationPath + "images//";
            }
        }

        private static string BaseDirectoryPathOnDisk
        {
            get
            {
                return System.Web.Hosting.HostingEnvironment.MapPath(BaseDirectoryPathUrl);                
            }
        }

        public ImageGallery()
		{
			//
			// TODO: Add constructor logic here
			//
		}			

		private enum RenderMode { Directory, FullSize};

		private RenderMode currentViewRenderMode
		{
			get
			{
				int rm = PageUtils.getFromForm("galleryMode", Convert.ToInt32(RenderMode.Directory));
				if (rm == Convert.ToInt32(RenderMode.FullSize))
					return RenderMode.FullSize;
				else
					return RenderMode.Directory;
			}
		}

		private const string DirSeperator = "/";


        private string[] getAllAvailableSubDirs()
        {
            ArrayList dirs = new ArrayList();
            addSubDirsRecursive(BaseDirectoryPathOnDisk, dirs);
            return (string[])dirs.ToArray(typeof(string));
        }

        private void addSubDirsRecursive(string rootDir, ArrayList dirs)
        {
            DirectoryInfo di = new DirectoryInfo(rootDir);
            if (!CmsContext.currentUserIsSuperAdmin && di.Name.StartsWith("_"))
                return;
            string[] subDirs = System.IO.Directory.GetDirectories(rootDir);
            string rootDisplay = "images" + rootDir.Substring(BaseDirectoryPathOnDisk.Length - 1);
            rootDisplay = rootDisplay.Replace(@"\", DirSeperator);
            if (!rootDisplay.EndsWith(DirSeperator))            
                rootDisplay = rootDisplay + DirSeperator;
            
            dirs.Add(rootDisplay);
            foreach (string d in subDirs)
            {                                
                addSubDirsRecursive(d, dirs);
            }
        }

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{						
			
			ImageGalleryDb db = new ImageGalleryDb();			
			ImageGalleryData data = new ImageGalleryData();
			data.subDir = "";
			data.thumbSize = 200;
			data.largeSize = 500;
			data.numThumbsPerRow = 3;
			
			data = db.getImageGallery(page, identifier, true);

			string ImageGalleryId = "ImageGallery_"+page.ID.ToString()+"_"+identifier.ToString();

			// ------- CHECK THE FORM FOR ACTIONS
			string action = Hatfield.Web.Portal.PageUtils.getFromForm(ImageGalleryId+"_Action","");
			if (action.Trim().ToLower() == "update")
			{				
				data.ImageGalleryId = PageUtils.getFromForm(ImageGalleryId+"_DataId",-1);
				data.subDir = PageUtils.getFromForm("subDir_"+ImageGalleryId,"");
				data.thumbSize = PageUtils.getFromForm("thumbSize_"+ImageGalleryId, data.thumbSize);
				data.largeSize = PageUtils.getFromForm("largeSize_"+ImageGalleryId, data.largeSize);
				data.numThumbsPerRow = PageUtils.getFromForm("numThumbsPerRow_"+ImageGalleryId, data.numThumbsPerRow);

				string[] captionIds = PageUtils.getFromForm(ImageGalleryId+"_captions");
				foreach(string captionId in captionIds)
				{
					// captionId is in the form 
					// "imgCaption"+ImageGalleryId+"_"+imgFilenameUnderAppPath;					
					if (captionId != "")
					{						
						string capId = System.Web.HttpUtility.UrlDecode(captionId);
						if (capId.StartsWith("imgCaption"+ImageGalleryId+"_"))
						{
							string imgFilenameUnderAppPath = capId.Substring(("imgCaption"+ImageGalleryId+"_").Length);
                            string caption = PageUtils.getFromForm(System.Web.HttpUtility.UrlEncode(capId), "");
							ImageGalleryImageData img = data.getImageData(imgFilenameUnderAppPath);
							img.Caption = caption;
							img.Filename = imgFilenameUnderAppPath;

							if (img.ImageGalleryImageId < 0)
								data.addImage(img);
						}
					}
				}

				db.saveUpdatedImageGallery(page,identifier, data);				
			}			

			// ------- START RENDERING
			// note: no need to put in the <form></form> tags.
			
			StringBuilder html = new StringBuilder();
			html.Append("<strong>Image Gallery Settings:</strong><br>");
			html.Append("<table>");
			
            string[] subDirs = getAllAvailableSubDirs();
            string s = PageUtils.getDropDownHtml("subDir_" + ImageGalleryId, "subDir_" + ImageGalleryId, subDirs, data.subDir);
			html.Append("<tr><td>Image SubDirectory:</td>");
            html.Append("<td>" + s + "</td></tr>");

            s = PageUtils.getInputTextHtml("numThumbsPerRow_" + ImageGalleryId, "numThumbsPerRow_" + ImageGalleryId, data.numThumbsPerRow.ToString(), 3, 5);
            html.Append("<tr><td>Number of Thumbnails per row:</td>");
            html.Append("<td>" + s + "</td></tr>");

			s = PageUtils.getInputTextHtml("thumbSize_"+ImageGalleryId, "thumbSize_"+ImageGalleryId, data.thumbSize.ToString(), 3, 5);			
			html.Append("<tr><td>Thumbnail Size:</td>");
            html.Append("<td>" + s + "</td></tr>");

			s = PageUtils.getInputTextHtml("largeSize_"+ImageGalleryId, "largeSize_"+ImageGalleryId, data.largeSize.ToString(), 3, 5);			
			html.Append("<tr><td>Full-Sized Image size:</td>");
            html.Append("<td>" + s + "</td></tr>");

						
			html.Append("</table>");

			string thumbViewHtml = getHtmlForThumbView(page, data,ImageGalleryId, true);
			writer.WriteLine(thumbViewHtml);

			html.Append("<input type=\"hidden\" name=\""+ImageGalleryId+"_Action\" value=\"update\">");			
			html.Append("<input type=\"hidden\" name=\""+ImageGalleryId+"_DataId\" value=\""+data.ImageGalleryId.ToString()+"\">");	
			
			writer.WriteLine(html.ToString());

		} // RenderEdit

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
		{
			ImageGalleryData data = new ImageGalleryData();
			data.subDir = "images"+DirSeperator+"ImageGallery"+DirSeperator;			
			data.thumbSize = 200;
			data.largeSize = 500;
			data.numThumbsPerRow = 3;
						
			ImageGalleryDb db = new ImageGalleryDb();
			data = db.getImageGallery(page, identifier, true);

			if (!data.subDir.EndsWith(DirSeperator))
				data.subDir += DirSeperator;

			
			
			if (currentViewRenderMode == RenderMode.FullSize)
			{ // -- render full size
                writer.Write("<div class=\"ImageGallery FullSize\">");
                string jpg = PageUtils.getFromForm("galleryImg", "");
				if (jpg == "")
				{
					writer.Write("Invalid galleryImg parameter");
					return;
				}

				string imgCaption = "";
                int currentImageDataIndex = -1;
				for(int i=0 ; i< data.ImageData.Length; i++)                
				{
                    ImageGalleryImageData d = data.ImageData[i];
					if (Path.GetFileName(d.Filename) == Path.GetFileName(jpg) || d.Filename == jpg)
					{
						imgCaption = "<p align=\"center\" class=\"caption full\">"+d.Caption+"</p>";
                        currentImageDataIndex = i;
                        break;
					}
				}				

				string imgFilenameUnderAppPath = data.subDir+Path.GetFileName(jpg);
				string largeUrl = showThumbPage.getThumbDisplayUrl(imgFilenameUnderAppPath, data.largeSize, -1);
				
				string backUrl = CmsContext.getUrlByPagePath(page.Path);
                writer.Write("<p class=\"ImageGalleryBackLink\"><a class=\"ImageGalleryBackLink\" href=\"" + backUrl + "\">&#171; back to thumbnails</a><p>");

                List<string> nextPrevLinks = new List<string>();
                if (currentImageDataIndex > 0) 
                {
                    NameValueCollection prevImgParams = new NameValueCollection();
                    prevImgParams.Add("galleryMode", Convert.ToInt32(RenderMode.FullSize).ToString());
                    prevImgParams.Add("galleryImg", Path.GetFileName(data.ImageData[currentImageDataIndex - 1].Filename));
                    string prevUrl = CmsContext.getUrlByPagePath(page.Path, prevImgParams);
                    string prevHtml = "<a class=\"ImageGalleryBackLink prev\" href=\"" + prevUrl + "\">&#171; prev</a>";
                    nextPrevLinks.Add(prevHtml);
                }

                if (data.ImageData.Length > 1 && currentImageDataIndex < (data.ImageData.Length - 1))
                {
                    NameValueCollection nextImgParams = new NameValueCollection();
                    nextImgParams.Add("galleryMode", Convert.ToInt32(RenderMode.FullSize).ToString());
                    nextImgParams.Add("galleryImg", Path.GetFileName(data.ImageData[currentImageDataIndex + 1].Filename));
                    string nextUrl = CmsContext.getUrlByPagePath(page.Path, nextImgParams);
                    string nextHtml = "<a class=\"ImageGalleryBackLink next\" href=\"" + nextUrl + "\">next &#187;</a>";
                    nextPrevLinks.Add(nextHtml);
                }

                if (nextPrevLinks.Count > 0)
                {
                    writer.Write("<p class=\"ImageGalleryBackLink\">" + string.Join(" | ", nextPrevLinks.ToArray()) + "</p>");
                }
                

                writer.Write("<img class=\"ImageGalleryFullSizedImage\" src=\"" + largeUrl + "\">");
				writer.WriteLine(imgCaption);
			}
			else
			{ // -- render the directory
                writer.Write("<div class=\"ImageGallery thumbnails\">");

                string ImageGalleryId = "ImageGallery_" + page.ID.ToString() + "_" + identifier.ToString();
				string thumbViewHtml = getHtmlForThumbView(page, data,ImageGalleryId, false);
				writer.WriteLine(thumbViewHtml);
				
			} // render directory
			writer.Write("</div>");

		} // RenderView



        private string getDirOnDiskToView(ImageGalleryData data)
        {
            return System.Web.Hosting.HostingEnvironment.MapPath(CmsContext.ApplicationPath + data.subDir);
        }

        private string getHtmlForThumbView(CmsPage page, ImageGalleryData data, string ImageGalleryId, bool inEditMode)
        {

            StringBuilder html = new StringBuilder();
            string DirOnDiskToView = getDirOnDiskToView(data);


            if (!Directory.Exists(DirOnDiskToView))
            {
                return ("Error with Image Gallery: ImageGallery directory does not exist!");

            }

            string[] JPGFiles = Directory.GetFiles(DirOnDiskToView, "*.jpg");
            if (JPGFiles.Length < 1)
            {
                return ("no images are in this image gallery");
            }

            html.Append("<table>");
            int imgCount = 0;
            ArrayList formCaptionNames = new ArrayList();
            foreach (string jpg in JPGFiles)
            {
                if (imgCount % data.numThumbsPerRow == 0)
                {
                    html.Append("<tr>");
                }

                string imgFilenameUnderAppPath = data.subDir + Path.GetFileName(jpg);
                string thumbUrl = showThumbPage.getThumbDisplayUrl(imgFilenameUnderAppPath, data.thumbSize, -1);

                ImageGalleryImageData imgData = data.getImageData(imgFilenameUnderAppPath);

                NameValueCollection imgParams = new NameValueCollection();
                imgParams.Add("galleryMode", Convert.ToInt32(RenderMode.FullSize).ToString());
                imgParams.Add("galleryImg", Path.GetFileName(jpg));

                string fullSizeUrl = CmsContext.getUrlByPagePath(page.Path, imgParams);

                html.Append("<td class=\"ImageGalleryImage_td\">");

                if (!inEditMode)
                    html.Append("<a class=\"ImageGalleryImageLink\" href=\"" + fullSizeUrl + "\">");

                html.Append("<img class=\"ImageGalleryImage\" src=\"" + thumbUrl + "\">");

                if (!inEditMode)
                    html.Append("</a>");

                html.Append("<br>");
                if (inEditMode)
                {
                    string tbName = System.Web.HttpUtility.UrlEncode("imgCaption" + ImageGalleryId + "_" + imgFilenameUnderAppPath);
                    formCaptionNames.Add(tbName);
                    string tb = PageUtils.getInputTextHtml(tbName, tbName, imgData.Caption, 15, 255);
                    tb = "<nobr>caption: " + tb + "</nobr>";
                    html.Append(tb);
                }
                else
                {
                    html.Append(imgData.Caption);
                }
                html.Append("</td>");
                if (imgCount % data.numThumbsPerRow == data.numThumbsPerRow)
                    html.Append("</tr>");

                imgCount++;
            }
            if (imgCount % data.numThumbsPerRow != data.numThumbsPerRow)
                html.Append("</tr>");
            html.Append("</table>");

            if (inEditMode)
            {
                string csv = "";
                foreach (string id in formCaptionNames)
                {
                    csv = csv + id + ",";
                }
                string h = PageUtils.getHiddenInputHtml(ImageGalleryId + "_captions", csv);
                html.Append(h);
            }

            return html.ToString();

        } // getHtmlForThumbView

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            Rss.RssItem rssItem = CreateAndInitRssItem(page, langToRenderFor);

            rssItem.Description = page.renderPlaceholderToString(placeholderDefinition, langToRenderFor, CmsPage.RenderPlaceholderFilterAction.RunAllPageAndPlaceholderFilters);

            return new Rss.RssItem[] { rssItem };
        }
	}
}
