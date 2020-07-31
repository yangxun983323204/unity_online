using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;

public class NetManager
{
    public enum NetEvent
    {
        ConnectSuccess,
        ConnectFail,
        Close
    }

    const short MAX_MESSAGE_PER_FRAME = 10;

    public MsgPacker Packer { get; set; } = null;
    /// <summary>
    /// 网络事件分发器
    /// </summary>
    public MsgDispatcher<NetEvent, string> NetEventDispatcher { get; } = new MsgDispatcher<NetEvent, string>();
    /// <summary>
    /// 网络消息分发器
    /// </summary>
    public MsgDispatcher<string, object> NetMsgDispatcher { get; } = new MsgDispatcher<string, object>();

    Socket _socket;
    ByteArray _recBuffer;
    WriteQueue _sendQueue;
    List<object> _msgList;
    HeartbeatMgr _heartbeat;

    public NetManager()
    {
        _heartbeat = new HeartbeatMgr(this);
    }

    bool _isConnecting = false;
    public void Connect(string ip,int port)
    {
        if (_socket!=null && _socket.Connected || _isConnecting)
            return;
        // init
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _recBuffer = new ByteArray();
        _sendQueue = new WriteQueue();
        _isConnecting = false;
        _isCloseing = false;
        _msgList = new List<object>();
        //
        _socket.NoDelay = true;
        _isConnecting = true;
        _socket.BeginConnect(ip, port, ConnectCallback, _socket);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket sock = ar.AsyncState as Socket;
            sock.EndConnect(ar);
            NetEventDispatcher.Trigger(NetEvent.ConnectSuccess, null);
            sock.BeginReceive(_recBuffer.Bytes, _recBuffer.WriteIdx, _recBuffer.Remain, 0, ReceiveCallback, sock);
        }
        catch (Exception e)
        {
            _socket = null;
            NetEventDispatcher.Trigger(NetEvent.ConnectFail, e.ToString());
        }
        finally
        {
            _isConnecting = false;
        }
    }

    bool _isCloseing = false;
    public void Close(string msg = null)
    {
        if (_isCloseing || _socket == null)
            return;

        lock (_socket)
        {
            if (!_socket.Connected)
            {
                _socket = null;
                NetEventDispatcher.Trigger(NetEvent.Close, msg);
                return;
            }

            if (_sendQueue.Count > 0)
                _isCloseing = true;
            else
            {
                _socket.Close();
                _socket = null;
                NetEventDispatcher.Trigger(NetEvent.Close, msg);
            }
        }
    }

    public void Process()
    {
        _heartbeat.Process();
        if (_msgList.Count <= 0)
            return;

        for (int i = 0; i < MAX_MESSAGE_PER_FRAME; i++)
        {
            object msg = null;
            lock (_msgList)
            {
                if (_msgList.Count > 0)
                {
                    msg = _msgList[0];
                    _msgList.RemoveAt(0);
                }
            }
            if (msg !=null)
            {
                NetMsgDispatcher.Trigger(Packer.GetMsgName(msg), msg);
                NetMsgDispatcher.Trigger(__INTERNAL_ANY_MSG, msg);
            }
            else
            { break; }
        }
    }

    public void EnableHeartbeat(Func<object> makePingMsg)
    {
        _heartbeat.Enable = true;
        _heartbeat.MakePingMsg = makePingMsg;
    }

    public void DisableHeartbeat() { _heartbeat.Enable = false; }

    public void Send(object msg)
    {
        if (_socket == null || !_socket.Connected || _isConnecting || _isCloseing)
            return;

        var sendBytes = Packer.Encode(msg);
        _sendQueue.EnqueueFrom(sendBytes);
        if (_sendQueue.Count == 1)
        {
            var node = _sendQueue.Peek();
            _socket.BeginSend(node.Bytes, node.ReadIdx, node.Length, 0, SendCallback, _socket);
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        var sock = (Socket)ar.AsyncState;
        if (sock == null || !sock.Connected)
            return;

        try
        {
            int count = sock.EndSend(ar);
            ByteArray node = _sendQueue.Peek();
            node.MoveReadIdx(count);
            if (node.Length == 0)
            {
                _sendQueue.Dequeue();
                node = _sendQueue.Peek();
            }

            if (node != null)// 发送下一个缓冲块
            {
                sock.BeginSend(node.Bytes, node.ReadIdx, node.Length, 0, SendCallback, sock);
            }
            else if (_isCloseing)
            {
                sock.Close();
            }
        }
        catch (Exception e)
        {
            sock.Close();
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            var sock = (Socket)ar.AsyncState;
            int count = sock.EndReceive(ar);
            if(count == 0)
            {
                Close();
                return;
            }

            _recBuffer.MoveWriteIdx(count);
            OnReceiveData();
            _recBuffer.Reserve(1024);
            sock.BeginReceive(_recBuffer.Bytes, _recBuffer.WriteIdx, _recBuffer.Remain, 0, ReceiveCallback, sock);
        }
        catch (Exception e)
        {
            Close(e.ToString());
        }
    }

    private void OnReceiveData()
    {
        int count = 0;
        object msg = null;
        try
        {
            if (Packer.Decode(_recBuffer.Bytes, _recBuffer.ReadIdx, _recBuffer.Length, out msg, out count))
            {
                _recBuffer.MoveReadIdx(count);
                _recBuffer.CheckAndMoveBytes();
                lock (_msgList)
                {
                    _msgList.Add(msg);
                }

                if (_recBuffer.Length > 2)// 解下一个包
                {
                    OnReceiveData();
                }
            }
        }
        catch (Exception e)
        {
            NetMsgDispatcher.Trigger(MSG_ERROR, e);
            Close(e.ToString());
        }
        
    }

    public const string MSG_ERROR = "OnError";
    public const string __INTERNAL_ANY_MSG = "__internal_any_msg";
}
