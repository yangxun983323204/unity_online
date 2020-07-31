using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
            return msg.GetType().ToString();
        }

        protected override byte[] EncodeMsg(object msg)
        {
            string s = JsonUtility.ToJson(msg);
            var bodyBytes = Encoding.UTF8.GetBytes(s);
            return bodyBytes;
        }

        protected override object DecodeMsg(string protoName, byte[] bytes, int offset, int count)
        {
            string s = Encoding.UTF8.GetString(bytes, offset, count);
            var msg = JsonUtility.FromJson(s, Type.GetType(protoName));
            return msg;
        }
    }
}