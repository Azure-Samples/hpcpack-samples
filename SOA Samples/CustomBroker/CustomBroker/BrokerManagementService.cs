//------------------------------------------------------------------------------
// <copyright file="BrokerManagementService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The Broker Management Service
// </summary>
//------------------------------------------------------------------------------
namespace SampleBroker
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// The Broker Management Service
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    internal class BrokerManagementService : IBrokerManagementService
    {
        /// <summary>
        /// Stores the exit wait handle
        /// </summary>
        private ManualResetEvent exitWaitHandle;

        /// <summary>
        /// Stores the broker entry
        /// </summary>
        private BrokerEntry entry;

        /// <summary>
        /// Initializes a new instance of the BrokerManagementService class
        /// </summary>
        /// <param name="exitWaitHandle">indicating the exit wait handle</param>
        public BrokerManagementService(ManualResetEvent exitWaitHandle)
        {
            this.exitWaitHandle = exitWaitHandle;
        }

        /// <summary>
        /// Ask broker to initialize
        /// </summary>
        /// <param name="startInfo">indicating the start info</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <returns>returns broker initialization result</returns>
        public BrokerInitializationResult Initialize(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo)
        {
            ThrowIfNull(startInfo, "startInfo");
            ThrowIfNull(brokerInfo, "brokerInfo");

            this.entry = new BrokerEntry(startInfo, brokerInfo, this);
            return this.entry.Result;
        }

        /// <summary>
        /// Ask to close the broker
        /// </summary>
        /// <param name="suspended">indicating whether the broker is asked to be suspended or closed</param>
        public void CloseBroker(bool suspended)
        {
            this.entry.Close();

            // Set the exit wait handle to allow process exit
            this.exitWaitHandle.Set();
        }

        /// <summary>
        /// Attach to the sample broker
        /// </summary>
        public void Attach()
        {
            // Just return to let client attach
        }

        /// <summary>
        /// Throw ArgumentNullException if the object is null
        /// </summary>
        /// <param name="obj">indicating the object</param>
        /// <param name="argumentName">indicating the argument name</param>
        private static void ThrowIfNull(object obj, string argumentName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}
