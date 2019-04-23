using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.IO;
using System.Collections.Generic;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

/*
 * This is a sample client for the new REST API of HPC Pack. Get the API spec at:
 * http://download.microsoft.com/download/B/D/B/BDB8782A-FAAF-457D-AF3D-0B157FEEDF4C/New%20Set%20of%20HPC%20Pack%20Scheduler%20REST%20API.pdf
 */

namespace RestClient2
{
    class ApiError : Exception
    {
        public HttpStatusCode Code { get; set; }

        public new string Message { get; set; }

        public ApiError(HttpStatusCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public override string ToString()
        {
            return $"ApiError: Code = {Code}, Message = {Message}";
        }
    }

    [Serializable]
    [DataContract(Name = "Property", Namespace = "http://schemas.microsoft.com/HPCS2008R2/common")]
    class RestProp
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

    static class Program
    {
        static void ShowHelp()
        {
            string help = @"
Usage:
{0} -c <server name> -u <user name> -p <password> [-t <tenent id> -i <client id> -r <resource id> [-R <redirect URI>]] [-C] [-d]

Options:
-c HPC server name to connect to.
-u Name of a HPC user on the server.
-p Password of the user.
-t Tenent ID of AAD
-i Client Application ID of AAD
-r Resource ID of AAD Application, i.e. the App ID URI, like https://sometenent.onmicrosoft.com/someapp
-R Redirect URI of AAD Client Application. The default value is http://hpcclient
-C The switch specifies that the user credentail is cached on the HPC server. It's not by default.
-d Debug mode.

NOTE:
When -t, -i -r and -R options are present, AAD is used for authentication; otherwise NTLM is used and -u and -p options must be present.
";
            Console.WriteLine(String.Format(help, System.Diagnostics.Process.GetCurrentProcess().ProcessName));
        }

        static async Task CheckHttpErrorAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                throw new ApiError(response.StatusCode, message);
            }
        }

        static T XmlStreamToObject<T>(Stream stream)
        {
            var dcs = new DataContractSerializer(typeof(T));
            return (T)dcs.ReadObject(stream);
        }

