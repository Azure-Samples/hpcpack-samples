//------------------------------------------------------------------------------
// <copyright file="IRequestReplyService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Standard request reply service contract
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
    /// Standard request reply service contract
    /// </summary>
    [ServiceContract]
    internal interface IRequestReplyService
    {
        /// <summary>
        /// Standard operation contract for request/reply
        /// </summary>
        /// <param name="request">request message</param>
        /// <returns>reply message</returns>
        [OperationContract(Action="*", ReplyAction="*", IsOneWay=false, AsyncPattern=false)]
        Message ProcessMessage(Message request);
    }
}
