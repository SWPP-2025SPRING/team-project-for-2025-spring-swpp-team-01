using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadybugMovement : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 120f;
    public float flightHeight = 2f;
    public float flightDuration = 5f;


    public float obstacleCheckDist = 0.8f;
    public LayerMask obstacleMask = ~0;

    private bool isMounted = false;
    private bool isFlying = false;
    private bool canFly = true;
    private bool isApproaching;

    private Rigidbody rb;
    private Animator animator;

    private Vector3 cachedInput = Vector3.zero;
    Coroutine approachRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        cachedInput = new Vector3(h, 0, v);

        bool isWalking = Mathf.Abs(v) > 0.01f;
        animator?.SetBool("is_walking", isWalking);

        if (Input.GetKey(KeyCode.Space) && canFly && !isFlying)
        {
            StartCoroutine(StartFlying());
        }

        if (Input.GetKeyUp(KeyCode.Space) && isFlying)
        {
            StartCoroutine(StartLanding());
        }
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        float h = cachedInput.x;
        float v = cachedInput.z;

        if (Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.6f;

            if (Physics.SphereCast(rayOrigin, 0.4f, forward, out RaycastHit hit, obstacleCheckDist, obstacleMask))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);
                if (angle > 85f) return; // 경사면은 통과, 거의 수직 벽만 막음
            }

            Vector3 next = rb.position + forward * v * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(next);
        }
    }
    
    IEnumerator StartFlying()
    {
        isFlying = true;
        canFly = false;
        rb.useGravity = false;

        animator?.SetTrigger("is_ascend");
        animator?.SetBool("is_flying", true);

        float ascendSpeed = 3f;
        float targetY = flightHeight;

        while (rb.position.y < targetY - 0.05f)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, ascendSpeed * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        while (Input.GetKey(KeyCode.Space))
        {
            Vector3 hover = rb.position;
            hover.y = targetY;
            rb.MovePosition(hover);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator StartLanding()
    {
        animator?.SetTrigger("is_descend");
        animator?.SetBool("is_flying", false);

        float descendSpeed = 2f;
        float groundY = rb.position.y - 1f;

        if (Physics.Raycast(rb.position + Vector3.up * 0.5f, Vector3.down, out var hit, 10f, obstacleMask))
        {
            groundY = hit.point.y;
        }

        while (rb.position.y > groundY + 0.05f)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, groundY, descendSpeed * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(new Vector3(rb.position.x, groundY, rb.position.z));
        rb.useGravity = true;
        isFlying = false;
        canFly = true;
        animator?.SetTrigger("descend_to_walk");
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            cachedInput = Vector3.zero;
            animator?.SetBool("is_walking", false);
        }
    }

    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

    IEnumerator MoveToTarget(Vector3 target)
    {
        isApproaching = true;
        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 next = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
            next.y = flightHeight;
            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log($"[BugFlight] hit {col.gameObject.name}");

        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        animator.SetTrigger("is_drop");

        var player = GetComponentInChildren<PlayerMovement>();
        player?.ForceFallFromBug();

        SetMounted(false);
    }
}
