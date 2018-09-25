//------------------------------------------------------------------------------
// <copyright file="IService1.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provide sample service contract
// </summary>
//------------------------------------------------------------------------------

namespace WebHttpDemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;

    /// <summary>
    /// Sample service contract
    /// </summary>
    [ServiceContract]
    public interface IService1
    {
        /// <summary>
        /// Sample service method
        /// </summary>
        /// <param name="value">any string</param>
        /// <returns>echoed string</returns>
        [OperationContract, WebGet(UriTemplate = "echo/{value}", ResponseFormat = WebMessageFormat.Xml)]
        string Echo(string value);
    }
}
