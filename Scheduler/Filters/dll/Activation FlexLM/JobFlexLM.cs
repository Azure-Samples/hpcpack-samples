// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

/*
 *  Note these sample sources utilize windows event logging and the resulting
 *  DLL should be registered with "weveutil.exe".  Please see MSDN or the post-build events in
 *  the csproj for the .exe version of the FlexLM sample.
 * 
*/


using System;
using System.Collections;
using System.Xml;
using System.IO;

using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

namespace FlexLM
{
    class JobFlexLM : IActivationFilter, IFilterLifespan  // note: IFilterLifespan is optional
    {
        private Hashtable _requestedFeatures = new Hashtable();

        private int _schedulerPass;
        private int _jobIndex;
        private bool _backfill;
        private int _resourceCount;
        private int _jobId;
        private bool _growing;

        /// <summary>
        /// Analyse the Job and if all the required licenses are available start the job.
        /// If there are not enough total licenses to satisfy the request, fail the job.
        /// If there are not enough licenses currently available, request the scheduler keep
        /// the allocated resources so that when the licenses become available the job can
        /// be scheduled.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="schedulerPass"></param>
        /// <param name="jobIndex"></param>
        /// <param name="backfill"></param>
        /// <param name="resourceCount"></param>
        /// <returns></returns>
        public ActivationFilterResponse FilterActivation(
            Stream jobXml,
            int schedulerPass,
            int jobIndex,
            bool backfill,
            int resourceCount)
        {
            ActivationFilterResponse returnDecision = ActivationFilterResponse.StartJob;

            _schedulerPass = schedulerPass;
            _jobIndex = jobIndex;
            _backfill = backfill;
            _resourceCount = resourceCount;

            if ((schedulerPass == 1) &&
                (jobIndex == 1))
            {
                // Scheduler restarted. Ensure we never use stale data from FlexLM server by clearing cache
                Cache.Clear();
            }

            ParseJobXml(jobXml);

            // If this job does not parse correctly or has no license requests, tell the Scheduler
            // to proceed and schedule the job as normal.
            if (_requestedFeatures.Count <= 0)
            {
                return ActivationFilterResponse.StartJob;
            }

            // If the job is flagged as backfill then there is a possibility that allowing it to
            // run might block a higher priority job from running due to the use of shared licenses.
            if (backfill)
            {
                // Some users may wish to disable this feature so that licenses are utilized as much as
                // possible.

                // Let another job take the backfill slot if there is one.
                return ActivationFilterResponse.DoNotRunKeepResourcesAllowOtherJobsToSchedule;
            }

            // Find out what licenses are available
            Cache.Load(schedulerPass, jobIndex);

            if (Cache.licenseInfo.PollServerFailed)
            {
                // FlexLM server is down. Hold the job and let other non-licensed Jobs
                // to be scheduled. Administrator can release the held jobs when the issue
                // is resolved using job modify /holduntil or just wait for the HoldUntil
                // time to elapse.
                return ActivationFilterResponse.HoldJobReleaseResourcesAllowOtherJobsToSchedule;
            }

            // Iterate through the requesting features and determine whether
            // there are enough license tokens.
            IDictionaryEnumerator iterator = _requestedFeatures.GetEnumerator();

            // Run lmutils lmstat to verify the available license tokens.
            while (iterator.MoveNext())
            {
                string featureName = iterator.Key as string;
                int numberRequested = (int)iterator.Value;
                if (Cache.licenseInfo.LicenseDirectory.ContainsKey(featureName))
                {
                    int total = Cache.licenseInfo.LicenseDirectory[featureName].Total;
                    int inUse = Cache.licenseInfo.LicenseDirectory[featureName].InUse;

                    if (numberRequested > total)
                    {
                        if (_growing)
                        {
                            // Ignore additional resources
                            returnDecision = ActivationFilterResponse.RejectAdditionOfResources;
                            break;
                        }

                        // Not enough licenses purchased to ever satisfy this job. 
                        SetProgressMessage("FlexLM error: Not enough licenses for " + featureName);
                        returnDecision = ActivationFilterResponse.FailJob;
                        break;
                    }
                    else if (numberRequested > total - inUse)
                    {
                        if (_growing)
                        {
                            // Ignore additional resources
                            returnDecision = ActivationFilterResponse.RejectAdditionOfResources;
                            break;
                        }

                        // Not enough resources to run this job on this pass. Reserve the licenses that are
                        // available and wait for more to become available
                        Cache.Reserve(featureName, total - inUse);
                        returnDecision = ActivationFilterResponse.DoNotRunKeepResourcesAllowOtherJobsToSchedule;
                    }
                    else
                    {
                        Cache.Reserve(featureName, numberRequested);
                    }
                }
                else
                {
                    if (_growing)
                    {
                        // Ignore additional resources
                        returnDecision = ActivationFilterResponse.RejectAdditionOfResources;
                        break;
                    }

                    // Requested a license we don't have
                    SetProgressMessage("FlexLM error: License " + featureName + " not found");
                    returnDecision = ActivationFilterResponse.FailJob;
                    break;
                }
            }

            // If we want the resources for this job, persist the updates
            if ((returnDecision == ActivationFilterResponse.DoNotRunKeepResourcesAllowOtherJobsToSchedule) ||
                (returnDecision == ActivationFilterResponse.StartJob))
            {
                Cache.Save();
            }

            return returnDecision;
        }

