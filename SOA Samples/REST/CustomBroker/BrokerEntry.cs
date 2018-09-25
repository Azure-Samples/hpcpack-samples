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
    using System.ServiceModel.Description;
    using System.ServiceModel.Web;
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
        private WebServiceHost frontendServiceHost;

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
            WebHttpBinding binding = new WebHttpBinding();
            binding.MaxBufferPoolSize = 5000000;
            binding.MaxBufferSize = 5000000;
            binding.MaxReceivedMessageSize = 5000000;

            binding.ReaderQuotas.MaxArrayLength = 5000000;
            binding.ReaderQuotas.MaxBytesPerRead = 5000000;
            binding.ReaderQuotas.MaxDepth = 5000000;
            binding.ReaderQuotas.MaxNameTableCharCount = 5000000;
            binding.ReaderQuotas.MaxStringContentLength = 5000000;

            this.frontendServiceHost = new WebServiceHost(frontend, new Uri(String.Format("http://{0}:8081/", Environment.MachineName)));
            ServiceEndpoint endpoint = this.frontendServiceHost.AddServiceEndpoint(typeof(IWebHttpFrontendService), binding, String.Empty);
            endpoint.Behaviors.Add(new WebHttpBehavior());

            string listenUri = endpoint.ListenUri.AbsoluteUri;
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
