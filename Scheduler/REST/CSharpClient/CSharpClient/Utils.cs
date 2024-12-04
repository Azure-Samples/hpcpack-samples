#if NET472
using System.Net.Http;
#endif

using System.Net.Http.Headers;
using System.Net;
using System.Runtime.Serialization;

namespace CSharpClient
{
    public class Utils
    {
        public static T StreamToObject<T>(Stream stream)
        {
            var dcs = new DataContractSerializer(typeof(T));
            return (T)dcs.ReadObject(stream)!;
        }

        public static Stream ObjectToStream<T>(T obj)
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
            return StreamToObject<T>(await message.Content.ReadAsStreamAsync());
        }

        public static HttpContent MakeXmlContentFromObject<T>(T obj)
        {
            var content = new StreamContent(ObjectToStream(obj));
            content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/xml") { CharSet = "utf-8" };
            return content;
        }

        public static void DecodeAndDisplayException(Exception ex)
        {
            string message = ex.ToString();

            // first we try to parse out any V3SP2 style error (string)
            if (ex is WebException)
            {
                WebException? wex = ex as WebException;

                using (Stream responseStream = new MemoryStream())
                {
                    try
                    {
                        DataContractSerializer dcsString = new(typeof(string));

                        string callChain = wex!.Response!.Headers["x-ms-hpc-authoritychain"]!;

                        Console.WriteLine("Call Chain: " + callChain);

                        CopyStream(wex.Response.GetResponseStream(), responseStream);  // make a copy in order to try newer error response body

                        responseStream.Position = 0;

                        string? errorBody = dcsString.ReadObject(responseStream) as string;

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

                                object sp3ErrorBodyObj = dcsv3SP3.ReadObject(responseStream)!;
                                HpcWebServiceFault? fault = sp3ErrorBodyObj as HpcWebServiceFault;

                                message = "V3SP3 Error body was: Code = " + fault!.Code + ", Message = " + fault.Message;
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

        public static void HandleException(Exception ex)
        {
            DecodeAndDisplayException(ex);

            Console.WriteLine("Hit return to exit.");
            Console.ReadLine();

            Environment.Exit(-1);
        }

        public static void CopyStream(Stream src, Stream dest)
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

        public static void DisplayRowset(RestRow[] rowset)
        {
            bool first = true;

            foreach (RestRow row in rowset)
            {
                if (first)
                {
                    Console.WriteLine("Id\t Owner\t Name\t State\t Priority");

                    first = false;
                }

                foreach (RestProp prop in row.Props!)
                {
                    Console.Write(prop.Value + "\t");
                }

                Console.WriteLine();
            }
        }
    }

    public class ServerFriendlyRetryBackoff
    {
        private static readonly Random _rand = new();

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
        public RestProp[]? Props;
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
        public int Code { get; set; }

        /// <summary>
        /// Gets the fault reason.
        /// </summary>
        [DataMember]
        public string? Message { get; set; }

        /// <summary>
        /// Gets the fault context.
        /// </summary>
        [DataMember]
        public KeyValuePair<string, string>[]? Values { get; set; }
    }
}
