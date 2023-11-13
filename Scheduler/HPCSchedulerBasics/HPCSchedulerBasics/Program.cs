// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Threading;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace HPCSchedulerBasics
{
    class Program
    {
        static ISchedulerJob job;
        static ISchedulerTask task;
        static ManualResetEvent jobFinishedEvent = new ManualResetEvent(false);

        static void ShowHelp()
        {
            string help = @"
                Usage:
                {0} [-c <cluster name>] [-u <user name>] [-d]

                Options:
                -c Provide a HPC cluster name to connect to. The default value is %CCP_SCHEDULER%.
                -u Provide a user name to connect as that user.
                -d Run in debug mode.
                ";
            Console.WriteLine(String.Format(help, System.Diagnostics.Process.GetCurrentProcess().ProcessName));
        }

        static int Main(string[] args)
        {
            bool debug = false;
            string clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");
            string userName = null;

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
            using (IScheduler scheduler = new Scheduler())
            {
                try
                {
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
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Could not connect to the scheduler: {0}", e.Message);
                    return 1; //abort if no connection could be made
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
                job.OnJobState += new EventHandler<JobStateEventArg>(job_OnJobState);

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
            } //Call scheduler.Dispose() to free the object when finished
        }

        static void job_OnJobState(object sender, JobStateEventArg e)
        {
            if (e.NewState == JobState.Finished) //the job is finished
            {
                task.Refresh(); // update the task object with updates from the scheduler

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