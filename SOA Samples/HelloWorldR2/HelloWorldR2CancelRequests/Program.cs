// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// This namespace is defined in the HPC Server 2016 SDK
// which includes the HPC SOA Session API.   
using Microsoft.Hpc.Scheduler.Session;
// This namespace is defined in the HPC Server 2016 SDK
using Microsoft.Hpc.Scheduler.Properties;
// This namespace is defined in the "EchoService" service reference
using HelloWorldR2CancelRequests.EchoService;
using System.ServiceModel;
using System.Threading;
using System.Configuration;


namespace HelloWorldR2CancelRequests
{
    class Program
    {
        static void Main(string[] args)
        {
            //change the headnode name  here
            const string headnode = "[headnode]";
            const string serviceName = "EchoService";
            const int numRequests = 100;

            SessionStartInfo info = new SessionStartInfo(headnode, serviceName);

            //the cluster need to have a minimum 2 cores to run this sample code
            info.SessionResourceUnitType = SessionUnitType.Core;
            info.MaximumUnits = 2;
            info.MinimumUnits = 2;

            Console.Write("Creating a session for EchoService...");

            using (DurableSession session = DurableSession.CreateSession(info))
            {
                Console.WriteLine("done session id = {0}", session.Id);
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
                int sessionId = session.Id;
                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session, binding))
                {

                    Console.Write("Sending {0} requests...", numRequests);

                    for (int i = 0; i < numRequests; i++)
                    {
                        EchoOnExitRequest request = new EchoOnExitRequest(new TimeSpan(0, 0, 1));
                        client.SendRequest<EchoOnExitRequest>(request, i);
                    }

                    client.EndRequests();
                    Console.WriteLine("done");

                    //separate a work thread to purge the client when the requests are processing
                    ThreadPool.QueueUserWorkItem(delegate
                    {

                        //wait 30 seconds to try cancel service tasks.
                        Console.Write("Will cancel the requests in 30 seconds.");
                        Thread.Sleep(30 * 1000);
                        try
                        {
                            client.Close(true);
                            Console.WriteLine("The broker client is purged.");
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine("Exception in callback when purging the client. {0}", ee.ToString());
                        }

                    });

                    // retieving the responses
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

                // Close the session.
                session.Close();

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
