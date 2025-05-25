using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadybugMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 180f;
    public float flightHeight = 2f;
    public float flightDuration = 5f;

    public float obstacleCheckDist = 0.8f;
    public LayerMask obstacleMask;

    private bool isMounted = false;
    private bool isFlying = false;
    private bool canFly = true;
    bool isApproaching;

    private Rigidbody rb;
    private Animator animator;

    private Vector3 cachedInput = Vector3.zero;

    Coroutine approachRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; // 시작 시 중력 O
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

        // 전진 애니메이션 설정
        bool isWalking = Mathf.Abs(v) > 0.01f;
        animator?.SetBool("is_walking", isWalking);

        // 비행 입력
        if (Input.GetKeyDown(KeyCode.Space) && canFly)
        {
            StartCoroutine(HandleFlight());
        }
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        float h = cachedInput.x;
        float v = cachedInput.z;

        // 회전: 전진 중일 때만
        if (Mathf.Abs(v) > 0.01f && Mathf.Abs(h) > 0.01f)
        {
            float turnAmount = h * rotationSpeed * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, turnAmount, 0);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }

        // 전진
        if (Mathf.Abs(v) > 0.01f)
        {
            Vector3 forward = rb.rotation * Vector3.forward;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;

            if (!Physics.SphereCast(rayOrigin, 0.4f, forward, out _, obstacleCheckDist, obstacleMask))
            {
                Vector3 next = rb.position + forward * v * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(next);
            }
        }
    }

    IEnumerator HandleFlight()
    {
        float groundY = rb.position.y;
        isFlying = true;
        canFly = false;

        animator?.SetTrigger("is_ascend");
        rb.useGravity = false;

        float ascendSpeed = 3f;
        while (rb.position.y < flightHeight - 0.05f)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, flightHeight, ascendSpeed * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        
        yield return new WaitForSeconds(flightDuration);

        animator?.SetTrigger("is_descend");
        float descendSpeed = 2f;
        while (rb.position.y > groundY)
        {
            Vector3 pos = rb.position;
            pos.y = Mathf.MoveTowards(pos.y, groundY, descendSpeed * Time.fixedDeltaTime);
            rb.MovePosition(pos);
            yield return new WaitForFixedUpdate();
        }

        
        rb.useGravity = true;
        isFlying = false;
        animator?.SetTrigger("descend_to_walk");

        yield return new WaitForSeconds(7f);
        canFly = true;
        Debug.Log("Flight ended → canFly = true");

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
        isMounted = false;
        isApproaching = true;
        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 next = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
            next.y = flightHeight;
            rb.MovePosition(next);
            yield return new WaitForFixedUpdate();
        }
        rb.velocity = rb.angularVelocity = Vector3.zero;
        isApproaching = false;
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log($"[BugFlight] hit {col.gameObject.name}");
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;
        var player = GetComponentInChildren<PlayerMovement>();
        animator.SetTrigger("is_drop");
        player?.ForceFallFromBug();
        SetMounted(false);
    }
}
