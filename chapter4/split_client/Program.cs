using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace split_client
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
            string input = null;
            while("c"!=input)
            {
                input = Console.ReadLine();
                if("c"!=input)
                    CallSend(socket,input);
            }
            Console.WriteLine("按下任意键退出...");
            Console.ReadKey();
        }

        static void Connect()
        {
            socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            socket.BeginConnect("127.0.0.1",8888, ConnectCB,socket);

        }

        static void CallSend(Socket sock,string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            Int16 bodyLen = (Int16)bytes.Length;
            var lenBytes = BitConverter.GetBytes(bodyLen);
            if(!BitConverter.IsLittleEndian)// 小端发送
                Array.Reverse(lenBytes);

            Array.Copy(lenBytes,sendBuffer,2);
            Array.Copy(bytes,0,sendBuffer,2,bytes.Length);
            sock.BeginSend(sendBuffer,0,2+bytes.Length,0,SendCB,sock);
        }

        static void ConnectCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var str = "你好";
                CallSend(sock,str);
                sock.BeginReceive(recBuffer,_bufferCount,recBuffer.Length - _bufferCount,0,ReceiveCB,sock);
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
        static int _bufferCount = 0;
        static void ReceiveCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var cnt = sock.EndReceive(ar);
                _bufferCount += cnt;
                OnReceiveData();
                Thread.Sleep(5000);// 等待5秒以造成粘包
                sock.BeginReceive(recBuffer,0,recBuffer.Length,0,ReceiveCB,sock);
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // 数据流拆分
        static void OnReceiveData()
        {
            if(_bufferCount<=2)
                return;
            
            Int16 bodyLen = (short)((recBuffer[1]<<8)|recBuffer[0]);// 发送的是小端，直接按小端解析
            if(_bufferCount<2+bodyLen)
                return;

            var s = Encoding.UTF8.GetString(recBuffer,2,bodyLen);
            Console.WriteLine("[Recv] "+s);
            var offset = 2 + bodyLen;
            var cnt = _bufferCount - offset;
            Array.Copy(recBuffer,offset,recBuffer,0,cnt);
            _bufferCount -= offset;
            OnReceiveData();
        }
    }
}
