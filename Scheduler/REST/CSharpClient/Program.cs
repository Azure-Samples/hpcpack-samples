// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the
// MICROSOFT LIMITED PUBLIC LICENSE version 1.1 (MS-LPL, http://go.microsoft.com/?linkid=9791213.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;

/*
 * The following sample sources no longer require the REST Starter Kit.
 *
 * In addition, the variable "credentialsAreCachedOnHN" can be set:
 *      true: if the credentials (ntlm or basic) are cached on the head node
 *      false: otherwise.
*/

namespace RestClient
{
    public class Program
    {
        const string ContinuationHeader = "x-ms-continuation-";
        const string QueryIdQueryParam = "QueryId";
        const string QueryIdHeader = ContinuationHeader + QueryIdQueryParam;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static void ShowHelp()
        {
            string help = @"
Usage:
{0} -c <server name> -u <user name> -p <password> [-C]

Options:
-c HPC server name to connect to.
-u Name of a HPC user on the server.
-p Password of the user.
-C The switch specifies that the user credentail is cached on the HPC server. It's not by default.
";
            Console.WriteLine(String.Format(help, System.Diagnostics.Process.GetCurrentProcess().ProcessName));
        }

        static async Task MainAsync(string[] args)
        {
            string credUserName = null;
            string credPassword = null;
            bool credentialsAreCachedOnHN = false;  // set to true if your creds were cached in the HN by ClusterManager, etc.
            string serverName = null;

            #region // Parse command line
            for (int i = 0; i < args.Length; i++)
            {
                try
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
                            throw new ArgumentException();
                    }
                }
                catch
                {
                    ShowHelp();
                    return;
                }
            }

            if (credUserName == null || credPassword == null || serverName == null)
            {
                ShowHelp();
                return;
            }
            #endregion

            ICredentials credsBasic = new NetworkCredential(credUserName, credPassword);
            string baseAddr = $"https://{serverName}/WindowsHpc";
            RestRow[] nodeGroupRows = null;
            int jobId = 0;

            // This disables enforcement of certificat trust chains which enables the use of self-signed certs.
            // Comment out this line to enforce trusted chains between your REST service and REST clients.
            ServicePointManager.ServerCertificateValidationCallback = (obj, cert, chain, err) => true;

            var handler = new HttpClientHandler() { Credentials = credsBasic };
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("api-version", "2012-11-01.4.0");

