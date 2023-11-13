// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace ExpandedPriorities
{
    class ExpandedPriorities
    {
        static void Main(string[] args)
        {
            string clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");

            //Create scheduler object and connect to scheduler
            using (IScheduler scheduler = new Scheduler())
            {
                scheduler.Connect(clusterName);
                //Create job
                ISchedulerJob job = scheduler.CreateJob();

                //you can set expanded priority by directly setting it to a value
                job.ExpandedPriority = 4000;

                //you can also use the ExpandedPriority class in Microsoft.Hpc.Scheduler.Properties for added convenience
                //Adding integers to existing priority levels:
                job.ExpandedPriority = ExpandedPriority.AboveNormal + 500;

                //This is equivalent to setting the value directly...
                job.ExpandedPriority = 3500;

                //...subtracting from a higher priority "bucket"...
                job.ExpandedPriority = ExpandedPriority.Highest - 500;

                //...or converting a JobPriorityEnum to ExpandedPriority...
                job.ExpandedPriority = ExpandedPriority.JobPriorityToExpandedPriority((int)JobPriority.AboveNormal) + 500;

                //For backwards compatibility with V1 and V2 clusters, Priority can be set from expanded priority
                job.Priority = ExpandedPriority.ExpandedPriorityToJobPriority(2345);

                //You can set the priority to its highest possible value without moving into the next priority "bucket"
                job.ExpandedPriority = ExpandedPriority.CeilingOfPriorityBucket(1001);

                //ExpandedPriority.LevelsPerPriorityBucket will tell you of how many priority levels there are between buckets
                job.ExpandedPriority = ExpandedPriority.AboveNormal + ExpandedPriority.LevelsPerPriorityBucket;

                scheduler.Close();
            }
        }
    }
}
