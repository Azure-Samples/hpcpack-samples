//------------------------------------------------------------------------------
// <copyright file="Service1.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provide sample service implementation
// </summary>
//------------------------------------------------------------------------------

namespace WebHttpDemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;

    /// <summary>
    /// Sample service implementation
    /// </summary>
    public class Service1 : IService1
    {
        /// <summary>
        /// Echo input value
        /// </summary>
        /// <param name="value">any string value</param>
        /// <returns>echoed string</returns>
        public string Echo(string value)
        {
            return "Echo: " + value;
        }
    }
}
