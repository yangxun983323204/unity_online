using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Collections.Concurrent;
using System.Text;

public class NetMgr
{
    Socket _socket;
    byte[] _recBuffer = new Byte[1024];
    Dictionary<string, Action<string>> _eventHandlers = new Dictionary<string, Action<string>>();
    ConcurrentQueue<string> _msgQueue = new ConcurrentQueue<string>();

    public string GetID()
    {
        if (_socket == null || !_socket.Connected)
            return null;
        else
            return _socket.LocalEndPoint.ToString();
    }

    public void AddListener(string name,Action<string> handler)
    {
        _eventHandlers.Add(name, handler);
    }

    public void Connect(string ip,int port)
    {
        if (_socket != null)
            return;

        _socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(ip, port);
        _socket.BeginReceive(_recBuffer, 0, 1024, 0, ReceiveCB, _socket);
    }

    private void ReceiveCB(IAsyncResult ar)
    {
        try
        {
            var sock = ar.AsyncState as Socket;
            var cnt = sock.EndReceive(ar);
            if (cnt<=0)
            {
                _socket = null;
                Debug.Log("Close");
                CallHandler("Close", null);
                return;
            }

            var str = Encoding.UTF8.GetString(_recBuffer, 0, cnt);
            _msgQueue.Enqueue(str);
            _socket.BeginReceive(_recBuffer, 0, 1024, 0, ReceiveCB, _socket);
        }
        catch (Exception e)
        {
            _socket = null;
            Debug.Log(e.ToString());
            CallHandler("Close", null);
        }
    }

    public void Send(string msg)
    {
        if (_socket == null || !_socket.Connected)
            return;

        var bytes = Encoding.UTF8.GetBytes(msg);
        _socket.Send(bytes);
    }

    public void Update()
    {
        if (_msgQueue.Count <= 0)
            return;

        string msg = null;
        if(_msgQueue.TryDequeue(out msg))
        {
            string[] kv = msg.Split('|');
            CallHandler(kv[0], kv[1]);
        }
    }

    public void Close()
    {
        if (_socket!=null)
        {
            _socket.Close();
            _socket = null;
        }
    }

    private void CallHandler(string name,string msg)
    {
        if (_eventHandlers.ContainsKey(name))
        {
            _eventHandlers[name].Invoke(msg);
        }
        else
        {
            Debug.LogWarning("未处理事件:" + name);
        }
    }
}
