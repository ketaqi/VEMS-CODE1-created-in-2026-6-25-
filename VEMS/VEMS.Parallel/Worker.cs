using System.Net.Sockets;
using System.Net;
using System.Text;

namespace VEMS.Parallel
{

    /// <summary>
    /// worker class for parallel computing
    /// </summary>
    public class Worker
    {


        public Socket CreateSocket(string host, int port)
        {
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new(ip, port);

            Socket w = new(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            Console.WriteLine("connecting ...");
            w.Connect(ipe);

            return w;
        }

        public void Test()
        {
            int port = 2000;
            string host = "10.0.110.251";

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new(ip, port);

            Socket c = new(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            Console.WriteLine("connecting ...");
            c.Connect(ipe);

            string sendStr = " hello from Huangsen!";
            byte[] bs = Encoding.ASCII.GetBytes(sendStr);
            Console.WriteLine("sending message ...");
            c.Send(bs, bs.Length, 0);
        }
    }
}
