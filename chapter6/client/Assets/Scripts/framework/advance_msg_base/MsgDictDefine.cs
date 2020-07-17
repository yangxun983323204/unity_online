using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MsgDictDefine : IMsgDefine
{
    private static List<string> _nameList = new List<string>();
    
    public static void Reg(params string[] names)
    {
        _nameList.Clear();
        foreach (var name in names)
        {
            if (!_nameList.Contains(name))
            {
                _nameList.Add(name);
            }
        }

        _nameList.Sort();
    }

    public static void LoadLines(string lines)
    {
        _nameList.Clear();
        StringReader sr = new StringReader(lines);
        string name = sr.ReadLine();
        while (!string.IsNullOrEmpty(name))
        {
            if (!_nameList.Contains(name))
            {
                _nameList.Add(name);
            }
            name = sr.ReadLine();
        }

        _nameList.Sort();
    }

    public static MsgDictDefine Instance
    {
        get
        {
            if (_inst == null) _inst = new MsgDictDefine();

            return _inst;
        }
    }

    private static MsgDictDefine _inst;

    private MsgDictDefine() { }

    public short GetID(string name)
    {
        var id = (short)_nameList.IndexOf(name);
        if (id < 0)
            throw new KeyNotFoundException(name);

        return id;
    }

    public string GetName(short id)
    {
        return _nameList[id];
    }
}
