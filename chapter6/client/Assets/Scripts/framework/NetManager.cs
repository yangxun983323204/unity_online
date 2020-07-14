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

    Socket _socket;
    ByteArray _recBuffer;
    WriteQueue _sendQueue;
    Dictionary<NetEvent, Action<string>> _eventListeners = new Dictionary<NetEvent, Action<string>>();

    public void AddEventListener(NetEvent e, Action<string> listener)
    {
        Debug.Assert(listener!=null);
        if (_eventListeners.ContainsKey(e))
            _eventListeners[e] += listener;
        else
            _eventListeners.Add(e, listener);
    }

    public void RemoveEventListener(NetEvent e, Action<string> listener)
    {
        Debug.Assert(listener != null);
        if (_eventListeners.ContainsKey(e))
            _eventListeners[e] -= listener;
        
        if(_eventListeners[e] == null)
            _eventListeners.Remove(e);
    }

    private void FireEvent(NetEvent e,string msg)
    {
        if (_eventListeners.ContainsKey(e))
            _eventListeners[e](msg);
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
            FireEvent(NetEvent.ConnectSuccess, null);
        }
        catch (Exception e)
        {
            FireEvent(NetEvent.ConnectFail, e.ToString());
        }
        finally
        {
            _isConnecting = false;
        }
    }

    bool _isCloseing = false;
    public void Close()
    {
        if (_isCloseing || _socket == null || !_socket.Connected)
            return;

        if (_sendQueue.Count > 0)
            _isCloseing = true;
        else
        {
            _socket.Close();
            FireEvent(NetEvent.Close, null);
        }
    }
}
