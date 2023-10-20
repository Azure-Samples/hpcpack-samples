// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace JobUse
{
    /// <summary>
    /// Location class is for use in displaying the current name of the app being used (in case it has been renamed)
    /// </summary>
    public class Location
    {
        public string locationFullPath = null;

        public Location()
        {
            Assembly A = Assembly.GetAssembly(this.GetType());
            this.locationFullPath = A.Location;
        }
    }

    class Program
    {
        // parameter strings
        const string JOBID = @"/id:";       // single integer or range seperated by dash
        const string USERNAME = @"/user:";     // Filter jobs that are owned by this user
        const string PROJECTNAME = @"/project:";  // Filter jobs that only have this project
        const string PROJECTIGNORE = @"/projecti:";  // Filter jobs that only have this project, ignore case
        const string PROJECTBEGIN = @"/projectb:";  // Filter jobs that only have this project that begin with this
        const string PROJECTCONTAIN = @"/projectc:";  // Filter jobs that only have this project contains this
        const string NODESONLY = @"/nodes"; // show node duration, not core duration
        const string DETAILED = @"/detailed";  // show duration of each core or node
        const string VERBOSE = @"/verbose";   // Useful for debugging

        const string HELPINFO = @"/?";

        const string DASH = @"-";  // For use with job ranges

        // Messages
        const string PARSEIDERR = @": Unable to parse job id";
        const string NOJOBSFOUND = @": No jobs found";
        const string CORERUNNING = @"Running";

        // members used within
        static int jobId = 0;
        static int jobIdRange = 0;
        static string userName = null;
        static string projectName = null;
        static string projectNameIgnore = null;
        static string projectNameBegins = null;
        static string projectNameContains = null;
        static string clusterName = "localhost";
        static DateTime dtInitial = DateTime.MinValue;
        static DateTime dtFinal = DateTime.MaxValue;
        static DateTime dtCurrent = DateTime.UtcNow;
        static bool bNodesOnly = false;  // True means check node duration; false, check core duration
        static bool bDetailed = false;  // True, show cores/nodes for each job; false, show summary for the job
        static bool bVerbose = false;  // Show extra debugging tracing

        // Used in conjunction with Location class for current name of application
        static string sExeFilename = null;

        static int iFilteredJobs = 0;
        static int iAllJobThreads = 0;
        static TimeSpan tsAllJobUsage = TimeSpan.Zero;

        static int iRoundToSecondsMinimum = 10;  // If a TimeSpan takes more than 10 seconds, round off the milliseconds

        /// <summary>
        /// Command Line entrypoint
        /// </summary>
        /// <param name="args">parsed parameter string</param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            // In case it has been renamed, find out what the current name in use is
            // Filename for Help or Error messages
            Location l = new Location();
            sExeFilename = Path.GetFileNameWithoutExtension(Path.GetFileName(l.locationFullPath));

            if (ParseParameters(args) == false)
            {
                return -1;
            }

            // If no job id or user is defined, give help message
            if ((jobId == 0) && (userName == null))
            {
                Help(sExeFilename);
                return -1;
            }

            try
            {
                using (IScheduler scheduler = new Scheduler())
                {
                    ISchedulerCollection jobs = null;
                    IFilterCollection filter = null;
                    ISortCollection sort = null;

                    scheduler.Connect(clusterName);

                    // Filter the jobs requested by the parameters, either a single job or a range, and/or a user
                    filter = scheduler.CreateFilterCollection();
                    if (jobIdRange != 0)
                    {
                        filter.Add(FilterOperator.GreaterThanOrEqual, JobPropertyIds.Id, jobId);
                        filter.Add(FilterOperator.LessThanOrEqual, JobPropertyIds.Id, jobIdRange);
                    }
                    else
                    {
                        filter.Add(FilterOperator.Equal, JobPropertyIds.Id, jobId);
                    }

                    if (userName != null)
                    {
                        filter.Add(FilterOperator.Equal, JobPropertyIds.Owner, userName);
                    }

                    // Sort the jobs relative to when they were created
                    sort = scheduler.CreateSortCollection();
                    sort.Add(SortProperty.SortOrder.Ascending, PropId.Job_CreateTime);

                    jobs = scheduler.GetJobList(filter, sort);

                    // Be sure the job(s) can be found
                    if (jobs.Count == 0)
                    {
                        Console.Error.WriteLine(sExeFilename + NOJOBSFOUND);
                        return -1;
                    }
                    else
                    {
                        foreach (ISchedulerJob job in jobs)
                        {
                            if (bVerbose)
                            {
                                Console.WriteLine("job {0} project: {1}", job.Id, job.Project);
                            }

                            // Is the job part of the project?
                            if ((projectName != null) && (job.Project != projectName))
                            {
                                continue;
                            }
                            else if ((projectNameIgnore != null) &&
                                (job.Project.Equals(projectNameIgnore, StringComparison.InvariantCultureIgnoreCase) == false))
                            {
                                continue;
                            }
                            else
                            {
                                // Exact match overrides StartsWith and Contains, only test them if previous tests are not attempted
                                if ((projectNameBegins != null) && (job.Project.StartsWith(projectNameBegins, StringComparison.InvariantCultureIgnoreCase) == false))
                                {
                                    continue;
                                }
                                if ((projectNameContains != null) && (job.Project.Contains(projectNameContains) == false))
                                {
                                    continue;
                                }
                            }

                            iFilteredJobs++;

                            Console.WriteLine("Job {0} - {1}", job.Id, job.State);
                            Console.WriteLine(job.Name);
                            CollectAllocatedInfo(job);
                        }
                    }

                    // Jobs were found within the range, but none had the appropriate project criteria
                    if (iFilteredJobs <= 0)
                    {
                        Console.Error.WriteLine(sExeFilename + NOJOBSFOUND);
                        return -1;
                    }

                    // Round up/down to seconds if greater than minimum
                    if (tsAllJobUsage.TotalSeconds >= iRoundToSecondsMinimum)
                    {
                        if (tsAllJobUsage.Milliseconds >= 500)
                        {
                            tsAllJobUsage = tsAllJobUsage.Add(TimeSpan.FromMilliseconds(1000 - tsAllJobUsage.Milliseconds));
                        }    
                        else
                        {
                            tsAllJobUsage = tsAllJobUsage.Subtract(TimeSpan.FromMilliseconds(tsAllJobUsage.Milliseconds));
                        }
                    }

                    // If a range of jobs was given, or other criteria created selected multiple jobs, show summary
                    if ((jobIdRange > 0) || (iFilteredJobs > 1))
                    {
                        Console.WriteLine("Number of jobs:  {0}", iFilteredJobs);
                        Console.WriteLine("Number of {0}: {1}", bNodesOnly ? "nodes" : "cores", iAllJobThreads);
                        Console.WriteLine("{0} usage across all jobs: {1}", bNodesOnly ? "Node" : "Core", tsAllJobUsage);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Exception!");
                Console.Error.WriteLine(e.Message);
            }

            return iFilteredJobs;
        }

        /// <summary>
        /// ParseParameters - parse through parameters, set up options
        /// </summary>
        /// <param name="args">args - string array of parameters</param>
        /// <returns>true if code should continue; false if it should exit (e.g. requested and gave help info)</returns>
        private static bool ParseParameters(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string str = args[i].Trim();

                if (str[0] != '/')
                {
                    Help(sExeFilename);
                    return false;
                }

                // Job Id and/or range of ids
                if (str.StartsWith(JOBID, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Does it contain a dash, i.e. is it a range?
                    if (str.Contains(DASH))
                    {
                        // Set jobIdRange
                        int iDashIndex = str.IndexOf(DASH);
                        if (!int.TryParse(str.Substring(iDashIndex + 1), out jobIdRange))
                        {
                            Console.Error.WriteLine(sExeFilename + PARSEIDERR);
                            return false;
                        }
                        if (!int.TryParse(str.Substring(JOBID.Length, iDashIndex - JOBID.Length), out jobId))
                        {
                            Console.Error.WriteLine(sExeFilename + PARSEIDERR);
                            return false;
                        }
                        if (bVerbose)
                        {
                            Console.WriteLine("jobId: {0}  jobIdRange: {1}", jobId, jobIdRange);
                        }
                    }
                    else if (!int.TryParse(str.Substring(JOBID.Length), out jobId))
                    { // Not a range, assume just an integer
                        Console.Error.WriteLine(sExeFilename + PARSEIDERR);
                        return false;
                    }
                }
                else if (str.StartsWith(USERNAME, StringComparison.InvariantCultureIgnoreCase))
                {  // User Name
                    userName = str.Substring(USERNAME.Length);
                    if (bVerbose)
                    {
                        Console.WriteLine("user: {0}", userName);
                    }
                }
                else if (str.StartsWith(PROJECTNAME, StringComparison.InvariantCultureIgnoreCase))
                {  // Project name
                    projectName = str.Substring(PROJECTNAME.Length);
                    if (bVerbose)
                    {
                        Console.WriteLine("{0} {1}", PROJECTNAME, projectName);
                    }
                }
                else if (str.StartsWith(PROJECTIGNORE, StringComparison.InvariantCultureIgnoreCase))
                {  // Project name
                    projectNameIgnore = str.Substring(PROJECTIGNORE.Length);
                    if (bVerbose)
                    {
                        Console.WriteLine("{0} {1}", PROJECTIGNORE, projectNameIgnore);
                    }
                }
                else if (str.StartsWith(PROJECTBEGIN, StringComparison.InvariantCultureIgnoreCase))
                {  // Project name
                    projectNameBegins = str.Substring(PROJECTBEGIN.Length);
                    if (bVerbose)
                    {
                        Console.WriteLine("{0} {1}", PROJECTBEGIN, projectNameBegins);
                    }
                }
                else if (str.StartsWith(PROJECTCONTAIN, StringComparison.InvariantCultureIgnoreCase))
                {  // Project name
                    projectNameContains = str.Substring(PROJECTCONTAIN.Length);
                    if (bVerbose)
                    {
                        Console.WriteLine("{0} {1}", PROJECTCONTAIN, projectName);
                    }
                }
                else if (str.Equals(NODESONLY, StringComparison.InvariantCultureIgnoreCase))
                { // Nodes (instead of cores) duration displayed
                    bNodesOnly = true;
                }
                else if (str.Equals(DETAILED, StringComparison.InvariantCultureIgnoreCase))
                { // Detailed display
                    bDetailed = true;
                }
                else if (str.Equals(VERBOSE, StringComparison.InvariantCultureIgnoreCase))
                {  // Verbose display (debugging)
                    bVerbose = true;
                }
                else
                {  // Not recognized, other than perhaps requesting help.  Show an error if help not requested, but show help regardless
                    if (!str.Equals(HELPINFO, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.Error.WriteLine("{0}: Unrecognized parameter {1}", sExeFilename, str);
                    }
                    Help(sExeFilename);
                    return false;
                }
            }
            return true;  // All parameters were recognized, continue
        }

        // To be used within rows of a PropertyIdCollection containing AllocationProperties
        // Calling prop.Add() in this order is important for accurate use
        internal enum RowIndex { NodeName = 0, NodeId, CoreId, StartTime, EndTime };

        /// <summary>
        /// collectAllocatedInfo - From a single job, collect a rowset of allocated resources and pass it on to core or node sorting
        /// </summary>
        /// <param name="job">ISchedulerJob job to collect allocation history</param>
        private static void CollectAllocatedInfo(ISchedulerJob job)
        {
            if (bVerbose)
            {
                Console.WriteLine("Entering collectAllocatedInfo:  job {0} project: {1}", job.Id, job.Project);
            }

            IPropertyIdCollection props = new PropertyIdCollection();
            props.Add(AllocationProperties.NodeName);
            props.Add(AllocationProperties.NodeId);
            props.Add(AllocationProperties.CoreId);
            props.Add(AllocationProperties.StartTime);
            props.Add(AllocationProperties.EndTime);

            // OpenJobAllocationHistory returns information sorted by ascending AllocationProperties.StartTime
            using (ISchedulerRowEnumerator rows = job.OpenJobAllocationHistoryEnumerator(props))
            {
                if (bNodesOnly)
                {
                    NodeDuration(rows);
                }
                else
                {
                    CoreDuration(rows);
                }
            }
            return;
        }

        /// <summary>
        /// CoreDuration - Sort information from the rows returned from OpenJobAllocationHistoryEnumerator
        /// </summary>
        /// <param name="rows">RowSet, allocated Core information from the job</param>
        private static void CoreDuration(ISchedulerRowEnumerator rows)
        {
            TimeSpan tsTotal = new TimeSpan(0);
            DateTime firstStart = DateTime.MaxValue;
            DateTime lastEnd = DateTime.MinValue;
            int iTotalThreads = 0;

            if (bVerbose)
            {
                Console.WriteLine("Entering CoreDuration");
            }

            foreach (PropertyRow row in rows)
            {
                DateTime dtEnd = (row[(int)RowIndex.EndTime].Id == AllocationProperties.EndTime) ? (DateTime)row[(int)RowIndex.EndTime].Value : dtCurrent;
                // Show each core only if /detailed was set
                if (bDetailed)
                {
                    Console.WriteLine("{0} {1}.{2} Start: {3} End: {4}",
                        row[(int)RowIndex.NodeName].Value,
                        row[(int)RowIndex.NodeId].Value,
                        row[(int)RowIndex.CoreId].Value,
                        row[(int)RowIndex.StartTime].Value,
                        ((dtEnd != dtCurrent) ? dtEnd.ToString() : CORERUNNING));
                }

                // Add the amount of time spent on using this core
                tsTotal += dtEnd - (DateTime)row[(int)RowIndex.StartTime].Value;

                // Set the earliest and latest times used by the job
                if (firstStart > (DateTime)row[(int)RowIndex.StartTime].Value)
                {
                    firstStart = (DateTime)row[(int)RowIndex.StartTime].Value;
                }
                if (lastEnd < dtEnd)
                {
                    lastEnd = dtEnd;
                }

                // Increment the number of cores opened by the job
                // Note:  The same core can be opened and closed multiple times and each duration will be incremented
                iTotalThreads++;
            }
            iAllJobThreads += iTotalThreads;
            tsAllJobUsage += tsTotal;

            // Round up/down to seconds
            if (bVerbose)
            {
                Console.WriteLine("Total Seconds:  {0}", tsTotal.TotalSeconds);
            }

            if (tsTotal.TotalSeconds >= iRoundToSecondsMinimum)
            {
                if (tsTotal.Milliseconds >= 500)
                {
                    tsTotal = tsTotal.Add(TimeSpan.FromSeconds(1));
                }
                //tsTotal = tsTotal.Subtract(TimeSpan.FromMilliseconds(tsTotal.Milliseconds));
                tsTotal = TimeSpan.FromSeconds((int)tsTotal.TotalSeconds);
            }

            Console.WriteLine("Total cores: {0} Total core usage: {1}", iTotalThreads, tsTotal.ToString());
        }

        /// <summary>
        /// NodeDuration - Sort information from the rows returned from OpenJobAllocationHistoryEnumerator
        /// </summary>
        /// <param name="rows">RowSet, allocated Core information from the job, to be resorted into node allocation</param>
        private static void NodeDuration(ISchedulerRowEnumerator rows)
        {
            if (bVerbose)
            {
                Console.WriteLine("Entering NodeDuration");
            }

            TimeSpan tsTotal = new TimeSpan(0);

            List<NodeUse> nodeList = new List<NodeUse>();

            // Convert core rowset into node list
            foreach (PropertyRow row in rows)
            {
                // Find the last item in this list that uses this node
                int iIndex = nodeList.FindLastIndex(
                    delegate (NodeUse n)
                    {
                        return (n.NodeId == (int)row[(int)RowIndex.NodeId].Value);
                    }
                );

                // If this node does not yet exist, or if the current start is beyond the endtime in the list, add a new list item
                if ((iIndex < 0) || (nodeList[iIndex].EndTime < (DateTime)row[(int)RowIndex.StartTime].Value))
                {
                    if (bVerbose)
                    {
                        Console.WriteLine("Add item to Node List");
                    }

                    // If the core is still running, set the end time to maximum so all other searches will be swallowed
                    DateTime coreEndTime = (row[(int)RowIndex.EndTime].Id == AllocationProperties.EndTime) ? (DateTime)row[(int)RowIndex.EndTime].Value : DateTime.MaxValue;
                    NodeUse nu = new NodeUse((int)row[(int)RowIndex.NodeId].Value,
                        (string)row[(int)RowIndex.NodeName].Value,
                        (DateTime)row[(int)RowIndex.StartTime].Value,
                        coreEndTime);
                    nodeList.Add(nu);
                    if (bVerbose)
                    {
                        Console.WriteLine("Added Node List item for: {0}", (string)row[(int)RowIndex.NodeName].Value);
                    }
                }
                else
                { // A node was found in the list that overlaps this core's duration
                    if (row[(int)RowIndex.EndTime].Id != AllocationProperties.EndTime)
                    {
                        // If the current core is still running, set the end time to maximum
                        nodeList[iIndex].EndTime = DateTime.MaxValue;
                    }
                    else if ((DateTime)row[(int)RowIndex.EndTime].Value > nodeList[iIndex].EndTime)
                    {
                        // If the current core endtime is greater than the list node endtime, extend the nodes duration
                        nodeList[iIndex].EndTime = (DateTime)row[(int)RowIndex.EndTime].Value;
                    }
                }
            }

            if (bVerbose)
            {
                Console.WriteLine("Node List created");
            }

            // Add all node duration and display information if appropriate
            foreach (NodeUse nodeUse in nodeList)
            {
                // Show each node only if /detailed was set
                if (bDetailed)
                {
                    Console.Write("{0} {1} Start: {2} End: ",
                        nodeUse.NodeName,
                        nodeUse.NodeId,
                        nodeUse.StartTime);
                    if (nodeUse.EndTime != DateTime.MaxValue)
                    {
                        Console.WriteLine((DateTime)nodeUse.EndTime);
                    }
                    else
                    {
                        Console.WriteLine(CORERUNNING);
                    }
                }

                if (bVerbose)
                {
                    Console.WriteLine("dtCurrent:  {0}", dtCurrent);
                }

                // If the node still has a core running, set the end length to the current time
                if (nodeUse.EndTime == DateTime.MaxValue)
                {
                    nodeUse.EndTime = dtCurrent;
                }

                // Add the amount of time spent on using this node
                tsTotal += nodeUse.EndTime - nodeUse.StartTime;
            }

            iAllJobThreads += nodeList.Count;
            tsAllJobUsage += tsTotal;

            // Round up/down to seconds
            if (tsTotal.TotalSeconds >= iRoundToSecondsMinimum)
            {
                if (tsTotal.Milliseconds >= 500)
                {
                    tsTotal = tsTotal.Add(TimeSpan.FromSeconds(1));
                }

                tsTotal = TimeSpan.FromSeconds((int)tsTotal.TotalSeconds);
            }

            Console.WriteLine("Total nodes: {0} Total node usage: {1}", nodeList.Count, tsTotal);
        }

        /// <summary>
        /// Show the potential parameters for the tool
        /// </summary>
        /// <param name="sFilename">Tool name currently in use</param>
        static void Help(string sFilename)
        {
            Console.WriteLine(sFilename);
            Console.WriteLine("  - Report of job's core or node use");
            Console.WriteLine(sFilename);
            Console.WriteLine("  - /id:<int>[-<int>]   Job id or range of ids to consider");
            Console.WriteLine("  - /user:<string>      Only jobs owned by this user are considered");
            Console.WriteLine("  - /project:<string>   Only jobs with the specific Project are considered");
            Console.WriteLine("  - /projecti:<string>  Only jobs with the specific Project are considered, case insensitive");
            Console.WriteLine("  - /projectb:<string>  Only jobs that begin with the specific Project are considered");
            Console.WriteLine("  - /projectc:<string>  Only jobs that contain the specific Project are considered");
            Console.WriteLine("  - /detailed           If present, list each core instead of summary of job use");
            Console.WriteLine("  - /nodes              If present, list each node (instead of the default core)");
            Console.WriteLine("");
            Console.WriteLine("    Note:  At least one item (Job id or range, user) must be declared");
            Console.WriteLine("           /project and /projecti cannot be combined with other project parameters");
            Console.WriteLine("           /projectb and /projectc can be combined together");
            Console.WriteLine("");
            Console.WriteLine("Examples:");
            Console.WriteLine(sFilename + " /id:45                  Core usage summary of Job 45");
            Console.WriteLine(sFilename + " /id:45 /detailed        Core usage of each core used by Job 45");
            Console.WriteLine(sFilename + " /id:45 /nodes           Node usage summary of Job 45");
            Console.WriteLine(sFilename + " /id:45 /nodes /detailed Node usage of each node used by Job 45");
            Console.WriteLine(sFilename + " /id:10-20               Core usage summary of each job from 10-20");
            Console.WriteLine(sFilename + " /id:10-20 /project:G    Core usage summary of each G project job, 10-20");
            Console.WriteLine(sFilename + " /id:10-20 /projectb:Red Core usage summary of jobs that begin with Red project within 10-20");
            Console.WriteLine(sFilename + " /user:John              Core usage summary of each job John owns");
            Console.WriteLine(sFilename + " /id:10-20 /user:John    Core usage summary of each job John owns, 10-20");
        }

        /// <summary>
        /// NodeUse contain the node id and name, start and end time
        /// The id & name are redundant, but for convenience of display are both kept
        /// The starttime is set when the object is created and cannot be changed
        /// The endtime can be adjusted to extend the duration of the node's use
        /// </summary>
        internal class NodeUse
        {
            int _nodeId = 0;
            string _nodeName = null;
            DateTime _startTime = DateTime.MaxValue;

            internal int NodeId { get { return _nodeId; } }
            internal string NodeName { get { return _nodeName; } }
            internal DateTime StartTime { get { return _startTime; } }
            internal DateTime EndTime { get; set; }

            internal NodeUse(int nodeId, string nodeName, DateTime startTime, DateTime endTime)
            {
                _nodeId = nodeId;
                _nodeName = nodeName;
                _startTime = startTime;
                EndTime = endTime;
            }
        }
    }
}
