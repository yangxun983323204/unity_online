using System.Collections;
using System.Collections.Generic;

public abstract class JsonMsgBase
{
    public virtual string ProtoName { get { return this.GetType().ToString(); } }
}