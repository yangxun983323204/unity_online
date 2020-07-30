using System;
using System.Collections;
using System.Collections.Generic;

public class HeartbeatMgr
{
    private bool _enable = false;
    public bool Enable {
        get { return _enable; }
        set { _enable = value;}
    }
    public int PingInterval { get; set; } = 30;
    public Func<object,bool> IsPing{get;set;}
    public Func<object> MakePongMsg { get; set; }

    NetManager _netMgr;

    public HeartbeatMgr(NetManager netMgr)
    {
        _netMgr = netMgr;
    }

    public void Process(ClientState client)
    {
        if (!Enable) return;

        var curr = DateTime.Now.Ticks;
        if (curr - client.LastMsgTime > PingInterval * 10000000)
        {
            client.LastMsgTime = curr;
            _netMgr.Send(client,MakePongMsg());
        }

        if (curr - client.LastMsgTime > PingInterval * 4 * 10000000)
        {
            _netMgr.Close(client);
        }
    }

    public void OnMsgRec(ClientState client,object msg)
    {
        if(!Enable)return;

        client.LastMsgTime = DateTime.Now.Ticks;
        if(IsPing!=null && MakePongMsg!=null){
            if(IsPing(msg)){
                var pong = MakePongMsg();
                _netMgr.Send(client,pong);
            }
        }
    }
}
