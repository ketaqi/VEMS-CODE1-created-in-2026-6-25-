using System.Net.Sockets;
using System.Net;
using System.Text;

namespace VEMS.Parallel
{
    public class Network
    {

        public const string Prefix = "[Network]: ";

        /// <summary>
        /// checks available IPs on the computer
        /// </summary>
        /// <returns> list of IPs </returns>
        public static List<IPAddress> CheckIPs()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ips = new();
            for (long i = 0; i < host.AddressList.Length; i++)
            {
                IPAddress ip = host.AddressList[i];
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    //Console.Write(ip.ToString());
                    ips.Add(ip);
            }
            return ips;
        }

        /// <summary>
        /// creates a socket for network communication
        /// </summary>
        /// <param name="ip"> host IP address </param>
        /// <param name="port"> host port </param>
        /// <returns> result socket </returns>
        public static Socket CreateSocket(string ip, int port)
        {
            IPAddress ipa = IPAddress.Parse(ip);
            IPEndPoint ipe = new(ipa, port);

            Socket s = new(AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            s.Bind(ipe);
            return s;
        }



        public static void StartService(Socket host)
        {
            Thread t = new(Accepting)
            {
                IsBackground = true
            };
            t.Start(host);
        }

        public static void Accepting(object? socket)
        {
            while(true)
            {
                Socket? s = socket as Socket;
                if (s != null)
                {
                    s.Listen();
                    Socket c = s.Accept();
                    if (c.RemoteEndPoint is IPEndPoint ipe)
                        Console.WriteLine($"{Prefix} connection accepted from ip: {ipe.Address} port: {ipe.Port}");

                    string recvStr = "";
                    byte[] recvBytes = new byte[1024];
                    int bytes;
                    bytes = c.Receive(recvBytes, recvBytes.Length, 0);
                    recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
                    Console.WriteLine($"{Prefix} received message: {recvStr}");

                }
            }
        }




        public static async void StartAsNode(string localIp, int localPort,
            string remoteIp, int remotPort)
        {
            // creates local ip endpoint 
            using Socket node = new(AddressFamily.InterNetwork, // localIPE.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);
            //node.Bind(localIPE);

            // creates remote ip endpoint and tries to connect
            IPEndPoint remoteIPE = new(IPAddress.Parse(remoteIp), remotPort);
            await node.ConnectAsync(remoteIPE);
            //node.Connect(remoteIPE);
            string message = $"<|NOD|> Connection from {node.LocalEndPoint} as a node <|EOM|>";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            _ = await node.SendAsync(messageBytes, SocketFlags.None);
            //_ = node.Send(messageBytes, SocketFlags.None);
            Console.WriteLine($"{Prefix} Node sent message: \"{message}\"");

            // starts receiving
            while(true)
            {
                byte[] buffer = new byte[1_024];
                int received = await node.ReceiveAsync(buffer, SocketFlags.None);
                string response = Encoding.UTF8.GetString(buffer, 0, received);
                if (response == "<|ACK|>")
                {
                    Console.WriteLine($"{Prefix} Socket client received acknowledgment: \"{response}\"");
                    break;
                }
            }

            // enters listening mode
            //node.Listen(100);
            ////var handler = await node.AcceptAsync();
            //node.Accept();
            //while (true)
            //{
            //    // ...

            //}

        }


    }
}