            #region // GET Nodegroup list
            try
            {
                string url = $"{baseAddr}/Nodegroups";
                Console.WriteLine($"GET {url}");
                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                nodeGroupRows = await GetObjectFromResponseAsync<RestRow[]>(response);
                Console.WriteLine("Nodegroups are:");
                foreach (RestRow row in nodeGroupRows)
                {
                    foreach (RestProp prop in row.Props)
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
            #endregion // GET Nodegroup list

            string[] nodesInGroup = null;

            #region GET a single Nodegroup
            try
            {
                // take the 0th nodegroup and fetch list of nodes
                string nodegroup = nodeGroupRows[0].Props[0].Value;
                string url = $"{baseAddr}/Nodegroup/{nodegroup}";
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
            #endregion

            #region // GET a single Node
            try
            {
                // take the 0th node and fetch list of properties
                string node = nodesInGroup[0];
                string url = $"{baseAddr}/Node/{node}";
                Console.WriteLine($"GET {url}");
                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                var nodeProperties = await GetObjectFromResponseAsync<RestProp[]>(response);
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
            #endregion // GET a single Node

            #region // GET the list of Nodes with Continuation Headers

            RestRow[] nodes = await ReadCollectionWithContinuationAsync(httpClient, baseAddr + "/Nodes", "Properties=Name,Id,MemorySize");

            #endregion // GET the list of Nodes with Continuation Headers

            #region Create and submit job using properties

            List<RestProp> createJobProps = new List<RestProp>();
            createJobProps.Add(new RestProp("MinNodes", "1"));
            createJobProps.Add(new RestProp("MaxNodes", "3"));
            createJobProps.Add(new RestProp("UnitType", "2")); // JobUnitType.Node
            createJobProps.Add(new RestProp("AutoCalculateMax", "false"));
            createJobProps.Add(new RestProp("AutoCalculateMin", "false"));
            createJobProps.Add(new RestProp("Priority", "0"));   // JobPriority.Lowest

            // first we create an empty job
            try
            {
                string url = $"{baseAddr}/Jobs";
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

            List<RestProp> taskProps = new List<RestProp>();
            taskProps.Add(new RestProp("MinNodes", "1"));
            taskProps.Add(new RestProp("MaxNodes", "3"));
            taskProps.Add(new RestProp("CommandLine", "hostname"));

            try
            {
                string url = $"{baseAddr}/Job/{jobId}/Tasks";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(taskProps.ToArray()));
                await HandleHttpErrorAsync(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            // here we construct any desired submit properties...
            List<RestProp> submitJobPropList = new List<RestProp>();
            submitJobPropList.Add(new RestProp("RunUntilCanceled", "true"));  // we will cancel this job below via REST call

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
                string url = $"{baseAddr}/Job/{jobId}/Submit";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(submitJobPropList.ToArray()));
                await HandleHttpErrorAsync(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            #endregion

            #region // Cancel job
            try
            {
                string url = $"{baseAddr}/Job/{jobId}/Cancel?Forced=false";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject("I canceled it!"));
                await HandleHttpErrorAsync(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            #endregion // cancel job


            #region // Check job properties

            try
            {
                string url = $"{baseAddr}/Job/{jobId}?Properties=Id,Name,State";
                Console.WriteLine($"GET {url}");
                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                var props = await GetObjectFromResponseAsync<RestProp[]>(response);

                // do something with the props
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            #endregion

            #region // Create job using XML

            string xml = null;

            try
            {
                string url = $"{baseAddr}/Job/{jobId}?Render=RestPropRender";
                Console.WriteLine($"GET {url}");
                var response = await httpClient.GetAsync(url);
                await HandleHttpErrorAsync(response);
                xml = await response.Content.ReadAsStringAsync();
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }

            // now we construct a request and write the xml to the body
            try
            {
                string url = $"{baseAddr}/Jobs/JobFile";
                Console.WriteLine($"POST {url}");
                var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(xml));
                await HandleHttpErrorAsync(response);
                jobId = await GetObjectFromResponseAsync<int>(response);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            #endregion

            #region List Jobs and use of Continuation Headers

            RestRow[] jobs = await ReadCollectionWithContinuationAsync(httpClient, baseAddr + "/Jobs", "Properties=Id,Name,State");

            DisplayRowset(jobs);

            #endregion

            #region List Jobs filtered by JobState

                // here we requeset all canceled jobs via $filter= and Render=
            jobs = await ReadCollectionWithContinuationAsync(httpClient, baseAddr + "/Jobs", "Render=RestPropRender&$filter=JobState eq Canceled");

            DisplayRowset(jobs);

            #endregion

            Console.WriteLine("OK!");
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
        public static async Task<RestRow[]> ReadCollectionWithContinuationAsync(HttpClient httpClient, string baseURI, string baseQueryString)
        {
            RestRow[] rows = null;

            try
            {
                // if initial attempt succeeds then return results
                rows = await ReadCollectionWithContinuationAndRetryAsync(httpClient, baseURI, baseQueryString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Iteration failed on: " + baseURI + ".  Starting over.");
                Console.WriteLine();

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
            List<RestRow> rows = new List<RestRow>();
            string queryId = null;
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
                HttpResponseMessage response = null;

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
                    RestRow[] restRowset = await GetObjectFromResponseAsync<RestRow[]>(response);
                    rows.AddRange(restRowset);

                    // the continuation header contains the continuation token.
                    // fetch it here and place on query string of next call
                    IEnumerable<string> values;
                    if (response.Headers.TryGetValues(QueryIdHeader, out values)) {
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

        public class ServerFriendlyRetryBackoff
        {
            private static Random _rand = new Random();

            /// <summary>
            /// Opinions vary widely on this topic.  In general it is polite to avoid
            /// having all clients immediately DoS the REST service when errors begin occuring.
            ///
            /// Many prefer a bounded exponential backoff for production.
            /// </summary>
            /// <returns></returns>
            public static int CalcDelayInMilliseconds()
            {
                int numMillisecs = (int)(5000 * _rand.NextDouble());

                return numMillisecs;
            }
        }


        [Serializable]
        [DataContract(Name = "Property", Namespace = "http://schemas.microsoft.com/HPCS2008R2/common")]
        public class RestProp
        {
            [DataMember()]
            public string Name;

            [DataMember()]
            public string Value;

            public RestProp(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        [Serializable]
        [DataContract(Name = "Object", Namespace = "http://schemas.microsoft.com/HPCS2008R2/common")]
        public class RestRow
        {
            [DataMember(Name = "Properties")]
            public RestProp[] Props;
        }

        [DataContract(Name = "HpcWebServiceFault", Namespace = "http://schemas.microsoft.com/HPCS2008R2/common")]
        [Serializable]
        public class HpcWebServiceFault
        {
            public HpcWebServiceFault()
            {
            }

            public HpcWebServiceFault(int code, string message, params KeyValuePair<string, string>[] values)
            {
                Code = code;
                Message = message;
                Values = values;
            }

            /// <summary>
            /// Gets the fault code.
            /// </summary>
            [DataMember]
            public int Code
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the fault reason.
            /// </summary>
            [DataMember]
            public string Message
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the fault context.
            /// </summary>
            [DataMember]
            public KeyValuePair<string, string>[] Values
            {
                get;
                set;
            }
        }

        #region Utils

        static T StreamToObject<T>(Stream stream)
        {
            var dcs = new DataContractSerializer(typeof(T));
            return (T)dcs.ReadObject(stream);
        }

        static Stream ObjectToStream<T>(T obj)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(T));
            Stream s = new MemoryStream();
            dcs.WriteObject(s, obj);
            s.Flush();
            s.Seek(0, SeekOrigin.Begin);
            return s;
        }

        static async Task<T> GetObjectFromResponseAsync<T>(HttpResponseMessage message)
        {
            return StreamToObject<T>(await message.Content.ReadAsStreamAsync());
        }

        static HttpContent MakeXmlContentFromObject<T>(T obj)
        {
            var content = new StreamContent(ObjectToStream(obj));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml") { CharSet = "utf-8" };
            return content;
        }

        static void DecodeAndDisplayException(Exception ex)
        {
            string message = ex.ToString();

            // first we try to parse out any V3SP2 style error (string)
            if (ex is WebException)
            {
                WebException wex = ex as WebException;

                using (Stream responseStream = new MemoryStream())
                {
                    try
                    {
                        DataContractSerializer dcsString = new DataContractSerializer(typeof(string));

                        string callChain = wex.Response.Headers["x-ms-hpc-authoritychain"];

                        Console.WriteLine("Call Chain: " + callChain);

                        CopyStream(wex.Response.GetResponseStream(), responseStream);  // make a copy in order to try newer error response body

                        responseStream.Position = 0;

                        string errorBody = dcsString.ReadObject(responseStream) as string;

                        message = "V3SP2 Error body was <string>: " + errorBody;
                    }
                    catch (Exception)
                    {
                        if (responseStream.Length > 0)
                        {
                            try
                            {
                                DataContractSerializer dcsv3SP3 = new DataContractSerializer(typeof(HpcWebServiceFault));

                                responseStream.Position = 0;

                                object sp3ErrorBodyObj = dcsv3SP3.ReadObject(responseStream);
                                HpcWebServiceFault fault = sp3ErrorBodyObj as HpcWebServiceFault;

                                message = "V3SP3 Error body was: Code = " + fault.Code + ", Message = " + fault.Message;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }

                Console.WriteLine("Exception received:");
                Console.WriteLine(message);
            }

            Console.WriteLine("Exception caught: " + ex.ToString());
        }

        static void HandleException(Exception ex)
        {
            DecodeAndDisplayException(ex);

            Console.WriteLine("Hit return to exit.");
            Console.ReadLine();

            Environment.Exit(-1);
        }

        private static void CopyStream(Stream src, Stream dest)
        {
            byte[] buff = new byte[1024];
            int bytesRead;

            while ((bytesRead = src.Read(buff, 0, buff.Length)) > 0)
            {
                dest.Write(buff, 0, bytesRead);
            }
        }

        public static async Task HandleHttpErrorAsync(HttpResponseMessage response)
        {
            HttpStatusCode statusCode = response.StatusCode;
            if (statusCode != HttpStatusCode.OK)
            {
                string message = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error: Code = {0}, Message = {1}", statusCode.ToString(), message);
                Console.WriteLine("Hit return to exit.");
                Environment.Exit(-1);
            }
        }

        static void DisplayRowset(RestRow[] rowset)
        {
            bool first = true;

            foreach (RestRow row in rowset)
            {
                if (first)
                {
                    Console.WriteLine("Id\t Owner\t Name\t State\t Priority");

                    first = false;
                }

                foreach (RestProp prop in row.Props)
                {
                    Console.Write(prop.Value + "\t");
                }

                Console.WriteLine();
            }
        }

        #endregion
    }
}
