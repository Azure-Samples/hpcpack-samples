// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Reflection;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;


// This sample code demonstrates using the HoldUntil property and method within the
// IScheduleJob class.  By setting this property from inside an activation filter, the job
// in question will remain in the queue, but will not again be considered to start until the
// time set to the property is reached.  Note that this should not be used with the idea
// that the job will start at that time.  Instead, the job will again be considered to start
// when resources are available, again going through comparison with other jobs regarding
// priorities, resources needed, eventually again going through the activation filter process.
// 
//
// Note:  If an activation filter takes too much time, checked by ActivationFilterTimeout parameter, the
// job will not be started.

namespace ActivationFilterSample
{
    public class MoveJobsToOffHours : IActivationFilter
    {
        // Reference the NameSpace here so that, if it is altered in the future, it may be changed
        // only in one spot and used in Help() & Main()
        private static string xmlNameSpace = @"http://schemas.microsoft.com/HPCS2008R2/scheduler/";

        private static string[] PeakUsers = { "DomainName\\UserName1", "DomainName\\UserName42" };

        const int starthours = 8; // beginning of peak working hours
        const int endhours = 17;  // ending of peak working hours
        const int startbuffer = 1; // Time (in hours) before peak hours that off-hour jobs cannot start

        public static TextWriter logFile = null;
        /// <summary>
        /// Entry point for an activation filter.
        /// </summary>
        /// <param name="jobXml"></param>
        /// XML stream containing the job in question.
        /// <param name="schedulerPass"></param>
        /// <param name="jobIndex"></param>
        /// <param name="backfill"></param>
        /// <param name="resourceCount"></param>
        /// <returns></returns>
        public ActivationFilterResponse FilterActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            if (setupLogFile() != 0) {
                return ActivationFilterResponse.FailJob;
            }

            ActivationFilterResponse retval = ActivationFilterResponse.FailJob;
            try {
                // If the job is submitted outside peak business hours, no change is necessary
                if (DuringOffHours()) {
                    logFile.WriteLine("AF: During Off Peak Hours, job starting");
                    return ActivationFilterResponse.StartJob;
                }

                // Currently during peak business hours
                // Check if user is authorized to start a job during these hours
                // If not, delay the start of the job until off peak hours are in play

                // Load the job file as an XmlDocument.
                XmlDocument doc = new XmlDocument();
                doc.Load(jobXml);

                XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("hpc", xmlNameSpace);

                // Find the job node in the XML document.
                XmlNode jobXML = doc.SelectSingleNode("/hpc:Job", nsMgr);

                if (jobXML == null) {
                    throw new Exception("No job in the xml file");
                }

                // Find the User attribute for the job.
                XmlAttributeCollection attrCol = jobXML.Attributes;
                XmlAttribute userAttr = attrCol["User"];
                string user = userAttr.Value;

                // If user does not have permission to run jobs during peak hours, adjust HoldUntil if needed
                if (!PeakHoursUser(user)) {
                    string jobIdString = attrCol["Id"].Value;
                    int jobId;
                    Int32.TryParse(jobIdString, out jobId);
                    if (jobId != 0) {
                        using (IScheduler scheduler = new Scheduler()) {
                            scheduler.Connect("localhost");
                            ISchedulerJob job = scheduler.OpenJob(jobId);

                            DateTime peakEnd = DateTime.Today.AddHours((double)endhours);

                            // If the job is not already set to delay until off peak hours, set it
                            // This property should be null, but could be non-null if some other
                            // thread has set it after scheduling called the activation filter
                            if ((job.HoldUntil == null) || (job.HoldUntil < peakEnd)) {
                                job.SetHoldUntil(peakEnd);
                                job.Commit();
                                logFile.WriteLine("Delay job {0} until off peak hours", jobId);
                            } else {
                                logFile.WriteLine("Job {0} already set to {1}", jobId, job.HoldUntil);
                            }
                            scheduler.Close();
                        } // using scheduler
                    } else {
                        logFile.WriteLine("jobId == 0, delaying job by default duration");
                    }

                    retval = ActivationFilterResponse.HoldJobReleaseResourcesAllowOtherJobsToSchedule;
                } else {
                    logFile.WriteLine("Job to run during peak hours");
                    retval = ActivationFilterResponse.StartJob;
                }

            } catch (IOException e) {
                logFile.WriteLine("Error Loading the XmlFile");
                logFile.WriteLine(e.ToString());
                retval = ActivationFilterResponse.FailJob;
            } catch (Exception e) {
                logFile.WriteLine(e.ToString());
                retval = ActivationFilterResponse.FailJob;
            } finally {
                logFile.Close();
            }

            return retval;
        }

        // Setup the log file
        // Return 0 = Success
        //       -1 = Failure
        private static int setupLogFile()
        {
            int retval = 0;
            try {
                string assemblyPathInclusive = Assembly.GetExecutingAssembly().Location;
                string assemblyPath = Path.GetDirectoryName(assemblyPathInclusive);
                String logFileName = Path.Combine(assemblyPath, "ActivationFilter.log");
                logFile = new StreamWriter(logFileName, true);
            } catch (Exception) {
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
            if (now.Hour < (starthours - startbuffer) || (now.Hour >= endhours)) {
                return true;
            }
            // If this is during the weekend, all hours are off peak
            if ((now.DayOfWeek == DayOfWeek.Saturday) || (now.DayOfWeek == DayOfWeek.Sunday)) {
                return true;
            }
            // Consider holidays
            if (IsHoliday(now)) {
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
                ((date.DayOfYear == 60) && (date.Month == 2))) { // and February 29th
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
            foreach (string peakUser in PeakUsers) {
                if (peakUser == user) {
                    return true;  // User found
                }
            }
            return false;  // Not found
        }

        /// <summary>
        /// Called when a filter in the filter chain AFTER this instance causes
        /// the job to be failed.
        /// </summary>
        /// <param name="jobXml"></param>
        /// <param name="schedulerPass"></param>
        /// <param name="jobIndex"></param>
        /// <param name="backfill"></param>
        /// <param name="resourceCount"></param>
        public void RevertActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
        }
    } // class MoveJobsToOffHours
} // namespace ActivationFilterSample
