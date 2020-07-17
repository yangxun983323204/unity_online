/*
 *  |包体长度(2)|协议名长度(2)|协议名|协议数据|   
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
/// <summary>
/// 消息序列化\反序列化器
/// </summary>
public abstract class MsgPacker 
{
    public byte[] Encode(object msg)
    {
        var nameBytes = EncodeName(GetMsgName(msg));
        var bodyBytes = EncodeMsg(msg);

        int len = nameBytes.Length + bodyBytes.Length;
        var sendBytes = new byte[2 + len];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        return sendBytes;
    }

    public bool Decode(byte[] bytes, int offset,int len, out object msg, out int count)
    {
        msg = null;
        count = 0;
        if (len <= 2) return false;

        short packBodyLen = (short)((bytes[offset + 1] << 8) | bytes[offset]);// 小端
        if (len < packBodyLen) return false;

        offset += 2;len -= 2;
        int msgNameLen = 0;
        string name;
        if (!DecodeName(bytes, offset,len, out name, out msgNameLen)) return false;

        offset += msgNameLen;len -= msgNameLen;
        int msgBodyLen = packBodyLen - msgNameLen;
        msg = DecodeMsg(name, bytes, offset, msgBodyLen);
        count = 2 + packBodyLen;
        return true;
    }

    public abstract string GetMsgName(Type type);

    public abstract string GetMsgName(object msg);

    protected abstract byte[] EncodeMsg(object msg);

    protected abstract object DecodeMsg(string protoName, byte[] bytes, int offset, int count);

    protected virtual byte[] EncodeName(string name)
    {
        var nameBytes = Encoding.UTF8.GetBytes(name);
        short len = (short)nameBytes.Length;
        var bytes = new byte[2+len];
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }

    protected virtual bool DecodeName(byte[] bytes,int offset,int size,out string name,out int count)
    {
        name = null;
        count = 0;
        if (offset + 2 > size)
            return false;

        short len = (short)((bytes[offset+1]<<8)|bytes[offset]);
        if (offset + 2 + len > bytes.Length)
            return false;

        count = 2 + len;
        name = Encoding.UTF8.GetString(bytes, offset + 2, len);
        return true;
    }
}
