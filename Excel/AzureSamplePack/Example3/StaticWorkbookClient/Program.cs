//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Sample of a SOA client that interacts with an Excel Workbook on an
//      HPC cluster.
// </summary>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Session;
using Microsoft.Hpc.Scheduler.Properties;

namespace StaticWorkbookClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // either fill in this value or use the command-line parameter

            string headnode = ""; 

            string serviceName = "StaticWorkbookService";
            string jobTemplate = "AzureTemplate";

            string relativePath = "workbooks";
            string spreadsheetName = "StaticWorkbook.xlsx";

            // SOA variables
            Session session = null;
            BrokerClient<IStaticWorkbookService> client = null;
            bool sessionCreatedSuccessfully = false;

            // parse command-line arguments

            for (int i = 0; i < args.Length; i++)
            {
                if( args[i].Equals( "-headnode" ) && i< args.Length-1 ){ headnode = args[++i] ;};
                if( args[i].Equals( "-serviceName" ) && i< args.Length-1 ){ serviceName = args[++i] ;};
                if( args[i].Equals( "-jobTemplate" ) && i< args.Length-1 ){ jobTemplate = args[++i] ;};
                if( args[i].Equals( "-relativePath" ) && i< args.Length-1 ){ relativePath = args[++i] ;};
                if (args[i].Equals("-spreadsheetName") && i < args.Length - 1) { spreadsheetName = args[++i]; };
            }

            if( null == headnode || headnode.Equals( "" ))
            {
                Console.WriteLine("");
                Console.WriteLine("Error: call this application with the argument -headnode [name].");
                Console.WriteLine( "");
                Console.WriteLine( "Arguments:");
                Console.WriteLine("");
                Console.WriteLine("-headnode [name]        the name of your cluster scheduler");
                Console.WriteLine("-serviceName [name]     optional, defaults to 'StaticWorkbookService'");
                Console.WriteLine("-jobTemplate [name]     optional, defaults to 'AzureTemplate'");
                Console.WriteLine("-relativePath [name]    optional, defaults to 'Workbooks'");
                Console.WriteLine("-spreadsheetName [name] optional, defaults to 'StaticWorkbook.xlsx'");
                Console.WriteLine("");
                return;
            }

            try
            {
                // Create the SOA session and client
                SessionStartInfo info = new SessionStartInfo(headnode, serviceName);
                info.SessionResourceUnitType = SessionUnitType.Core;
                info.MinimumUnits = 1;
                info.MaximumUnits = 128;
                info.Secure = false;
                info.JobTemplate = jobTemplate;

                Console.WriteLine("Creating session and client");
                session = Session.CreateSession(info);
                client = new BrokerClient<IStaticWorkbookService>(session);

                // The Azure VM nodes use an environment variable to 
                // describe the root installation path for packages 
                // uploaded with hpcpack.  We used a relative path to
                // upload the spreadsheets, so we add that path to 
                // the environment variable to locate the spreadsheet.
                string spreadsheetPath = string.Format(@"%CCP_PACKAGE_ROOT%\{0}\{1}", relativePath, spreadsheetName);

                
                // call the workbook with different sets of parameters.
                // we're just pasting values into the spreadsheet, and requesting
                // some other cells as the results after the calculation.
                // have a look at the spreadsheet to see what cells we
                // are updating and what cells we are retrieving after
                // the calculation.

                // these values are constant, we are varying strike and time
                double spotPrice = 100;
                double volatility = 0.25;
                double riskFreeRate = 0.035;
                double dividendYield = 0.02;

                for (double strike = 75; strike <= 125; strike += 5)
                {
                    for (double timeInMonths = 1; timeInMonths <= 24; timeInMonths++)
                    {
                        double timeInYears = timeInMonths / 12.0;

                        // the values are passed to the service using the 
                        // Excel range identifiers.  we can pass as many as
                        // necessary.  just make sure they're the same length,
                        // and that they match up (e.g. C5 -> spotPrice).
                        string[] ranges = { "C5", "C6", "C7", "C8", "C9", "C10" };
                        object[] values = { spotPrice, strike, timeInYears, volatility, riskFreeRate, dividendYield };

                        // the output values are the cells we want to retrieve
                        // when the calculation is complete
                        string[] outputRanges = { "C15", "C16" };

                        // to map our results back to the input parameters, we
                        // can use the index value
                        CalculateParametersRequest req = new CalculateParametersRequest();
                        req.spreadsheetPath = spreadsheetPath;
                        req.inputRanges = ranges;
                        req.inputValues = values;
                        req.outputRanges = outputRanges;

                        // we're passing the strike price and time values as "user data"
                        // attached to the request.  that will let us tie the results 
                        // back to the input data when the calculation completes.
                        client.SendRequest<CalculateParametersRequest>(req, new double[] { strike, timeInMonths });
                    }
                }

                client.EndRequests();
                sessionCreatedSuccessfully = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start session and send requests.\n" +
                                  "Verify that head node is correctly specified and all\n" +
                                  "sample documentation steps have been followed.\n" +
                                  "Error: {0}", e.ToString());
            }

            if (sessionCreatedSuccessfully)
            {
                Console.WriteLine("Waiting for response(s)...");

                // note that results will be returned in random order, based on which node
                // calculates and returns a value first.  because the input parameters
                // are returned as well, if we wanted we could sort the results before
                // printing.  for now, we will just output them to the terminal.

                try
                {
                    BrokerResponseEnumerator<CalculateParametersResponse> responses = client.GetResponses<CalculateParametersResponse>();
                    foreach (BrokerResponse<CalculateParametersResponse> response in responses)
                    {
                        double[] userData = response.GetUserData<double[]>();
                        object[] results = response.Result.CalculateParametersResult;

                        Console.WriteLine("Strike {0:0.00},\tExpiry {1:00} months: Call {2:00.00}, Put {3:00.00}",
                            userData[0], (int)userData[1], Double.Parse(results[0].ToString()), Double.Parse(results[1].ToString()));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to get responses. Error: {0}", e.ToString());
                }
            }
                       
            // done - clean up
            Console.WriteLine("\r\nCleaning up...");
            try
            {
                if (client != null)
                {
                    client.Close();
                }
            }
            catch
            {
                Console.WriteLine("Failed to close client cleanly.");
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                }
            }

            try
            {
                if (session != null)
                {
                    session.Close();
                }
            }
            catch
            {
                Console.WriteLine("Failed to close session cleanly. Check for running jobs.");
            }
            finally
            {
                if(session != null)
                {
                    session.Dispose();
                }
            }

        }
    }
}
