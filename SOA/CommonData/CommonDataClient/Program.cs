// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
// This namespace is defined in the HPC Server 2016 SDK
using Microsoft.Hpc.Scheduler.Session.Data;
// This namespace is defined in the HPC Server 2016 SDK
using Microsoft.Hpc.Scheduler.Session;
// This namespace is defined in the "CommonDataService" service reference
using CommonDataClient.CommonDataService;

namespace CommonDataClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string hostname = "[headnode]";

            // DataClient Id used to identify the data, this should be unique across the cluster
            string raw_data_id = "raw_data_id";
            string dictionary_data_id = "dictionary_data_id";

            Console.WriteLine("Start. " + DateTime.Now.ToLongTimeString());

            // Create a DataClient to store a Dictionary data
            using (DataClient client = DataClient.Create(hostname, dictionary_data_id))
            {
                Console.WriteLine("Data {0} Created. {1} ", dictionary_data_id, DateTime.Now.ToLongTimeString());
                // Here we have a DataClient whose life cycle is not managed by SOA
                Dictionary<string, string> objects = new Dictionary<string, string>();
                objects.Add("key1", "value1");
                objects.Add("key2", "value2");
                // WriteAll() can only be called once on a data client. 
                client.WriteAll<Dictionary<string, string>>(objects);
            }

            SessionStartInfo sessionStartInfo = new SessionStartInfo(hostname, "CommonDataService");
            // Pass DataClient Id in SOA session's environment variable so that it could be read from service code
            sessionStartInfo.Environments.Add("DICTIONARY_DATA_ID", dictionary_data_id);

            using (Session session = Session.CreateSession(sessionStartInfo))
            {
                Console.WriteLine("Session {0} Created. {1} ", session.Id, DateTime.Now.ToLongTimeString());

                // Create a DataClient to store the raw data read from the file
                using (DataClient client = DataClient.Create(hostname, raw_data_id))
                {
                    Console.WriteLine("Data {0} Created {1}. ", client.Id, DateTime.Now.ToLongTimeString());
                    // Add a data life cycle management so that it'll be deleted when session is done
                    // Otherwise, the data will have to be cleaned up by client
                    client.SetDataLifeCycle(new DataLifeCycle(session.Id));
                    // WriteRawBytesAll() doesn't serialize the object and will write the byte stream directly.
                    // Use this when you want to transport a file or have non-.Net code that cannot handle .net serialization. 
                    client.WriteRawBytesAll(File.ReadAllBytes("DataFile.txt"));
                }

                // Send/Receive SOA requests
                using (BrokerClient<ICommonDataService> client = new BrokerClient<ICommonDataService>(session))
                {
                    Console.WriteLine("Send Requests. " + DateTime.Now.ToLongTimeString());
                    client.SendRequest<GetDataRequest>(new GetDataRequest(raw_data_id));
                    client.EndRequests();
                    Console.WriteLine("Get Response. " + DateTime.Now.ToLongTimeString());
                    foreach (BrokerResponse<GetDataResponse> resp in client.GetResponses<GetDataResponse>())
                    {
                        string result = resp.Result.GetDataResult;
                        Console.WriteLine(result);
                    }
                }
                Console.WriteLine("Start closing session. " + DateTime.Now.ToLongTimeString());
            }
            Console.WriteLine("Session Closed. " + DateTime.Now.ToLongTimeString());
            
            Console.WriteLine("Start cleaning up the Data {0}. {1} ", dictionary_data_id, DateTime.Now.ToLongTimeString());
            // We should delete the DataClient "dictionary_data_id" here since it's not managed by SOA
            DataClient.Delete(hostname, dictionary_data_id);
            
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
