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

namespace HelloWorldR2ServiceVersioning
{
    class Program
    {
        static void Main(string[] args)
        {
            //change the headnode name here
            const string headnode = "[headnode]";
            const string serviceName = "Microsoft.Hpc.Excel.XllContainer64";
            
            //Query service versions of Microsoft.Hpc.Excel.XllContainer64
            Version[] versions = SessionBase.GetServiceVersions(headnode, serviceName);
                        
            foreach (Version version in versions)
            {
                Console.WriteLine("Microsoft.Hpc.Excel.XllContainer64 version {0} is found in the service registration.", version.ToString());
            }
            
            //Get the latest version for the versions are already sorted, 
            Version latest = versions[0]; 
            //Here is should be version 1.1 for v3 sp2
            Console.WriteLine("The latest version is {0}", latest);

            //Create a session for Microsoft.Hpc.Excel.XllContainer64 with the latest version
            SessionStartInfo info = new SessionStartInfo(headnode, serviceName, latest);

            Console.WriteLine("Creating a session for Microsoft.Hpc.Excel.XllContainer64 version {0} ...", latest);

            // Create a durable session 
            using (DurableSession session = DurableSession.CreateSession(info))
            {
                Console.WriteLine("done session id = {0}", session.Id);
                //explict close the session to free the resource
                session.Close();
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
