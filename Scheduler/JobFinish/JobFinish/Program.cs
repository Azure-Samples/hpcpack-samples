// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace JobFinish
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string? clustername = Environment.GetEnvironmentVariable("CCP_SCHEDULER");

            using IScheduler scheduler = new Scheduler();
            scheduler.Connect(clustername);
            ISchedulerJob job = scheduler.CreateJob();

            job.UnitType = JobUnitType.Core;
            job.MinimumNumberOfCores = 1;
            job.MaximumNumberOfCores = 1;
            scheduler.AddJob(job);

            ISchedulerTask task = job.CreateTask();
            task.CommandLine = @"ping -t localhost";
            job.AddTask(task);

            scheduler.SubmitJob(job, null, null);
            Console.WriteLine("Job {0} Submitted ", job.Id);
            Console.WriteLine("Sleep 5 seconds...");

            await Task.Delay(5 * 1000);

            job.Refresh();

            Console.WriteLine("Job id: {0}, job state: {1}", job.Id, job.State);
            Console.WriteLine("Call job.Finish()");

            ((ISchedulerJobV3)job).Finish();

            Console.WriteLine("Sleep 3 seconds...");
            await Task.Delay(3 * 1000);

            job.Refresh();
            task.Refresh();

            Console.WriteLine("After job.Finish(), job id: {0}, job state: {1}", job.Id, job.State);
            Console.WriteLine("Output message: {0}", task.Output);
        }
    }
}
