// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Threading;
// This namespace is defined in the HPC Server 2016 SDK
// which includes the HPC SOA Session API.   
using Microsoft.Hpc.Scheduler.Session;
// This namespace is defined in the "EchoService" service reference
using HelloWorldR2ReliableBrokerClient.EchoService;

namespace HelloWorldR2ReliableBrokerClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //change the headnode name here
            const string headnode = "[headnode]";
            const string serviceName = "EchoService";
            const int numRequests = 12;
            SessionStartInfo startInfo = new SessionStartInfo(headnode, serviceName);
            // If the cluster is non-domain joined, add the following statement
            // startInfo.Secure = false;

            startInfo.BrokerSettings.SessionIdleTimeout = 15 * 60 * 1000;
            startInfo.BrokerSettings.ClientIdleTimeout = 15 * 60 * 1000;

            Console.WriteLine("Creating a session for EchoService...");
            const int retryCountMax = 20;
            const int retryIntervalMs = 5000;

            DurableSession session = DurableSession.CreateSession(startInfo);
            // Create a durable session 
            int sessionId = session.Id;
            Console.WriteLine("Done session id = {0}", sessionId);

            //send requests with reliable broker client
            bool successFlag = false;
            int retryCount = 0;
            
            using (BrokerClient<IService1> client = new BrokerClient<IService1>(session))
            {
                Console.WriteLine("Sending {0} requests...", numRequests);
                while (!successFlag && retryCount++ < retryCountMax)
                {
                    try
                    {
                        for (int i = 0; i < numRequests; i++)
                        {
                            client.SendRequest(new EchoRequest(i.ToString()),i);
                        }

                        client.EndRequests();
                        successFlag = true;
                        Console.WriteLine("done");
                    }
                    catch (Exception e)
                    {
                        //general exceptions
                        Console.WriteLine("Exception {0}", e.ToString());
                        Thread.Sleep(retryIntervalMs);
                    }
                }
            }

            //attach the session
            _ = new SessionAttachInfo(headnode, sessionId);

            retryCount = 0;
            Console.WriteLine("Retrieving responses...");
            using (BrokerClient<IService1> client = new BrokerClient<IService1>(session))
            {
                int responseCount = 0;
                retryCount = 0;
                while (responseCount < numRequests && retryCount++ < retryCountMax)
                {
                    try
                    {
                        foreach (var response in client.GetResponses<EchoResponse>())
                        {
                            Console.WriteLine("\tReceived response for request {0}: {1}", response.GetUserData<int>(), response.Result.EchoResult);
                            responseCount++;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Thread.Sleep(retryIntervalMs);
                    }
                } 
            }

            Console.WriteLine("Close the session...");
            session.Close(true);
            
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
