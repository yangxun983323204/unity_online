using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace async_echo_client
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
            socket.BeginConnect("127.0.0.1",8888, ConnectCB,socket);

        }

        static void ConnectCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var str = "你好";
                var bytes = Encoding.UTF8.GetBytes(str);
                Array.Copy(bytes,sendBuffer,bytes.Length);
                sock.BeginSend(sendBuffer,0,bytes.Length,0,SendCB,sock);
                sock.BeginReceive(recBuffer,0,recBuffer.Length,0,ReceiveCB,sock);
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static byte[] sendBuffer = new byte[1024];
        static void SendCB(IAsyncResult ar)
        {
            var sock = ar.AsyncState as Socket;
            var cnt = sock.EndSend(ar);
            var str = Encoding.UTF8.GetString(sendBuffer,0,cnt);
            Console.WriteLine($"发送:{str}");
        }

        static byte[] recBuffer = new byte[1024];
        static void ReceiveCB(IAsyncResult ar)
        {
            var sock = ar.AsyncState as Socket;
            
            var cnt = sock.EndReceive(ar);
            var recStr = Encoding.UTF8.GetString(recBuffer,0,cnt);
            Console.WriteLine($"收到:{recStr}");
            sock.Close();
        }
    }
}
