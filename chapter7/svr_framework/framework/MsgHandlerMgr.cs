using System;
using System.Collections.Generic;
using System.Reflection;

// 参数为：收到的消息，所属客户端
using HandleMethod = System.Action<object,ClientState>;

internal class HandlerInfo
{
    Dictionary<string,HandleMethod> _methods = new Dictionary<string, HandleMethod>();

    public void CollectInfo(object inst)
    {
        var methods = inst.GetType().GetMethods(BindingFlags.Instance|BindingFlags.Public);
        foreach(var m in methods){
            if(m.Name.StartsWith("MSG_")){
                var args = m.GetParameters();
                if(args.Length == 2 && args[0].ParameterType == typeof(object) && args[1].ParameterType == typeof(ClientState)){
                    var action = (HandleMethod)Delegate.CreateDelegate(typeof(HandleMethod),inst,m);
                    _methods.Add(m.Name.Replace("MSG_",""),action);
                }
            }
        }
    }

    public void Invoke(string type,object msg,ClientState client)
    {
        if(_methods.ContainsKey(type))
            _methods[type].Invoke(msg,client);
    }
}

public class MsgHandlerMgr
{
    Dictionary<object,HandlerInfo> _handlers = new Dictionary<object, HandlerInfo>();

    public void AddHandler(object obj)
    {
        if(!_handlers.ContainsKey(obj)){
            var handler = new HandlerInfo();
            handler.CollectInfo(obj);
            _handlers.Add(obj,handler);
        }
    }

    public void RemoveHandler(object obj)
    {
        if(_handlers.ContainsKey(obj)){
            _handlers.Remove(obj);
        }
    }

    public void HandleMsg(string type,object msg,ClientState client)
    {
        var dot = type.LastIndexOf('.');
        if(dot>=0)
            type = type.Substring(dot+1);
        foreach(var handler in _handlers.Values)
            handler.Invoke(type,msg,client);
    }
}