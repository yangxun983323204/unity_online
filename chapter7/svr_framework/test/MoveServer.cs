using JsonMsg;
using System;

public class MoveServer
{
    private NetManager _netMgr = new NetManager();

    public MoveServer()
    {
        MsgDictDefine.Reg(
                "JsonMsg.MsgMove",
                "JsonMsg.MsgPing"
                );

        var packer = new JsonMsgPacker();
        packer.MsgDefine = MsgDictDefine.Instance;
        _netMgr.Packer = packer;
        _netMgr.AddMsgHandler(this);
        _netMgr.StartLoop(8888);
    }

//=========================================================================================

    public void MSG_OnConnect(object msg,ClientState client)
    {
        Console.WriteLine("Connect:"+client.Sock.RemoteEndPoint);
    }

    public void MSG_OnDisconnect(object msg,ClientState client)
    {
        Console.WriteLine("Disconnect:"+client.Sock.RemoteEndPoint);
    }

    public void MSG_OnTimer(object msg,ClientState client){

    }

    public void MSG_MsgMove(object msg,ClientState client)
    {
        var move = msg as MsgMove;
        var str = String.Format("move:[{0},{1},{2}]", move.X.ToString(), move.Y.ToString(), move.Z.ToString());
        Console.WriteLine(str);
        move.X += 1;
        _netMgr.Send(client,move);
    }

    public void MSG_MsgPing(object msg,ClientState client)
    {
        _netMgr.Send(client,msg);
    }
}

namespace JsonMsg
{
    public class MsgPing : JsonMsgBase
    {
    }

    public class MsgMove : JsonMsgBase
    {
        public float X = 0;
        public float Y = 0;
        public float Z = 0;
    }
}
