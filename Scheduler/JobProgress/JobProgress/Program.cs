// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace JobProgress
{
    class Program
    {
        static ManualResetEvent jobStatus = new ManualResetEvent(false);

        static async Task Main(string[] args)
        {
            string clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");

            //create a scheduler object used to connect to the scheduler
            using (IScheduler scheduler = new Scheduler())
            {
                //connect to the scheduler
                Console.WriteLine("Connecting to cluster {0}", clusterName);
                scheduler.Connect(clusterName);

                //create a job equivalent to the cmdline string: job submit /parametric:1-500 "echo *"
                Console.WriteLine("Creating parametric sweep job");
                //first create a SchedulerJob object
                ISchedulerJob job = scheduler.CreateJob();
                //and a task object
                ISchedulerTask task = job.CreateTask();

                //set the command line to "echo *"
                task.CommandLine = "echo *";

                //and we set the parametric task settings
                task.Type = TaskType.ParametricSweep;
                task.StartValue = 1;
                task.IncrementValue = 1;
                task.EndValue = 500;

                //add the task to the job
                job.AddTask(task);

                //Create an event handler so that we know when the job starts running
                job.OnJobState += new EventHandler<JobStateEventArg>(Job_OnJobState);

                //and submit
                //you will be prompted for your credentials if they aren't already cached
                Console.WriteLine("Submitting job...");
                scheduler.SubmitJob(job, null, null);
                Console.WriteLine("Job submitted");

                //Wait for the job to start running
                jobStatus.WaitOne();
                jobStatus.Reset();

                //you can get realtime updates on the job through the api
                //we'll keep checking every second for 5 seconds
                for (int i = 0; i < 5; i++)
                {
                    //refresh the job object with updates from the cluster
                    job.Refresh();
                    Console.Write("Current job progress: " + job.Progress);
                    Console.SetCursorPosition(0, Console.CursorTop);
                    //we want to check again after a second
                    await Task.Delay(1 * 1000);
                }

                //this field isn't read-only. You can specify your own progress value depending on your needs
                Console.WriteLine();
                Console.WriteLine("Manually changing job progress");
                job.Progress = 0;
                //commit the changes to the server
                job.Commit();

                Console.WriteLine("Current job progress: " + job.Progress);

                //you can also set progress messages, which will also be viewable in the Job Management UI
                Console.WriteLine("Setting job progress message");
                job.ProgressMessage = "Job is still running";
                //commit the changes to the server
                job.Commit();

                Console.WriteLine("Progress message: " + job.ProgressMessage);

                //Wait for the job to finish
                Console.WriteLine("Waiting for the job to finish...");
                jobStatus.WaitOne();

                //job.Progress will no longer increment automatically
                //the job will finish regardless of the value of job.Progress
                Console.WriteLine("Finished job progress: " + job.Progress);

                //close the scheduler connection
                scheduler.Close();
            }
        }

        static void Job_OnJobState(object sender, JobStateEventArg e)
        {
            //we want to check that the job is in the Running state or these finishing states.
            if (e.NewState == JobState.Running ||
                e.NewState == JobState.Finished ||
                e.NewState == JobState.Canceled ||
                e.NewState == JobState.Failed)
            {
                Console.WriteLine("Job status has been converted to: " + e.NewState.ToString());
                //allow the main thread to continue
                jobStatus.Set();
            }
        }
    }
}
