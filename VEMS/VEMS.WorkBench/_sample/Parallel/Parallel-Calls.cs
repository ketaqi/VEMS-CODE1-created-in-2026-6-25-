//var localIp = "192.168.7.1";
//var localPort = 1024;
var remoteIp = "10.0.110.251";
var remotePort = 2024;

Socket s = new Socket(AddressFamily.InterNetwork,
    SocketType.Stream,
    ProtocolType.Tcp);
// tries to connect
await s.ConnectAsync(remoteIp, remotePort);
string message = "<|RWI|> 5";
//"<|RUN|> VFrame f = VFrame.CreateFrame(); VFrame.RefreshShow(f);"; 
// $"Connection from {s.LocalEndPoint} <|EOM|>"; "<|ACK|>";
byte[] messageBytes = Encoding.UTF8.GetBytes(message);
_ = await s.SendAsync(messageBytes, SocketFlags.None);
Console.WriteLine($"Message sent from {s.LocalEndPoint}: \"{message}\"");

//Network.StartAsNode(localIp, localPort, remoteIp, remotePort);

//var ip = Network.CheckIPs()[0].ToString();
//var port = 10024;
//var s = Network.CreateSocket(ip, port);
//s.Connect(ip2, port2);
//var hello = $"hello from {ip}: {port} as a node";
//var bs = Encoding.ASCII.GetBytes(hello);
//s.Send(bs, bs.Length, 0);


//for (int i = 0; i < 501; i++)
//{
//    var port = 2024 + i;
//    var s = Network.CreateSocket(ip, port);

//    s.Connect(ip2, port2);
//    //var hello = $"hello from {ip}: {port}";
//    //var bs = Encoding.ASCII.GetBytes(hello);
//    //s.Send(bs, bs.Length, 0);
//}