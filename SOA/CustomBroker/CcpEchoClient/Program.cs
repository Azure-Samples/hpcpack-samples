//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The main entry point for the application.
// </summary>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using Microsoft.Hpc.Scheduler.Session;
using Microsoft.Hpc.EchoSvcClient.Durable;

namespace ccpEchoClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //change the headnode name here
            const string headnode = "[headnode]";
            const string serviceName = "CcpEchoSvc";
            const int numRequests = 12;
            SessionStartInfo info = new SessionStartInfo(headnode, serviceName);

            Console.Write("Creating a session for CcpEchoSvc...");


            // Create session 
            using (Session session = Session.CreateSession(info))
            {
                Console.WriteLine("Session {0} has been created", session.Id);
                
                NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport);
                var cf = new ChannelFactory<IEchoSvc>(binding, session.EndpointReference);
                var channel = cf.CreateChannel();

                ManualResetEvent wait = new ManualResetEvent(false);

                int total = numRequests;
                for (int i = 0; i < numRequests; i++)
                {
                    channel.BeginEcho(new EchoRequest("hello world " + i), (ar) =>
                    {
                        Console.WriteLine(String.Format("Echo result: {0}", channel.EndEcho(ar).EchoResult));
                        if (Interlocked.Decrement(ref total) == 0)
                        {
                            wait.Set();
                        }
                    }, null);
                }

                wait.WaitOne();
                //explict close the session to free the resource
                session.Close();
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
