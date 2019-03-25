using System;
using System.IO;
using System.Net;

namespace Chapter2_MutationalFuzzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = args[0];
            int index = url.IndexOf ("?");
            string[] parameters = url.Remove(0, index + 1).Split("&");
            foreach (string parameter in parameters) {
                string xssUrl = url.Replace (parameter, parameter + "fd<xss>sa");
                string sqlUrl = url.Replace (parameter, parameter + "fa'sa");

                HttpWebRequest request = (HttpWebRequest) WebRequest.Create (sqlUrl);
                request.Method = "GET";

                string sqlResponse = string.Empty;
                using (StreamReader streamReader = new StreamReader(
                    request.GetResponse().GetResponseStream())) 
                {
                    sqlResponse = streamReader.ReadToEnd();
                }

                request = (HttpWebRequest) WebRequest.Create(xssUrl);
                request.Method = "GET";

                // Set cookie for DVWA 
                //request.Headers["Cookie"] = "PHPSESSID=ili3kosuj47i2v8egr2fgpge41; security=low";

                string xssResponse = string.Empty;
                using (StreamReader streamReader = new StreamReader(
                    request.GetResponse().GetResponseStream())) 
                {
                    xssResponse = streamReader.ReadToEnd();
                }

                if(xssResponse.Contains("<xss>")){
                    Console.WriteLine("Possible XSS point found in parameter: " + parameter);
                }

                if(sqlResponse.Contains("error in your SQL syntax")){
                    Console.WriteLine("SQL injection point found in parameter: " + parameter);
                }
            }
        }
    }
}
