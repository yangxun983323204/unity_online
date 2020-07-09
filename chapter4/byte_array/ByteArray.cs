using System;
using System.Collections.Generic;

public class ByteArray{
    public byte[] bytes;
    public int ReadIdx{get;private set;}
    public int WriteIdx{get;private set;}
    public int Length{get{return WriteIdx-ReadIdx;}}

    public int Remain{get{return Capacity-WriteIdx;}}
    public int Capacity{get;private set;}
    private int _min = 1024;
    public ByteArray(byte[] data){
        Capacity = data.Length;
        bytes = data;
        ReadIdx = 0;
        WriteIdx = data.Length;
    }

    public ByteArray(int size = 1024)
    {
        Capacity = size;
        bytes = new byte[Capacity];
        ReadIdx = 0;
        WriteIdx = 0;
        _min = size;
    }

    public void Move(ushort offset){
        ReadIdx += offset;
    }

    public void Resize(int size)
    {
        if(size<Length) return;
        if(size<_min) return;
        int n =1;
        while(n<size) n*=2;
        Capacity = n;
        var newBytes = new byte[Capacity];
        Array.Copy(bytes,ReadIdx,newBytes,0,Length);
        bytes = newBytes;
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
        Array.Copy(bytes,ReadIdx,bytes,0,Length);
        WriteIdx = Length;
        ReadIdx = 0;
    }

    public int Write(byte[] data,int offset,int count)
    {
        if(Remain<count)
            Resize(Length+count);

        Array.Copy(data,offset,bytes,WriteIdx,count);
        WriteIdx += count;
        return count;
    }

    public int Read(byte[] data,int offset,int count)
    {
        count = Math.Min(count,Length);
        Array.Copy(bytes,ReadIdx,data,offset,count);
        ReadIdx += count;
        CheckAndMoveBytes();
        return count;
    }
}