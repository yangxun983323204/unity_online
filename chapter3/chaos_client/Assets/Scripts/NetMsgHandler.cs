using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMsgHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Main.Net.AddListener("Enter", OnEnter);
        Main.Net.AddListener("Move", OnMove);
        Main.Net.AddListener("Leave", OnLeave);
        Main.Net.AddListener("List", OnList);
        Main.Net.AddListener("Close", OnClose);
    }

    private void OnClose(string obj)
    {
        Debug.Log("OnClose");
    }

    private void OnLeave(string obj)
    {
        Debug.Log("OnLeave:" + obj);
        var splits = obj.Split(',');
        var id = splits[0];
        Main.LeaveGhost(id);
    }

    private void OnMove(string obj)
    {
        Debug.Log("OnMove:" + obj);
        var splits = obj.Split(',');
        var id = splits[0];
        var x = float.Parse(splits[1]);
        var y = float.Parse(splits[2]);
        var z = float.Parse(splits[3]);
        if (Main.Ghosts.ContainsKey(id))
        {
            var actor = Main.Ghosts[id];
            actor.MoveTo(new Vector3(x, y, z));
        }
        else
        {
            Debug.Log("找不到同步对象:"+id);
        }
    }

    private void OnEnter(string obj)
    {
        Debug.Log("OnEnter:" + obj);
        var splits = obj.Split(',');
        var id = splits[0];
        Debug.Log($"{id},{Main.Net.GetID()}");
        if (id == Main.Net.GetID())
        {
            Proto.CallList();
            return;
        }
        var x = float.Parse(splits[1]);
        var y = float.Parse(splits[2]);
        var z = float.Parse(splits[3]);
        var yEuler = float.Parse(splits[4]);
        Main.NewGhost(id, new Vector3(x, y, z), yEuler);
    }

    private void OnList(string obj)
    {
        Debug.Log("OnList:" + obj);
        var splits = obj.Split('&');
        foreach (var msg in splits)
        {
            var splits2 = msg.Split(',');
            var id = splits2[0];
            Debug.Log($"{id},{Main.Net.GetID()}");
            if (id == Main.Net.GetID())
            {
                continue;
            }
            var x = float.Parse(splits2[1]);
            var y = float.Parse(splits2[2]);
            var z = float.Parse(splits2[3]);
            var yEuler = float.Parse(splits2[4]);
            Main.NewGhost(id, new Vector3(x, y, z), yEuler);
        }
    }
}
