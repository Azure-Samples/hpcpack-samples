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
// This namespace is defined in the "EchoService" service reference
using HelloWorldR2SessionPool.EchoService;
using System.ServiceModel;

namespace HelloWorldR2SessionPool
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
            //session in the session pool should be a shared session
            info.ShareSession = true;
            info.UseSessionPool = true;
            Console.Write("Creating a session using session pool for EchoService...");

            using (DurableSession session = DurableSession.CreateSession(info))
            {
                Console.WriteLine("done session id = {0}", session.Id);
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);

                //to make sure the client id is unique among the sessions
                string clientId = Guid.NewGuid().ToString();
                
                using (BrokerClient<IService1> client = new BrokerClient<IService1>(clientId, session, binding))
                {
                    Console.Write("Sending {0} requests...", numRequests);
                    for (int i = 0; i < numRequests; i++)
                    {
                        EchoRequest request = new EchoRequest("hello world!");
                        client.SendRequest<EchoRequest>(request, i);
                    }

                    client.EndRequests();
                    Console.WriteLine("done");

                    Console.WriteLine("Retrieving responses...");
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

                //should not purge the session if the session is expected to stay in the session pool
                //the shared session is kept in the session pool
                session.Close(false);

            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
