using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace poll_client
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
            onUpdate += PollRecive;
            string input = null;
            while("c"!=input)
            {
                input = Console.ReadLine();
                if("c"!=input)
                    CallSend(socket,input);
            }
            WaitExit();
        }

#region 线程实现Update
        static event Action onUpdate;
        static bool _threadRun = true;
        static void Run()
        {
            Thread th = new Thread(()=>{
                while(_threadRun){
                    onUpdate?.Invoke();
                    Thread.Sleep(1);
                }

            });
            th.Start();
        }
        static void WaitExit()
        {
            Console.WriteLine("按下任意键退出...");
            Console.ReadKey();
            _threadRun = false;
        }
#endregion

        static void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            socket.BeginConnect("127.0.0.1",8888, ConnectCB,socket);

        }

        static void CallSend(Socket sock,string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            Array.Copy(bytes,sendBuffer,bytes.Length);
            sock.BeginSend(sendBuffer,0,bytes.Length,0,SendCB,sock);
        }

        static void ConnectCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var str = "你好";
                CallSend(sock,str);
                Run();
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static byte[] sendBuffer = new byte[1024];
        static void SendCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var cnt = sock.EndSend(ar);
                var str = Encoding.UTF8.GetString(sendBuffer,0,cnt);
                Console.WriteLine($"发送:{str}");
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static byte[] recBuffer = new byte[1024];
        static void PollRecive()
        {
            if(socket == null)
                return;

            if(socket.Poll(0,SelectMode.SelectRead)){
                int cnt = socket.Receive(recBuffer);
                var recStr = Encoding.UTF8.GetString(recBuffer,0,cnt);
                Console.WriteLine($"收到:{recStr}");
            }
        }
    }
}
