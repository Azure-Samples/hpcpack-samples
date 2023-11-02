// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Microsoft.Hpc.Scheduler.Session;
using FireNRecollect.EchoService;
using System.ServiceModel;

namespace FireNRecollect
{
    class Program
    {
        static void Main(string[] args)
        {
            const string headnode = "[headnode]";
            const string serviceName = "EchoService";

            if (args.Length == 1)
            {
                // attach to the session
                int sessionId = int.Parse(args[0]);
                SessionAttachInfo info = new SessionAttachInfo(headnode, sessionId);

                Console.WriteLine("Attaching to session {0}...", sessionId);
                // Create attach to a session 
                using (DurableSession session = DurableSession.AttachSession(info))
                {
                    Console.WriteLine("done.");

                    // Create a client proxy
                    using (BrokerClient<IService1> client = new BrokerClient<IService1>(session))
                    {
                        Console.WriteLine("Retrieving results...");
                        // Get all the results
                        foreach (BrokerResponse<EchoResponse> response in client.GetResponses<EchoResponse>())
                        {
                            string reply = response.Result.EchoResult;
                            Console.WriteLine("\tReceived response for request {0}: {1}", response.GetUserData<int>(), reply);
                        }
                        Console.WriteLine("Done retrieving results.");
                    }

                    // Close the session to reclaim the system storage
                    // used to store the results.  After the session is closed
                    // you cannot attatch to the same session again
                    session.Close();
                }
            }
            else
            {
                // Create a durable session, fire the requests and exit
                SessionStartInfo info = new SessionStartInfo(headnode, serviceName);
                // If the cluster is non-domain joined, add the following statement
                // info.Secure = false;

                Console.WriteLine("Creating a session...");
                using (DurableSession session = DurableSession.CreateSession(info))
                {
                    Console.WriteLine("done session id = {0}.", session.Id);
                    NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
                    // If the cluster is non-domain joined, use the following statement
                    // NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

                    using (BrokerClient<IService1> client = new BrokerClient<IService1>(session, binding))
                    {
                        Console.WriteLine("Sending requests...");
                        for (int i = 0; i < 12; i++)
                        {
                            EchoRequest request = new EchoRequest("hello world!");
                            client.SendRequest(request, i);
                        }
                        client.EndRequests();
                        Console.WriteLine("done");
                    }

                    Console.WriteLine("Type \"FileNRecollect.exe {0}\" to collect the results", session.Id);
                }
            }
        }
    }
}

