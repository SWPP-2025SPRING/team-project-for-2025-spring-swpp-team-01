using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, 3);
    // Start is called before the first frame update
    void Start()
    {
        transform.position = target.position + target.rotation * offset;
        transform.LookAt(target);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + target.rotation * offset;
        transform.LookAt(target);
    }
}
