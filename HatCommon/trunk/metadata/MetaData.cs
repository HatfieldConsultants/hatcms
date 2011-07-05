using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Hatfield.Web.Portal.MetaData;
using SE.Halligang.CsXmpToolkit;
using SE.Halligang.CsXmpToolkit.Schemas;

namespace Hatfield.Web.Portal
{
    public class MetaDataItem
    {
        public string Name = "";
        public string ItemValue = "";

        public MetaDataItem(string name, string val)
        {
            Name = name;
            ItemValue = val;
        }

        public MetaDataItem(string name, DateTime val)
        {
            Name = name;
            ItemValue = ValueFromDateTime(val);
        }

        public MetaDataItem(string name, int val)
        {
            Name = name;
            ItemValue = (val).ToString();
        }

        public static string ValueFromDateTime(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class MetaDataUtils
    {
        public static string[] imageExtensions = new string[] { ".jpg", ".jpeg", ".jpe", ".gif", ".tif", ".png" };
        public static string[] oleExtensions = new string[] { ".doc", ".xls", ".ppt", ".dot", ".pps" };
        public static string[] xmpExtensions = new string[] { ".pdf", ".psd", ".ai", ".indd", ".ind", ".jpg", ".jpeg", ".jpe", ".gif", ".tif", ".png" };

        public static bool CanExtractImageMetaData(string imageFilename)
        {
            string ext = System.IO.Path.GetExtension(imageFilename);
            if (!ext.StartsWith("."))
                ext = "." + ext;
            ext = ext.ToLower();


            if (StringUtils.IndexOf(imageExtensions, ext, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }

        public static bool CanExtractOLEMetaData(string filename)
        {
            if (PageUtils.IsRunningOnMono())
                return false;

            string ext = System.IO.Path.GetExtension(filename);
            if (!ext.StartsWith("."))
                ext = "." + ext;
            ext = ext.ToLower();


            if (StringUtils.IndexOf(oleExtensions, ext, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;

        }

        public static bool CanExtractXmpData(string filename)
        {
            if (PageUtils.IsRunningOnMono())
                return false;

            string ext = System.IO.Path.GetExtension(filename);
            if (!ext.StartsWith("."))
                ext = "." + ext;
            ext = ext.ToLower();


            if (StringUtils.IndexOf(xmpExtensions, ext, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return true;

            return false;
        }

        public static MetaDataItem[] GetFromImageFile(string imageFilename)
        {
            // -- extract Exif Data
            List<MetaDataItem> ret = new List<MetaDataItem>();
            if (CanExtractImageMetaData(imageFilename))
            {
                ret.AddRange(GetExifData(imageFilename));
                // -- extract XMP
                ret.AddRange(GetXmpData(imageFilename));
            }
            return ret.ToArray();
        }

        public static MetaDataItem[] GetFromOLEDocument(string filename)
        {
            List<MetaDataItem> ret = new List<MetaDataItem>();
            if (CanExtractOLEMetaData(filename))
            {
                try
                {
#if !MONO
                    if (!PageUtils.IsRunningOnMono())
                    {
                        DSOFile.OleDocumentPropertiesClass propClass = new DSOFile.OleDocumentPropertiesClass();

                        propClass.Open(filename, true, DSOFile.dsoFileOpenOptions.dsoOptionDontAutoCreate | DSOFile.dsoFileOpenOptions.dsoOptionOnlyOpenOLEFiles);

                        if (propClass.SummaryProperties.ApplicationName != null)
                            ret.Add(new MetaDataItem("OLE:ApplicationName", propClass.SummaryProperties.ApplicationName));

                        if (propClass.SummaryProperties.Author != null)
                            ret.Add(new MetaDataItem("OLE:Author", propClass.SummaryProperties.Author));
                        if (propClass.SummaryProperties.ByteCount > 0)
                            ret.Add(new MetaDataItem("OLE:ByteCount", propClass.SummaryProperties.ByteCount));
                        if (propClass.SummaryProperties.Category != null)
                            ret.Add(new MetaDataItem("OLE:Category", propClass.SummaryProperties.Category));
                        if (propClass.SummaryProperties.CharacterCount > 0)
                            ret.Add(new MetaDataItem("OLE:CharacterCount", propClass.SummaryProperties.CharacterCount));
                        if (propClass.SummaryProperties.CharacterCountWithSpaces > 0)
                            ret.Add(new MetaDataItem("OLE:CharacterCountWithSpaces", propClass.SummaryProperties.CharacterCountWithSpaces));
                        if (propClass.SummaryProperties.Comments != null)
                            ret.Add(new MetaDataItem("OLE:Comments", propClass.SummaryProperties.Comments));
                        if (propClass.SummaryProperties.Company != null)
                            ret.Add(new MetaDataItem("OLE:Company", propClass.SummaryProperties.Company));
                        if (propClass.SummaryProperties.DateCreated != null)
                            ret.Add(new MetaDataItem("OLE:DateCreated", Convert.ToDateTime(propClass.SummaryProperties.DateCreated)));
                        if (propClass.SummaryProperties.DateLastPrinted != null)
                            ret.Add(new MetaDataItem("OLE:DateLastPrinted", Convert.ToDateTime(propClass.SummaryProperties.DateLastPrinted)));
                        if (propClass.SummaryProperties.DateLastSaved != null)
                            ret.Add(new MetaDataItem("OLE:DateLastSaved", Convert.ToDateTime(propClass.SummaryProperties.DateLastSaved)));
                        /*
                        if(propClass.SummaryProperties.DigitalSignature != null)
                            ret.Add(new MetaDataItem("OLE:DigitalSignature", propClass.SummaryProperties.DigitalSignature));*/
                        if (propClass.SummaryProperties.HiddenSlideCount > 0)
                            ret.Add(new MetaDataItem("OLE:HiddenSlideCount", propClass.SummaryProperties.HiddenSlideCount));
                        if (propClass.SummaryProperties.Keywords != null)
                            ret.Add(new MetaDataItem("OLE:Keywords", propClass.SummaryProperties.Keywords));
                        if (propClass.SummaryProperties.LastSavedBy != null)
                            ret.Add(new MetaDataItem("OLE:LastSavedBy", propClass.SummaryProperties.LastSavedBy));
                        if (propClass.SummaryProperties.LineCount > 0)
                            ret.Add(new MetaDataItem("OLE:LineCount", propClass.SummaryProperties.LineCount));
                        if (propClass.SummaryProperties.Manager != null)
                            ret.Add(new MetaDataItem("OLE:Manager", propClass.SummaryProperties.Manager));
                        if (propClass.SummaryProperties.MultimediaClipCount > 0)
                            ret.Add(new MetaDataItem("OLE:MultimediaClipCount", propClass.SummaryProperties.MultimediaClipCount));
                        if (propClass.SummaryProperties.NoteCount > 0)
                            ret.Add(new MetaDataItem("OLE:NoteCount", propClass.SummaryProperties.NoteCount));
                        if (propClass.SummaryProperties.PageCount > 0)
                            ret.Add(new MetaDataItem("OLE:PageCount", propClass.SummaryProperties.PageCount));
                        if (propClass.SummaryProperties.ParagraphCount > 0)
                            ret.Add(new MetaDataItem("OLE:ParagraphCount", propClass.SummaryProperties.ParagraphCount));
                        if (propClass.SummaryProperties.PresentationFormat != null)
                            ret.Add(new MetaDataItem("OLE:PresentationFormat", propClass.SummaryProperties.PresentationFormat));
                        if (propClass.SummaryProperties.RevisionNumber != null)
                            ret.Add(new MetaDataItem("OLE:RevisionNumber", propClass.SummaryProperties.RevisionNumber));
                        /*
                        if(propClass.SummaryProperties.SharedDocument != null)
                            ret.Add(new MetaDataItem("OLE:SharedDocument", propClass.SummaryProperties.SharedDocument)); */
                        if (propClass.SummaryProperties.SlideCount > 0)
                            ret.Add(new MetaDataItem("OLE:SlideCount", propClass.SummaryProperties.SlideCount));
                        if (propClass.SummaryProperties.Subject != null)
                            ret.Add(new MetaDataItem("OLE:Subject", propClass.SummaryProperties.Subject));
                        if (propClass.SummaryProperties.Template != null)
                            ret.Add(new MetaDataItem("OLE:Template", propClass.SummaryProperties.Template));
                        if (propClass.SummaryProperties.Title != null)
                            ret.Add(new MetaDataItem("OLE:Title", propClass.SummaryProperties.Title));
                        if (propClass.SummaryProperties.TotalEditTime > 0)
                            ret.Add(new MetaDataItem("OLE:TotalEditTime", propClass.SummaryProperties.TotalEditTime));
                        if (propClass.SummaryProperties.Version != null)
                            ret.Add(new MetaDataItem("OLE:Version", propClass.SummaryProperties.Version));
                        if (propClass.SummaryProperties.WordCount > 0)
                            ret.Add(new MetaDataItem("OLE:WordCount", propClass.SummaryProperties.WordCount));
                    } // if
#endif

                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            } // if
            return ret.ToArray();
        }


        public static MetaDataItem[] GetExifData(string imageFilename)
        {

            List<MetaDataItem> ret = new List<MetaDataItem>();

            try
            {
                Bitmap mbp = new Bitmap(imageFilename);

                ret.Add(new MetaDataItem("IMAGE:Width", mbp.Width));
                ret.Add(new MetaDataItem("IMAGE:Height", mbp.Height));

                try
                {
                    string sp = "";
                    EXIFextractor exif = new EXIFextractor(ref mbp, sp);
                    foreach (PairOfObjects s in exif)
                    {
                        if (s.Second.ToString().Trim() != "" && s.Second.ToString() != "0" && s.Second.ToString() != "-")
                        {
                            MetaDataItem i = new MetaDataItem("EXIF:" + s.First, s.Second.ToString().Trim());
                            ret.Add(i);
                        }
                    }
                }
                finally
                {
                    mbp.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return ret.ToArray();
        }

        private static string cleanXmpString(string val)
        {
            if (val == null)
                return "";

            if (val.IndexOf("\\376\\377") == 0)
            {
                val = val.Substring("\\376\\377".Length);
                val = val.Replace("\\000", "");
            }

            return val.Trim();
        }

        public static MetaDataItem[] GetXmpData(string filename)
        {
            if (PageUtils.IsRunningOnMono())
                return new MetaDataItem[0];

            List<MetaDataItem> ret = new List<MetaDataItem>();

            try
            {
                Xmp xmp = Xmp.FromFile(filename, XmpFileMode.ReadOnly);
                DublinCore dc = new DublinCore(xmp);
                if (dc.Contributor.Count > 0)
                {
                    foreach (string con in dc.Contributor)
                    {
                        ret.Add(new MetaDataItem("Xmp:Contributor", cleanXmpString(con)));
                    } // foreach
                }

                if (dc.Coverage != null)
                    ret.Add(new MetaDataItem("Xmp:Coverage", cleanXmpString(dc.Coverage)));

                if (dc.Creator.Count > 0)
                {
                    foreach (string s in dc.Creator)
                    {
                        ret.Add(new MetaDataItem("Xmp:Creator", cleanXmpString(s)));
                    }
                }

                if (dc.Date.Count > 0)
                {
                    foreach (DateTime dt in dc.Date)
                    {
                        ret.Add(new MetaDataItem("Xmp:Date", (dt)));
                    }
                }

                if (dc.Description != null && dc.Description.DefaultValue != null)
                    ret.Add(new MetaDataItem("Xmp:Description", cleanXmpString(dc.Description.DefaultValue)));

                if (dc.Format != null)
                    ret.Add(new MetaDataItem("Xmp:Format", cleanXmpString(dc.Format)));

                if (dc.Language.Count > 0)
                {
                    foreach (string lang in dc.Language)
                    {
                        ret.Add(new MetaDataItem("Xmp:Language", cleanXmpString(lang)));
                    }
                }

                if (dc.Publisher.Count > 0)
                {
                    foreach (string pub in dc.Publisher)
                    {
                        ret.Add(new MetaDataItem("Xmp:Publisher", cleanXmpString(pub)));
                    }
                }

                if (dc.Relation.Count > 0)
                {
                    foreach (string rel in dc.Relation)
                    {
                        ret.Add(new MetaDataItem("Xmp:Relation", cleanXmpString(rel)));
                    }
                }


                if (dc.Rights != null && dc.Rights.DefaultValue != null)
                    ret.Add(new MetaDataItem("Xmp:Rights", (dc.Rights.DefaultValue)));

                if (dc.Source != null)
                    ret.Add(new MetaDataItem("Xmp:Source", cleanXmpString(dc.Source)));

                if (dc.Subject.Count > 0)
                {
                    foreach (string sub in dc.Subject)
                    {
                        ret.Add(new MetaDataItem("Xmp:Subject", cleanXmpString(sub)));
                    }
                }

                if (dc.Title != null && dc.Title.DefaultValue != null)
                    ret.Add(new MetaDataItem("Xmp:Title", (dc.Title.DefaultValue)));

                if (dc.Type != null)
                    ret.Add(new MetaDataItem("Xmp:Type", cleanXmpString(dc.Type)));
            }
            catch (DllNotFoundException dllEx)
            {
                throw dllEx;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return ret.ToArray();
        }

    }
}
