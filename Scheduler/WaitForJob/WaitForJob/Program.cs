// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Threading;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace WaitForjob
{
    class Program
    {
        const JobState exitStates = JobState.Finished | JobState.Failed | JobState.Canceled;

        /// <summary>
        /// Waits for the specified job to reach a terminal state of Finished, Failed or Canceled
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="job"></param>
        static void WaitForJob(IScheduler scheduler, ISchedulerJob job)
        {
            ManualResetEvent checkJobState = new ManualResetEvent(false);
            // Event handler for when the job state changes
            EventHandler<JobStateEventArg> jobStatusCheck = (sender, e) =>
            {
                Console.WriteLine(string.Format("  Job {0} state is now {1}.", job.Id, e.NewState));
                // Become one of the states
                if ((e.NewState & exitStates) != 0)
                {
                    checkJobState.Set();
                }
            };

            // Event handler for when the eventing channel gets reconnected after a failure
            EventHandler<ConnectionEventArg> schedulerConnectionEvent = (sender, e) =>
            {
                if (e.Code == ConnectionEventCode.EventReconnect)
                {
                    Console.WriteLine("  Reconnect event detected");
                    // Signal the thread to recheck the job state since the job state event may have been missed while we were disconnected.
                    checkJobState.Set();
                }
                else
                {
                    Console.WriteLine(string.Format("  schedulerConnectionEvent {0}.", e.Code));
                }
            };
            Console.WriteLine(string.Format("Waiting for job {0}...", job.Id));

            // Register event handlers before checkJobState is Reset
            job.OnJobState += jobStatusCheck;
            scheduler.OnSchedulerReconnect += schedulerConnectionEvent;
            
            try
            {
                do
                {
                    // Always Reset before job.Refresh to avoid losing state transitions
                    checkJobState.Reset();  
                    job.Refresh();

                    if ((job.State & exitStates) != 0)
                    {
                        Console.WriteLine(string.Format("Job {0} completed with state {1}.", job.Id, job.State));
                        return;
                    }

                    checkJobState.WaitOne();
                } while (true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected exception occurred. Message: {e.Message}");
            }
            finally
            {
                // must unregester handlers using the same job and scheduler objects that were used to register them above
                // see comment "Register event handlers"
                job.OnJobState -= jobStatusCheck;
                scheduler.OnSchedulerReconnect -= schedulerConnectionEvent;
            }
        }

        static void Main(string[] args)
        {
            string headNode = Environment.GetEnvironmentVariable("CCP_SCHEDULER");

            using (IScheduler scheduler = new Scheduler())
            {
                scheduler.Connect(headNode);

                ISchedulerJob job = scheduler.CreateJob();
                ISchedulerTask task = job.CreateTask();
                task.CommandLine = "ping localhost -n 20";
                job.AddTask(task);
                scheduler.SubmitJob(job, null, null);

                Console.WriteLine("End the connection by ending process HpcScheduler.exe on the head node");
                Console.WriteLine("In Task Manageer -> More Details -> Details tab -> find HpcScheduler.exe -> End Task");
                Console.WriteLine("HpcScheduler.exe will restart automatically after a while");
                Console.WriteLine();
                WaitForJob(scheduler, job);
            }
        }
    }
}