        static Stream ObjectToXmlStream<T>(T obj)
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
            return XmlStreamToObject<T>(await message.Content.ReadAsStreamAsync());
        }

        static HttpContent MakeXmlContentFromObject<T>(T obj)
        {
            var content = new StreamContent(ObjectToXmlStream(obj));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml") { CharSet = "utf-8" };
            return content;
        }

        static async Task<int> CreateJob(HttpClient httpClient, NetworkCredential cred)
        {
            List<RestProp> createJobProps = new List<RestProp>();
            createJobProps.Add(new RestProp("MinNodes", "1"));
            createJobProps.Add(new RestProp("MaxNodes", "3"));
            createJobProps.Add(new RestProp("UnitType", "2")); // JobUnitType.Node
            createJobProps.Add(new RestProp("AutoCalculateMax", "false"));
            createJobProps.Add(new RestProp("AutoCalculateMin", "false"));
            createJobProps.Add(new RestProp("Priority", "0"));   // JobPriority.Lowest

            // First we create an empty job
            string url = "/hpc/jobs";
            Console.WriteLine($"POST {url}");
            var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(createJobProps.ToArray()));
            await CheckHttpErrorAsync(response);

            int jobId = await GetObjectFromResponseAsync<int>(response);
            Console.WriteLine($"The created job id is {jobId}.");

            // Then add task
            List<RestProp> taskProps = new List<RestProp>();
            taskProps.Add(new RestProp("MinNodes", "1"));
            taskProps.Add(new RestProp("MaxNodes", "3"));
            taskProps.Add(new RestProp("CommandLine", "hostname"));

            url = $"/hpc/jobs/{jobId}/Tasks";
            Console.WriteLine($"POST {url}");
            response = await httpClient.PostAsync(url, MakeXmlContentFromObject(taskProps.ToArray()));
            await CheckHttpErrorAsync(response);

            // Finally add any desired submit properties and submit
            List<RestProp> submitJobPropList = new List<RestProp>();
            if (cred != null)
            {
                //  Supply these properties if your credentials are not cached on the head node
                submitJobPropList.Add(new RestProp("UserName", cred.UserName));
                submitJobPropList.Add(new RestProp("Password", cred.Password));
            }
            url = $"/hpc/jobs/{jobId}/Submit";
            Console.WriteLine($"POST {url}");
            response = await httpClient.PostAsync(url, MakeXmlContentFromObject(submitJobPropList.ToArray()));
            await CheckHttpErrorAsync(response);

            return jobId;
        }

        static async Task ShowJob(HttpClient httpClient, int jobId)
        {
            string url = $"/hpc/jobs/{jobId}?Properties=Id,Owner,State";
            Console.WriteLine($"GET {url}");
            var response = await httpClient.GetAsync(url);
            await CheckHttpErrorAsync(response);
            var props = await GetObjectFromResponseAsync<RestProp[]>(response);

            foreach (var prop in props) {
                Console.WriteLine($"{prop.Name}:\t{prop.Value}");
            }
        }

        static async Task MainAsync(string[] args)
        {
            string username = null;
            string password = null;
            string tenentid = null;
            string clientid = null;
            string resourceid = null;
            string redirectUri = "http://hpcclient";
            bool credentialsAreCachedOnHN = false;  // set to true if your creds were cached in the HN by ClusterManager, etc.
            string serverName = null;
            bool debug = false;

            for (int i = 0; i < args.Length; i++)
            {
                try {
                    switch (args[i])
                    {
                        case "-u":
                            username = args[++i];
                            break;
                        case "-p":
                            password = args[++i];
                            break;
                        case "-t":
                            tenentid = args[++i];
                            break;
                        case "-i":
                            clientid = args[++i];
                            break;
                        case "-r":
                            resourceid = args[++i];
                            break;
                        case "-R":
                            redirectUri = args[++i];
                            break;
                        case "-c":
                            serverName = args[++i];
                            break;
                        case "-C":
                            credentialsAreCachedOnHN = true;
                            break;
                        case "-d":
                            debug = true;
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

            if (serverName == null ||
                (!credentialsAreCachedOnHN && (username == null || password == null)) ||
                //When any option of AAD is null, then NTML is used and thus username and password must be present
                ((tenentid == null || clientid == null || resourceid == null) && (username == null || password == null))) 
            {
                ShowHelp();
                return;
            }

            if (debug)
            {
                Console.WriteLine("Press any key to continue...");
                Console.Read();
            }

            // This disables enforcement of certificat trust chains which enables the use of self-signed certs.
            // Comment out this line to enforce trusted chains between your REST service and REST clients.
            ServicePointManager.ServerCertificateValidationCallback = (obj, cert, chain, err) => true;

            string token = null;
            if (tenentid != null && clientid != null && resourceid != null)
            {
                var ac = new AuthenticationContext($"https://login.microsoftonline.com/{tenentid}");
                var result = await ac.AcquireTokenAsync(resourceid, clientid, new Uri(redirectUri), new PlatformParameters(PromptBehavior.Auto));
                token = result.AccessToken;
            }

            NetworkCredential cred = (username != null && password != null) ? new NetworkCredential(username, password) : null;
            string apiBase = $"https://{serverName}";
            var handler = new HttpClientHandler();
            if (token == null)
            {
                //Auth by NTLM
                handler.Credentials = cred;
            }
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };
            httpClient.DefaultRequestHeaders.Add("api-version", "2016-11-01.5.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            if (token != null)
            {
                //Auth by AAD
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var jobId = await CreateJob(httpClient, credentialsAreCachedOnHN ? null : cred);
                await ShowJob(httpClient, jobId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            Console.WriteLine("OK!");
        }

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
    }
}
