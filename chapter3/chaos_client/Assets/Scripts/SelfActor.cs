using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfActor : BaseActor
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray,out hit) && hit.collider.tag == "Ground")
            {
                var pos = hit.point + Vector3.up * 0.1f;
                MoveTo(pos);
                Proto.CallMove(pos);
            }
        }
    }
}
