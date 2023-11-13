﻿// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
// This namespace is defined in the HPC Server 2016 SDK
// which includes the HPC SOA Session API.   
using Microsoft.Hpc.Scheduler.Session;
// This namespace is defined in the "EchoService" service reference
using HelloWorldR2Linq.EchoService;
using System.ServiceModel;

namespace HelloWorldR2Linq
{
    class Program
    {
        static void Main(string[] args)
        {
            //change the headnode name here
            const string headnode = "[headnode]";
            const string serviceName = "EchoService";
            const int numRequests = 12;

            SessionStartInfo info = new SessionStartInfo(headnode, serviceName);
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
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
                // If the cluster is non-domain joined, use the following statement
                // NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

                // Create a BrokerClient proxy
                // This proxy is able to map One-Way, Duplex message exchange patterns 
                // with the Request / Reply Services.  As such, the client program can send the
                // requests, exit and re-attach to the session to retrieve responses (see the 
                // FireNRecollect project for details
                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session, binding))
                {
                    Console.WriteLine($"Sending {numRequests} requests...");
                    for (int i = 0; i < numRequests; i++)
                    {
                        // EchoRequest are created as you add Service Reference
                        // EchoService to the project
                        EchoRequest request = new EchoRequest("hello world!" + i);
                        client.SendRequest(request, i);
                    }

                    // Flush the message.  After this call, the runtime system
                    // starts processing the request messages.  If this call is not called,
                    // the system will not process the requests.  The client.GetResponses() will return
                    // with an empty collection
                    client.EndRequests();
                    Console.WriteLine("done");

                    Console.WriteLine("Retrieving responses...");

                    // GetResponses from the runtime system
                    // EchoResponse class is created as you add Service Reference "EchoService"
                    // to the project
                    try
                    {
                        IEnumerable<string> query = from s in client.GetResponses<EchoResponse>()
                                                 where s.Result.EchoResult.Contains("hello")
                                                 orderby s.GetUserData <int>()
                                                 select s.Result.EchoResult;

                        foreach (string k in query)
                        {
                            Console.WriteLine("\tReceived response for request {0}", k);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error occured {0}", ex.Message);
                    }
                    
                    Console.WriteLine("Done retrieving {0} responses", numRequests);
                }

                //explict close the session to free the resource
                session.Close();
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
