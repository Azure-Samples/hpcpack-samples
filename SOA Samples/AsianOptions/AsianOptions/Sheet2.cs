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
using Microsoft.Hpc.Scheduler.Session;
using System.ServiceModel;
using Microsoft.Hpc.Scheduler.Properties;

namespace AsianOptions
{
    public partial class Sheet2
    {
        #region data & type

        [Serializable]
        class cellContext
        {
            public string range;
            public int iteration;
        }

        class result
        {
            public double sumPrice;
            public double sumSquarePrice;
            public double average;
            public double min;
            public double max;
            public double stdDev;
            public double stdErr;
            public int count;

            public result()
            {
                sumPrice = 0;
                sumSquarePrice = 0;
                average = 0;
                min = double.MaxValue;
                max = double.MinValue;
                stdDev = 0;
                stdErr = 0;
                count = 0;
            }
        }

        readonly string[] cols = { "D", "E", "F", "G", "H", "I", "J", "K", "L", "M" };

        private static Excel.Range rngUp, rngDown, rngInitial, rngExercise, rngInterest, rngPeriods, rngRuns, rngInterestStart, rngInterestEnd, rngStep;

        private static bool job_submited = false;
        
        #endregion

        private void Sheet2_Startup(object sender, System.EventArgs e)
        {
            rngUp = this.Range["B2", missing];
            rngDown = this.Range["B3", missing];
            rngInterest = this.Range["B4", missing];
            rngInitial = this.Range["B5", missing];
            rngPeriods = this.Range["B6", missing];
            rngExercise = this.Range["B7", missing];
            rngRuns = this.Range["B8", missing];
            rngInterestStart = this.Range["B9", missing];
            rngInterestEnd = this.Range["B10", missing];
            rngStep = this.Range["B11", missing];
        }

        private void Sheet2_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.retrieve.Click += new System.EventHandler(this.retrieve_Click);
            this.submit.Click += new System.EventHandler(this.submit_Click);
            this.clear.Click += new System.EventHandler(this.clear_Click);
            this.Startup += new System.EventHandler(this.Sheet2_Startup);
            this.Shutdown += new System.EventHandler(this.Sheet2_Shutdown);

        }

        #endregion

        private void retrieve_Click(object sender, EventArgs e) {
            if (job_submited)
            {
                ThreadPool.QueueUserWorkItem((x) => retrieveResp());
            }
            job_submited = false;
        }

