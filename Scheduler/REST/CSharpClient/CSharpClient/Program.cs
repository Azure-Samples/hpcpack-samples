// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

#if NET472
using System.Net.Http;
#endif

using static CSharpClient.Utils;
using System.Net;

/*
 * The following sample sources no longer require the REST Starter Kit.
 *
 * In addition, the variable "credentialsAreCachedOnHN" can be set:
 *      true: if the credentials (ntlm or basic) are cached on the head node
 *      false: otherwise.
*/

namespace CSharpClient
{
    public class Program
    {
        const string ContinuationHeader = "x-ms-continuation-";
        const string QueryIdQueryParam = "QueryId";
        const string QueryIdHeader = ContinuationHeader + QueryIdQueryParam;

        static string[]? ServerNames { get; set; }

        static int ServerConter { get; set; } = 0;

        static string ApiBase
        {
            get
            {
                //NOTE: Usually, a load balancer is used to select a backend server. This is a demo
                //on how to do it on client side when you have no better choice.
                int index = ServerConter++ % ServerNames!.Length;
                return $"https://{ServerNames[index]}/WindowsHpc";
            }
        }

        static void ShowHelp()
        {
            string help = @"
Usage:
{0} -c <server name> -u <user name> -p <password> [-C]

Options:
-c HPC server name to connect to. It can be single name or list of names separated by ','.
-u Name of a HPC user on the server.
-p Password of the user.
-C The switch specifies that the user credentail is cached on the HPC server. It's not by default.
";
            Console.WriteLine(string.Format(help, System.Diagnostics.Process.GetCurrentProcess().ProcessName));
        }

        static async Task Main(string[] args)
        {
            string? credUserName = null;
            string? credPassword = null;
            string? serverName = null;

            bool credentialsAreCachedOnHN = false;  // set to true if your creds were cached in the HN by ClusterManager, etc.

            // Parse command line
            try
            {
                ParseCommandLine(args, ref credUserName!, ref credPassword!, ref serverName!, ref credentialsAreCachedOnHN);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when parsing command line: " + ex.Message);
                ShowHelp();
                return;
            }

            if (credUserName == null || credPassword == null || serverName == null)
            {
                Console.WriteLine("One of credUserName, credPassword, serverName is not provided");
                ShowHelp();
                return;
            }

            ServerNames = serverName.Split(',');
            ICredentials credsBasic = new NetworkCredential(credUserName, credPassword);

            // This disables enforcement of certificat trust chains which enables the use of self-signed certs.
            // Comment out this line to enforce trusted chains between your REST service and REST clients.
            ServicePointManager.ServerCertificateValidationCallback = (obj, cert, chain, err) => true;

            var handler = new HttpClientHandler() { Credentials = credsBasic };
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("api-version", "2012-11-01.4.0");

            // GET Nodegroup list
            RestRow[]? nodeGroupRows = await GetNodegroupList(httpClient);
            Console.WriteLine("---Completed GET Nodegroup list---");

            // GET single Nodegroup
            // take the 0th nodegroup and fetch list of nodes
            string nodegroup = nodeGroupRows![0].Props![0].Value;
            string[]? nodesInGroup = await GetSingleNodegroup(httpClient, nodegroup);
            Console.WriteLine("---Completed GET single Nodegroup---");

            // GET single Node
            // take the 0th node and fetch list of properties
            string node = nodesInGroup![0];
            await GetSingleNode(httpClient, node);
            Console.WriteLine("---Completed GET single Node---");

            // GET the list of Nodes with Continuation Headers
            RestRow[]? nodes = await ReadCollectionWithContinuationAsync(httpClient, ApiBase + "/Nodes", "Properties=Name,Id,MemorySize");
            Console.WriteLine("---Completed GET the list of Nodes with Continuation Headers---");

            // Create and submit job using properties
            int jobId = await CreateJobUsingProperties(httpClient, credentialsAreCachedOnHN, credUserName, credPassword);
            Console.WriteLine("---Completed Create and submit job using properties---");

            // Cancel job
            await CancelJob(httpClient, jobId);
            Console.WriteLine("---Completed Cancel job---");

            // GET job properties
            await GetJobProperties(httpClient, jobId);
            Console.WriteLine("---Completed GET job properties---");

            // Create job using XML
            await CreateJobUsingXML(httpClient, jobId);
            Console.WriteLine("---Completed Create job using XML---");
            
            // List Jobs and use of Continuation Headers
            RestRow[]? jobs = await ReadCollectionWithContinuationAsync(httpClient, ApiBase + "/Jobs", "Properties=Id,Name,State");
            DisplayRowset(jobs!);
            Console.WriteLine("---Completed List Jobs and use of Continuation Headers---");

            // List Jobs filtered by JobState
            // here we requeset all canceled jobs via $filter= and Render=
            jobs = await ReadCollectionWithContinuationAsync(httpClient, ApiBase + "/Jobs", "Render=RestPropRender&$filter=JobState eq Canceled");
            DisplayRowset(jobs!);
            Console.WriteLine("---Completed List Jobs filtered by JobState---");

            Console.WriteLine("Completed for all samples!");
        }

