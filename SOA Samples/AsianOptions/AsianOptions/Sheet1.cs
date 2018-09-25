using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.VisualStudio.Tools.Applications.Runtime;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using AsianOptions.AsianOptionsService;
using System.Threading;
using System.Diagnostics;
 

namespace AsianOptions
{
    public partial class Sheet1
    {
        private static Excel.Range rngUp, rngDown, rngInitial, rngExercise, rngInterest, rngPeriods, rngRuns, rngAsianCallValue;

        private void Sheet1_Startup(object sender, System.EventArgs e)
        {
            rngUp = this.Range["B2", missing];
            rngDown = this.Range["B3", missing];
            rngInterest = this.Range["B4", missing];
            rngInitial = this.Range["B5", missing];
            rngPeriods = this.Range["B6", missing];
            rngExercise = this.Range["B7", missing];
            rngRuns = this.Range["B8", missing];
            rngAsianCallValue = this.Range["B9", missing];
        }

        private void Sheet1_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Run.Click += new System.EventHandler(this.Run_Click);
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            this.Shutdown += new System.EventHandler(this.Sheet1_Shutdown);
            this.Startup += new System.EventHandler(this.Sheet1_Startup);

        }

        #endregion

        private void Run_Click(object sender, EventArgs e)
        {
            #region Initialization
            double initial = (double)rngInitial.Value2;
            double exercise = (double)rngInitial.Value2;
            double up = (double)rngUp.Value2;
            double down = (double)rngDown.Value2;
            double interest = (double) rngInterest.Value2;
            int periods = Convert.ToInt32(rngPeriods.Value2);
            int runs = Convert.ToInt32(rngRuns.Value2);
            Service1Client client = null;

            double sumPrice = 0.0;
            double sumSquarePrice = 0.0;
            double min = double.MaxValue;
            double max = double.MinValue;
            double stdDev = 0.0;
            double stdErr = 0.0;
            #endregion


            // Run for a number of iterations
            string[] cols = { "D", "E", "F", "G", "H", "I", "J", "K", "L", "M" };

            AutoResetEvent finishedEvt = new AutoResetEvent(false);
            int count = 0;

            Stopwatch timer = Stopwatch.StartNew();

            if (Globals.Client == null)
                Globals.GetSession();

            client = Globals.Client;

            // Set time out to MaxValue so that we'll not have timeout exceptions
            client.InnerChannel.OperationTimeout = new TimeSpan(1, 0, 0);

            foreach (string col in cols)
            {
                for (int i = 2; i <= 11; i++)
                {
                   
                    client.BeginPriceAsianOptions(new PriceAsianOptionsRequest(initial, exercise, up, down, interest, periods, runs),
                        #region callback
                        (IAsyncResult result) =>
                        {
                            double price = client.EndPriceAsianOptions(result).PriceAsianOptionsResult;

                            // Populate the cell: Cell Id is stored in result.AsyncState
                            this.Range[(string)result.AsyncState, missing].Value2 = price;

                            Interlocked.Increment(ref count);

                            min = Math.Min(min, price);
                            max = Math.Max(max, price);

                            sumPrice += price;
                            sumSquarePrice += price * price;

                            stdDev = Math.Sqrt(sumSquarePrice - sumPrice * sumPrice / count) / ((count == 1) ? 1 : count - 1);
                            stdErr = stdDev / Math.Sqrt(count);
                            
                            if (count == cols.Length * 10)
                                finishedEvt.Set();
                        },
                        #endregion
                        string.Format("{0}{1}", col, i)  // Context: which correponds to result.AsyncState
                    );
                }
            }

            finishedEvt.WaitOne();
            timer.Stop();

            #region Summarize
            this.Range["D13", missing].Value2 = sumPrice / count;
            this.Range["D14", missing].Value2 = min;
            this.Range["D15", missing].Value2 = max;
            this.Range["D16", missing].Value2 = stdDev;
            this.Range["D17", missing].Value2 = stdErr;

            this.Range["D18", missing].Value2 = timer.Elapsed.TotalMilliseconds / 1000.0;
            #endregion

        }

        private void Clear_Click(object sender, EventArgs e)
        {
            this.Range["D2", "M11"].Clear();
            this.Range["D13", "D18"].Clear();
        }
    }
}
