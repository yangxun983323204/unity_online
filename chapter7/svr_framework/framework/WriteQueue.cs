using System;
using System.Collections.Generic;

public class WriteQueue:Queue<ByteArray>{

    object _lock = new object();

    public new int Count {
        get {
            int v;
            lock (_lock)
            {
                v = base.Count;
            }
            return v;
        }
    }

    public ByteArray EnqueueFrom(byte[] data){
        var ba = new ByteArray(data);
        lock (_lock){
            Enqueue(ba);
        }
        return ba;
    }

    public new void Enqueue(ByteArray data){
        lock(_lock){
            base.Enqueue(data);
        }
    }

    public new ByteArray Peek(){
        ByteArray v = null;
        lock (_lock){
            if(Count>0)
                v = base.Peek();
            else
                v = null;
        }
        return v;
    }

    public new ByteArray Dequeue(){
        ByteArray v;
        lock(_lock){
            v = base.Dequeue();
        }
        return v;
    }
}