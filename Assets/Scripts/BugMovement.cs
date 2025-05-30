using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class BugMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSmoothTime = 0.1f;
    public float obstacleCheckDist = 0.8f;
    public LayerMask obstacleMask;
    bool isMounted;
    bool isApproaching;
    Rigidbody rb;
    float turnSmoothVelocity;
    Vector3 cachedInput = Vector3.zero;
    Coroutine approachRoutine;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                                     RigidbodyConstraints.FreezeRotationZ;
    }
    void Update()
    {
        if (!isMounted) return;
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        cachedInput = new Vector3(h, 0, v).normalized;
    }
    void FixedUpdate()
    {
        if (!isMounted) return;
        if (cachedInput.sqrMagnitude < 0.01f) return;
        float targetAngle = Mathf.Atan2(cachedInput.x, cachedInput.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y,
                                            targetAngle,
                                            ref turnSmoothVelocity,
                                            rotationSmoothTime);
        rb.MoveRotation(Quaternion.Euler(0, angle, 0));
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        if (Physics.SphereCast(rayOrigin, 0.4f, transform.forward,
                               out _, obstacleCheckDist, obstacleMask))
        {
            return;
        }
        Vector3 next = rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(next);
    }
    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            // 바로 멈추도록 속도 초기화
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }
    IEnumerator MoveToTarget(Vector3 target)
    {
        SetMounted(false);
        isApproaching = true;
        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 next = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }
        rb.velocity = rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }
    void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;
        var player = GetComponentInChildren<PlayerMovement>();
        player?.ForceFallFromBug();
        SetMounted(false);
    }
}
