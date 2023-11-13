// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.IO;
using System.Xml;
using Microsoft.Hpc.Scheduler;

// This sample code demonstrates using the HoldUntil property and method within the
// IScheduleJob class.  By setting this property from inside an activation filter, the job
// in question will remain in the queue, but will not again be considered to start until the
// time set to the property is reached.  Note that this should not be used with the idea
// that the job will start at that time.  Instead, the job will again be considered to start
// when resources are available, again going through comparison with other jobs regarding
// priorities, resources needed, eventually again going through the activation filter process.
// 
// This activation filter needs to be placed within the cluster's configuration in order to take be functioning
// To do so, the administrator can use:
//    cluscfg setparams ActivationFilterProgram=\\Full\Path\To\ActivationFilterSample.exe
// 
// An activation filter can return any of the following integers:
//   0 - Start the job with the resources available
//   1 - Block the queue.  Do not start this job, and do not consider starting other jobs
//   2 - Reserve resources for this job, but do not start it.  Other jobs in the queue will be considered
//   3 - Hold the job.  Do not reserve resources for this job, and do not consider starting it again
//       until the time set has passed.  If no time is set within the filter itself, the time is set
//       adding the DefaultHoldDuration to the current time.
//   4 - Fail the job.  Remove it from the queue and change the state to Failed
//
// Note:  If an activation filter takes too much time, checked by ActivationFilterTimeout parameter, the
// job will not be started.

namespace ActivationHoldUntil
{
    class MoveJobsToOffHours
    {
        // Reference the NameSpace here so that, if it is altered in the future, it may be changed
        // only in one spot and used in Help() & Main()
        private static string xmlNameSpace = @"http://schemas.microsoft.com/HPCS2008R2/scheduler/";

        private static string[] peakUsers = { "DomainName\\UserName1", "DomainName\\UserName42" };

        private enum AFReturnValue
        {
            StartJob = 0,
            BlockQueue = 1,
            DoNotStartJob = 2,
            HoldJobUntil = 3,
            FailJob = 4,
            FilterFailure = -1  // Undefined return.  Job will not start
        }

        const int starthours = 8; // beginning of peak working hours
        const int endhours = 17;  // ending of peak working hours
        const int startbuffer = 1; // Time (in hours) before peak hours that off-hour jobs cannot start

        public static TextWriter logFile = null;

