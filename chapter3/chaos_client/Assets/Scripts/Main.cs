using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public static NetMgr Net { get; private set; }
    public static Dictionary<string, SyncActor> Ghosts { get; set; }
    // Start is called before the first frame update
    void Awake()
    {
        Net = new NetMgr();
        Ghosts = new Dictionary<string, SyncActor>();
        Net.Connect("127.0.0.1", 8888);
        EnterGame();
    }

    private void Update()
    {
        Net.Update();
    }

    private void OnDestroy()
    {
        Proto.CallLeave();
        Net.Close();
    }

    public static void EnterGame()
    {
        var go = Instantiate(Resources.Load<GameObject>("Prefabs/mon_goblinWizard"));
        go.transform.position = new Vector3(Random.Range(-5f, 5f), 0.1f, Random.Range(-5f, 5f));
        go.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
        go.AddComponent<SelfActor>();
        Proto.CallEnter(go.transform.position,go.transform.localEulerAngles.y);
    }

    public static void NewGhost(string key,Vector3 pos,float yEuler)
    {
        var go = Instantiate(Resources.Load<GameObject>("Prefabs/mon_goblinWizard"));
        go.transform.position = pos;
        go.transform.eulerAngles = new Vector3(0, yEuler, 0);
        var actor = go.AddComponent<SyncActor>();
        Ghosts.Add(key, actor);
    }

    public static void LeaveGhost(string id)
    {
        if (Ghosts.ContainsKey(id))
        {
            var actor = Ghosts[id];
            Ghosts.Remove(id);
            Destroy(actor.gameObject);
        }
        else
        {
            Debug.Log("找不到同步对象:" + id);
        }
    }
}
