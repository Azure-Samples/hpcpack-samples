//------------------------------------------------------------------------------
// <copyright file="IDuplexCallbackService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Standard duplex callback service contract
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
    /// Standard duplex callback service contract
    /// </summary>
    [ServiceContract]
    internal interface IDuplexCallbackService
    {
        /// <summary>
        /// Standard callback operation to send back response
        /// </summary>
        /// <param name="response">indicating the response message</param>
        [OperationContract(Action = "*", IsOneWay = true)]
        void SendResponse(Message response);
    }
}
