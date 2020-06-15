using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace select_echo_server
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
        static Socket _listener;
        static Dictionary<Socket,ClientState> _clients = new Dictionary<Socket, ClientState>();

        static void Main(string[] args)
        {
            _listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            var ip = IPAddress.Parse("127.0.0.1");
            var ep = new IPEndPoint(ip,8888);
            _listener.Bind(ep);
            _listener.Listen(0);
            Console.WriteLine("开始监听");
            onUpdate += Select;
            Run();
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

        static void SendCB(IAsyncResult ar)
        {
            try
            {
                var state = ar.AsyncState as ClientState;
                var clientSock = state.socket;
                int cnt = clientSock.EndSend(ar);
                var str = Encoding.UTF8.GetString(state.sendBuffer,0,cnt);
                Console.WriteLine($"发送：{str}");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static List<ClientState> _needRemove = new List<ClientState>(10);

        static List<Socket> _selectList = new List<Socket>();
        static void Select()
        {
            _selectList.Clear();
            _selectList.Add(_listener);
            foreach(var sock in _clients.Keys)
                _selectList.Add(sock);

            Socket.Select(_selectList,null,null,0);
            foreach(var sock in _selectList){
                if(sock == _listener){
                    var clientSock = _listener.Accept();
                    Console.WriteLine($"接受连接：{clientSock.RemoteEndPoint}");
                    var state = new ClientState(){ socket = clientSock,remote = clientSock.RemoteEndPoint.ToString() };
                    _clients.Add(clientSock,state);
                }else{
                    var state = _clients[sock];
                    try{
                        if(state.socket.Poll(0,SelectMode.SelectRead)){
                            int cnt = state.socket.Receive(state.recBuffer);
                            if(cnt == 0)
                            {
                                _needRemove.Add(state);
                                continue;
                            }
                            
                            BroadCast(state,cnt);
                        }
                    }catch{
                        _needRemove.Add(state);
                    }
                }
            }

            foreach(var state in _needRemove){
                try{
                    state.socket.Close();
                }
                catch{}
                _clients.Remove(state.socket);
                Console.WriteLine($"断开连接：{state.remote}");
            }
            _needRemove.Clear();
        }
        
        static void BroadCast(ClientState state,int cnt)
        {
            var recStr = Encoding.UTF8.GetString(state.recBuffer,0,cnt);
            Console.WriteLine($"收到:{recStr}");
            var sendBytes = Encoding.UTF8.GetBytes(recStr);
            
            foreach(var s in _clients.Values)
            {
                if(s!=state)
                {
                    Array.Copy(sendBytes,s.sendBuffer,sendBytes.Length);
                    try{
                        s.socket.BeginSend(s.sendBuffer,0,sendBytes.Length,0,SendCB,s);
                    }catch{}
                }
            }
        }
    }
}
