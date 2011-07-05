using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Hatfield.Web.Portal.Imaging
{
    public class Thumbnail2
    {

        public static Size calculateDisplayWidthAndHeight(int[] BitmapWidthAndHeight, int displayBoxWidth, int displayBoxHeight)
        {

            if (BitmapWidthAndHeight.Length != 2 || BitmapWidthAndHeight[0] < 1 || BitmapWidthAndHeight[1] < 1)
                return new Size();

            int bmpWidth = BitmapWidthAndHeight[0];
            int bmpHeight = BitmapWidthAndHeight[1];

            int boxWidth = displayBoxWidth;
            int boxHeight = displayBoxHeight;
            int new_width = -1;
            int new_height = -1;

            if (boxWidth <= 0 && boxHeight <= 0)
            {
                return new Size(BitmapWidthAndHeight[0], BitmapWidthAndHeight[1]);
            }
            else if (boxWidth <= 0)
            {
                // -- only use the height
                new_width = (boxHeight * bmpWidth) / bmpHeight;
                new_height = boxHeight;
                return new Size(new_width, new_height);
            }
            else if (boxHeight <= 0)
            {
                // -- use only the width
                new_width = boxWidth;
                new_height = (boxWidth * bmpHeight) / bmpWidth;
                return new Size(new_width, new_height);
            }

            if (bmpWidth < boxWidth && bmpHeight < boxHeight)
            {   // don't resize if the image is small
                new_width = bmpWidth;
                new_height = bmpHeight;
            }
            else if (boxHeight <= 0 || (bmpWidth > bmpHeight))
            {
                // landscape picture
                new_width = boxWidth;
                new_height = (boxWidth * bmpHeight) / bmpWidth;
            }
            else
            {
                // portrait picture                                    
                new_width = (boxHeight * bmpWidth) / bmpHeight;
                new_height = boxHeight;
            }

            return new Size(new_width, new_height);

        }


        public static Size getDisplayWidthAndHeight(string fileNameOnDisk, int displayBoxWidth, int displayBoxHeight)
        {
            Bitmap bmp = new Bitmap(1, 1);
            try
            {
                int boxWidth = displayBoxWidth;
                int boxHeight = displayBoxHeight;

                bmp = new Bitmap(fileNameOnDisk);

                return calculateDisplayWidthAndHeight(new int[] { bmp.Width, bmp.Height }, displayBoxWidth, displayBoxHeight);

            }
            catch
            { }
            finally
            {
                bmp.Dispose();
                GC.Collect();
            }
            return new Size();
        }


        public static byte[] CreateThumbnail(string fileNameOnDisk, int displayBoxWidth, int displayBoxHeight)
        {
            Size OutputWidthAndHeight = getDisplayWidthAndHeight(fileNameOnDisk, displayBoxWidth, displayBoxHeight);



            int thumbWidth = OutputWidthAndHeight.Width;
            int thumbHeight = OutputWidthAndHeight.Height;
            // throw new Exception("CreatThumbnail 1" + fileNameOnDisk);
            Bitmap bmp;
            try
            {

                bmp = new Bitmap(fileNameOnDisk);
            }
            catch
            {
                bmp = new Bitmap(thumbWidth, thumbHeight); //If we cant load the image, create a blank one with ThumbSize
                // throw new Exception("CreatThumbnail 5" + thumbWidth + ";" + thumbHeight);
            }

            // throw new Exception("CreatThumbnail 3");
            Bitmap retBmp = new Bitmap(thumbWidth, thumbHeight);  // System.Drawing.Imaging.PixelFormat.Format64bppPArgb
            Graphics grp = Graphics.FromImage(retBmp);

            // throw new Exception("CreatThumbnail 4");
            if (!PageUtils.IsRunningOnMono())
            {
                grp.PixelOffsetMode = PixelOffsetMode.None;
            }

            grp.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grp.CompositingQuality = CompositingQuality.HighQuality;

            grp.SmoothingMode = SmoothingMode.HighQuality;

            // throw new Exception("CreatThumbnail 2");
            grp.DrawImage(bmp, 0, 0, thumbWidth, thumbHeight);

            // make a memory stream to work with the image bytes
            MemoryStream imageStream = new MemoryStream();

            retBmp.Save(imageStream, bmp.RawFormat);

            // make byte array the same size as the image
            byte[] imageContent = new Byte[imageStream.Length];
            // rewind the memory stream
            imageStream.Position = 0;

            // load the byte array with the image
            imageStream.Read(imageContent, 0, (int)imageStream.Length);

            retBmp.Dispose();
            bmp.Dispose();

            GC.Collect();

            return imageContent;
        } // CreateThumbnail


    } // class Thumbnail
}
