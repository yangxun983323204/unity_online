using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

namespace chaos_server
{
    static class NetMsgHandler
    {
        static Dictionary<string,MethodInfo> _msgHandlers = new Dictionary<string,MethodInfo>(20);
        static NetMsgHandler()
        {
            var methods = typeof(NetMsgHandler).GetMethods(BindingFlags.Static|BindingFlags.Public);
            foreach(var m in methods){
                if(m.Name.StartsWith("Msg")){
                    _msgHandlers.Add(m.Name.Substring(3),m);
                }
            }
        }

        public static void Call(ClientState state,string rawstring)
        {
            try{
                var splits = rawstring.Split('|');
                if(_msgHandlers.ContainsKey(splits[0])){
                    _msgHandlers[splits[0]].Invoke(null,new object[]{state,splits[1]});
                    Console.WriteLine("处理消息:"+splits[0]);
                }
                else{
                    Console.WriteLine("未处理消息:"+splits[0]);
                }
            }
            catch(Exception e){
                Console.WriteLine(e);
            }
        }

        private static void BroadCast(ClientState state,string msg)
        {
            foreach(var s in Program._clients.Values)
            {
                Program.Send(state,msg);
            }
        }

        private static void BroadCastOther(ClientState state,string msg)
        {
            foreach(var s in Program._clients.Values)
            {
                if(s!=state)
                {
                    Program.Send(state,msg);
                }
            }
        }
        public static void MsgEnter(ClientState client,string msg)
        {
            var splits = msg.Split(',');
            client.id = splits[0];
            client.x = float.Parse(splits[1]);
            client.y = float.Parse(splits[2]);
            client.z = float.Parse(splits[3]);
            client.yEuler = float.Parse(splits[4]);
            BroadCast(client,"Enter|"+msg);
        }

        public static void MsgLeave(ClientState client,string msg)
        {
            BroadCastOther(client,"Leave|"+msg);
        }

        public static void MsgMove(ClientState client,string msg)
        {
            BroadCastOther(client,"Move|"+msg);
        }

        public static void MsgList(ClientState client,string msg)
        {
            Proto.SendList(client);
        }
    }
}