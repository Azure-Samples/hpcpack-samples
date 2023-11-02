﻿// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
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
using HelloWorldR2MultiBatch.EchoService;
using System.ServiceModel;
using System.Threading;

namespace HelloWorldR2MultiBatch
{
    class Program
    {
        const string headnode = "[headnode]";
        const string serviceName = "EchoService"; 
        const int TotalThread = 3;

        static void Main(string[] args)
        {
            SessionStartInfo info = new SessionStartInfo(headnode ,serviceName);
            // If the cluster is non-domain joined, add the following statement
            // info.Secure = false;

            Console.WriteLine("Creating a session for EchoService...");

            // Create a durable session 
            // Request and response messages in a durable session are persisted so that
            // in event of failure, no requests nor responses will be lost.  Another authorized
            // client can attached to a session with the same session Id and retrieve responses
            using (DurableSession session = DurableSession.CreateSession(info))
            {
                Console.WriteLine("done session id = {0}", session.Id);

                Thread[] threads = new Thread[TotalThread];

                for (int i = 0; i < TotalThread; i++)
                {
                    threads[i] = new Thread(new ThreadStart(() => { Worker(session.Id); }));
                    threads[i].Start();
                }

                for (int i = 0; i < TotalThread; i++)
                {
                    threads[i].Join();
                }
                
                session.Close();
            }

            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }

        private static void Worker(int sessionId)
        {
            DurableSession session = DurableSession.AttachSession(new SessionAttachInfo(headnode, sessionId));
            int numRequests = 32;

            NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
            // If the cluster is non-domain joined, use the following statement
            // NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

            string guid = Guid.NewGuid().ToString();

            // Create a BrokerClient proxy
            // This proxy is able to map One-Way, Duplex message exchange patterns 
            // with the Request / Reply Services.  As such, the client program can send the
            // requests, exit and re-attach to the session to retrieve responses (see the 
            // FireNRecollect project for details

            using (BrokerClient<IService1> client = new BrokerClient<IService1>(guid, session, binding))
            {
                Console.WriteLine("Sending {0} requests...", numRequests);
                for (int i = 0; i < numRequests; i++)
                {
                    // EchoRequest are created as you add Service Reference
                    // EchoService to the project
                    EchoRequest request = new EchoRequest("hello world!");
                    client.SendRequest(request, i);
                }

                // Flush the message.  After this call, the runtime system
                // starts processing the request messages.  If this call is not called,
                // the system will not process the requests.  The client.GetResponses() will return
                // with an empty collection
                client.EndRequests();
                client.Close();
                Console.WriteLine("done");
            }

            using (BrokerClient<IService1> client = new BrokerClient<IService1>(guid, session, binding))
            {
                Console.WriteLine("Retrieving responses...");

                // GetResponses from the runtime system
                // EchoResponse class is created as you add Service Reference "EchoService"
                // to the project

                foreach (var response in client.GetResponses<EchoResponse>())
                {
                    try
                    {
                        string reply = response.Result.EchoResult;
                        Console.WriteLine("\tReceived response for request {0}: {1}", response.GetUserData<int>(), reply);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error occured while processing {0}-th request: {1}", response.GetUserData<int>(), ex.Message);
                    }
                }

                Console.WriteLine("Done retrieving {0} responses", numRequests);
            }
        }
    }
}
