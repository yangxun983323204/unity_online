using System;
using System.Collections.Generic;

public class WriteQueue:Queue<ByteArray>{

    object _lock = new object();
    public ByteArray EnqueueFrom(byte[] data){
        lock(_lock){
            var ba = new ByteArray(data);
            Enqueue(ba);
            return ba;
        }
    }

    public new void Enqueue(ByteArray data){
        lock(_lock){
            base.Enqueue(data);
        }
    }

    public new ByteArray Peek(){
        lock(_lock){
            if(Count>0)
                return base.Peek();
            else
                return null;
        }
    }

    public new ByteArray Dequeue(){
        lock(_lock){
            return base.Dequeue();
        }
    }
}