using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace echo_server
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            var ip = IPAddress.Parse("127.0.0.1");
            var ep = new IPEndPoint(ip,8888);
            listener.Bind(ep);
            listener.Listen(0);
            Console.WriteLine("开始监听");
            while(true)
            {
                var socket = listener.Accept();
                Console.WriteLine($"接受连接：{socket.RemoteEndPoint}");
                var buffer = new byte[1024];
                var cnt = socket.Receive(buffer);
                var str = Encoding.UTF8.GetString(buffer,0,cnt);
                Console.WriteLine($"收到：{str}");
                var echoStr = $"我已收到你发的{str}";
                socket.Send(Encoding.UTF8.GetBytes(echoStr));
            }
        }
    }
}