        /// <summary>
        /// Main entrypoint for Activation Filter
        /// </summary>
        /// <param name="args">
        /// Expect only a single argument containing an XML file
        /// </param>
        /// <returns>
        /// int value from Activation Filter
        /// </returns>
        static int Main(string[] args)
        {
            string clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");

            if (SetupLogFile() != 0)
            {
                return (int)AFReturnValue.FilterFailure;
            }

            int retval = (int)AFReturnValue.FilterFailure;
            try
            {
                // If the job is submitted outside peak business hours, no change is necessary
                if (DuringOffHours())
                {
                    logFile.WriteLine("AF: During Off Peak Hours, job starting");
                    return (int)AFReturnValue.StartJob;
                }

                // Currently during peak business hours
                // Check if user is authorized to start a job during these hours
                // If not, delay the start of the job until off peak hours are in play

                // Check that there is only one argument.  If more than 1 argument exists,
                // put a warning in the log file, but try to process it anyway
                if (args.Length != 1)
                {
                    logFile.WriteLine("Only 1 parameter expected containing the name of the job xml file");
                    logFile.WriteLine("Received {0} parameters", args.Length);
                    // If no parameters exist, cannot parse XML file
                    if (args.Length == 0)
                    {
                        return (int)AFReturnValue.FilterFailure;
                    }
                }

                string fileName = args[0];

                // Load the job file as an XmlDocument.
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);

                XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("hpc", xmlNameSpace);

                // Find the job node in the XML document.
                XmlNode jobXML = doc.SelectSingleNode("/hpc:Job", nsMgr) ?? throw new Exception("No job in the xml file");

                // Find the User attribute for the job.
                XmlAttributeCollection attrCol = jobXML.Attributes;
                XmlAttribute userAttr = attrCol["UserName"];
                string user = userAttr.Value;

                // If user does not have permission to run jobs during peak hours, adjust HoldUntil if needed
                if (!PeakHoursUser(user))
                {
                    string jobIdString = attrCol["Id"].Value;
                    int.TryParse(jobIdString, out int jobId);
                    if (jobId != 0)
                    {
                        using (IScheduler scheduler = new Scheduler())
                        {
                            scheduler.Connect(clusterName);
                            ISchedulerJob job = scheduler.OpenJob(jobId);

                            DateTime peakEnd = DateTime.Today.AddHours((double)endhours);

                            // If the job is not already set to delay until off peak hours, set it
                            // This property should be null, but could be non-null if some other
                            // thread has set it after scheduling called the activation filter
                            if ((job.HoldUntil == null) || (job.HoldUntil < peakEnd))
                            {
                                job.SetHoldUntil(peakEnd);
                                job.Commit();
                                logFile.WriteLine("Delay job {0} until off peak hours", jobId);
                            }
                            else
                            {
                                logFile.WriteLine("Job {0} already set to {1}", jobId, job.HoldUntil);
                            }
                            scheduler.Close();
                        } // using scheduler
                    }
                    else
                    {
                        logFile.WriteLine("jobId == 0, delaying job by default duration");
                    }

                    retval = (int)AFReturnValue.HoldJobUntil;
                }
                else
                {
                    logFile.WriteLine("Job to run during peak hours");
                    retval = (int)AFReturnValue.StartJob;
                }

            }
            catch (IOException e)
            {
                logFile.WriteLine("Error Loading the XmlFile");
                logFile.WriteLine(e.ToString());
                retval = (int)AFReturnValue.FilterFailure;
            }
            catch (Exception e)
            {
                logFile.WriteLine(e.ToString());
                retval = (int)AFReturnValue.FilterFailure;
            }
            finally
            {
                logFile.Close();
            }

            return retval;
        }

        // Setup the log file
        // Return 0 = Success
        //       -1 = Failure
        private static int SetupLogFile()
        {
            int retval = 0;
            try
            {
                string logFileName = "SubmissionFilter.log";
                logFile = new StreamWriter(logFileName, true);
            }
            catch (Exception)
            {
                retval = -1;
            }
            return retval;
        }

        /// <summary>
        /// DuringOffHours() tests if current time is outside peak work hours
        /// </summary>
        /// <returns>
        ///   true  - Outside peak working hours
        ///   false - Within peak working hours
        /// </returns>
        private static bool DuringOffHours()
        {
            DateTime now = DateTime.Now;

            // Before morning or after evening
            if (now.Hour < (starthours - startbuffer) || (now.Hour >= endhours))
            {
                return true;
            }
            // If this is during the weekend, all hours are off peak
            if ((now.DayOfWeek == DayOfWeek.Saturday) || (now.DayOfWeek == DayOfWeek.Sunday))
            {
                return true;
            }
            // Consider holidays
            if (IsHoliday(now))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// IsHoliday() tests if this is a cluster holiday
        /// </summary>
        /// <param name="date">Date to be tested</param>
        /// <returns>
        ///   true -  Celebrate this holiday
        ///   false - Not a (recognized) holiday
        /// </returns>
        private static bool IsHoliday(DateTime date)
        {
            bool Holiday = false;
            if ((date.DayOfYear == 1) ||  // Currently only celebrating New Years Day
                ((date.DayOfYear == 60) && (date.Month == 2)))
            { // and February 29th
                Holiday = true;
            }
            return Holiday;
        }

        /// <summary>
        /// PeakHoursUser() checks the database for those who may run jobs during peak hours
        /// </summary>
        /// <param name="user">User name</param>
        /// <returns>
        ///   true - User found
        ///   false - User not found
        /// </returns>
        private static bool PeakHoursUser(string user)
        {
            foreach (string peakUser in peakUsers)
            {
                if (peakUser == user)
                {
                    return true;  // User found
                }
            }
            return false;  // Not found
        }

    } // class MoveJobsToOffHours
} // namespace ActivationFilterSample