        /// <summary>
        /// This routine will traverse a collection utilizing the continuation tokens/headers.
        /// This can be called for any of the APIs that use continuation headers.
        ///
        /// On Retry:
        ///     Continuation spans several calls any of which can fail.
        ///     It is important to remember that the REST API keeps enough state
        ///     to satisfy continuation calls.  If this state is lost, continuation cannot
        ///     complete.
        ///
        ///     When the REST API is hosted in "Windows Azure HPC Scheduler" (WAHS) there
        ///     are often several instances of the REST API service in a load-balanced
        ///     configuration.  Any one of these instances can fail or be rebooted for
        ///     host patching, etc.
        ///
        ///     The following continuation code will attempt to retry any single
        ///     continuation call.  This will compensate for transient network
        ///     issues, etc.
        ///
        ///     After the final attempted retry fails, the entire operation is restarted
        ///     once.  This will provide recovery for when the authoritative instance in WAHS
        ///     is lost (failure or patch-reboot, etc.).
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="baseQueryString"></param>
        public static async Task<RestRow[]?> ReadCollectionWithContinuationAsync(HttpClient httpClient, string baseURI, string baseQueryString)
        {
            RestRow[]? rows = null;

            try
            {
                // if initial attempt succeeds then return results
                rows = await ReadCollectionWithContinuationAndRetryAsync(httpClient, baseURI, baseQueryString);
            }
            catch (Exception ex1)
            {
                Console.WriteLine("Iteration failed on: " + baseURI + ".  Starting over.");
                Console.WriteLine($"Exception message: {ex1.Message}");

                try
                {
                    // restart entire operation and try again
                    rows = await ReadCollectionWithContinuationAndRetryAsync(httpClient, baseURI, baseQueryString);
                }
                catch (Exception ex2)
                {
                    HandleException(ex2);
                }
            }

            return rows;
        }

        /// <summary>
        /// The continuation token is contained in the continuation header.
        /// The contents of this header are to be put on the query string of the
        /// next call.  This tells the REST API to return the next batch of data.
        /// </summary>
        /// <param name="baseURI"></param>
        /// <param name="baseQueryString"></param>
        /// <returns></returns>
        public static async Task<RestRow[]> ReadCollectionWithContinuationAndRetryAsync(HttpClient httpClient, string baseURI, string baseQueryString)
        {
            List<RestRow> rows = new();
            string? queryId = null;
            string continuationSeperator;
            string combinedURI;

            // some mechanics around constructing the proper query string
            if (string.IsNullOrEmpty(baseQueryString))
            {
                // with no base query string, the continuation token would be first parameter and "?" is used
                continuationSeperator = "?";
                combinedURI = baseURI;
            }
            else
            {
                // with a base query string, the continutaion token will be appened to the end and "&" is used
                continuationSeperator = "&";
                combinedURI = baseURI + "?" + baseQueryString;
            }

            int retryCount = 0;
            bool proceedToNextContinuation = true;  // used during retry.
            string uri = combinedURI;

            while (true)
            {
                HttpResponseMessage? response = null;

                try
                {
                    Console.WriteLine($"GET {uri}");
                    response = await httpClient.GetAsync(uri);
                }
                catch (Exception ex)
                {
                    retryCount++;

                    DecodeAndDisplayException(ex);

                    if (retryCount > 3)
                    {
                        throw;  // alert caller that this continuation has failed
                    }

                    proceedToNextContinuation = false; // signal code to retry current continuation

                    int delay = ServerFriendlyRetryBackoff.CalcDelayInMilliseconds();

                    System.Threading.Thread.Sleep(delay);
                }

                if (proceedToNextContinuation)
                {
                    RestRow[] restRowset = await GetObjectFromResponseAsync<RestRow[]>(response!);
                    rows.AddRange(restRowset);

                    // the continuation header contains the continuation token.
                    // fetch it here and place on query string of next call
                    if (response!.Headers.TryGetValues(QueryIdHeader, out IEnumerable<string>? values))
                    {
                        foreach (var id in values)
                        {
                            queryId = id;
                        }
                    }
                    else
                    {
                        // continuation is complete when no continuation header is returned
                        break;
                    }

                    // after the first call, we add the continuation header to query string to fetch the "next" batch of data
                    uri = combinedURI + continuationSeperator + QueryIdQueryParam + "=" + queryId;
                }
                proceedToNextContinuation = true;
            }
            return rows.ToArray();
        }

