using System;
using System.Collections.Generic;

/*
Sample Microsoft Windows HPC Version 3 Activation Filter

Demonstrates integration with FlexLM server so that:

 * Jobs are failed if there are insufficient licenses purchased or they request invalid licenses.
    
 * Jobs are not run until sufficient licenses are available.

 * If a Job requests a license that is reserved for a higher priority Job then it
   will not run until the higher priority Job has all its licenses.
 
 * Licenses are reserved for a configurable time period after a Job is started to allow the Job
   to contact the FlexLM server and claim the licenses. While a license is reserved, the
   Activation FIlter will not hand the license(s) to other Jobs in the queue.

 * If the FlexLM server is down then Jobs requiring licenses are "Held" for a default period of time.
   The Administrator can wake the jobs early using "job modify /holduntil
    
*/
 
namespace FlexLM
{
    // The return code from the activation filter affects the schedulers next step.
    public enum ReturnValues : int
    {
        StartJob = 0,
        DontRunHoldQueue = 1,
        DontRunKeepResourcesAllowOtherJobsToSchedule = 2,
        HoldJobReleaseResourcesAllowOtherJobsToSchedule = 3,
        FailJob = 4,
        AddResourcesToRunningJob = 0,
        RejectAdditionofResources = 1
    }

    class Program
    {
        // Parameters passed from Scheduler
        static string jobxml;
        static int schedulerPass;
        static int jobIndex;
        static bool backfill;
        static int resourceCount;

        /// <summary>
        /// Entry from HPC Scheduler
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            ReturnValues returnValue = ReturnValues.FailJob;
            try
            {
                ParseArgs(args);

                returnValue = Job.ProcessJobXml(jobxml, schedulerPass, jobIndex, backfill, resourceCount);
            }
            catch (Exception ex)
            {
                FlexLM_Activation_Sample.EventWriteException(ex.Message);                
            }
            return (int)returnValue;
        }

        /// <summary>
        /// Analyse inputs from Scheduler and save values in private variables
        /// </summary>
        /// <param name="args"></param>
        static void ParseArgs(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Usage: FlexLM.exe <inputJobTermFile> <scheduler pass> <job index in pass> <backfill> <resource count>");
                throw new ArgumentException("Usage: FlexLM.exe <inputJobTermFile> <scheduler pass> <job index in pass> <backfill> <resource count>");
            }
            jobxml = args[0];

            schedulerPass = ParseInt("scheduler pass", args[1]);
            jobIndex = ParseInt("job index", args[2]);
            backfill = bool.Parse(args[3]);
            resourceCount = ParseInt("resource count", args[4]);
        }

        /// <summary>
        /// Helper function
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static int ParseInt(string name, string value)
        {
            try
            {
                return int.Parse(value);
            }
            catch
            {
                throw new ArgumentException(name + " is not an integer");
            }
        }
    }
}
