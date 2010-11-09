using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.UI;
using System.IO;

namespace Hatfield.Web.Portal.Data
{
    /// <summary>
    /// A Class to convert a dataset to an html stream which can be used to display the dataset
    /// in MS Excel.
    ///
    /// note: for fancier Excel output, take a look at this free component: 
    /// http://www.carlosag.net/Tools/ExcelXmlWriter/Default.aspx

    /// </summary>
    public class OutputDataSetToExcelFile
    {
        public OutputDataSetToExcelFile()
        {
            throw new ArgumentException("All functions in OutputDataSetToExcelFile are static - you should not create an OutputDataSetToExcelFile object");
        }

        private static void setupResponseStream(HttpResponse responseStream, string outputFilename)
        {
            responseStream.Clear();
            responseStream.Charset = "";
            //set the response mime type for excel
            responseStream.ContentType = "application/vnd.ms-excel";

            responseStream.AppendHeader("content-disposition", "attachment; filename=" + outputFilename);

        }

        private static void writeContentsTitle(string contentsTitle, HttpResponse responseStream)
        {
            if (contentsTitle.Trim() != "")
            {
                responseStream.Write("<div align=\"center\"><strong>" + contentsTitle + "</strong></div>");
            }
        }

        private static void writeContentsFooter(string contentsFooter, HttpResponse responseStream)
        {
            if (contentsFooter.Trim() != "")
            {
                responseStream.Write("<p>" + contentsFooter + "</p>");
            }
        }

        private static void clearDataGridControls(System.Web.UI.Control control)
        {
            // translated from VB: http://forums.asp.net/p/466278/466278.aspx
            for (int i = control.Controls.Count - 1; i >= 0; i--)
            {
                clearDataGridControls(control.Controls[i]);
            }

            if (!(control is System.Web.UI.WebControls.TableCell))
            {
                Type t = control.GetType();
                if (t.GetProperty("SelectedItem") != null)
                {
                    System.Web.UI.LiteralControl literal = new System.Web.UI.LiteralControl();
                    control.Parent.Controls.Add(literal);
                    try
                    {
                        literal.Text = t.GetProperty("SelectedItem").GetValue(control, null).ToString();
                    }
                    catch
                    { }
                    control.Parent.Controls.Remove(control);
                }
                else if (t.GetProperty("Text") != null)
                {
                    System.Web.UI.LiteralControl literal = new System.Web.UI.LiteralControl();
                    control.Parent.Controls.Add(literal);
                    try
                    {
                        literal.Text = t.GetProperty("Text").GetValue(control, null).ToString();
                    }
                    catch
                    { }
                    control.Parent.Controls.Remove(control);
                }
                else if (control is LinkButton)
                {
                    control.Parent.Controls.Add(new LiteralControl((control as LinkButton).Text));
                    control.Parent.Controls.Remove(control);
                }
                else if (control is ImageButton)
                {
                    control.Parent.Controls.Add(new LiteralControl((control as ImageButton).AlternateText));
                    control.Parent.Controls.Remove(control);
                }
                else if (control is HyperLink)
                {
                    control.Parent.Controls.Add(new LiteralControl((control as HyperLink).Text));
                    control.Parent.Controls.Remove(control);
                }
                else if (control is DropDownList)
                {
                    control.Parent.Controls.Add(new LiteralControl((control as DropDownList).SelectedItem.Text));
                    control.Parent.Controls.Remove(control);
                }
                else if (control is CheckBox)
                {
                    control.Parent.Controls.Add(new LiteralControl((control as CheckBox).Checked ? "True" : "False"));
                    control.Parent.Controls.Remove(control);
                }
            }
        } // 


        /// <summary>
        /// Outputs a GridView control to the responseStream as an Excel file.
        /// Note: calls responseStream.End(), so nothing will function after OutputToResponse() to called.
        /// </summary>
        /// <param name="gridView">The GridView control to output to an Excel file.</param>
        /// <param name="outputFilename">The filename users will download</param>
        /// <param name="contentsTitle">The title to include at the top of the Excel file</param>
        /// <param name="contentsFooter">The footer to include at the bottom of the Excel file</param>
        /// <param name="responseStream">The response to send the Excel file to.</param>
        public static void OutputToResponse(GridView gridView, string outputFilename, string contentsTitle, string contentsFooter, HttpResponse responseStream)
        {
            // -- setup the response headers
            setupResponseStream(responseStream, outputFilename);

            writeContentsTitle(contentsTitle, responseStream);

            //create a string writer
            StringWriter sw = new StringWriter();
            //create an HtmlTextWriter which uses the StringWriter
            HtmlTextWriter htmlWriter = new HtmlTextWriter(sw);


            gridView.AllowPaging = false;
            gridView.PageSize = int.MaxValue;
            gridView.PageIndex = 0;

            // Not safe actually, calling twice will remove all the formatted header rows
            //gridView.DataBind(); // it appears to be safe to callDataBind twice on a GridView.

            // clear controls. if you do not do this, you get the Exception "'DataGridLinkButton' must be placed inside a form tag with runat=server";
            foreach (Control c in gridView.Controls)
            {
                clearDataGridControls(c);
            }
            //tell the datagrid to render itself to our htmltextwriter
            try
            {
                gridView.RenderControl(htmlWriter);
            }
            catch (HttpException ex)
            {
                // http://forums.asp.net/t/1503482.aspx
                throw new Exception("You need to add the function \"public override void VerifyRenderingInServerForm(Control control) { return; }\" to the page containing the GridView!", ex);
            }


            // write the resulting HTML
            responseStream.Write(sw.ToString());

            writeContentsFooter(contentsFooter, responseStream);


            responseStream.End();
        }

