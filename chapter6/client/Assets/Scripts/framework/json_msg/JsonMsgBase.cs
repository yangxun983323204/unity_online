using System.Collections;
using System.Collections.Generic;

namespace JsonMsg
{
    public abstract class JsonMsgBase
    {
        public virtual string ProtoName { get { return this.GetType().ToString(); } }
    }
}