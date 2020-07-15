using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace async_echo_server
{
    class ClientState
    {
        public Socket socket;
        public string remote;
        public byte[] recBuffer = new byte[1024];
        public byte[] sendBuffer = new byte[1024];
    }

    class Program
    {
        static Dictionary<Socket,ClientState> _clients = new Dictionary<Socket, ClientState>();

        static void Main(string[] args)
        {
            var listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            var ip = IPAddress.Parse("127.0.0.1");
            var ep = new IPEndPoint(ip,8888);
            listener.Bind(ep);
            listener.Listen(0);
            Console.WriteLine("开始监听");
            listener.BeginAccept(AcceptCB,listener);
            Console.ReadKey();
        }

        static void AcceptCB(IAsyncResult ar)
        {
            try
            {
                var listener = ar.AsyncState as Socket;
                var clientSock = listener.EndAccept(ar);
                Console.WriteLine($"接受连接：{clientSock.RemoteEndPoint}");
                var state = new ClientState(){ socket = clientSock,remote = clientSock.RemoteEndPoint.ToString() };
                _clients.Add(clientSock,state);

                clientSock.BeginReceive(state.recBuffer,0,1024,0,ReceiveCB,state);
                listener.BeginAccept(AcceptCB,listener);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void SendCB(IAsyncResult ar)
        {
            try
            {
                 var state = ar.AsyncState as ClientState;
                var clientSock = state.socket;
                int cnt = clientSock.EndSend(ar);
                var str = Encoding.UTF8.GetString(state.sendBuffer,0,cnt);
                Console.WriteLine($"发送：{str}");

                clientSock.BeginReceive(state.recBuffer,0,1024,0,ReceiveCB,state);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void ReceiveCB(IAsyncResult ar)
        {
            try
            {
                var state = ar.AsyncState as ClientState;
                var clientSock = state.socket;
                int cnt = clientSock.EndReceive(ar);
                if(cnt == 0){
                    clientSock.Close();
                    _clients.Remove(clientSock);
                    Console.WriteLine($"断开连接：{state.remote}");
                    return;
                }

                var recStr = Encoding.UTF8.GetString(state.recBuffer,0,cnt);
                var sendBytes = Encoding.UTF8.GetBytes(recStr);
                Array.Copy(sendBytes,state.sendBuffer,sendBytes.Length);

                clientSock.BeginSend(state.sendBuffer,0,sendBytes.Length,0,SendCB,state);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
