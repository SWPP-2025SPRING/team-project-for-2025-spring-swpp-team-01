using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillbugMovement : MonoBehaviour, IRideableBug
{
    public float baseSpeed = 2f;
    public float accelerationRate = 2f;
    public float rotationSpeed = 180f;
    public float obstacleCheckDist = 0.8f;
    public float moveSpeed = 3f; // ApproachTo 이동 속도용
    public LayerMask obstacleMask;

    private float currentSpeed;
    private bool isMounted = false;
    private bool isApproaching = false;

    private Rigidbody rb;
    private Animator animator;
    private Coroutine approachRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        animator = GetComponent<Animator>();
        currentSpeed = baseSpeed;
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        // 회전
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // 전진
        Vector3 forward = rb.rotation * Vector3.forward;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;

        if (!Physics.SphereCast(rayOrigin, 0.4f, forward, out RaycastHit hit, obstacleCheckDist, obstacleMask))
        {
            currentSpeed += accelerationRate * Time.fixedDeltaTime; // 무제한 가속
            Vector3 next = rb.position + forward * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(next);
        }

        animator.SetBool("is_rolling", true);
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;

        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            currentSpeed = baseSpeed;
            animator.SetBool("is_rolling", false);
        }
        else
        {
            currentSpeed = baseSpeed;
            animator.SetBool("is_rolling", true);
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

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        var player = GetComponentInChildren<PlayerMovement>();
        animator?.SetTrigger("is_dropping");
        Debug.Log("is_dropping triggered");

        player?.ForceFallFromBug();
        SetMounted(false);
    }
}
