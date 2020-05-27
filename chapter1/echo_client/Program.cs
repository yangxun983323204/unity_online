using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace echo_client
{
    class Program
    {
        static Socket socket;
        static void Main(string[] args)
        {
            Console.WriteLine("按下任意键开始连接...");
            Console.ReadKey();
            try
            {
                Connect();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("按下任意键退出...");
            Console.ReadKey();
        }

        static void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            socket.Connect("127.0.0.1",8888);

            var str = "你好";
            var bytes = Encoding.UTF8.GetBytes(str);
            socket.Send(bytes);
            Console.WriteLine($"发送:{str}");
            var buffer = new byte[1024];
            var cnt = socket.Receive(buffer);
            var recStr = Encoding.UTF8.GetString(buffer,0,cnt);
            Console.WriteLine($"收到:{recStr}");
            socket.Close();
        }
    }
}
