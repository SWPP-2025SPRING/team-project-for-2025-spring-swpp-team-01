using UnityEngine;
using System.Collections;

public class BugFlightMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    public bool isMounted = false;
    private float flightHeight = 2f;

    void Update()
    {
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 move = transform.forward * moveSpeed * Time.deltaTime;

            Vector3 nextPos = transform.position + move;
            nextPos.y = flightHeight;
            transform.position = nextPos;
        }
        else
        {
            Vector3 pos = transform.position;
            pos.y = flightHeight;
            transform.position = pos;
        }
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
    }
    
    public void ApproachTo(Vector3 target)
    {
        StartCoroutine(MoveToTarget(target));
    }

    private IEnumerator MoveToTarget(Vector3 target)
    {
        isMounted = false;

        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 move = dir * moveSpeed * Time.deltaTime;
            Vector3 newPos = transform.position + move;
            newPos.y = flightHeight;
            transform.position = newPos;
            yield return null;
        }

        Debug.Log("비행 벌레 도착 완료");
    }
}
