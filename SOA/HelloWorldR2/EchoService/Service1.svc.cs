// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using Microsoft.Hpc.Scheduler.Session;

namespace EchoService
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in Web.config and in the associated .svc file.
    public class Service1 : IService1
    {
        private static bool onExitCalled = false;
        private static EventHandler<EventArgs> eventHandler = new EventHandler<EventArgs>(OnExiting);
        private static StreamWriter sw = null;

        private static string computerName = string.Empty;
        private static string jobId = string.Empty;
        private static string taskId = string.Empty;

        const string ComputerNameEnvVar = "COMPUTERNAME";
        const string JobIDEnvVar = "CCP_JOBID";
        const string TaskIDEnvVar = "CCP_TASKSYSTEMID";

        static Service1()
        {
            computerName = Environment.GetEnvironmentVariable(ComputerNameEnvVar);
            jobId = Environment.GetEnvironmentVariable(JobIDEnvVar);
            taskId = Environment.GetEnvironmentVariable(TaskIDEnvVar);
            //add the OnExiting handler
            ServiceContext.OnExiting += eventHandler;
            // this is an example of shared resources, set the file buffer size to 1M bytes and disable the autoflush
            sw = new StreamWriter(Environment.CurrentDirectory + "\\sample." + jobId + "." + taskId + ".txt", false, Encoding.Unicode, 1000 * 1000);
            sw.AutoFlush = false;
        }

        /// <summary>
        /// The service routin
        /// </summary>
        /// <param name="input">the input string</param>
        /// <returns>the output string which is same as input</returns>
        public string Echo(string input)
        {
            // traces in user code
            ServiceContext.Logger.TraceEvent(TraceEventType.Start, 100, "In Echo Service: input {0}", input);
            ServiceContext.Logger.TraceData(TraceEventType.Information, 200, input);
            ServiceContext.Logger.TraceInformation(input);
            ServiceContext.Logger.TraceEvent(TraceEventType.Stop, 300, "In Echo Service: input {0}", input);

            return input;
        }
        /// <summary>
        /// The delay echo service
        /// </summary>
        /// <param name="delayMs">the delay time in milliseconds</param>
        /// <returns>the delay time in milliseconds</returns>
        public int EchoDelay(int delayMs)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (watch .ElapsedMilliseconds < delayMs)
            {   
            }
            watch.Stop();
            return delayMs;
        }

        /// <summary>
        /// The fault echo service
        /// </summary>
        /// <param name="exceptionType">the type of fault exception to throw</param>
        /// <returns>the type of fault exception to throw</returns>
        public string EchoFault(string exceptionType)
        {

            switch (exceptionType.ToLower())
            {
                case "dividebyzero":
                    {
                        int i = 0;
                        i = 0 / i;
                        //System.Threading.Thread.CurrentThread.Abort();
                        return null;
                    }
                case "outofmemoryexception":
                    {
                        throw new FaultException<OutOfMemoryException>(new OutOfMemoryException(), "Testing fault.OutOfMemoryException");
                    }
                case "dividebyzeroexception":
                    {
                        throw new FaultException<DivideByZeroException>(new DivideByZeroException(), "Testing fault.DivideByZeroException");
                    }
                case "argumentexception":
                    {
                        throw new FaultException<ArgumentException>(new ArgumentException(), "Testing fault.ArgumentException");
                    }
                case "argumentnullexception":
                    {
                        throw new FaultException<ArgumentNullException>(new ArgumentNullException(), "Testing fault.ArgumentNullException");
                    }
                
            }

            return exceptionType;
        }




        /// <summary>
        /// To demo OnExiting graceful exit 
        /// </summary>
        /// <param name="delay">The time consumed by the requests</param>
        /// <returns>The result string shows if the request is processed or exited</returns>
        public string EchoOnExit(TimeSpan delay)
        {
            Console.WriteLine("{0}-{1}-{2} Enter EchoOnExit", computerName, jobId, taskId);
            sw.WriteLine("{0}-{1}-{2} Enter EchoOnExit", computerName, jobId, taskId);

            Console.WriteLine("Global OnExit flag before the delay : {0}", onExitCalled);

            // to check if the OnExit is called on this service host
            // if so, suspend the service host to wait for the OnExit handler and the task exit 
            if (onExitCalled)
            {
                Console.WriteLine("The service task is exiting gracefully. Suspend this request to wait the task to exit.");
                Thread.Sleep(Int32.MaxValue);
            }

            //start the delay time and meanwhile checking the global flag if OnExiting is called
            sw.WriteLine("Start to delay : {0}.", delay.ToString());
            
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (watch.Elapsed < delay)
            {
                Thread.Sleep(1);
                
            }
            watch.Stop();
                        
            Console.WriteLine("{0}-{1}-{2} Leave EchoOnExit", computerName, jobId, taskId);
            sw.WriteLine("{0}-{1}-{2} Leave EchoOnExit", computerName, jobId, taskId);


            return string.Format("{0}-{1}-{2} EchoOnExit", computerName, jobId, taskId);

        }

        private static void OnExiting(object o, EventArgs e)
        {
            //release the resource here in the set timeout for 15 sec
            Console.WriteLine("{0}-{1}-{2} OnExiting is called. Releasing the resources in 3 seconds.", computerName, jobId, taskId);
            onExitCalled = true;
            
            //flush the file buffer and close the stream
            sw.Flush();
            sw.Close();
            
            Console.WriteLine("{0}-{1}-{2} The resources are released.The service task gracefully exit here.", computerName, jobId, taskId);

            return;
        }
    }
}
