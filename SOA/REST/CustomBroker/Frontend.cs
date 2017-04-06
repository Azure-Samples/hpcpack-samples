//------------------------------------------------------------------------------
// <copyright file="Frontend.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Frontend service
// </summary>
//------------------------------------------------------------------------------
namespace SampleBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    /// <summary>
    /// Frontend service
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal sealed class Frontend : IWebHttpFrontendService
    {
        /// <summary>
        /// Stores the client manager
        /// </summary>
        private ServiceClientManager clientManager;

        /// <summary>
        /// Initializes a new instance of the Frontend class
        /// </summary>
        /// <param name="clientManager">indicating the client manager</param>
        public Frontend(ServiceClientManager clientManager)
        {
            this.clientManager = clientManager;
        }

        /// <summary>
        /// Receive request message
        /// </summary>
        /// <param name="request">indicating the request message</param>
        /// <returns>response message</returns>
        [OperationBehavior(AutoDisposeParameters = false)]
        public Message ReceiveRequest(Message request)
        {
            // Foward the request context to the service client manager
            return this.clientManager.ReceiveRequest(request);
        }
    }
}
