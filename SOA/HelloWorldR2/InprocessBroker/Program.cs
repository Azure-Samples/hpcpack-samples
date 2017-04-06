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
using System.ServiceModel;
using Microsoft.Hpc.Scheduler.Session;
using InprocessBroker.EchoService; 

namespace InprocessBroker
{
    class Program
    {
        static void Main(string[] args)
        {
            // Replace [HeadName] with the real machine name of the HeadNode in your cluster, say "[headnode]"
            const string headnode = "[HeadName]";                                           
            const string serviceName = "EchoService";
            int numOfRequests = 10; 
            
            SessionStartInfo sessionStartInfo = new SessionStartInfo(headnode, serviceName);

            // UseInprocessBroker indicates what kind of broker session is going to use. True means inprocess broker will be adopted
            // while false means a dedicated broker node will be used by the session.
            Console.WriteLine("Enable UseInprocessBroker...");
            sessionStartInfo.UseInprocessBroker = true;

            // Inprocess broker is only supported with non-shared interactive session,
            // so please make sure the property ShareSession of SessionStartInfo is set to false.
            sessionStartInfo.ShareSession = false;             

            Console.WriteLine("Creating an interactive session...");
            using (Session session = Session.CreateSession(sessionStartInfo))
            {
                Console.WriteLine("Interactive Session {0} created.", session.Id);

                // With inprocess broker, client can directly talk to its service hosts on computer nodes, so the EndPointReference is null.
                Console.WriteLine("Session's EndpointReference: {0}.", session.EndpointReference == null ? "NULL": session.EndpointReference.ToString());

                // Inprocess broker only support V3 style broker client, so you should create broker client as below instead of 
                // Service1Client tcpclient = new Service1Client(new NetTcpBinding(SecurityMode.Transport), session.NetTcpEndpointReference);
                using (BrokerClient<IService1> client = new BrokerClient<IService1>(session))
                {
                    Console.WriteLine("Sending {0} requests...", numOfRequests);
                    for (int i = 0; i < numOfRequests; i++)
                    {
                        EchoRequest request = new EchoRequest("Hello World " + i.ToString());
                        client.SendRequest<EchoRequest>(request, i);
                    }

                    Console.WriteLine("Calling EndRequests to notify the end of sending requests... ");
                    client.EndRequests();

                    Console.WriteLine("Retrieving responses...");

                    int count = 0;
                    foreach (BrokerResponse<EchoResponse> response in client.GetResponses<EchoResponse>())
                    {
                        string reply = response.Result.EchoResult;
                        Console.WriteLine("Received response for request {0}: {1}", response.GetUserData<int>(), reply);
                        count++;
                    }

                    if (count != numOfRequests)
                    {
                        Console.WriteLine("Error: Responses lost. Expected {0} responses, but actually {1} returned.", numOfRequests, count);                       
                    }

                    Console.WriteLine("Retrieving results done."); 
                }

                session.Close(); 
            }
            Console.WriteLine("Session closed."); 

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
