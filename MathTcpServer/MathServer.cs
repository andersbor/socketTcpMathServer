using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MathTcpServer  
{  
    public class MathServer
    {
        public int Port { get; }
        public IPAddress LocalAddress { get; }
        private TcpListener _server;
        // https://msdn.microsoft.com/en-us/library/x13ttww7.aspx
        private volatile bool _listening = true;

        public MathServer(int port)
        {
            Port = port;
            //LocalAddress = IPAddress.Parse("127.0.0.1");
            //LocalAddress = IPAddress.Loopback;
            string hostName = Dns.GetHostName();
            IPHostEntry s = Dns.GetHostEntry(hostName);
            LocalAddress = s.AddressList[0];
            //IPAddress localAddress = Dns.GetHostEntry("127.0.0.1");
        }

        public void Start()
        {
            _server = new TcpListener(LocalAddress, Port);
            _server.Start();
            Console.WriteLine("Math TCP server started on " + LocalAddress + " port " + Port);
            while (_listening)
            {
                // http://stackoverflow.com/questions/26822610/how-to-gracefully-close-tcplistener-tcpclient-connection
                if (!_server.Pending()) //checking for pending connections 
                {
                    Thread.Sleep(50);
                    continue;
                }
                TcpClient client = _server.AcceptTcpClient();
                Console.WriteLine("Connected!");
                //DoIt(client);
                Task.Run(() => DoIt(client));
            }
        }

        public void Stop()
        {
            _server.Stop();
            _listening = false;
        }

        private void DoIt(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                StreamReader reader = new StreamReader(stream);
                string request = reader.ReadLine();
                Console.WriteLine("Request: " + request);

                string response = GenerateResponse(request);
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(response);
                writer.Flush();

                if ("STOP".Equals(request))
                {
                    Stop();
                }
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        private static string GenerateResponse(string request)
        {
            if ("STOP".Equals(request))
            {
                return "server stopping";
            }
            string[] parts = request.Split(' '); // split by space
            if (parts.Length != 3)
                return "Illegal request: " + request;
            var operation = parts[0];
            var numberStr1 = parts[1];
            string numberStr2 = parts[2];
            string response;
            try
            {
                double number1 = double.Parse(numberStr1);
                double number2 = double.Parse(numberStr2);
                switch (operation)
                {
                    case "ADD":
                        double result = number1 + number2;
                        response = result.ToString();
                        break;
                    case "SUB":
                        result = number1 - number2;
                        response = result.ToString();
                        break;
                    default:
                        response = "Illegal request: No such operation: " + operation;
                        break;
                }
            }
            catch (FormatException)
            {
                response = "Illegal request: Not a number";
            }
            return response;
        }
    }
}