        private void submit_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem((x) => submitReq());
            job_submited = true;
        }

        private void retrieveResp() 
        {
            #region Init
            result[] results = new result[10];
            for (int i = 0; i < 10; i++)
                results[i] = new result();
            #endregion

            SessionAttachInfo attachInfo = new SessionAttachInfo(Config.headNode, Convert.ToInt32(this.Range["D20", missing].Value2));

            using (DurableSession session = DurableSession.AttachSession(attachInfo))
            {
                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session))
                {

                    foreach (BrokerResponse<PriceAsianOptionsResponse> response in client.GetResponses<PriceAsianOptionsResponse>())
                    {
                        cellContext idx = response.GetUserData<cellContext>();
                        double price = response.Result.PriceAsianOptionsResult;
                        Interlocked.Increment(ref results[idx.iteration].count);
                        
                        this.Range[idx.range, missing].Value2 = price;

                        results[idx.iteration].min = Math.Min(results[idx.iteration].min, price);
                        results[idx.iteration].max = Math.Max(results[idx.iteration].max, price);

                        results[idx.iteration].sumPrice += price;
                        results[idx.iteration].sumSquarePrice += price * price;

                        results[idx.iteration].stdDev = Math.Sqrt(results[idx.iteration].sumSquarePrice - results[idx.iteration].sumPrice * results[idx.iteration].sumPrice / results[idx.iteration].count) / ((results[idx.iteration].count == 1) ? 1 : results[idx.iteration].count - 1);
                        results[idx.iteration].stdErr = results[idx.iteration].stdDev / Math.Sqrt(results[idx.iteration].count);

                        if (results[idx.iteration].count == 100)
                        {
                            int i = idx.iteration;
                            this.Range[string.Format("{0}14", cols[i]), missing].Value2 = results[i].sumPrice / results[i].count;
                            this.Range[string.Format("{0}15", cols[i]), missing].Value2 = results[i].min;
                            this.Range[string.Format("{0}16", cols[i]), missing].Value2 = results[i].max;
                            this.Range[string.Format("{0}17", cols[i]), missing].Value2 = results[i].stdDev;
                            this.Range[string.Format("{0}18", cols[i]), missing].Value2 = results[i].stdErr;
                        }
                    }
                }
                session.Close();
            }

            #region Summarize
            for (int i = 0; i < 10; i++)
            {
                this.Range[string.Format("{0}14",cols[i]), missing].Value2 = results[i].sumPrice / results[i].count;
                this.Range[string.Format("{0}15", cols[i]), missing].Value2 = results[i].min;
                this.Range[string.Format("{0}16", cols[i]), missing].Value2 = results[i].max;
                this.Range[string.Format("{0}17", cols[i]), missing].Value2 = results[i].stdDev;
                this.Range[string.Format("{0}18", cols[i]), missing].Value2 = results[i].stdErr;
            }
            #endregion

        }

        private void submitReq()
        {
            #region Initialization
            double initial = (double)rngInitial.Value2;
            double exercise = (double)rngInitial.Value2;
            double up = (double)rngUp.Value2;
            double down = (double)rngDown.Value2;
            double interest = (double)rngInterest.Value2;
            int periods = Convert.ToInt32(rngPeriods.Value2);
            int runs = Convert.ToInt32(rngRuns.Value2);
            double interestStart = (double)rngInterestStart.Value2;
            double interestEnd = (double)rngInterestEnd.Value2;
            double interestStep = (double)rngStep.Value2;
            #endregion

            #region fire request

            SessionStartInfo info = new SessionStartInfo(Config.headNode, "AsianOptionsService");
            info.Secure = false;
            info.BrokerSettings.SessionIdleTimeout = 12 * 60 * 60;  // 12 hours
            
            DurableSession.SetInterfaceMode(false, IntPtr.Zero); //set interface mode to non console
            
            using (DurableSession session = DurableSession.CreateSession(info))
            {
                this.Range["C20", missing].Value2 = "Session Created. SessionId";
                this.Range["D20", missing].Value2 = session.Id;
                Thread.Sleep(1000);
                this.Range["C21", missing].Value2 = " Sending Req...";

                NetTcpBinding binding;
                binding = new NetTcpBinding(SecurityMode.None);

                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session, binding))
                {
                    int count =0;
                    int reqCount = 0;

                    for (double interestIdx = interestStart; interestIdx < interestEnd; interestIdx += interestStep, count++)
                    {
                        this.Range["C21", missing].Value2 = string.Format("Sending Req Batch {0}", count);

                        bool batch_succeed = false;
                        int batch_retrycount = 0;

                        while (!batch_succeed && batch_retrycount < 3)
                        {

                            for (int j = 0; j < cols.Length; j++)
                            {
                                string col = cols[j];

                                for (int i = 2; i <= 11; i++)
                                {
                                    PriceAsianOptionsRequest priceRequest = new PriceAsianOptionsRequest(initial, exercise, up, down, interestIdx, periods, runs);
                                    cellContext ctx = new cellContext();
                                    ctx.range = string.Format("{0}{1}", col, i);
                                    ctx.iteration = count;

                                    bool i_succeed = false;
                                    int i_retrycount = 0;
                                    while (!i_succeed && i_retrycount < 3)
                                    {
                                        try
                                        {
                                            client.SendRequest<PriceAsianOptionsRequest>(priceRequest, ctx);
                                            i_succeed = true;
                                            this.Range["D21", missing].Value2 = string.Format("{0} Req sent.", ++reqCount);

                                        }
                                        catch (Exception)
                                        {
                                            // Populate the cell with an error message
                                            this.Range[ctx.range, missing].Value2 = "#SendErr#";
                                            i_retrycount++;
                                        }
                                    }

                                    if (!i_succeed)
                                    {
                                        this.Range["C22", missing].Value2 = "Session failed.";
                                        this.Range["D20", missing].Clear();
                                        session.Close();
                                        return;
                                    }
                                }
                            }

                            try
                            {
                                client.Flush();
                                this.Range["C22", missing].Value2= string.Format("Req Batch {0} Flushed.", count);
                                batch_succeed = true;
                            }
                            catch (Exception)
                            {
                                // Populate the cell with an error message
                                this.Range["C22", missing].Value2 = "ClientFlush failed.";
                                batch_retrycount++;
                            }

                            if (!batch_succeed)
                            {
                                this.Range["C22", missing].Value2 = "Session failed.";
                                this.Range["D20", missing].Clear();
                                session.Close();
                                return;
                            }

                        }
                    }
                
                    client.EndRequests();
                    this.Range["C21", missing].Value2 = "Closing.";
                }
            }

            this.Range["C21", missing].Value2 = "Request sent.";
            this.Range["C22", missing].Value2 = "Request flushed.";
 
            #endregion
        }

        private void clear_Click(object sender, EventArgs e)
        {
            this.Range["D2", "M11"].Clear();
            this.Range["D13", "M18"].Clear();
        }

                     

    }
}
