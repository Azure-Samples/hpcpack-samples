//------------------------------------------------------------------------------
// <copyright file="BrokerEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provide entry operation for the sample broker
// </summary>
//------------------------------------------------------------------------------
namespace SampleBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Provide entry operation for the sample broker
    /// </summary>
    internal sealed class BrokerEntry
    {
        /// <summary>
        /// Stores the frontend service host
        /// </summary>
        private ServiceHost frontendServiceHost;

        /// <summary>
        /// Stores the broker initialization result
        /// </summary>
        private BrokerInitializationResult result;

        /// <summary>
        /// Stores the service client manager
        /// </summary>
        private ServiceClientManager clientManager;

        /// <summary>
        /// Initializes a new instance of the BrokerEntry class
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        public BrokerEntry(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo, BrokerManagementService serviceInstance)
        {
            if (startInfo.TransportScheme != TransportScheme.NetTcp)
            {
                throw new NotSupportedException("Sample broker does not support transport scheme other than NetTcp.");
            }

            if (brokerInfo.Durable)
            {
                throw new NotSupportedException("Sample broker does not support durable session.");
            }

            this.clientManager = new ServiceClientManager(brokerInfo.SessionId, brokerInfo.Headnode, serviceInstance);
            Frontend frontend = new Frontend(this.clientManager);

            // Choose different binding configuration by start info
            NetTcpBinding frontendBinding;
            if (startInfo.Secure)
            {
                frontendBinding = new NetTcpBinding();
            }
            else
            {
                frontendBinding = new NetTcpBinding(SecurityMode.None);
            }

            frontendBinding.PortSharingEnabled = true;

            this.frontendServiceHost = new ServiceHost(frontend, new Uri(String.Format("net.tcp://{0}:9091/SampleBroker", Environment.MachineName)));
            string listenUri = this.frontendServiceHost.AddServiceEndpoint(typeof(IDuplexService), frontendBinding, String.Empty).ListenUri.AbsoluteUri;
            this.frontendServiceHost.Open();

            this.result = new BrokerInitializationResult();
            this.result.BrokerEpr = new string[3] { listenUri, null, null };
            this.result.ControllerEpr = new string[3];
            this.result.ResponseEpr = new string[3];
        }

        /// <summary>
        /// Gets the result
        /// </summary>
        public BrokerInitializationResult Result
        {
            get { return this.result; }
        }

        /// <summary>
        /// Close the broker
        /// </summary>
        public void Close()
        {
            try
            {
                this.frontendServiceHost.Close();
            }
            catch (Exception e)
            {
                Trace.TraceError("[BrokerEntry] Failed to close frontend: {0}", e);
                this.frontendServiceHost.Abort();
            }

            try
            {
                this.clientManager.FinishServiceJob();
            }
            catch (Exception e)
            {
                Trace.TraceError("[BrokerEntry] Failed to finish service job: {0}", e);
            }
        }
    }
}
