using System.Net.Sockets;
using System.Net;
using System.Text;

namespace VEMS.Parallel
{

    /// <summary>
    /// master class for parallel computing
    /// </summary>
    public class Master
    {

        public static Socket CreateSocket(string host, int port)
        {
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new(ip, port);

            Socket s = new(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            s.Bind(ipe);
            s.Listen(20); // ???
            Console.WriteLine("waiting for incoming connection ...");

            //
            Socket t = s.Accept();
            Console.WriteLine("connection built");
            //

            return s;
        }

        public void Test()
        {

            int port = 2000;
            string host = "10.0.110.251";

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new(ip, port);

            Socket s = new(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            s.Bind(ipe);
            s.Listen(0);
            Console.WriteLine("waiting for incoming connection ...");

            Socket t = s.Accept();
            Console.WriteLine("connection built");
            string recvStr = "";
            byte[] recvBytes = new byte[1024];
            int bytes;
            bytes = t.Receive(recvBytes, recvBytes.Length, 0);
            recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);

            Console.WriteLine("server get message:{0}", recvStr);
            string sendStr = "message successfully received";
            byte[] bs = Encoding.ASCII.GetBytes(sendStr);
            t.Send(bs, bs.Length, 0);
            t.Close();
            s.Close();
            Console.ReadLine();

        }

    }
}
