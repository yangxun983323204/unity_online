using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace JsonMsg
{
    public class JsonMsgPacker : AdvanceMsgPacker
    {
        public override string GetMsgName(Type type)
        {
            return type.ToString();
        }

        public override string GetMsgName(object msg)
        {
            return (msg as JsonMsgBase).ProtoName;
        }

        protected override byte[] EncodeMsg(object msg)
        {
            var jsonMsg = msg as JsonMsgBase;
            string s = JsonConvert.SerializeObject(msg);
            var bodyBytes = Encoding.UTF8.GetBytes(s);
            return bodyBytes;
        }

        protected override object DecodeMsg(string protoName, byte[] bytes, int offset, int count)
        {
            string s = Encoding.UTF8.GetString(bytes, offset, count);
            JsonMsgBase msg = (JsonMsgBase)JsonConvert.DeserializeObject(s, Type.GetType(protoName));
            return msg;
        }
    }
}