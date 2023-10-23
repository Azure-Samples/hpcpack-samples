// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Hpc.Scheduler;

namespace ReconnectEvents
{
    class Program
    {
        //event handler we'll use to monitor the connection status
        static ManualResetEvent connected = new ManualResetEvent(true);
        
        static async Task Main(string[] args)
        {
            string clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");

            using (IScheduler scheduler = new Scheduler())
            {
                scheduler.Connect(clusterName);
                //create the event handler
                //the event handler must be created after connection is made to the scheduler
                scheduler.OnSchedulerReconnect += new EventHandler<ConnectionEventArg>(Scheduler_OnSchedulerReconnect);

                Console.WriteLine("Submitting a job every 2 seconds");
                Console.WriteLine("End the connection by ending process HpcScheduler.exe on the head node");
                Console.WriteLine("In Task Manageer -> More Details -> Details tab -> find HpcScheduler.exe -> End Task");
                Console.WriteLine("HpcScheduler.exe will restart automatically after a while");
                Console.WriteLine("Press CTRL+C to exit");
                Console.WriteLine();

                try
                {
                    while (true)
                    {
                        await SubmitJobs(scheduler);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected exception occurred in SubmitJobs. Message: {e.Message}");
                }   
                finally
                {
                    scheduler.Dispose();
                }
            }
        }

        static void Scheduler_OnSchedulerReconnect(object sender, ConnectionEventArg e)
        {
            //check for Disconnect event
            if (e.Code == ConnectionEventCode.StoreDisconnect)
            {
                Console.WriteLine("Disconnect event detected");
                //signal the thread to stop submitting jobs
                connected.Reset();
            }
            else if (e.Code == ConnectionEventCode.StoreReconnect)
            {
                Console.WriteLine("Reconnect event detected");
                //signal the thread to continue submitting jobs
                connected.Set();
            }
        }

        static async Task SubmitJobs(IScheduler scheduler)
        {
            //wait for a maximum of 1 second for scheduler connect before exiting
            if (connected.WaitOne(1 * 1000))
            {
                //create a job equivalent to "job submit echo Hello World" 
                ISchedulerJob job = scheduler.CreateJob();
                ISchedulerTask task = job.CreateTask();
                task.CommandLine = "echo Hello World";
                job.AddTask(task);
                scheduler.SubmitJob(job, null, null);

                job.Refresh();
                Console.WriteLine("Job {0} was submitted", job.Id);

                //pause for 2 seconds
                await Task.Delay(2 * 1000);
            }
        }
    }
}
