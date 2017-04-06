// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Threading;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace ReconnectEvents
{
    class Program
    {
        //event handler we'll use to monitor the connection status
        static ManualResetEvent connected = new ManualResetEvent(true);
        static IScheduler scheduler;

        static void Main(string[] args)
        {

            string clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");
            IStringCollection headnode = new StringCollection();
            headnode.Add(clusterName);

            //create scheduler object
            scheduler = new Scheduler();
            //connect to the cluster
            scheduler.Connect(clusterName);
            //create the event handler
            //the event handler must be created after connection is made to the scheduler
            scheduler.OnSchedulerReconnect += new EventHandler<ConnectionEventArg>(scheduler_OnSchedulerReconnect);

            Console.WriteLine("Submitting a job every 2 seconds");
            Console.WriteLine("On the headnode, forcibly end the connection by ending procress HpcSchedulerStateful.exe which is consuming biggest amount of memory");
            Console.WriteLine("Press CTRL+C to exit");
            Console.WriteLine();

            try
            {
                while (true) submitJobs();
            }
            finally
            {
                scheduler.Dispose();
            }
            
        }

        static void scheduler_OnSchedulerReconnect(object sender, ConnectionEventArg e)
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

        static bool submitJobs()
        {
            //wait for a maximum of one minute for scheduler connect before exiting
            if (connected.WaitOne(1*1000)) //timesout in one second
            {
                //create a job equivalent to "job submit echo Hello World" 
                ISchedulerJob job = scheduler.CreateJob();
                ISchedulerTask task = job.CreateTask();
                task.CommandLine = "echo Hello World";
                job.AddTask(task);
                scheduler.SubmitJob(job, null, null);

                job.Refresh();
                Console.WriteLine("Job {0} was submitted", job.Id);

                Thread.Sleep(2 * 1000); //pause for 2 seconds
                return true;
            }
            return false;
        }

    }
}