        /// <summary>
        /// Outputs a DataGrid to the responseStream as an Excel file.
        /// Note: calls responseStream.End(), so nothing will function after OutputToResponse() to called.
        /// </summary>
        /// <param name="dataGrid">The DataGrid to output to an Excel file</param>
        /// <param name="outputFilename">The filename users will download</param>
        /// <param name="contentsTitle">The title to include at the top of the Excel file</param>
        /// <param name="contentsFooter">The footer to include at the bottom of the Excel file</param>
        /// <param name="responseStream">The response to send the Excel file to.</param>
        public static void OutputToResponse(DataGrid dataGrid, string outputFilename, string contentsTitle, string contentsFooter, HttpResponse responseStream)
        {

            // -- setup the response headers
            setupResponseStream(responseStream, outputFilename);

            writeContentsTitle(contentsTitle, responseStream);

            //create a string writer
            StringWriter sw = new StringWriter();
            //create an htmltextwriter which uses the stringwriter
            HtmlTextWriter htmlWriter = new HtmlTextWriter(sw);

            dataGrid.AllowPaging = false;
            dataGrid.PageSize = int.MaxValue;
            dataGrid.CurrentPageIndex = 0;

            // -- do not call DataBind because it adds extra (duplicate) columns when rendering a DataGrid! 
            //      DataBind() should be called prior to this Output call being made.
            //      Unfortunately there's no way to tell if a control is already DataBound, so we have to assume it here.
            // dataGrid.DataBind();

            // clear controls. if you do not do this, you get the Exception "'DataGridLinkButton' must be placed inside a form tag with runat=server";
            foreach (System.Web.UI.Control c in dataGrid.Controls)
            {
                clearDataGridControls(c);
            }


            //tell the datagrid to render itself to our htmltextwriter
            dataGrid.RenderControl(htmlWriter);

            //all that's left is to output the html
            responseStream.Write(sw.ToString());

            writeContentsFooter(contentsFooter, responseStream);

            responseStream.End();
        }

        /// <summary>
        /// Outputs the first table in the DataSet to the responseStream as an Excel file.
        /// Note: calls responseStream.End(), so nothing will function after OutputToResponse() to called.
        /// </summary>
        /// <param name="ds">The DataSet to output (only the first table is output)</param>
        /// <param name="outputFilename">The filename users will download</param>
        /// <param name="contentsTitle">The title to include at the top of the Excel file</param>
        /// <param name="contentsFooter">The footer to include at the bottom of the Excel file</param>
        /// <param name="responseStream">The response to send the Excel file to.</param>
        public static void OutputToResponse(DataSet ds, string outputFilename, string contentsTitle, string contentsFooter, HttpResponse responseStream)
        {
            OutputToResponse(ds.Tables[0], outputFilename, contentsTitle, contentsFooter, responseStream);
        }

        /// <summary>
        /// Outputs a DataTable to the responseStream.
        /// Note: calls responseStream.End(), so nothing will function after OutputToResponse() to called.
        /// </summary>
        /// <param name="table">the table to output as an excel file</param>
        /// <param name="outputFilename">The filename users will download</param>
        /// <param name="contentsTitle">The title to include at the top of the Excel file</param>
        /// <param name="contentsFooter">The footer to include at the bottom of the Excel file</param>
        /// <param name="responseStream">The response to send the Excel file to.</param>
        public static void OutputToResponse(DataTable table, string outputFilename, string contentsTitle, string contentsFooter, HttpResponse responseStream)
        {
            //let's make sure the table name exists			
            if (table == null)
                throw new NullReferenceException("The DataTable provided is null");


            // -- add the table to a datagrid
            DataGrid dg = new DataGrid();
            dg.HeaderStyle.Font.Bold = true;
            dg.DataSource = table;
            //bind the datagrid
            dg.DataBind();

            OutputToResponse(dg, outputFilename, contentsTitle, contentsFooter, responseStream);

        }

    }
}
