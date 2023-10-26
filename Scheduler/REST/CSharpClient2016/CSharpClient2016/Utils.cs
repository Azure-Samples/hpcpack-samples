using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CSharpClient2016
{
    public class Utils
    {
        public static void ShowHelp()
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

        public static async Task CheckHttpErrorAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                throw new ApiError(response.StatusCode, message);
            }
        }

        public static T XmlStreamToObject<T>(Stream stream)
        {
            var dcs = new DataContractSerializer(typeof(T));
            return (T)dcs.ReadObject(stream);
        }

        public static Stream ObjectToXmlStream<T>(T obj)
        {
            DataContractSerializer dcs = new DataContractSerializer(typeof(T));
            Stream s = new MemoryStream();
            dcs.WriteObject(s, obj);
            s.Flush();
            s.Seek(0, SeekOrigin.Begin);
            return s;
        }

        public static async Task<T> GetObjectFromResponseAsync<T>(HttpResponseMessage message)
        {
            return XmlStreamToObject<T>(await message.Content.ReadAsStreamAsync());
        }

        public static HttpContent MakeXmlContentFromObject<T>(T obj)
        {
            var content = new StreamContent(ObjectToXmlStream(obj));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml") { CharSet = "utf-8" };
            return content;
        }
    }


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
}
