using System;
using System.Data;
using System.Collections;

namespace HatCMS.Placeholders
{
	/// <summary>
	/// Summary description for ImageGalleryDb.
	/// </summary>
	public class ImageGalleryDb: PlaceholderDb
	{
		#region ImageGallery		

		public ImageGalleryData getImageGallery(CmsPage page, int identifier, bool createNewIfDoesNotExist)
		{
			if (page.ID < 0 || identifier < 0)
				return new ImageGalleryData();
			
			string sql = "";
			sql = "select g.ImageGalleryId, g.subDir, g.thumbSize, g.largeSize, g.numThumbsPerRow, i.ImageGalleryImageId, i.Caption, I.Filename from imagegallery g left join imagegalleryimages i on i.ImageGalleryId = g.ImageGalleryId";			
			sql += " where g.pageid = "+page.ID.ToString()+" and g.identifier = "+identifier.ToString()+" and g.deleted is null;";
			
			DataSet ds = this.RunSelectQuery(sql);
			if (this.hasRows(ds))
			{
				ImageGalleryData data = new ImageGalleryData();
				DataRow dr = ds.Tables[0].Rows[0];
				data.ImageGalleryId = Convert.ToInt32(dr["ImageGalleryId"]);
				data.subDir = (dr["subDir"].ToString().Trim());
				data.thumbSize = Convert.ToInt32(dr["thumbSize"]);
				data.largeSize = Convert.ToInt32(dr["largeSize"]);
				data.numThumbsPerRow = Convert.ToInt32(dr["numThumbsPerRow"]);
				foreach(DataRow r in ds.Tables[0].Rows)
				{					
					if (r["ImageGalleryImageId"] != System.DBNull.Value && r["ImageGalleryImageId"] != null)
					{
						ImageGalleryImageData img = new ImageGalleryImageData();
						img.Filename = r["Filename"].ToString();
						img.Caption = r["Caption"].ToString();
						img.ImageGalleryImageId = Convert.ToInt32(r["ImageGalleryImageId"]);
						data.addImage(img);
					}
				}
				return data;
			}
			else
			{				
				if(createNewIfDoesNotExist)
				{
					ImageGalleryData data = new ImageGalleryData();
					bool b = createNewImageGallery(page,identifier, data );	
					
					if (!b)
					{
						throw new Exception("getImageGallery database error: Error creating new placeholder");
					}
					else
					{
						return data;
					}
				}
				else
				{
					throw new Exception("getImageGallery database error: placeholder does not exist");
				}
			}
			
		} // getImageGallery

		private bool InsertOrUpdateImageGalleryImages(CmsPage page,int identifier,ImageGalleryData data)
		{
			if (data.ImageGalleryId < 0)
				return false;

			foreach(ImageGalleryImageData img in data.ImageData)
			{
				if (img.ImageGalleryImageId < 0)
				{
					// -- insert
					string sql = "Insert into imagegalleryimages (ImageGalleryId, Caption, Filename) VALUES ";
					sql += "(";
					sql += data.ImageGalleryId.ToString()+", ";
					sql += "\""+this.dbEncode(img.Caption)+"\", ";
					sql += "\""+dbEncode(img.Filename)+"\" ";
					sql += ")";
					int newId = this.RunInsertQuery(sql);
					if (newId < 0)
						return false;
					else
						img.ImageGalleryImageId = newId; // continue
				}
				else
				{
					// -- update
					string sql = "Update imagegalleryimages SET ";
					
					sql += "ImageGalleryId = "+data.ImageGalleryId.ToString()+", ";
					sql += "Caption = \""+img.Caption+"\", ";
					sql += "Filename = \""+img.Filename+"\" ";
					
					sql += " where ImageGalleryImageId = "+img.ImageGalleryImageId;

					int numAffected = this.RunUpdateQuery(sql);
					if (numAffected < 0)
						return false;
					// else continue
				}
			} // foreach
			return true;
		} // InsertOrUpdateImageGalleryImages

		public bool createNewImageGallery(CmsPage page, int identifier, ImageGalleryData data)
		{			
			string sql = "insert into imagegallery (pageid, identifier, subDir, thumbSize, largeSize, numThumbsPerRow ) values (";
			sql = sql +page.ID.ToString()+","+identifier.ToString()+",";
			sql += "\""+dbEncode(data.subDir)+"\", ";
			sql += data.thumbSize.ToString()+", ";
			sql += data.largeSize.ToString()+", ";
			sql += data.numThumbsPerRow.ToString()+" ";
			sql += "); ";
			
			int newId = this.RunInsertQuery(sql);
			if (newId > -1)
			{
				data.ImageGalleryId = newId;
				bool b = InsertOrUpdateImageGalleryImages(page, identifier, data); 
				if (b)
					return page.setLastUpdatedDateTimeToNow();
				else
					return false;
			}
			else
				return false;

		}

		public bool saveUpdatedImageGallery(CmsPage page, int identifier, ImageGalleryData data)
		{

			string sql = "update imagegallery set ";
			sql += "subDir = \""+dbEncode(data.subDir)+"\", ";
			sql += "thumbSize = "+data.thumbSize.ToString()+", ";
			sql += "largeSize = "+data.largeSize.ToString()+", ";
			sql += "numThumbsPerRow = "+data.numThumbsPerRow.ToString()+" ";
			sql += " where pageid= "+page.ID.ToString();
			sql += " AND identifier = "+identifier.ToString()+"; ";
			//sql = sql + " SELECT LAST_INSERT_ID() as newId;";

			int numAffected = this.RunUpdateQuery(sql);
			if (numAffected > 0)
			{
				bool b = InsertOrUpdateImageGalleryImages(page, identifier, data); 
				if (b)
					return page.setLastUpdatedDateTimeToNow();
				else
					return false;
			}
			else
				return false;

		}
		#endregion
	}
}
