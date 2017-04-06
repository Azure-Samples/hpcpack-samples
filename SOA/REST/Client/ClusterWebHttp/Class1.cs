//------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Service contract
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
    /// Service Interface
    /// </summary>
    [ServiceContract]
    public interface IService1
    {
        /// <summary>
        /// service operation contract
        /// </summary>
        /// <param name="value">any string</param>
        /// <returns>echoed string</returns>
        [OperationContract, WebGet(UriTemplate = "echo/{value}", ResponseFormat = WebMessageFormat.Xml)]
        string Echo(string value);
    }
}
