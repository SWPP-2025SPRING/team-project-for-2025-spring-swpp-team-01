using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -4); // 뒤쪽 offset
    public Vector3 lookat_offset = new Vector3(0, 0, 0); // 뒤쪽 offset
    private Vector3 camera_aim;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        
        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        camera_aim = target.position;
        camera_aim = camera_aim + lookat_offset;
        transform.LookAt(camera_aim);
    }
}

