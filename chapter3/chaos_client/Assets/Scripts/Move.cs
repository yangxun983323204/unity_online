using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public float Speed { get; set; } = 2;
    private Vector3 _targetPos;
    private bool _moveing = false;
    private Animation _anim;

    private void OnEnable()
    {
        _anim = gameObject.GetComponent<Animation>();
        PlayAnim("Idle");
    }

    private void nDisable()
    {
        PlayAnim("Idle");
    }

    public void MoveTo(Vector3 pos)
    {
        _targetPos = pos;
        _moveing = true;
        PlayAnim("Run");
    }

    private void Update()
    {
        if (!_moveing)
            return;

        transform.position = Vector3.MoveTowards(transform.position, _targetPos, Time.deltaTime*Speed);
        transform.LookAt(_targetPos);
        if (Vector3.Distance(transform.position,_targetPos)<0.05f)
        {
            _moveing = false;
            PlayAnim("Idle");
        }
    }

    private void PlayAnim(string name)
    {
        if (!_anim.IsPlaying(name))
        {
            _anim.Play(name);
        }
    }
}
