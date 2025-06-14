using UnityEngine;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    public float acceleration = 20f;
    public float maxSpeed = 3f;
    public float angularAcceleration = 200f;
    public float maxAngularSpeed = 180f;
    public float obstacleCheckDist = 0.8f;
    public LayerMask obstacleMask;
    private Rigidbody rb;
    private Animator animator;
    public bool isMounted = false;
    public Transform mountedBug;
    private bool isFalling = false;
    private Vector3 velocity;
    private Vector3 mountedLocalPos;
    private Quaternion mountedLocalRot;
    // 추가: walk 전략
    private WalkMovementStrategy walkStrategy;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;
        rb.isKinematic = false;
        walkStrategy = new WalkMovementStrategy(
            rb,
            animator,
            obstacleMask,
            acceleration,
            maxSpeed,
            angularAcceleration,
            maxAngularSpeed,
            obstacleCheckDist
        );
    }
    void Update()
    {
        if (isMounted)
        {
            if (Input.GetKeyDown(KeyCode.E)) Unmount();
            return;
        }
        if (Input.GetKeyDown(KeyCode.E)) TryMount();
    }
    void FixedUpdate()
    {
        if (isMounted || isFalling) return;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        walkStrategy.HandleMovement(x, z);
    }
    void LateUpdate()
    {
        if (isMounted && mountedBug != null)
        {
            transform.localPosition = mountedLocalPos;
            transform.localRotation = mountedLocalRot;
        }
    }
    void TryCallBug()
    {
        float searchRadius = 20f;
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Bug")) continue;
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }
        if (closest && closest.TryGetComponent<IRideableBug>(out var rideable))
        {
            rideable.ApproachTo(transform.position);
        }
    }
    void TryMount()
    {
        float radius = 0.5f;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        Transform closest = null;
        float minDist = Mathf.Infinity;
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Bug")) continue;
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = hit.transform;
            }
        }
        if (closest != null)
        {
            Mount(closest);
        }
    }
    void Mount(Transform bug)
    {
        isMounted = true;
        mountedBug = bug;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        var col = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = false;
        transform.SetParent(bug);
        if (bug.TryGetComponent<LadybugMovement>(out _))
            mountedLocalPos = new Vector3(0, -0.2f, 0.5f);
        else
            mountedLocalPos = new Vector3(0, -1.2f, 0);
        mountedLocalRot = Quaternion.identity;
        transform.localPosition = mountedLocalPos;
        transform.localRotation = mountedLocalRot;
        animator.SetTrigger("is_riding");
        animator.SetBool("is_riding_on_bug", true);
        if (bug.TryGetComponent<IRideableBug>(out var rideable))
            rideable.SetMounted(true);
    }
    public void Unmount()
    {
        if (!isMounted || mountedBug == null) return;
        isMounted = false;
        Vector3 unmountDir = (-mountedBug.forward + Vector3.up).normalized;
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce(unmountDir * 4f, ForceMode.VelocityChange);
        var col = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = false;
        StartCoroutine(EnableColliderAfterDelay(col, 0.05f));
        animator.SetBool("is_riding_on_bug", false);
        if (mountedBug.TryGetComponent<IRideableBug>(out var rideable))
            rideable.SetMounted(false);
        mountedBug = null;
    }
    IEnumerator EnableColliderAfterDelay(CapsuleCollider col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null) col.enabled = true;
        Debug.Log("[Unmount] Collider enabled after delay");
    }
    void Fall()
    {
        isFalling = true;
        StartCoroutine(SimulateFall());
        Debug.Log("장애물과 충돌하여 추락");
    }
    public virtual void ForceFallFromBug()
    {
        if (isMounted)
        {
            Unmount();
        }
        Fall();
    }
    IEnumerator SimulateFall()
    {
        float fallDuration = 3f;
        float fallSpeed = -10f;
        float elapsed = 0f;
        animator.SetTrigger("is_falling");
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        RecoverFromFall();
    }
    void RecoverFromFall()
    {
        isFalling = false;
        rb.velocity = Vector3.zero;
        Debug.Log("추락 후 복구됨");
    }
}