namespace MathTcpServer
{
    public class Program
    {
        public const int Port = 14593;
        static void Main()
        {
            MathServer server = new MathServer(Port);
            server.Start();
        }
    }
}