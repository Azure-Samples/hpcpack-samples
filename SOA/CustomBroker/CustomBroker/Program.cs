//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The main entry point for the application.
// </summary>
//------------------------------------------------------------------------------
namespace SampleBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">indicating arguments</param>
        /// <returns>returns the exit code</returns>
        private static int Main(string[] args)
        {
            ManualResetEvent exitWaitHandle = new ManualResetEvent(false);

            int pid = Process.GetCurrentProcess().Id;
            Uri brokerManagementServiceAddress = new Uri(String.Format("http://localhost:9093/BrokerManagementService/{0}", pid));

            BrokerManagementService instance;

            try
            {
                instance = new BrokerManagementService(exitWaitHandle);

                Trace.TraceInformation("[Main] Try open broker management service at {0}.", brokerManagementServiceAddress.ToString());
                ServiceHost host = new ServiceHost(instance, brokerManagementServiceAddress);
                BasicHttpBinding hardCodedBrokerManagementServiceBinding = new BasicHttpBinding();
                host.AddServiceEndpoint(typeof(IBrokerManagementService), hardCodedBrokerManagementServiceBinding, String.Empty);
                host.Open();

                Trace.TraceInformation("[Main] Open broker management service succeeded.");
            }
            catch (Exception e)
            {
                Trace.TraceError("[Main] Failed to open broker management service: {0}", e);
                return (int)BrokerShimExitCode.FailedOpenServiceHost;
            }

            bool createdNew;
            EventWaitHandle initializeWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, String.Format("HpcBroker{0}", pid), out createdNew);
            if (createdNew)
            {
                Trace.TraceError("[Main] Initialize wait handle has not been created by the broker launcher.");
                return (int)BrokerShimExitCode.InitializeWaitHandleNotExist;
            }

            if (!initializeWaitHandle.Set())
            {
                Trace.TraceError("[Main] Failed to set the initialize wait handle.");
                return (int)BrokerShimExitCode.FailedToSetInitializeWaitHandle;
            }

            // Wait for exit
            exitWaitHandle.WaitOne();


            return (int)BrokerShimExitCode.Success;
        }
    }
}
