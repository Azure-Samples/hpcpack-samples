// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
// This namespace is defined in the HPC Server 2016 SDK
// which includes the HPC SOA Session API.   
using Microsoft.Hpc.Scheduler.Session;
// This namespace is defined in the "EchoService" service reference
// These namespaces are defined in the HPC Server 2016 SDK
// which includes the HPC Scheduler API.   
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;
using HelloWorldR2OnExit.EchoService;
using System.ServiceModel;
using System.Threading;

namespace HelloWorldR2OnExit
{
    class Program
    {
        static void Main(string[] args)
        {
            //change the headnode name here
            const string headnode = "[headnode]";
            const string serviceName = "EchoService";
            const int numRequests = 8;

            SessionStartInfo info = new SessionStartInfo(headnode, serviceName);
            // If the cluster is non-domain joined, add the following statement
            // info.Secure = false;

            //the sample code needs at least 2 cores in the cluster
            info.SessionResourceUnitType = SessionUnitType.Core;
            info.MaximumUnits = 2;
            info.MinimumUnits = 2;

            Console.WriteLine("Creating a session for EchoService...");
            using (DurableSession session = DurableSession.CreateSession(info))
            {
                Console.WriteLine("done session id = {0}", session.Id);

                NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
                // If the cluster is non-domain joined, use the following statement
                // NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session, binding))
                {
                    Console.WriteLine("Sending {0} requests...", numRequests);

                    for (int i = 0; i < numRequests; i++)
                    {
                        EchoOnExitRequest request = new EchoOnExitRequest(new TimeSpan(0, 0, 5));
                        client.SendRequest(request, i);
                    }

                    client.EndRequests();
                    Console.WriteLine("done");

                    // cancel half of the service tasks when processing the requests
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        //wait 5 seconds to try cancel service tasks.
                        Thread.Sleep(3 * 1000);
                        try
                        {
                            Scheduler scheduler = new Scheduler();
                            try
                            {
                                scheduler.Connect(headnode);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error connecting store.{0}", e.ToString());
                                return;
                            }

                            int jobId = session.GetProperty<int>("HPC_ServiceJobId");
                            ISchedulerJob job = scheduler.OpenJob(jobId);
                            job.Refresh();
                            ISchedulerCollection taskList = job.GetTaskList(null, null, true);
                            int onFlag = 0;
                            foreach (ISchedulerTask task in taskList)
                            {
                                // cancel half of the service tasks
                                if (onFlag++ % 2 == 0)
                                {
                                    try
                                    {
                                        if (task.State == TaskState.Running)
                                        {
                                            Console.WriteLine("Try to cancel task {0}", task.TaskId);
                                            job.CancelTask(task.TaskId);
                                            job.Commit();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Got exception when trying to cancel task {0}:{1}", task.TaskId, ex.Message);
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception when trying to cancel the service tasks. {0}", ex.Message);
                        }
                    });

                    Console.WriteLine("Retrieving responses...");

                    try
                    {
                        int count = 0;

                        foreach (var response in client.GetResponses<EchoOnExitResponse>())
                        {
                            try
                            {
                                string reply = response.Result.EchoOnExitResult;
                                Console.WriteLine("\tReceived response for request {0}: {1}", response.GetUserData<int>(), reply);
                                count++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error occured while processing {0}-th request: {1}", response.GetUserData<int>(), ex.Message);
                            }
                        }

                        Console.WriteLine("Done retrieving responses.{0}/{1} responses retrieved ", count, numRequests);
                    }
                    catch (SessionException ex)
                    {
                        Console.WriteLine("SessionException while getting responses: {0}", ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception while getting responses: {0}", ex.Message);
                    }
                }

                // Close connections and delete messages stored in the system
                session.Close();

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
