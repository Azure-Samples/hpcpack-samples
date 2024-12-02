// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace HPCSchedulerBasics
{
    class Program
    {
        static ISchedulerJob? job;
        static ISchedulerTask? task;
        private static readonly ManualResetEvent jobFinishedEvent = new(false);

        static void ShowHelp()
        {
            string help = @"
                Usage:
                {0} [-u <user name>] [-c <cluster name>] [-d]

                Options:
                -u Required. It provides the username to connect to the cluster.
                -c Optional. It provides the HPC cluster name. If you don't provide it, the default value will be %CCP_SCHEDULER%.
                -d Optional. Runs in debug mode, and it will print more useful information for debugging.
                ";
            Console.WriteLine(String.Format(help, System.Diagnostics.Process.GetCurrentProcess().ProcessName));
        }

        static int Main(string[] args)
        {
            bool debug = false;
            string? clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");
            string? userName = null;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-d":
                        debug = true;
                        break;
                    case "-u":
                        if (++i == args.Length)
                        {
                            ShowHelp();
                            return 1;
                        }
                        userName = args[i];
                        break;
                    case "-c":
                        if (++i == args.Length)
                        {
                            ShowHelp();
                            return 1;
                        }
                        clusterName = args[i];
                        break;
                    default:
                        ShowHelp();
                        return 1;
                }
            }

            if (debug)
            {
                Console.Write("Press any key to start...");
                Console.Read();
            }

            // Create a scheduler object to be used to 
            // establish a connection to the scheduler on the headnode
            using IScheduler scheduler = new Scheduler();
            if (userName != null)
            {
                // Connect to the scheduler as another user
                Console.WriteLine("Connecting to {0} as {1}...", clusterName, userName);
                scheduler.ConnectServiceAsClient(clusterName, () => userName);
            }
            else
            {
                // Connect to the scheduler
                Console.WriteLine("Connecting to {0}...", clusterName);
                scheduler.Connect(clusterName);
            }

            //Create a job to submit to the scheduler
            //the job will be equivalent to the CLI command: job submit /numcores:1-1 "echo hello world"
            job = scheduler.CreateJob();

            //Some of the optional job parameters to specify. If omitted, defaults are:
            // Name = {blank}
            // UnitType = Core
            // Min/Max Resources = Autocalculated
            // etc...

            job.Name = "HPCSchedulerBasics Job";
            Console.WriteLine("Creating job name {0}...", job.Name);

            job.UnitType = JobUnitType.Core;

            job.AutoCalculateMin = false;
            job.AutoCalculateMax = false;

            job.MinimumNumberOfCores = 1;
            job.MaximumNumberOfCores = 1;

            //Create a task to submit to the job
            task = job.CreateTask();
            task.Name = "Hello World";
            Console.WriteLine("Creating a {0} task...", task.Name);

            //The commandline parameter tells the scheduler what the task should do
            //CommandLine is the only mandatory parameter you must set for every task
            task.CommandLine = "echo Hello World";

            //Don't forget to add the task to the job!
            job.AddTask(task);

            //Use callback to check if a job is finished
            job.OnJobState += new EventHandler<JobStateEventArg>(Job_OnJobState);

            //And to submit the job.
            //You can specify your username and password in the parameters, or set them to null and you will be prompted for your credentials
            Console.WriteLine("Submitting job to the cluster...");
            Console.WriteLine();

            scheduler.SubmitJob(job, null, null);

            //wait for job to finish
            jobFinishedEvent.WaitOne();

            //Close the connection
            scheduler.Close();

            return 0;
            //Call scheduler.Dispose() to free the object when finished
        }

        static void Job_OnJobState(object? sender, JobStateEventArg e)
        {
            if (e.NewState == JobState.Finished) //the job is finished
            {
                task!.Refresh(); // update the task object with updates from the scheduler

                Console.WriteLine("Job completed.");
                Console.WriteLine("Output: " + task.Output); //print the task's output
                jobFinishedEvent.Set();
            }
            else if (e.NewState == JobState.Canceled || e.NewState == JobState.Failed)
            {
                Console.WriteLine("Job did not finish.");
                jobFinishedEvent.Set();
            }
            else if (e.NewState == JobState.Queued && e.PreviousState != JobState.Validating)
            {
                Console.WriteLine("The job is currently queued.");
                Console.WriteLine("Waiting for job to start...");
            }
        }
    }
}