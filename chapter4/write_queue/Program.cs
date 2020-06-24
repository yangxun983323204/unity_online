using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace write_queue
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
                sock.BeginReceive(recBuffer,_recBufferCnt,recBuffer.Length - _recBufferCnt,0,ReceiveCB,sock);
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static byte[] sendBuffer = new byte[1024];
        static int _sendLen= 0;
        static int _sendIdx = 0;
        static void CallSend(Socket sock,string msg)
        {
            var bytes = Encoding.UTF8.GetBytes(msg);
            Int16 bodyLen = (Int16)bytes.Length;
            var lenBytes = BitConverter.GetBytes(bodyLen);
            if(!BitConverter.IsLittleEndian)// 小端发送
                Array.Reverse(lenBytes);

            Array.Copy(lenBytes,sendBuffer,2);
            Array.Copy(bytes,0,sendBuffer,2,bytes.Length);
            _sendLen = 2+bytes.Length;
            sock.BeginSend(sendBuffer,0,_sendLen,0,SendCB,sock);
        }

        static void SendCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var cnt = sock.EndSend(ar);
                _sendIdx += cnt;
                _sendLen -= cnt;
                var str = Encoding.UTF8.GetString(sendBuffer,0,cnt);
                Console.WriteLine($"发送:{str}");
                if(_sendLen>0){
                    sock.BeginSend(sendBuffer,_sendIdx,_sendLen,0,SendCB,sock);
                }
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static byte[] recBuffer = new byte[1024];
        static int _recBufferCnt = 0;
        static void ReceiveCB(IAsyncResult ar)
        {
            try
            {
                var sock = ar.AsyncState as Socket;
                var cnt = sock.EndReceive(ar);
                _recBufferCnt += cnt;
                OnReceiveData();
                Thread.Sleep(5000);// 等待5秒以造成粘包
                sock.BeginReceive(recBuffer,0,recBuffer.Length,0,ReceiveCB,sock);
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // 数据流拆分，消息头有两字节的长度标识
        static void OnReceiveData()
        {
            if(_recBufferCnt<=2)
                return;
            
            Int16 bodyLen = (short)((recBuffer[1]<<8)|recBuffer[0]);// 发送的是小端，直接按小端解析
            if(_recBufferCnt<2+bodyLen)
                return;

            var s = Encoding.UTF8.GetString(recBuffer,2,bodyLen);
            Console.WriteLine("[Recv] "+s);
            var offset = 2 + bodyLen;
            var cnt = _recBufferCnt - offset;
            Array.Copy(recBuffer,offset,recBuffer,0,cnt);
            _recBufferCnt -= offset;
            OnReceiveData();
        }
    }
}
