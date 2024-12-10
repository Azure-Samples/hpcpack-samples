using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using static CSharpClient2016.Utils;

/*
 * This is a sample client for the new REST API of HPC Pack. Get the API spec at:
 * http://download.microsoft.com/download/B/D/B/BDB8782A-FAAF-457D-AF3D-0B157FEEDF4C/New%20Set%20of%20HPC%20Pack%20Scheduler%20REST%20API.pdf
 * 
 * The main differences between this new and the old one are:
 * 1) New API uses NTML/AAD for authentication while the old one uses Basic Auth.
 * 2) New API accepts/returns JSON/XML body while the old one does XML only.
 * 3) Some URL changes.
 * 
 * For more details, check above document.
 */

namespace CSharpClient2016
{
    public class Program
    {
        static async Task<int> CreateJob(HttpClient httpClient, NetworkCredential? cred)
        {
            List<RestProp> createJobProps = new()
            {
                new RestProp("MinNodes", "1"),
                new RestProp("MaxNodes", "3"),
                new RestProp("UnitType", "2"), // JobUnitType.Node
                new RestProp("AutoCalculateMax", "false"),
                new RestProp("AutoCalculateMin", "false"),
                new RestProp("Priority", "0") // JobPriority.Lowest
            };

            // First we create an empty job
            string url = "/hpc/jobs";
            Console.WriteLine($"POST {url}");
            var response = await httpClient.PostAsync(url, MakeXmlContentFromObject(createJobProps.ToArray()));
            await CheckHttpErrorAsync(response);

            int jobId = await GetObjectFromResponseAsync<int>(response);
            Console.WriteLine($"The created job id is {jobId}.");

            // Then add task
            List<RestProp> taskProps = new()
            {
                new RestProp("MinNodes", "1"),
                new RestProp("MaxNodes", "3"),
                new RestProp("CommandLine", "hostname")
            };

            url = $"/hpc/jobs/{jobId}/Tasks";
            Console.WriteLine($"POST {url}");
            response = await httpClient.PostAsync(url, MakeXmlContentFromObject(taskProps.ToArray()));
            await CheckHttpErrorAsync(response);

            // Finally add any desired submit properties and submit
            List<RestProp> submitJobPropList = new();
            if (cred != null)
            {
                // Supply these properties if your credentials are not cached on the head node
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

            foreach (var prop in props)
            {
                Console.WriteLine($"{prop.Name}:\t{prop.Value}");
            }
        }

        static void ParseCommandLine(string[] args, ref string? username, ref string? password, ref string? tenentid, 
            ref string? clientid, ref string? resourceid, ref string? redirectUri, ref bool credentialsAreCachedOnHN,
            ref string? serverName, ref bool debug)
        {
            for (int i = 0; i < args.Length; i++)
            {
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
        }

        public static async Task Main(string[] args)
        {
            // It's enough to provide username, password, serverName.
            string? username = null;
            string? password = null;
            string? tenentid = null;
            string? clientid = null;
            string? resourceid = null;
            string? redirectUri = "http://hpcclient";
            bool credentialsAreCachedOnHN = false; // set to true if your creds were cached in the HN by ClusterManager, etc.
            string? serverName = null;
            bool debug = false;

            try
            {
                ParseCommandLine(args, ref username, ref password, ref tenentid, ref clientid,
                    ref resourceid, ref redirectUri, ref credentialsAreCachedOnHN, ref serverName, ref debug);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when parsing command line: {ex.Message}");
                ShowHelp();
                return;
            }

            if (serverName == null ||
                (!credentialsAreCachedOnHN && (username == null || password == null)) ||
                // When any option of AAD is null, then NTML is used and thus username and password must be present
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

            string? token = null;
            if (tenentid != null && clientid != null && resourceid != null)
            {
                var ac = new AuthenticationContext($"https://login.microsoftonline.com/{tenentid}");
#if NET472
                var result = await ac.AcquireTokenAsync(resourceid, clientid, new Uri(redirectUri!), new PlatformParameters(PromptBehavior.Auto));
#else
                var result = await ac.AcquireTokenAsync(resourceid, clientid, new Uri(redirectUri!), new PlatformParameters());
#endif
                token = result.AccessToken;
            }

            NetworkCredential? cred = (username != null && password != null) ? new NetworkCredential(username, password) : null;
            
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };

            if (token == null)
            {
                //Auth by NTLM
                handler.Credentials = cred;
            }

            string apiBase = $"https://{serverName}";
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
                Console.WriteLine("---Complete Create Job---");

                await ShowJob(httpClient, jobId);
                Console.WriteLine("---Complete Show Job---");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            Console.WriteLine("Completed for all samples!");
        }
    }
}
