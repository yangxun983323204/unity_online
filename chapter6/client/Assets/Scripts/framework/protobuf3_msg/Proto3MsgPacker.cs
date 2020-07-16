using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;

namespace Proto3Msg
{
    public class Proto3MsgPacker : MsgPacker
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
            var tmp = new byte[count];
            Array.Copy(bytes, offset, tmp, 0, count);
            var istream = new CodedInputStream(tmp);
            var msg = Activator.CreateInstance(Type.GetType(protoName)) as IMessage;
            istream.ReadMessage(msg);
            return msg;
        }

        protected override byte[] EncodeMsg(object msg)
        {
            using (var ms = new MemoryStream())
            {
                var ostream = new CodedOutputStream(ms);
                ostream.WriteMessage(msg as IMessage);
                ostream.Flush();
                return ms.ToArray();
            }
        }
    }
}