        private void LogEvent(string Message)
        {
            FlexLM_Activation_Sample.EventWriteInvalidJobXml("StreamBased", _schedulerPass, _jobIndex, _backfill, _resourceCount, Message);
        }

        private void ParseJobXml(Stream jobXml)
        {
            XmlDocument inputJob = new XmlDocument();

            inputJob.Load(jobXml);
            // The base XML node in the document.
            XmlNode job = inputJob.DocumentElement;

            // Create the namespace that is used for the job XML schema.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(inputJob.NameTable);
            nsmgr.AddNamespace("ab", @"http://schemas.microsoft.com/HPCS2008R2/scheduler/");

            // Get the job ID in case the job needs to be failed.
            XmlNode jobidnode = job.SelectSingleNode(@"@Id", nsmgr);
            if (jobidnode != null)
            {
                string JobIdStr = jobidnode.InnerXml;
                Int32.TryParse(JobIdStr, out _jobId);
            }
            else
            {
                LogEvent(@"Unable to extract jobid from job file");
                return;
            }

            // Get the job state to determine if this is an initial allocation or growing.
            jobidnode = job.SelectSingleNode(@"@State", nsmgr);
            if (jobidnode != null)
            {
                string JobStateStr = jobidnode.InnerXml;
                if (String.IsNullOrEmpty(JobStateStr))
                {
                    LogEvent(@"Unable to extract job state from job file");
                    return;
                }
                if (string.Compare(JobStateStr, "Running", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    _growing = true;
                }
            }
            else
            {
                LogEvent(@"Unable to extract job state from job file");
                return;
            }


            // Get the SoftwareLicense job property.
            XmlNode jobnode = job.SelectSingleNode(@"@SoftwareLicense", nsmgr);

            if (jobnode == null)
            {
                // This job doesn't require licenses
                return;
            }

            // Extract the value of the SoftwareLicense property.
            string licenseStr = jobnode.InnerXml;
            if (String.IsNullOrEmpty(licenseStr))
            {
                // This job doesn't require licenses
                return;
            }

            // Get all of the license feature:number pairs.
            char[] split = { ',' };
            string[] tokens = licenseStr.Split(split);

            // Loop over all feature:number pairs.
            foreach (string str in tokens)
            {
                string[] ftokens = str.Split(':');
                if (ftokens.Length == 2)
                {
                    Int32 numberOfLicenses = 0;
                    if (Int32.TryParse(ftokens[1], out numberOfLicenses))
                    {
                        _requestedFeatures[ftokens[0]] = numberOfLicenses;
                    }
                    else
                    {
                        if (ftokens[1] == "*")
                        {
                            _requestedFeatures[ftokens[0]] = _resourceCount;
                        }
                        else
                        {
                            LogEvent(@"number of licenses not in an integer format");
                        }
                    }
                }
                else
                {
                    LogEvent(@"Number of licenses not specified");
                }
            }
            return;
        }

        /// <summary>
        /// Fail job with the supplied message.
        /// </summary>
        /// <param name="message">
        /// The message to attach to the failed job.
        /// </param>
        private void SetProgressMessage(String message)
        {
            LogEvent(message);
            try
            {
                using (Microsoft.Hpc.Scheduler.IScheduler scheduler = new Microsoft.Hpc.Scheduler.Scheduler())
                {
                    scheduler.Connect("localhost");
                    Microsoft.Hpc.Scheduler.ISchedulerJob job = scheduler.OpenJob(_jobId);
                    job.ProgressMessage = message;
                    job.Commit();
                }
            }
            catch (Exception e)
            {
                LogEvent("Failed to report job rejection: " + e.Message);
            }
        }

        public void RevertActivation(
            Stream jobXml,
            int schedulerPass,
            int jobIndex,
            bool backfill,
            int resourceCount)
        {
            // any cancelation code here (release licenses, etc)
        }

        // note that IFilterLifespan is optional.  If you do not need setup/teardown code executed
        // do not declare or implement IFilterLifespan 

        public void OnFilterLoad()
        {
            // setup code here
        }

        public void OnFilterUnload()
        {
            // teardown code here
        }
    }
}