        public static void ParseCommandLine(string[] args, ref string credUserName, ref string credPassword,
            ref string serverName, ref bool credentialsAreCachedOnHN)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-u":
                        credUserName = args[++i];
                        break;
                    case "-p":
                        credPassword = args[++i];
                        break;
                    case "-c":
                        serverName = args[++i];
                        break;
                    case "-C":
                        credentialsAreCachedOnHN = true;
                        break;
                    default:
                        throw new ArgumentException($"Can't parse commandline, invalid argument {args[i]} in args list");
                }
            }
        }

        public static async Task<RestRow[]?> GetNodegroupList(HttpClient httpClient)
        {
            RestRow[]? nodeGroupRows = null;
            try
            {
                string url = $"{ApiBase}/Nodegroups";
                Console.WriteLine($"GET {url}");

                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                nodeGroupRows = await GetObjectFromResponseAsync<RestRow[]>(response);
                Console.WriteLine("Nodegroups are:");

                foreach (RestRow row in nodeGroupRows)
                {
                    foreach (RestProp prop in row.Props!)
                    {
                        Console.WriteLine(prop.Value);
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return nodeGroupRows;
        }

        public static async Task<string[]?> GetSingleNodegroup(HttpClient httpClient, string nodegroup)
        {
            string[]? nodesInGroup = null;
            try
            {
                string url = $"{ApiBase}/Nodegroup/{nodegroup}";
                Console.WriteLine($"GET {url}");

                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                nodesInGroup = await GetObjectFromResponseAsync<string[]>(response);
                Console.WriteLine("For Nodegroup: " + nodegroup);

                foreach (string node in nodesInGroup)
                {
                    Console.WriteLine(node);
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nodesInGroup;
        }

        public static async Task<RestProp[]?> GetSingleNode(HttpClient httpClient, string node)
        {
            RestProp[]? nodeProperties = null;
            try
            {
                string url = $"{ApiBase}/Node/{node}";
                Console.WriteLine($"GET {url}");

                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                nodeProperties = await GetObjectFromResponseAsync<RestProp[]>(response);
                Console.WriteLine("For Node: " + node);

                foreach (RestProp prop in nodeProperties)
                {
                    Console.WriteLine("Name: " + prop.Name + ", Value = " + prop.Value);
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nodeProperties;
        }

        public static async Task<int> CreateJobUsingProperties(HttpClient httpClient, bool credentialsAreCachedOnHN,
            string credUserName, string credPassword)
        {
            List<RestProp> createJobProps = new()
            {
                new RestProp("MinNodes", "1"),
                new RestProp("MaxNodes", "3"),
                new RestProp("UnitType", "2"), // JobUnitType.Node
                new RestProp("AutoCalculateMax", "false"),
                new RestProp("AutoCalculateMin", "false"),
                new RestProp("Priority", "0")   // JobPriority.Lowest
            };

            int jobId = 0;
            // first we create an empty job
            try
            {
                string url = $"{ApiBase}/Jobs";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(createJobProps.ToArray()));
                await HandleHttpErrorAsync(response);

                jobId = await GetObjectFromResponseAsync<int>(response);
                Console.WriteLine($"The created job id is {jobId}.");
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            // we now have an empty job, now add task

            List<RestProp> restProps = new()
            {
                new RestProp("MinNodes", "1"),
                new RestProp("MaxNodes", "3"),
                new RestProp("CommandLine", "hostname")
            };
            List<RestProp> taskProps = restProps;

            try
            {
                string url = $"{ApiBase}/Job/{jobId}/Tasks";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(taskProps.ToArray()));
                await HandleHttpErrorAsync(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            // here we construct any desired submit properties...
            List<RestProp> submitJobPropList = new()
            {
                new RestProp("RunUntilCanceled", "true")  // we will cancel this job below via REST call
            };

            // add any other properties here
            if (!credentialsAreCachedOnHN)
            {
                //  Supply these properties if your credentials are not cached on the head node
                submitJobPropList.Add(new RestProp("UserName", credUserName));
                submitJobPropList.Add(new RestProp("Password", credPassword));
            }

            // submit the job
            try
            {
                string url = $"{ApiBase}/Job/{jobId}/Submit";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(submitJobPropList.ToArray()));
                await HandleHttpErrorAsync(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return jobId;
        }

        public static async Task CancelJob(HttpClient httpClient, int jobId)
        {
            try
            {
                string url = $"{ApiBase}/Job/{jobId}/Cancel?Forced=false";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject("I canceled it!"));
                await HandleHttpErrorAsync(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public static async Task<RestProp[]?> GetJobProperties(HttpClient httpClient, int jobId)
        {
            RestProp[]? props = null;
            try
            {
                string url = $"{ApiBase}/Job/{jobId}?Properties=Id,Name,State";
                Console.WriteLine($"GET {url}");
                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                props = await GetObjectFromResponseAsync<RestProp[]>(response);

                // do something with the props
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return props;
        }

        public static async Task<int> CreateJobUsingXML(HttpClient httpClient, int jobId)
        {
            string? xml = null;

            try
            {
                string url = $"{ApiBase}/Job/{jobId}?Render=RestPropRender";
                Console.WriteLine($"GET {url}");
                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                xml = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            // now we construct a request and write the xml to the body
            int createdJobId = 0;
            try
            {
                string url = $"{ApiBase}/Jobs/JobFile";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(xml));
                await HandleHttpErrorAsync(response);
                createdJobId = await GetObjectFromResponseAsync<int>(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            Console.WriteLine($"The original job id is {jobId}");
            Console.WriteLine($"The created job id is {createdJobId}.");
            return createdJobId;
        }
    }
}
