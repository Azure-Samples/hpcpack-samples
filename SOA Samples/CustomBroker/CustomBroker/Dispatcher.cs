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
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// Represents a service host
    /// </summary>
    internal sealed class Dispatcher
    {
        /// <summary>
        /// Stores the backend binding
        /// </summary>
        private static readonly NetTcpBinding backendBinding;

        /// <summary>
        /// Stores the service host epr format
        /// </summary>
        private const string ServiceHostEprFormat = "net.tcp://{0}:{3}/{1}/{2}/_defaultEndpoint";

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
        /// Initializes static members of the Dispatcher class
        /// </summary>
        static Dispatcher()
        {
            backendBinding = new NetTcpBinding(SecurityMode.Transport);
            backendBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            backendBinding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
        }

        /// <summary>
        /// Initializes a new instance of the Dispatcher class
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="taskId">indicating the task id</param>
        /// <param name="capacity">indicating the capacity</param>
        /// <param name="machineName">indicating the machine name</param>
        public Dispatcher(int sessionId, int taskId, int capacity, string machineName, int index)
        {
            this.client = new ServiceClient(backendBinding, new EndpointAddress(String.Format(ServiceHostEprFormat, machineName, sessionId, taskId, 9100 + index)));
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
        /// Process request
        /// </summary>
        /// <param name="request">indicating the request message</param>
        /// <param name="callback">indicating the callback</param>
        public void ProcessRequest(Message request, IDuplexCallbackService callback)
        {
            // Send request to service host for processing
            lock (this.client)
            {
                this.client.BeginProcessMessage(request, this.ReceiveResponse, new object[] { request.Headers.MessageId, callback });
            }
        }

        /// <summary>
        /// Callback raised when response is received
        /// </summary>
        /// <param name="result">indicating the async result</param>
        private void ReceiveResponse(IAsyncResult result)
        {
            object[] objArr = result.AsyncState as object[];
            UniqueId messageId = objArr[0] as UniqueId;
            IDuplexCallbackService callback = objArr[1] as IDuplexCallbackService;
            Message response;
            try
            {
                lock (this.client)
                {
                    response = this.client.EndProcessMessage(result);
                }
            }
            catch (Exception e)
            {
                // Communication/Timeout/EndpointNotFound exception could throw here
                // Build fault message instead and send back to client
                Trace.TraceError("[ServiceClientManager] Failed to process the message: {0}", e);
                response = this.BuildFaultMessage(messageId, e);
            }

            try
            {
                // Send back response to client side
                callback.SendResponse(response);
            }
            catch (Exception e)
            {
                // Failed to send response
                // Swallow the exception and log it
                Trace.TraceError("[ServiceClientManager] Failed to send back response: {0}", e);
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

            /// <summary>
            /// Async Pattern
            /// Begin method for ProcessMessage
            /// </summary>
            /// <param name="request">request message</param>
            /// <param name="callback">async callback</param>
            /// <param name="asyncState">async state</param>
            /// <returns>async result</returns>
            public IAsyncResult BeginProcessMessage(Message request, AsyncCallback callback, object asyncState)
            {
                return this.Channel.BeginProcessMessage(request, callback, asyncState);
            }

            /// <summary>
            /// Async Pattern
            /// End method for ProcessMessage
            /// </summary>
            /// <param name="ar">async result</param>
            /// <returns>reply message</returns>
            public Message EndProcessMessage(IAsyncResult ar)
            {
                return this.Channel.EndProcessMessage(ar);
            }
        }
    }
}
