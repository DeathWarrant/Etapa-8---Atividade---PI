using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public float yAxisOffset = 0.0f;
    public float zAxisOffset = 0.0f;
    public GameObject target = null;

    private void Update()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (target != null)
        {
            Vector3 positionVector = new Vector3(target.transform.position.x, target.transform.position.y + yAxisOffset, target.transform.position.z + zAxisOffset);

            transform.position = positionVector;
        }
        else
        {

        }
    }
}
