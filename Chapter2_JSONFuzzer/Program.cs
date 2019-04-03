using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace Chapter2_JSONFuzzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = args[0];
            string requestFile = args[1];
            string[] request = null;

            using(StreamReader streamReader = new StreamReader(File.OpenRead(requestFile))) {
                request = streamReader.ReadToEnd().Split("\n");
            }

            string json = request[request.Length - 1];
            JObject jsonObject = JObject.Parse(json);
            IterateAndFuzz(url, jsonObject);
        }

        private static void IterateAndFuzz(string url, JObject jsonObject)
        {
            foreach (var pair in (JObject)jsonObject.DeepClone())
            {
                if( pair.Value.Type == JTokenType.String || pair.Value.Type == JTokenType.Integer )
                {
                    Console.WriteLine("Fuzzing key: " + pair.Key);

                    JToken oldVal = pair.Value;
                    jsonObject[pair.Key] = pair.Value.ToString() + "'";

                    if(Fuzz(url, jsonObject.Root))
                    {
                        Console.WriteLine("SQL injection vector: " + pair.Key);
                    }
                    else
                    {
                        Console.WriteLine(pair.Key + " does not seem vulnerable.");
                    }
                    
                    jsonObject[pair.Key] = oldVal;
                }
            }
        }

        private static bool Fuzz(string url, JToken jsonObject)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonObject.ToString());

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = data.Length;
            request.ContentType = "application/javascript";

            using(Stream stream = request.GetRequestStream()){
                stream.Write(data, 0, data.Length);
            }

            try
            {
                request.GetResponse();
            }
            catch (WebException webException)
            {
                string response = String.Empty;
                using(StreamReader streamReader = new StreamReader(webException.Response.GetResponseStream()))
                {
                    response = streamReader.ReadToEnd();
                }

                return (response.Contains("syntax error") || response.Contains("unterminated"));
            }

            return false;
        }
    }
}
