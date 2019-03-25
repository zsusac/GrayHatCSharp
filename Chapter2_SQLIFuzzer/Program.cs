using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Chapter2_SQLIFuzzer
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] requestLines = File.ReadAllLines(args[0]);
            string[] parameters = requestLines[requestLines.Length - 1].Split("&");
            string host = String.Empty;
            StringBuilder requestBuilder = new StringBuilder();

            foreach (string line in requestLines)
            {
                if(line.StartsWith("Host:"))
                {
                    host = line.Split(" ")[1].Replace("\r", String.Empty);
                }
                requestBuilder.Append(line).Append("\n");
            }

            string request = requestBuilder.Append("\r\n").ToString();

            IPEndPoint rhost = new IPEndPoint(IPAddress.Parse(host), 80);

            foreach (string parameter in parameters)
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(rhost);

                string val = parameter.Split("=")[1];
                string req = request.Replace("=" + val, "=" + val + "'");

                byte[] requestBytes = Encoding.ASCII.GetBytes(req);
                socket.Send(requestBytes);

                string response = String.Empty;
                byte[] buffer = new byte[socket.ReceiveBufferSize];

                socket.Receive(buffer);
                response = Encoding.ASCII.GetString(buffer);

                if(response.Contains("error in your SQL syntax"))
                {
                    Console.WriteLine("Parameter " + parameter + " seems vulnerable to SQL injection with value: " + val + "'"); 
                }

                socket.Close();
            }
        }
    }
}
