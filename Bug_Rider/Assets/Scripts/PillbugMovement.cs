using System.Collections;
using UnityEngine;
public class PillbugMovement : MonoBehaviour, IRideableBug
{
    public float baseSpeed = 2f;
    public float maxSpeed = 15f;
    public float decayRate = 1.2f;
    public float accelerationRate = 2f;
    public float rotationSpeed = 180f;
    public float obstacleCheckDist = 0.8f;
    public float moveSpeed = 3f;
    public float impulseStrength = 15f;
    public LayerMask obstacleMask;
    private float currentSpeed;
    private float speedTime = 0f;
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
        speedTime = 0f;
        rb.centerOfMass = Vector3.zero;
    }
    void FixedUpdate()
    {
        if (!isMounted) return;
        float h = Input.GetAxis("Horizontal");
        if (Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
        Vector3 forward = rb.rotation * Vector3.forward;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        if (!Physics.SphereCast(rayOrigin, 0.4f, forward, out RaycastHit hit, obstacleCheckDist, obstacleMask))
        {
            speedTime += Time.fixedDeltaTime;
            currentSpeed = maxSpeed * (1f - Mathf.Exp(-decayRate * speedTime));
            rb.AddForce(forward * currentSpeed, ForceMode.Acceleration);
        }
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limited = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limited.x, 0f, limited.z);
        }
        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
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
            speedTime = 0f;
            animator.SetBool("is_rolling", false);
        }
        else
        {
            currentSpeed = baseSpeed;
            speedTime = 0f;
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
}