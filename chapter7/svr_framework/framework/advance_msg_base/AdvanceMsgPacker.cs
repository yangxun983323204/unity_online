/*
 *  |包体长度(2)|协议id(2)|协议数据|   
 */
using System.Collections;
using System.Collections.Generic;

public abstract class AdvanceMsgPacker : MsgPacker
{
    public IMsgDefine MsgDefine { get; set; }

    protected override byte[] EncodeName(string name)
    {
        if(MsgDefine == null)
            return base.EncodeName(name);
        else
        {
            short id = MsgDefine.GetID(name);
            var bytes = new byte[2];
            bytes[0] = (byte)(id % 256);
            bytes[1] = (byte)(id / 256);
            return bytes;
        }
    }

    protected override bool DecodeName(byte[] bytes, int offset, int size, out string name, out int count)
    {
        if (MsgDefine == null)
            return base.DecodeName(bytes, offset,size, out name, out count);
        else
        {
            name = null;
            count = 0;
            if (offset + 2 > size)
                return false;

            short id = (short)((bytes[offset + 1] << 8) | bytes[offset]);
            name = MsgDefine.GetName(id);
            count = 2;
            return true;
        }
    }
}
