using System;
using System.Collections;
using System.Collections.Generic;

public class HeartbeatMgr
{
    private bool _enable = false;
    public bool Enable {
        get { return _enable; }
        set { _enable = value;Reset(null); }
    }
    public int PingInterval { get; set; } = 30;
    public Func<object> MakePingMsg { get; set; }

    NetManager _netMgr;
    long _lastPingTime = 0;
    long _lastPongTime = 0;

    public HeartbeatMgr(NetManager netMgr)
    {
        _netMgr = netMgr;
        _netMgr.NetMsgDispatcher.AddListener(NetManager.__INTERNAL_ANY_MSG, OnMsgRec);
        _netMgr.NetEventDispatcher.AddListener(NetManager.NetEvent.ConnectSuccess, Reset);
    }

    private void Reset(string msg)
    {
        _lastPingTime = DateTime.Now.Ticks;
        _lastPongTime = DateTime.Now.Ticks;
    }

    public void Process()
    {
        if (!Enable) return;

        var curr = DateTime.Now.Ticks;
        if (curr - _lastPingTime > PingInterval * 10000000)
        {
            _lastPingTime = curr;
            _netMgr.Send(MakePingMsg());
        }

        if (curr - _lastPongTime > PingInterval * 4 * 10000000)
        {
            _netMgr.Close();
        }
    }

    private void OnMsgRec(object msg)
    {
        _lastPingTime = _lastPongTime = DateTime.Now.Ticks;
    }
}
