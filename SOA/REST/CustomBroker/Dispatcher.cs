//------------------------------------------------------------------------------
// <copyright file="Dispatcher.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents a service host
// </summary>
//------------------------------------------------------------------------------
namespace SampleBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Threading;
    using System.Xml;

    /// <summary>
    /// Represents a service host
    /// </summary>
    internal sealed class Dispatcher
    {
        /// <summary>
        /// Stores the backend binding
        /// </summary>
        private static readonly WebHttpBinding backendBinding;

        /// <summary>
        /// Stores the service host epr format
        /// </summary>
        private const string ServiceHostEprFormat = "http://{0}:8088/";

        /// <summary>
        /// Stores the service client
        /// </summary>
        private ServiceClient client;

        /// <summary>
        /// Stores the task id
        /// </summary>
        private int taskId;

        /// <summary>
        /// Stores the capacity
        /// </summary>
        private int capacity;

        /// <summary>
        /// Stores the dispatcher url
        /// </summary>
        private string url;

        /// <summary>
        /// Initializes static members of the Dispatcher class
        /// </summary>
        static Dispatcher()
        {
            backendBinding = new WebHttpBinding(WebHttpSecurityMode.None);

            backendBinding.MaxBufferPoolSize = 5000000;
            backendBinding.MaxBufferSize = 5000000;
            backendBinding.MaxReceivedMessageSize = 5000000;

            backendBinding.ReaderQuotas.MaxArrayLength = 5000000;
            backendBinding.ReaderQuotas.MaxBytesPerRead = 5000000;
            backendBinding.ReaderQuotas.MaxDepth = 5000000;
            backendBinding.ReaderQuotas.MaxNameTableCharCount = 5000000;
            backendBinding.ReaderQuotas.MaxStringContentLength = 5000000;
        }

        /// <summary>
        /// Initializes a new instance of the Dispatcher class
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="taskId">indicating the task id</param>
        /// <param name="capacity">indicating the capacity</param>
        /// <param name="machineName">indicating the machine name</param>
        public Dispatcher(int sessionId, int taskId, int capacity, string machineName)
        {
            this.url = String.Format(ServiceHostEprFormat, machineName, sessionId, taskId);

            this.client = new ServiceClient(backendBinding, new EndpointAddress(this.url));
            this.client.Endpoint.Behaviors.Add(new WebHttpBehavior());

            this.taskId = taskId;
            this.capacity = capacity;
        }

        /// <summary>
        /// Gets the task id
        /// </summary>
        public int TaskId
        {
            get { return this.taskId; }
        }

        /// <summary>
        /// Gets the capacity
        /// </summary>
        public int Capacity
        {
            get { return this.capacity; }
        }

        /// <summary>
        /// Redirect message to web service host for processing
        /// </summary>
        /// <param name="request">incoming request message</param>
        /// <returns>response message</returns>
        public Message ProcessRequest(Message request)
        {
            // Send request to service host for processing
            lock (this.client)
            {
                Uri oldUri = request.Headers.To;
                string oldUrl = oldUri.AbsoluteUri;

                Uri newUri = null;
                if (oldUrl.ToLower().StartsWith("http://"))
                {
                    int slashAfterPort = oldUrl.IndexOf('/', 8);
                    string path = oldUrl.Substring(slashAfterPort + 1);

                    newUri = new Uri(this.url + path);
                }
                else
                {
                    newUri = new Uri(this.url + oldUrl);
                }

                request.Headers.To = newUri;
                return this.client.ProcessMessage(request);
             }
        }

        /// <summary>
        /// Build fault message
        /// </summary>
        /// <param name="messageId">indicating the message id</param>
        /// <param name="ex">indicating the exception</param>
        /// <returns>returns the fault message</returns>
        private Message BuildFaultMessage(UniqueId messageId, Exception ex)
        {
            MessageFault fault = MessageFault.CreateFault(FaultCode.CreateReceiverFaultCode("FailProcessRequest", "http://tempuri.org"), String.Format("Failed to process request: {0}", ex));
            Message faultMessage = Message.CreateMessage(MessageVersion.Default, fault, "http://tempuri.org/FailProcessRequest");
            faultMessage.Headers.RelatesTo = messageId;
            return faultMessage;
        }

        /// <summary>
        /// Client proxy for service
        /// </summary>
        private class ServiceClient : ClientBase<IRequestReplyService>, IRequestReplyService
        {
            /// <summary>
            /// Initializes a new instance of the ServiceClient class
            /// </summary>
            /// <param name="binding">indicating the binding</param>
            /// <param name="remoteAddress">indicating the remote address</param>
            public ServiceClient(Binding binding, EndpointAddress remoteAddress)
                : base(binding, remoteAddress)
            {
            }

            /// <summary>
            /// Standard operation contract for request/reply
            /// </summary>
            /// <param name="request">request message</param>
            /// <returns>reply message</returns>
            public Message ProcessMessage(Message request)
            {
                return this.Channel.ProcessMessage(request);
            }
        }
    }
}
