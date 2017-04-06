//------------------------------------------------------------------------------
// <copyright file="ServiceClientManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Manager for dispatchers
// </summary>
//------------------------------------------------------------------------------
namespace SampleBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Manager for dispatchers
    /// </summary>
    internal sealed class ServiceClientManager : ISchedulerNotify
    {
        /// <summary>
        /// Stores the scheduler adapter binding
        /// </summary>
        private static readonly NetTcpBinding schedulerAdapterBinding;

        /// <summary>
        /// Stores the session id
        /// </summary>
        private int sessionId;

        /// <summary>
        /// Stores the dispatcher list
        /// </summary>
        private SortedList<int, Dispatcher> dispatcherList;

        /// <summary>
        /// Stores the scheduler adapter client
        /// </summary>
        private SchedulerAdapterClient schedulerAdapterClient;

        /// <summary>
        /// Stores the broker management service instance
        /// </summary>
        private BrokerManagementService serviceInstance;

        /// <summary>
        /// Initializes static members of the ServiceClientManager class
        /// </summary>
        static ServiceClientManager()
        {
            // Initializes scheduler adapter client binding
            // This binding is hardcoded
            schedulerAdapterBinding = new NetTcpBinding(SecurityMode.Transport);
            schedulerAdapterBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            schedulerAdapterBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            schedulerAdapterBinding.MaxConnections = 1000;
            schedulerAdapterBinding.MaxBufferPoolSize = 256000;
            schedulerAdapterBinding.MaxBufferSize = 256000;
            schedulerAdapterBinding.MaxReceivedMessageSize = 256000;
            schedulerAdapterBinding.ReaderQuotas.MaxArrayLength = 256000;
        }

        /// <summary>
        /// Initializes a new instance of the ServiceClientManager class
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="headNode">indicating the head node</param>
        public ServiceClientManager(int sessionId, string headNode, BrokerManagementService serviceInstance)
        {
            this.serviceInstance = serviceInstance;

            this.sessionId = sessionId;
            this.dispatcherList = new SortedList<int, Dispatcher>();
            this.schedulerAdapterClient = new SchedulerAdapterClient(headNode, new InstanceContext(this), schedulerAdapterBinding);

            // Register to scheduler adapter so that callback could be raised
            this.schedulerAdapterClient.RegisterJob(sessionId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Finish the service job
        /// </summary>
        public void FinishServiceJob()
        {
            this.schedulerAdapterClient.FinishJob(this.sessionId, String.Empty).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Receive request context from frontend and send request to service host
        /// </summary>
        /// <param name="request">indicating the request message</param>
        /// <param name="callback">indicating the callback</param>
        public void ReceiveRequest(Message request, IDuplexCallbackService callback)
        {
            // Try to get a dispatcher from the dispatcher list
            // Put the loadbalancing logic here to choose a service host for dispatching messages
            Dispatcher dispatcher;
            while (true)
            {
                if (this.dispatcherList.Count > 0)
                {
                    lock (this.dispatcherList)
                    {
                        if (this.dispatcherList.Count > 0)
                        {
                            // Randomly choose a service client as a sample dispatching tacitc
                            Random r = new Random();
                            int index = r.Next(this.dispatcherList.Count);
                            dispatcher = this.dispatcherList.Values[index];
                            break;
                        }
                    }
                }

                // Sleep 1 second to wait for service host ready
                Thread.Sleep(1000);
            }

            dispatcher.ProcessRequest(request, callback);
        }

        /// <summary>
        /// Triggeres when job state changed
        /// </summary>
        /// <param name="state">indicating the job state</param>
        public Task JobStateChanged(JobState state)
        {
            if (state == JobState.Canceled || state == JobState.Finished)
            {
                serviceInstance.CloseBroker(false);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Triggered when task state changed
        /// </summary>
        /// <param name="taskInfoList">indicating the task info list</param>
        public Task TaskStateChanged(List<TaskInfo> taskInfoList)
        {
            lock (this.dispatcherList)
            {
                foreach (TaskInfo info in taskInfoList)
                {
                    switch (info.State)
                    {
                        // If task is changed to Running state and client dic does not have this task
                        // Create service client for this task and add it into the client dic.
                        case TaskState.Running:
                            if (!this.dispatcherList.ContainsKey(info.Id))
                            {
                                this.dispatcherList.Add(info.Id, new Dispatcher(this.sessionId, info.Id, info.Capacity, info.MachineName, info.FirstCoreIndex));
                            }

                            break;

                        // If task is changed to Canceling, Finishing, Canceled, Finished state and client dic contains the service client
                        // Remove the service client from the client dic
                        case TaskState.Canceling:
                        case TaskState.Canceled:
                        case TaskState.Finishing:
                        case TaskState.Finished:
                            this.dispatcherList.Remove(info.Id);
                            break;
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}
