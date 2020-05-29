using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseActor : MonoBehaviour
{
    public string ID { get; private set; }
    public float Speed { get; set; } = 2;

    private Move _moveCtrl;

    public void SetID(string id)
    {
        ID = id;
    }

    protected void Start()
    {
        _moveCtrl = gameObject.AddComponent<Move>();
        _moveCtrl.Speed = Speed;
    }

    public void MoveTo(Vector3 pos)
    {
        _moveCtrl.MoveTo(pos);
    }
}
