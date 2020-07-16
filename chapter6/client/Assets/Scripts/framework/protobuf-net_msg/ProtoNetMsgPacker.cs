using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ProtoNetMsg
{
    public class ProtoNetMsgPacker : MsgPacker
    {
        public override string GetMsgName(Type type)
        {
            return type.FullName;
        }

        public override string GetMsgName(object msg)
        {
            return GetMsgName(msg.GetType());
        }

        protected override object DecodeMsg(string protoName, byte[] bytes, int offset, int count)
        {
            using (var ms = new MemoryStream(bytes,offset,count))
            {
                var type = System.Type.GetType(protoName);
                return ProtoBuf.Serializer.NonGeneric.Deserialize(type,ms);
            }
        }

        protected override byte[] EncodeMsg(object msg)
        {
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.NonGeneric.Serialize(ms, msg);
                return ms.ToArray();
            }
        }
    }
}
