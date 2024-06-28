// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;
using System.Diagnostics;

namespace DynamicNodeGroups
{
    class Program
    {
        static ManualResetEvent running = new ManualResetEvent(false);

        //for best results, run this sample code in queued scheduling mode
        static async Task Main(string[] args)
        {
            string? clusterName = Environment.GetEnvironmentVariable("CCP_SCHEDULER");
            using (IScheduler scheduler = new Scheduler())
            {
                Console.WriteLine("Connecting to {0}", clusterName);
                scheduler.Connect(clusterName);
                Console.WriteLine("Connected");
                //assume you have two nodegroups, NodeGroup1 and NodeGroup2
                IStringCollection nodeGroup1 = scheduler.GetNodesInNodeGroup("NodeGroup1");
                IStringCollection nodeGroup2 = scheduler.GetNodesInNodeGroup("NodeGroup2");
                if (nodeGroup1.Count == 0 || nodeGroup2.Count == 0)
                {
                    Console.WriteLine("Node groups are not set up correctly");
                    return;
                }

                //and nodes in NodeGroup2 are not in NodeGroup1, and vise versa.
                string nodeToMove = "";
                foreach (string node in nodeGroup2)
                {
                    if (!nodeGroup1.Contains(node))
                    {
                        nodeToMove = node;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(nodeToMove))
                {
                    Console.WriteLine("No eligible nodes to move");
                    return;
                }

                //create a job to run on NodeGroup1
                ISchedulerJob job = scheduler.CreateJob();
                job.NodeGroups.Add("NodeGroup1");
                //Set unit type to node, but let it autocalculate resources
                job.UnitType = JobUnitType.Node;

                ISchedulerTask task = job.CreateTask();
                task.CommandLine = "uname";

                //TaskType.Service means starting new instances endlessly until the task is canceled
                task.Type = TaskType.Service;
                job.AddTask(task);

                job.OnTaskState += new EventHandler<TaskStateEventArg>(Job_OnTaskState);
                Console.WriteLine("Submitting job on NodeGroup1");
                scheduler.SubmitJob(job, null, null);
                Console.WriteLine("Job {0} Submitted", job.Id);

                //wait for the job to start running
                running.WaitOne();

                job.Refresh();
                int allocationCount = job.AllocatedNodes.Count;
                Console.WriteLine("Number of allocated nodes: {0}", allocationCount);

                //Check the status of NodeGroup1 nodes
                int idleCores = 0;
                foreach (string nodename in nodeGroup1)
                {
                    ISchedulerNode node = scheduler.OpenNodeByName(nodename);
                    idleCores += node.GetCounters().IdleCoreCount;
                }

                //There are no more idle cores remaining in this node group
                //So we'll place one of the nodes from NodeGroup2 allow the job to grow 
                if (idleCores == 0)
                {
                    running.Reset();

                    //Changing nodegroups is available through the UI or PowerShell
                    string powershellScript = string.Format("add-pssnapin microsoft.hpc; " +
                        "add-hpcgroup -scheduler {0} -name {1} -nodename {2}",
                        clusterName, "NodeGroup1", nodeToMove);
                    Console.WriteLine("Command: {0}", powershellScript);
                    Console.WriteLine($"Move node {nodeToMove} to NodeGroup1");

                    // requires x64-version build to enable PowerShell support
                    var processStartInfo = new ProcessStartInfo("powershell.exe", powershellScript);
                    var process = new Process();
                    process.StartInfo = processStartInfo;
                    process.Start();

                    running.WaitOne();
                    Console.WriteLine("(Waiting 5 seconds for job to update the scheduler)");
                    await Task.Delay(5 * 1000);
                    job.Refresh();

                    //verify that job has grown
                    int newAllocationCount = job.AllocatedNodes.Count;
                    Console.WriteLine("newAllocationCount: {0}", newAllocationCount);
                    Console.WriteLine("allocationCount: {0}", allocationCount);
                    
                    if (newAllocationCount > allocationCount)
                    {
                        Console.WriteLine("Job has grown to {0} nodes", newAllocationCount);
                    }
                }
                else
                {
                    Console.WriteLine("There are still idle cores in the NodeGroup1");
                }
            }
        }

        static void Job_OnTaskState(object? sender, TaskStateEventArg e)
        {
            if (e.NewState == TaskState.Running)
            {
                running.Set();
            }
        }
    }
}
