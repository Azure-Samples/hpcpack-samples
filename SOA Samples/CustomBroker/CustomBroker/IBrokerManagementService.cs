//------------------------------------------------------------------------------
// <copyright file="IBrokerManagementService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Interface for Broker Management Service
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Interface for Broker Management Service
    /// </summary>
    [ServiceContract(Name = "IBrokerManagementService", Namespace = "http://hpc.microsoft.com/brokermanagement/")]
    internal interface IBrokerManagementService
    {
        /// <summary>
        /// Ask broker to initialize
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <returns>returns broker initialization result</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        BrokerInitializationResult Initialize(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo);

        /// <summary>
        /// Attach to the broker
        /// broker would throw exception if it does not allow client to attach to it
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        void Attach();

        /// <summary>
        /// Ask to close the broker
        /// </summary>
        /// <param name="suspended">indicating whether the broker is asked to be suspended or closed</param>
        [OperationContract]
        void CloseBroker(bool suspended);
    }
}
