//------------------------------------------------------------------------------
// <copyright file="IWebHttpFrontendService.cs" company="Microsoft">
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
    using System.ServiceModel.Web;
    using System.Text;

    /// <summary>
    /// Standard duplex service contract
    /// </summary>
    [ServiceContract]
    internal interface IWebHttpFrontendService
    {
        /// <summary>
        /// Standard duplex service operation to receive a request
        /// </summary>
        /// <param name="request">indicating the request</param>
        /// <returns>response message</returns>
        [OperationContract(Action = "*", ReplyAction = "*", IsOneWay = false)]
        [WebInvoke(Method = "*", UriTemplate = "*")]
        Message ReceiveRequest(Message request);
    }
}
