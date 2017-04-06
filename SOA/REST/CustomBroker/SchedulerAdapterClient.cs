//------------------------------------------------------------------------------
// <copyright file="SchedulerAdapterClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The client implementation for the scheduler adapter
// </summary>
//------------------------------------------------------------------------------
namespace SampleBroker
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// The client implementation for the scheduler adapter
    /// </summary>
    internal class SchedulerAdapterClient : DuplexClientBase<ISchedulerAdapter>, ISchedulerAdapter
    {
        /// <summary>
        /// Stores the timeout
        /// </summary>
        private static readonly TimeSpan SchedulerAdapterTimeout = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Initializes a new instance of the SchedulerAdapterClient class
        /// </summary>
        /// <param name="headnode">indicating the headnode</param>
        /// <param name="instanceContext">indicating the instance context</param>
        /// <param name="binding">indicating the binding</param>
        public SchedulerAdapterClient(string headnode, InstanceContext instanceContext, Binding binding)
            : base(instanceContext, binding, new EndpointAddress(String.Format("net.tcp://{0}:9092/SchedulerDelegation", headnode)))
        {
            this.InnerChannel.OperationTimeout = SchedulerAdapterTimeout;

            foreach (OperationDescription op in this.Endpoint.Contract.Operations)
            {
                DataContractSerializerOperationBehavior dataContractBehavior = op.Behaviors.Find<DataContractSerializerOperationBehavior>() as DataContractSerializerOperationBehavior;
                if (dataContractBehavior != null)
                {
                    dataContractBehavior.MaxItemsInObjectGraph = int.MaxValue;
                }
            }
        }

        /// <summary>
        /// Start to subscribe the job and task event
        /// </summary>
        /// <param name="jobid">indicating the job id</param>
        public async Task<Tuple<JobState, int, int>> RegisterJob(int jobid)
        {
            return await this.Channel.RegisterJob(jobid).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the job's properties
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="properties">the properties table</param>
        /// <returns>returns a value indicating whether update successfully</returns>
        public Task<bool> UpdateBrokerInfo(int jobid, Dictionary<string, object> properties)
        {
            // Does not need this operation
            throw new NotSupportedException();
        }

        /// <summary>
        /// Finish a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        public async Task FinishJob(int jobid, string reason)
        {
            await this.Channel.FinishJob(jobid, reason).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a node to job's exclude node list
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="nodeName">name of the node to be excluded</param>
        /// <param name="maxExcludeNodeCount">maximum number of nodes that could be excluded</param>
        /// <returns>true if the node is successfully blacklisted, or the job is failed. false otherwise</returns>
        public bool ExcludeNode(int jobid, string nodeName, int maxExcludeNodeCount)
        {
            // Does not need this operation
            throw new NotSupportedException();
        }

        #region ISchedulerAdapter Members

        /// <summary>
        /// Exclude node. Not implemented yet.
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="nodeName">name of the node</param>
        /// <returns>true if node is blacklisted successfully, false otherwise</returns>
        public Task<bool> ExcludeNode(int jobid, string nodeName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fail a job. Not implemented yet.
        /// </summary>
        /// <param name="jobid">id of the job to fail</param>
        /// <param name="reason">reason for this operation</param>
        public Task FailJob(int jobid, string reason)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requeue or fail a job. Not implemented yet.
        /// </summary>
        /// <param name="jobid">id of the job to requeue or fail</param>
        /// <param name="reason">reason for this operation</param>
        public Task RequeueOrFailJob(int jobid, string reason)
        {
            throw new NotImplementedException();
        }

        public Task<int?> GetTaskErrorCode(int jobId, int globalTaskId)
        {
            return Task.FromResult<int?>(null);
        }

        //This method is used to support graceful preemption, but we don't need it in the sample code.
        public Task<Tuple<bool, int, List<int>, List<int>>> GetGracefulPreemptionInfo(int jobId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FinishTask(int jobId, int taskUniqueId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
