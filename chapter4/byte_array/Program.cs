using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace byte_array
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

        static void ConnectCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var str = "你好";
                CallSend(sock,str);
                _recBuffer.Reserve(128);
                sock.BeginReceive(_recBuffer.Bytes,_recBuffer.WriteIdx,_recBuffer.Remain,0,ReceiveCB,sock);
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }
        static WriteQueue _wQueue = new WriteQueue();
        static void CallSend(Socket sock,string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            Int16 bodyLen = (Int16)bytes.Length;
            var lenBytes = BitConverter.GetBytes(bodyLen);
            if(!BitConverter.IsLittleEndian)// 小端发送
                Array.Reverse(lenBytes);

            var data = new byte[2+bytes.Length];
            Array.Copy(lenBytes,data,2);
            Array.Copy(bytes,0,data,2,bytes.Length);
            var ba = _wQueue.EnqueueFrom(data);
            if(_wQueue.Count == 1)
                sock.BeginSend(ba.Bytes,0,ba.Length,0,SendCB,sock);
        }

        static void SendCB(IAsyncResult ar)
        {
            try
            {
                var ba = _wQueue.Peek();
                var sock = ar.AsyncState as Socket;
                var cnt = sock.EndSend(ar);
                var str = Encoding.UTF8.GetString(ba.Bytes,ba.ReadIdx,cnt);
                ba.MoveReadIdx(cnt);
                Console.WriteLine($"发送:{str}");
                if(ba.Length == 0)
                    _wQueue.Dequeue();
                    ba = _wQueue.Peek();

                if(ba!=null){
                    sock.BeginSend(ba.Bytes,ba.ReadIdx,ba.Length,0,SendCB,sock);
                }
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static ByteArray _recBuffer = new ByteArray(1024);
        static void ReceiveCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var cnt = sock.EndReceive(ar);
                _recBuffer.MoveWriteIdx(cnt);
                OnReceiveData();
                Thread.Sleep(5000);// 等待5秒以造成粘包
                _recBuffer.Reserve(128);
                sock.BeginReceive(_recBuffer.Bytes,_recBuffer.WriteIdx,_recBuffer.Remain,0,ReceiveCB,sock);
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // 数据流拆分，消息头有两字节的长度标识
        static void OnReceiveData()
        {
            if(_recBuffer.Length<=2)
                return;
            
            Int16 bodyLen = _recBuffer.ReadInt16();
            if(_recBuffer.Length<bodyLen)
                return;

            var s = Encoding.UTF8.GetString(_recBuffer.Bytes,_recBuffer.ReadIdx,bodyLen);
            Console.WriteLine("[Recv] "+s);
            _recBuffer.MoveReadIdx(bodyLen);
            OnReceiveData();
        }
    }
}
