// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System.IO;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Hpc.Scheduler.AddInFilter.HpcClient;

// This sample code is intended as a diagnostic aid and provides two main features:
//      1: It is a "combination filter" that implements all of the filter interfaces.
//          In all cases the filter methods return "success".
//          A single class implements all interfaces and shows one way
//          "shared state" can be established between a submission filter and an
//          activation filter.
//      
//      2: Diagnostic logging to the windows application log is provided.
//          This is useful for confirming that all methods are called
//          and the order in which they are called.
// 

namespace CombinedDiagnosticFilterWithEventLogging
{
    public class ActvSubCombo : ISubmissionFilter, IActivationFilter, IFilterLifespan
    {
        private EventLog _log = new EventLog();
        public TextWriter logFile = null;

        private int _onFilterLoadCalls = 0;
        private int _onFilterUnloadCalls = 0;

        private int _filterSubmissionCalls = 0;
        private int _revertSubmissionCalls = 0;

        private int _filterActivationCalls = 0;
        private int _revertActivationCalls = 0;

        private string _location = Assembly.GetExecutingAssembly().Location;


        public SubmissionFilterResponse FilterSubmission(Stream jobXmlIn, out Stream jobXmlModified)
        {
            LogEventMsg("FilterSubmission");

            SubmissionFilterResponse retval = SubmissionFilterResponse.SuccessNoJobChange;

            jobXmlModified = null;

            _filterSubmissionCalls++;

            return retval;
        }

        public void RevertSubmission(Stream jobXml)
        {
            LogEventMsg("RevertSubmission");

            _revertSubmissionCalls++;
        }

        private void LogEventMsg(string text)
        {
            _log.WriteEntry(text + ": " + _location);
        }

        public void OnFilterLoad()
        {
            LogEventMsg("OnFilterLoad");
            _onFilterLoadCalls++;
        }

        public void OnFilterUnload()
        {
            LogEventMsg("OnFilterUnload");

            _onFilterUnloadCalls++;
        }

        public ActivationFilterResponse FilterActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            LogEventMsg("FilterActivation");

            ActivationFilterResponse retVal = ActivationFilterResponse.StartJob;

            _filterActivationCalls++;

            return retVal;
        }

        public void RevertActivation(Stream jobXml, int schedulerPass, int jobIndex, bool backfill, int resourceCount)
        {
            LogEventMsg("FilterActivation");

            _revertActivationCalls++;
        }

        private string Stream2String(Stream s)
        {
            string ret = null;

            s.Position = 0;

            byte[] content = new byte[s.Length];

            s.Read(content, 0, (int)s.Length);

            s.Position = 0;

            using (MemoryStream m = new MemoryStream(content))
            using (StreamReader reader = new StreamReader(m))
            {
                string xml = reader.ReadToEnd();

                ret = xml;
            }

            return ret;
        }

        public ActvSubCombo()
        {
            _log.Source = "ActvSubCombo Filter";

            _log.Log = "Application";
        }
    }
}
