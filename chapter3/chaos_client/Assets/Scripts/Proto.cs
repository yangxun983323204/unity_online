using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Proto
{
    public static void CallEnter(Vector3 pos,float yEuler)
    {
        Main.Net.Send("Enter|" + $"{Main.Net.GetID()},{pos.x},{pos.y},{pos.z},{yEuler}");
    }

    public static void CallMove(Vector3 pos)
    {
        Main.Net.Send("Move|" + $"{Main.Net.GetID()},{pos.x},{pos.y},{pos.z}");
    }

    public static void CallLeave()
    {
        Main.Net.Send("Leave|" + $"{Main.Net.GetID()}");
    }

    public static void CallList()
    {
        Main.Net.Send("List|" + $"{Main.Net.GetID()}");
    }
}
