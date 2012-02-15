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
using System.Text;
using Hatfield.Web.Portal;

namespace HatCMS.Placeholders
{
    #region Data Holding Classes
    public class GoogleMapInfo
    {
        public enum MapDisplay { G_NORMAL_MAP, G_SATELLITE_MAP, G_HYBRID_MAP, G_PHYSICAL_MAP, G_SATELLITE_3D_MAP }
        /// <summary>
        /// for www.hatfieldgroup.com: ABQIAAAAP0sGW_JYKHyQqJPTT-bcdBT0v2-N1ak2IBWTqHFZvGSHzV8GlRRMFZaqNprKmLAm2KsA5s4BfBkSIw
        /// </summary>
        public MapDisplay displayType = MapDisplay.G_NORMAL_MAP;
        public string APIKey = "";
        public string PopupHtml = "";
        public double Latitude = 0;
        public double Longitude = 0;
        public int intitialZoomLevel = 13; // 1..17
        public string KMLOverlayUrl = "";
    }
    #endregion

    public class GoogleMap : BaseCmsPlaceholder
    {
        public override CmsDependency[] getDependencies()
        {
            List<CmsDependency> ret = new List<CmsDependency>();
            ret.Add(new CmsDatabaseTableDependency(@"
                CREATE TABLE  `googlemap` (
                  `GoogleMapId` int(10) unsigned NOT NULL AUTO_INCREMENT,
                  `PageId` int(10) unsigned NOT NULL,
                  `Identifier` int(10) unsigned NOT NULL,
                  `APIKey` varchar(255) NOT NULL,
                  `PopupHtml` varchar(255) NOT NULL,
                  `Latitude` double NOT NULL,
                  `Longitude` double NOT NULL,
                  `intitialZoomLevel` int(10) unsigned NOT NULL DEFAULT '13',
                  `MapType` varchar(50) NOT NULL DEFAULT 'G_NORMAL_MAP',
                  `Deleted` datetime DEFAULT NULL,
                  PRIMARY KEY (`GoogleMapId`),
                  KEY `googlemap_secondary` (`PageId`,`Identifier`,`Deleted`)
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8;"));
            return ret.ToArray();
        }


        public override RevertToRevisionResult RevertToRevision(CmsPage oldPage, CmsPage currentPage, int[] identifiers, CmsLanguage language)
        {
            return RevertToRevisionResult.NotImplemented; // this placeholder doesn't implement revisions
        }
                

        public override void RenderInEditMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string width = "400px";
            string height = "200px";
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                width = PlaceholderUtils.getParameterValue("width", width, paramList);
                height = PlaceholderUtils.getParameterValue("height", height, paramList);
            }
            else
            {
                throw new ArgumentException("Invalid CmsTemplateEngineVersion");
            }

            GoogleMapInfo mapInfo = (new GoogleMapDb()).getGoogleMap(page, identifier, true);
            string mapId = "GoogleMap_" + page.Id.ToString() + "_" + identifier.ToString();

            string action = PageUtils.getFromForm(mapId + "Action", "");
            if (action.Trim().ToLower() == "update")
            {
                mapInfo.APIKey = PageUtils.getFromForm(mapId + "API", mapInfo.APIKey);
                string PopupHtml = PageUtils.getFromForm(mapId + "PopupHtml", mapInfo.PopupHtml);
                PopupHtml = PopupHtml.Replace(Environment.NewLine, " ");
                PopupHtml = PopupHtml.Replace("\r", " ");
                PopupHtml = PopupHtml.Replace("\n", " ");
                mapInfo.PopupHtml = PopupHtml;
                mapInfo.Latitude = PageUtils.getFromForm(mapId + "Latitude", mapInfo.Latitude);
                mapInfo.Longitude = PageUtils.getFromForm(mapId + "Longitude", mapInfo.Longitude);
                mapInfo.intitialZoomLevel = PageUtils.getFromForm(mapId + "intitialZoomLevel", mapInfo.intitialZoomLevel);
                mapInfo.displayType = (GoogleMapInfo.MapDisplay)PageUtils.getFromForm(mapId + "type", typeof(GoogleMapInfo.MapDisplay), GoogleMapInfo.MapDisplay.G_NORMAL_MAP);
                mapInfo.KMLOverlayUrl = PageUtils.getFromForm(mapId + "KMLOverlayUrl", mapInfo.KMLOverlayUrl);

                (new GoogleMapDb()).saveUpdatedGoogleMap(page, identifier, mapInfo);
            }
            
            StringBuilder html = new StringBuilder();

            html.Append("Google Maps API Key: ");
            html.Append(PageUtils.getInputTextHtml(mapId + "API", mapInfo + "API", mapInfo.APIKey, 30, 255));
            html.Append("<br>");
            html.Append("KML Overlay Url (specifying a KML will override Popup information below): ");
            html.Append(PageUtils.getInputTextHtml(mapId + "KMLOverlayUrl", mapInfo + "KMLOverlayUrl", mapInfo.KMLOverlayUrl.ToString(), 50, 255));
            html.Append("<br>");
            html.Append("Popup Html (max 255 chars):");
            html.Append(PageUtils.getTextAreaHtml(mapId + "PopupHtml", mapInfo + "PopupHtml", mapInfo.PopupHtml, 50, 2));
            html.Append("<br>");
            html.Append("Popup Latitude (decimal degrees): ");
            html.Append(PageUtils.getInputTextHtml(mapId + "Latitude", mapInfo + "Latitude", mapInfo.Latitude.ToString(), 5, 255));
            html.Append("<br>");
            html.Append("Popup Longitude (decimal degrees): ");
            html.Append(PageUtils.getInputTextHtml(mapId + "Longitude", mapInfo + "Longitude", mapInfo.Longitude.ToString(), 5, 255));
            html.Append("<br>");
            html.Append("Display Type: "+PageUtils.getDropDownHtml(mapId + "type", mapInfo + "type", Enum.GetNames(typeof(GoogleMapInfo.MapDisplay)), Enum.GetName(typeof(GoogleMapInfo.MapDisplay), mapInfo.displayType)));
            html.Append("<br>");
            string[] zoomLevels = new string[] { "1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17" };
            html.Append("Initial Zoom Level: " + PageUtils.getDropDownHtml(mapId + "intitialZoomLevel", mapInfo + "intitialZoomLevel", zoomLevels, mapInfo.intitialZoomLevel.ToString() ));
            html.Append("<br>");
            html.Append(PageUtils.getHiddenInputHtml(mapId + "Action", "update"));

            // -- Render the map
            html.Append(getMapHtml(mapInfo, page, identifier, width, height));

            writer.Write(html.ToString());
        } // RenderEdit

        public override void RenderInViewMode(HtmlTextWriter writer, CmsPage page, int identifier, CmsLanguage langToRenderFor, string[] paramList)
        {
            string width = "400px";
            string height = "200px";
            if (CmsConfig.TemplateEngineVersion == CmsTemplateEngineVersion.v2)
            {
                width = PlaceholderUtils.getParameterValue("width", width, paramList);
                height = PlaceholderUtils.getParameterValue("height", height, paramList);
            }
            else
            {
                throw new ArgumentException("Invalid CmsTemplateEngineVersion");
            }

            GoogleMapInfo mapInfo = (new GoogleMapDb()).getGoogleMap(page, identifier, true);

            StringBuilder html = new StringBuilder();

            html.Append(getMapHtml(mapInfo, page, identifier, width, height));

            writer.Write(html.ToString());

        } // RenderView

        private string getMapHtml(GoogleMapInfo info, CmsPage page, int identifier, string width, string height)
        {
            StringBuilder html = new StringBuilder();

            // -- only render map if all values have been entered in
            bool renderMap = false;
            if (info.APIKey != "" && info.Latitude != Double.MinValue && info.Longitude != Double.MinValue)
                renderMap = true;
            if (info.KMLOverlayUrl != "")
                renderMap = true;
            
            if (renderMap)
            {
                page.HeadSection.AddJavascriptFile(JavascriptGroup.Library, "http://maps.google.com/maps?file=api&amp;v=2&amp;key=" + info.APIKey);

                string mapId = "GoogleMap_" + page.Id.ToString() + "_" + identifier.ToString();
                string loadFunctionName = "LoadMap" + mapId;
                // string onLoadJS = CmsPage.getOnloadJavascript(loadFunctionName);

                string newLine = Environment.NewLine;
                StringBuilder js = new StringBuilder();
                
                js.Append("function " + loadFunctionName + " () {" + newLine);
                js.Append("   if (GBrowserIsCompatible()) {" + newLine);
                js.Append("       var map = new GMap2(document.getElementById(\"" + mapId + "\"));" + newLine);
                js.Append("       map.addControl(new GSmallMapControl());" + newLine);
                js.Append("       map.addControl(new GMapTypeControl());" + newLine);
                string mapType = Enum.GetName(typeof(GoogleMapInfo.MapDisplay), info.displayType);
                js.Append("       map.setMapType(" + mapType + ");" + newLine);


                bool usePopup = (String.Compare(info.KMLOverlayUrl.Trim(), "") == 0);
                if (usePopup)
                {
                    js.Append("   var OfficeLatLng = new GLatLng(" + info.Latitude.ToString() + ", " + info.Longitude + ");" + newLine);

                    js.Append("   map.setCenter(OfficeLatLng, " + info.intitialZoomLevel.ToString() + ");" + newLine);

                    js.Append("   // Creates a marker at the given point" + newLine);
                    js.Append("   function createMarker(point, address) {" + newLine);
                    js.Append("       var marker = new GMarker(point);" + newLine);

                    js.Append("       map.openInfoWindowHtml(point, address);" + newLine);

                    js.Append("       GEvent.addListener(marker, \"click\", function() {" + newLine);
                    js.Append("           marker.openInfoWindowHtml(address);" + newLine);
                    js.Append("           });" + newLine);
                    js.Append("       return marker;" + newLine);
                    js.Append("   } // createMarker " + newLine);

                    js.Append("   map.addOverlay(createMarker(OfficeLatLng, \"" + info.PopupHtml + "\"));" + newLine);

                    
                }
                else
                {
                    // KML overlay
                    // html.Append("var gx = new GGeoXml(\"http://kml.lover.googlepages.com/my-vacation-photos.kml\");" + newLine);
                    js.Append("   var gx = new GGeoXml(\"" + info.KMLOverlayUrl + "\");" + newLine);

                    js.Append("   map.addOverlay(gx);" + newLine);

                }
                js.Append("   } // if combatible" + newLine);
                js.Append("} // " + loadFunctionName + newLine);

                page.HeadSection.AddJSStatements(js.ToString());
                page.HeadSection.AddJSOnReady(loadFunctionName + "();");

                

                html.Append("<div id=\"" + mapId + "\" style=\"width: " + width + "; height: " + height + "; clear: both;\"></div>");
            }
            return html.ToString();
        } // getMapHtml

        public override Rss.RssItem[] GetRssFeedItems(CmsPage page, CmsPlaceholderDefinition placeholderDefinition, CmsLanguage langToRenderFor)
        {
            return new Rss.RssItem[0]; // do not render anything in RSS.
        }
    } // GoogleMap
}
