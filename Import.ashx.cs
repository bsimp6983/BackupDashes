using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OfficeOpenXml;
using System.IO;

namespace DowntimeCollection_Demo
{
    /// <summary>
    /// Summary description for Import
    /// </summary>
    public class Import : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            //importExcel();
        }

        public void importExcel()
        {
            try
            {
                const int startRow = 1;

                string filePath = HttpContext.Current.Server.MapPath("~/App_Data/txi.xlsx");

                // Get the file we are going to process
                var existingFile = new FileInfo(filePath);

                List<DowntimeData> dtData = new List<DowntimeData>();
                // Open and read the XlSX file.
                using (var package = new ExcelPackage(existingFile))
                {
                    using (DB db = new DB())
                    {
                        // Get the work book in the file
                        ExcelWorkbook workBook = package.Workbook;
                        if (workBook != null)
                        {
                            if (workBook.Worksheets.Count > 0)
                            {
                                string client = "TXI";
                                // Get the first worksheet
                                int index = 0;

                                List<Options> options = (from o in db.Options
                                                         where o.Client == client
                                                         select o).ToList();

                                // UPDATE [thrivedcs].[dbo].[DowntimeData] SET Line = 'Finish_Mill_3' WHERE Line = 'company-demo' AND Client = 'TXI';
                                // int counter = 0;

                                foreach(ExcelWorksheet currentWorksheet in workBook.Worksheets.ToList())
                                {
                                    string line = "Kiln_2";

                                    if(index == 1)
                                        line = "Raw_Mill_2";
                                    else if(index == 2)
                                        line = "Finish_Mill_3";

                                    // read some data
                                    object dateHeader = currentWorksheet.Cells[startRow, 1].Value;//Date
                                    object eventStartHeader = currentWorksheet.Cells[startRow, 3].Value;//EvenStart
                                    object minutesHeader = currentWorksheet.Cells[startRow, 6].Value;//Minutes
                                    object reasonHeader = currentWorksheet.Cells[startRow, 7].Value;//Reason
                                    object stoppageHeader = currentWorksheet.Cells[startRow, 9].Value;//Stoppage Nature

                                    for (int rowNumber = startRow + 1; rowNumber <= currentWorksheet.Dimension.End.Row; rowNumber++)
                                        // read each row from the start of the data (start row + 1 header row) to the end of the spreadsheet.
                                    {
                                        object oDate = currentWorksheet.Cells[rowNumber, 1].Value;
                                        object oEventStart = currentWorksheet.Cells[rowNumber, 3].Value;
                                        object oMinutes = currentWorksheet.Cells[rowNumber, 6].Value;//It's a Date in H:MM:SS format
                                        object oReason = currentWorksheet.Cells[rowNumber, 7].Value;
                                        object oStoppageOfNature = currentWorksheet.Cells[rowNumber, 9].Value;



                                        if ((oEventStart != null && oEventStart.ToString() != "") && (oStoppageOfNature != null && oStoppageOfNature.ToString() != ""))
                                        {
                                            decimal minutes = 0;

                                            if (oMinutes != null)
                                            {
                                                DateTime minTime = (DateTime)oMinutes;

                                                minutes = Convert.ToDecimal((minTime.TimeOfDay.TotalHours / 60) + minTime.TimeOfDay.TotalMinutes);
                                            }

                                            DateTime EventStart = DateTime.Now;

                                            try
                                            {
                                                EventStart = (DateTime)oEventStart;
                                            }
                                            catch (Exception e)
                                            {
                                                Console.Write(e.Message);
                                            }

                                            DateTime EventStop = EventStart.AddMinutes((double)minutes);
                                            string reason = oReason.ToString();
                                            string nos = oStoppageOfNature.ToString();

                                            Options opt = (from o in options
                                                            where o.Name.Equals(nos, StringComparison.InvariantCultureIgnoreCase)
                                                            select o).FirstOrDefault();

                                            if (opt != null)
                                            {
                                                DowntimeData dt = new DowntimeData();
                                                dt.Client = client;
                                                dt.Comment = reason;
                                                dt.EventStart = EventStart;
                                                dt.EventStop = EventStop;
                                                dt.Line = line;
                                                dt.Minutes = minutes;
                                                dt.IsCreatedByAcromag = true;

                                                dtData.Add(dt);

                                                db.AddToDowntimeDataSet(dt);

                                                db.SaveChanges();

                                                if (dt.ID > 0)
                                                {
                                                    NatureOfStoppage NOS = new NatureOfStoppage();
                                                    NOS.OptionId = opt.Id;
                                                    NOS.DowntimeId = dt.ID;

                                                    db.AddToNatureOfStoppages(NOS);
                                                }
                                            }
                                            else
                                            {
                                                Console.Write("NOS IS EMPTY");
                                            }

                                        }
                                    }

                                    index++;
                                }

                                Console.Write("DONE" + dtData.Count);
                            }
                        }
                    }
                }
            }
            catch (IOException ioEx)
            {
                if (!String.IsNullOrEmpty(ioEx.Message))
                {
                    if (ioEx.Message.Contains("because it is being used by another process."))
                    {
                        Console.WriteLine("Could not read example data. Please make sure it not open in Excel.");
                    }
                }
                Console.WriteLine("Could not read example data. " + ioEx.Message, ioEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while reading example data. " + ex.Message, ex);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("press any key to exit.");
            }

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}