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
using Microsoft.Hpc.Scheduler.Session;
using Microsoft.Hpc.Scheduler.Properties;
using System.ServiceModel;
using Microsoft.Hpc.Scheduler;

namespace AsianOptions
{
    internal sealed partial class Globals
    {
        private static Service1Client _Client = null;
        internal static Service1Client Client
        {
            get
            {
                return _Client;
            }
            set
            {
                if ((_Client == null))
                {
                    _Client = value;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }

        internal static Session GetSession()
        {
            SessionStartInfo info = new SessionStartInfo(Config.headNode, "AsianOptionsService");

            info.SessionResourceUnitType = SessionUnitType.Core;
            info.MinimumUnits = 1;
            info.MaximumUnits = 16;
            info.Secure = false;
            //info.ShareSession = true;
            info.BrokerSettings.SessionIdleTimeout = 12 * 60 * 60;  // 12 hours

            Session.SetInterfaceMode(false, IntPtr.Zero); //set interface mode to non console

            Session session = Session.CreateSession(info);
            Client = new Service1Client(new NetTcpBinding(SecurityMode.None, false), session.EndpointReference);
         

            // Warm up the service
            //for (int i = 0; i < 32; i++)
            //{
            //    Globals.Client.BeginPriceAsianOptions(30, 30, 1.4, 0.8, 1.08, 2, 2, null, null);
            //}

            //session.AutoClose = false;
            return session;
        }
    }
     
    public partial class ThisWorkbook
    {
        private static Session session = null;

        private void ThisWorkbook_Startup(object sender, System.EventArgs e)
        {
            // Check to see if a Shared Session has been created
            //IScheduler scheduler = new Scheduler();
            //scheduler.Connect(Config.headNode);

            //scheduler.SetInterfaceMode(false, (IntPtr)null);


            //IFilterCollection filter = new FilterCollection();
            //filter.Add(new FilterProperty(FilterOperator.Equal, JobPropertyIds.State, JobState.Running));
            //filter.Add(new FilterProperty(FilterOperator.Equal, JobPropertyIds.JobType, JobType.Broker));

            //ISchedulerCollection jobs = scheduler.GetJobList(filter, null);

            //var matchedJobs = from job in (IEnumerable<ISchedulerJob>)jobs
            //                  where job.Name.Contains("AsianOptions")
            //                  select job;

            //if (matchedJobs.Count<ISchedulerJob>() > 0)
            //{
            //    ISchedulerJob brokerJob = matchedJobs.ElementAt<ISchedulerJob>(0);
            //    Globals.Client = new Service1Client(new NetTcpBinding(SecurityMode.None, false),
            //                                        new EndpointAddress(brokerJob.EndpointAddresses[0]));
            //}
            //else
            //{
            //    session = Globals.GetSession();
            //}
            // enable the "Run" & "Clear" buttons on Sheet 1
            Globals.Sheet1.Run.Enabled = true;
            Globals.Sheet1.Clear.Enabled = true;
        }



        private void ThisWorkbook_Shutdown(object sender, System.EventArgs e)
        {
            // TODO for the new build, call Session.Close(session.Id);
            if (session != null)
                session.Dispose();  
        }

        #region VSTO Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Shutdown += new System.EventHandler(this.ThisWorkbook_Shutdown);
            this.Startup += new System.EventHandler(this.ThisWorkbook_Startup);

        }

        #endregion

    }
}
