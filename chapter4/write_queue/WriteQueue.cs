using System;
using System.Collections.Generic;

public class ByteArray{
    public byte[] bytes;
    public int readIdx = 0;
    public int writeIdx = 0;
    public int Length{get{return writeIdx-readIdx;}}
    public ByteArray(byte[] data){
        bytes = data;
        readIdx = 0;
        writeIdx = data.Length;
    }

    public void Move(ushort offset){
        readIdx += offset;
    }
}

public class WriteQueue:Queue<ByteArray>{

    public ByteArray EnqueueFrom(byte[] data){
        var ba = new ByteArray(data);
        Enqueue(ba);
        return ba;
    }
}