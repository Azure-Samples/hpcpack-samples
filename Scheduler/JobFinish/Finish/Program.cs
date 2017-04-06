// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace Finish
{
    class Program
    {
        static void Main(string[] args)
        {
            IScheduler scheduler = new Scheduler();
            string clustername = null;
            string username = null;

            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: Finish clustername username ");
                return;
            }
            clustername = args[0];
            username = args[1];

            scheduler.Connect(clustername);

            ISchedulerJob job = scheduler.CreateJob();

            job.UnitType = JobUnitType.Core;
            job.MinimumNumberOfCores = 1;
            job.MaximumNumberOfCores = 1;

            
            scheduler.AddJob(job);

            ISchedulerTask task = job.CreateTask();
            
            task.CommandLine = @"ping -t localhost";            

            job.AddTask(task);


            scheduler.SubmitJob(job, username, null);
            Console.WriteLine("job {0} Submitted ", job.Id);

            Thread.Sleep(12 * 1000);

            job.Refresh();
            Console.WriteLine("Job {0} State {1}", job.Id,job.State);
            
            ((ISchedulerJobV3) job).Finish();
            Thread.Sleep(10000);
            job.Refresh();
            task.Refresh();
            Console.WriteLine("After finish Job {0} State {1} message {2}", job.Id, job.State,task.Output);
        }
    }
}
