using UnityEngine;
using System.Collections;

public class BugMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public bool isMounted = false;

    private float turnSmoothVelocity;
    public float rotationSmoothTime = 0.1f;

    private bool isApproaching = false;

    void Update()
    {
        if (isApproaching || !isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 move = transform.forward * moveSpeed * Time.deltaTime;
            transform.position += move;
        }
    }

    public void ApproachTo(Vector3 target)
    {
        StartCoroutine(MoveToTarget(target));
    }

    private IEnumerator MoveToTarget(Vector3 target)
    {
        isMounted = false;
        isApproaching = true;

        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            yield return null;
        }

        Debug.Log("벌레 도착 완료, 탑승 가능");
        isApproaching = false;
    }


    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
    }
}
