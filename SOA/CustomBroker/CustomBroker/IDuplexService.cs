//------------------------------------------------------------------------------
// <copyright file="IDuplexService.cs" company="Microsoft">
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
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;

    /// <summary>
    /// Standard duplex service contract
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IDuplexCallbackService))]
    internal interface IDuplexService
    {
        /// <summary>
        /// Standard duplex service operation to receive a request
        /// </summary>
        /// <param name="request">indicating the request</param>
        [OperationContract(Action = "*", IsOneWay = true)]
        void ReceiveRequest(Message request);
    }
}
