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
using Microsoft.Hpc.Scheduler.Session;
using HelloWorldR2Callback.EchoService;
using System.ServiceModel;
using System.Threading;

namespace HelloWorldR2Callback
{
    class Program
    {
        static void Main(string[] args)
        {
            const string headnode = "[headnode]";
            const string serviceName = "EchoService";
            const int numRequests = 12;
            int count = 0;

            SessionStartInfo info = new SessionStartInfo(headnode, serviceName);
            AutoResetEvent done = new AutoResetEvent(false);

            Console.Write("Creating a session for EchoService...");
            using (Session session = Session.CreateSession(info))
            {
                Console.WriteLine("done session id = {0}", session.Id);
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);

                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session, binding))
                {
                    //set getresponse handler
                    client.SetResponseHandler<EchoResponse>((item) =>
                    {
                        try
                        {
                            Console.WriteLine("\tReceived response for request {0}: {1}",
                            item.GetUserData<int>(), item.Result.EchoResult);
                        }
                        catch (SessionException ex)
                        {
                            Console.WriteLine("SessionException while getting responses in callback: {0}", ex.Message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Exception while getting responses in callback: {0}", ex.Message);
                        }

                        if (Interlocked.Increment(ref count) == numRequests)
                            done.Set();

                    });

                    // start to send requests
                    Console.Write("Sending {0} requests...", numRequests);

                    for (int i = 0; i < numRequests; i++)
                    {
                        EchoRequest request = new EchoRequest("hello world!");
                        client.SendRequest<EchoRequest>(request, i);
                    }
                    client.EndRequests();
                    Console.WriteLine("done");

                    Console.WriteLine("Retrieving responses...");

                    // Main thread block here waiting for the retrieval process
                    // to complete.  As the thread that receives the "numRequests"-th 
                    // responses does a Set() on the event, "done.WaitOne()" will pop
                    done.WaitOne();
                    Console.WriteLine("Done retrieving {0} responses", numRequests);
                }

                // Close connections and delete messages stored in the system
                session.Close();
            }

            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }
    }
}

