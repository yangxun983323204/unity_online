using System;
using System.Collections.Generic;

public class ByteArray{
    public byte[] Bytes;
    public int ReadIdx{get;private set;}
    public int WriteIdx{get;private set;}
    public int Length{get{return WriteIdx-ReadIdx;}}
    public int Remain{get{return Capacity-WriteIdx;}}
    public int Capacity{get;private set;}
    private int _min = 1024;

    public ByteArray(byte[] data){
        Capacity = data.Length;
        Bytes = data;
        ReadIdx = 0;
        WriteIdx = data.Length;
    }

    public ByteArray(int size = 1024)
    {
        if(size<1024)size = 1024;
        Capacity = size;
        Bytes = new byte[Capacity];
        ReadIdx = 0;
        WriteIdx = 0;
        _min = size;
    }

    public void MoveReadIdx(int offset){
        ReadIdx += offset;
    }

    public void MoveWriteIdx(int offset){
        WriteIdx += offset;
    }

    public void Resize(int size)
    {
        if(size<Length) return;
        if(size<_min) return;
        int n =1;
        while(n<size) n*=2;
        Capacity = n;
        var newBytes = new byte[Capacity];
        Array.Copy(Bytes,ReadIdx,newBytes,0,Length);
        Bytes = newBytes;
        WriteIdx = Length;
        ReadIdx = 0;
    }

    public void CheckAndMoveBytes()
    {
        if(Length<8)
            MoveBytes();
    }

    public void MoveBytes()
    {
        Array.Copy(Bytes,ReadIdx,Bytes,0,Length);
        WriteIdx = Length;
        ReadIdx = 0;
    }
    // 预留空间
    public void Reserve(int dataLen)
    {
        if(Remain<dataLen){
            CheckAndMoveBytes();
            if(Remain<dataLen)
                Resize(Length+dataLen);
        }
    }

    public int Write(byte[] data,int offset,int count)
    {
        if(Remain<count)
            Resize(Length+count);

        Array.Copy(data,offset,Bytes,WriteIdx,count);
        WriteIdx += count;
        return count;
    }

    public int Read(byte[] data,int offset,int count)
    {
        count = Math.Min(count,Length);
        Array.Copy(Bytes,ReadIdx,data,offset,count);
        ReadIdx += count;
        CheckAndMoveBytes();
        return count;
    }

    public Int16 ReadInt16()
    {
        if(Length<2)return 0;
        Int16 ret = (Int16)((Bytes[ReadIdx + 1]<<8)|Bytes[ReadIdx]);// 小端
        ReadIdx+=2;
        CheckAndMoveBytes();
        return ret;
    }

    public Int32 ReadInt32()
    {
        if(Length<4)return 0;
        Int32 ret = (Int32)(
                    (Bytes[ReadIdx + 3]<<24)|
                    (Bytes[ReadIdx + 2]<<16)|
                    (Bytes[ReadIdx + 1]<<8)|
                    Bytes[ReadIdx]
                    );// 小端
        ReadIdx+=4;
        CheckAndMoveBytes();
        return ret;
    }
}