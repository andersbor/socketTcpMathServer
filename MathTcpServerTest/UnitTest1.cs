using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using MathTcpServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
  
namespace MathTcpServerTest
{
    [TestClass]
    public class UnitTest1
    {
        private static MathServer _server;
        private const int Port  = 12345;

        [ClassInitialize]
        public static void StartUp(TestContext context)
        {         
            _server = new MathServer(Port);
            Task.Run(() => _server.Start());
            //_server.Start(); // test will run the server, and nothing else will happen
        }

        [ClassCleanup]
        public static void Stop()
        {
            //GetResponse("STOP");
            _server.Stop();
        }

        [TestMethod]
        public void TestServer()
        {
            Assert.AreEqual("9.9", GetResponse("ADD 4.4 5.5"));
            Assert.AreEqual("1.2", GetResponse("SUB 5.6 4.4"));
            Assert.AreEqual("Illegal request: No such operation: plop", GetResponse("plop 4.4 5.5"));
            Assert.AreEqual("Illegal request: Not a number", GetResponse("ADD 4.4 abc"));
            Assert.AreEqual("Illegal request: ADD 4.4", GetResponse("ADD 4.4"));
        }

        private static string GetResponse(string request)
        {
            string server = "localhost"; // Dns.GetHostName();
            using (TcpClient client = new TcpClient(server, Port))
            {
                NetworkStream stream = client.GetStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(request);
                writer.Flush();
                StreamReader reader = new StreamReader(stream);
                string response = reader.ReadLine();
                return response;
            }
        }
    }
}