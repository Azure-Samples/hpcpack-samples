using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.IO;
using System.Collections.Generic;

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
{0} -c <server name> -u <user name> -p <password> [-C] [-d]

Options:
-c HPC server name to connect to.
-u Name of a HPC user on the server.
-p Password of the user.
-C The switch specifies that the user credentail is cached on the HPC server. It's not by default.
-d Debug mode.
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
            bool credentialsAreCachedOnHN = false;  // set to true if your creds were cached in the HN by ClusterManager, etc.
            string serverName = null;
            bool debug = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-u":
                        if (++i == args.Length)
                        {
                            ShowHelp();
                            return;
                        }
                        username = args[i];
                        break;
                    case "-p":
                        if (++i == args.Length)
                        {
                            ShowHelp();
                            return;
                        }
                        password = args[i];
                        break;
                    case "-c":
                        if (++i == args.Length)
                        {
                            ShowHelp();
                            return;
                        }
                        serverName = args[i];
                        break;
                    case "-C":
                        credentialsAreCachedOnHN = true;
                        break;
                    case "-d":
                        debug = true;
                        break;
                    default:
                        ShowHelp();
                        return;
                }
            }

            if (username == null || password == null || serverName == null)
            {
                ShowHelp();
                return;
            }

            if (debug)
            {
                Console.WriteLine("Press any key to continue...");
                Console.Read();
            }

            var cred = new NetworkCredential(username, password);
            string apiBase = $"https://{serverName}";
            var handler = new HttpClientHandler() {
                Credentials = cred,
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };
            httpClient.DefaultRequestHeaders.Add("api-version", "2016-11-01.5.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

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
