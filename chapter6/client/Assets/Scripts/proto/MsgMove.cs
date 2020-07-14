using System.Collections;
using System.Collections.Generic;

public class MsgMove : MsgBase
{
    public override string ProtoName => "MsgMove";

    public float X = 0;
    public float Y = 0;
    public float Z = 0;
}
