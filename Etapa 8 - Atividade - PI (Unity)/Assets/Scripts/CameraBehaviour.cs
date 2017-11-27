using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    public float smoothSpeed = 0.0f;
    public Transform target = null;
    public Transform targetMenu = null;
    public Vector3 offset = Vector3.zero;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera has no target to follow.");
        }
    }

    private void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        if (GameControllerBehaviour.gameControllerInstance.GetGameState() == 1)
        {
            transform.eulerAngles = new Vector3(45.0f, 0.0f, 0.0f);
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            transform.position = smoothedPosition;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, targetMenu.position.y + 10, targetMenu.position.z + -15);
            transform.Translate(Vector3.right * 2 * Time.deltaTime);
            transform.LookAt(targetMenu);
        }
    }
}
