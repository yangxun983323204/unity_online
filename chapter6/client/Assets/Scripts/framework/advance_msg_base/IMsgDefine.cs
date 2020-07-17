using System.Collections;
using System.Collections.Generic;

public interface IMsgDefine
{
    short GetID(string name);
    string GetName(short id);
}
