using UnityEngine;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    public float acceleration = 12f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 45f;
    public float angularAcceleration = 200f;
    public float maxAngularSpeed = 180f;
    private float currentAngularSpeed = 0f;
    private Rigidbody rb;
    private Animator animator;
    public bool isMounted = false;
    public Transform mountedBug;
    private bool isFalling = false;
    private Vector3 velocity; // 낙하 등 임시 사용
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true; // X, Z축 회전 고정(넘어짐 방지)
        rb.isKinematic = false;
    }
    void FixedUpdate()
    {
        // 탑승 중이면 이동·회전·힘 적용 모두 중지 (벌레 오브젝트가 조종)
        if (isMounted)
        {
            if (Input.GetKeyDown(KeyCode.E)) Unmount();
            return;
        }
        //
        if (Input.GetKeyDown(KeyCode.E)) TryMount();
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        // 회전(A/D)
        if (Mathf.Abs(x) > 0.1f)
            transform.Rotate(0f, x * rotationSpeed * Time.fixedDeltaTime, 0f);
        // 가속(W/S)
        if (Mathf.Abs(z) > 0.1f)
            rb.AddForce(transform.forward * z * acceleration, ForceMode.Acceleration);
        // 최대 속도 제한(수평 성분만)
        Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limited = flatVel.normalized * maxSpeed;
            rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
        }
        // 애니메이터 처리
        animator.SetBool("is_running", flatVel.magnitude > 0.1f);
        //if (Input.GetKeyDown(KeyCode.F)) TryCallBug();
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
        float radius = 3f;
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
        // Rigidbody 움직임 멈추고, 물리엔진에서 제외
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        // Collider 비활성화
        var col = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = false;
        // 상태 확인 로그
        Debug.Log($"[Mount] isKinematic: {rb.isKinematic}, Collider.enabled: {col?.enabled}");
        transform.SetParent(bug);
        if (bug.TryGetComponent<LadybugMovement>(out _))
            transform.localPosition = new Vector3(0, -0.2f, 0.5f);
        else
            transform.localPosition = new Vector3(0, -1.2f, 0);
        transform.rotation = bug.rotation;
        animator.SetTrigger("is_riding");
        animator.SetBool("is_riding_on_bug", true);
        if (bug.TryGetComponent<IRideableBug>(out var rideable))
            rideable.SetMounted(true);
    }
    public void Unmount()
    {
        if (!isMounted || mountedBug == null) return;
        isMounted = false;
        // Rigidbody를 다시 물리엔진에 포함
        transform.SetParent(null);
        rb.isKinematic = false;
        // 약간 위/뒤로 힘을 줘서 튕겨 나가게 함
        Vector3 unmountDir = (-mountedBug.forward + Vector3.up).normalized;
        rb.AddForce(unmountDir * 4f, ForceMode.VelocityChange);
        // Collider 활성화(0.1초 후)
        var col = GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = false;
        StartCoroutine(EnableColliderAfterDelay(col, 0.05f));
        // animator
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
            // 탑승 해제 먼저 수행
            Unmount();
        }
        Fall(); // 기존 추락 로직 실행
    }
    IEnumerator SimulateFall()
    {
        float fallDuration = 2f;
        float fallSpeed = -10f;
        float elapsed = 0f;
        animator.SetTrigger("is_falling");
        // 낙하시 수직속도만 직접 제어(물리엔진 우선)
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (!isFalling) Fall();
        }
    }
}