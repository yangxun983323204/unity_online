using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

public class NetManager
{
    public MsgPacker Packer { get; set; } = null;
    private Func<object,bool> _isPing;
    private Func<object> _makePongMsg;
    private Socket _listenfd;
    private Dictionary<Socket,ClientState> _clients;
    private List<Socket> _checkRead;
    private HeartbeatMgr _heartbeat;

    public NetManager(int initCapacity = 100)
    {
        _clients = new Dictionary<Socket, ClientState>(initCapacity);
        _checkRead = new List<Socket>(initCapacity);
        _heartbeat = new HeartbeatMgr(this);
    }

    public void StartLoop(int listenPort)
    {

        _listenfd = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        var addr = IPAddress.Parse("0.0.0.0");
        var ep = new IPEndPoint(addr,listenPort);
        _listenfd.Bind(ep);
        _listenfd.Listen(0);
        while(true){
            ResetCheckRead();
            Socket.Select(_checkRead,null,null,1000);
            for(int i = _checkRead.Count - 1;i>=0;i--){
                Socket s = _checkRead[i];
                if(s == _listenfd)
                    Accept();
                else
                    ReadClient(s);
            }
        }

        Timer();
    }
    private void ResetCheckRead()
    {
        _checkRead.Clear();
        _checkRead.Add(_listenfd);
        foreach(var s in _clients.Values){
            _checkRead.Add(s.Sock);
        }
    }
    private void Accept()
    {
        try{
            Socket clientfd = _listenfd.Accept();
            var client = new ClientState();
            client.Sock = clientfd;
            _clients.Add(clientfd,client);
        }catch(SocketException e){
            // todo
        }
    }
    private void ReadClient(Socket clientfd)
    {
        var client = _clients[clientfd];
        var recBuffer = client.RecBuffer;
        int count = 0;
        if(recBuffer.Remain<=0){
            OnReceiveData(client);
            recBuffer.MoveBytes();
        }

        if(recBuffer.Remain<=0){
            Close(client);
            return;
        }

        try{
            count = clientfd.Receive(recBuffer.Bytes,recBuffer.WriteIdx,recBuffer.Remain,0);
        }catch(SocketException e){
            Close(client);
            return;
        }

        if(count <= 0){
            Close(client);
            return;
        }

        recBuffer.MoveWriteIdx(count);
        OnReceiveData(client);
        recBuffer.CheckAndMoveBytes();
    }
    public void Close(ClientState client)
    {
        CallMsgHandler("OnDisconnect",null,client);
        client.Sock.Close();
        _clients.Remove(client.Sock);
    }
    private void OnReceiveData(ClientState client)
    {
        int count = 0;
        object msg = null;
        var recBuffer = client.RecBuffer;
        if(Packer.Decode(recBuffer.Bytes,recBuffer.ReadIdx,recBuffer.Length,out msg,out count))
        {
            recBuffer.MoveReadIdx(count);
            recBuffer.CheckAndMoveBytes();
            CallMsgHandler(Packer.GetMsgName(msg),msg,client);
            if(recBuffer.Length > 2)
            {
                OnReceiveData(client);
            }
        }
    }
    private void Timer()
    {
        foreach(var client in _clients.Values)
            _heartbeat.Process(client);

        CallMsgHandler("OnTimer",null,null);
    }
    public void Send(ClientState client,object msg)
    {
        if(client == null || !client.Sock.Connected) return;

        var bytes = Packer.Encode(msg);
        var socket = client.Sock;
        var sendQueue = client.SendQueue;
        sendQueue.EnqueueFrom(bytes);
        if (sendQueue.Count == 1)
        {
            var node = sendQueue.Peek();
            socket.BeginSend(node.Bytes, node.ReadIdx, node.Length, 0, SendCallback, client);
        }
    }
    private void SendCallback(IAsyncResult ar)
    {
        var client = (ClientState)ar.AsyncState;
        if(client == null) return;

        var sock = client.Sock;
        if (sock == null || !sock.Connected)
            return;

        try
        {
            var sendQueue = client.SendQueue;
            int count = sock.EndSend(ar);
            ByteArray node = sendQueue.Peek();
            node.MoveReadIdx(count);
            if (node.Length == 0)
            {
                sendQueue.Dequeue();
                node = sendQueue.Peek();
            }

            if (node != null)// 发送下一个缓冲块
            {
                sock.BeginSend(node.Bytes, node.ReadIdx, node.Length, 0, SendCallback, client);
            }
        }
        catch (Exception e)
        {
            Close(client);
        }
    }
    private void CallMsgHandler(string name,object msg,ClientState client)
    {
        // todo call
        // 心跳回应
        _heartbeat.OnMsgRec(client,msg);
    }
    public void EnableHeartbeat(Func<object,bool> isPing,Func<object> makePongMsg)
    {
        _heartbeat.Enable = true;
        _heartbeat.IsPing = isPing;
        _heartbeat.MakePongMsg = makePongMsg;
    }
    public void DisableHeartbeat() { _heartbeat.Enable = false; }
}