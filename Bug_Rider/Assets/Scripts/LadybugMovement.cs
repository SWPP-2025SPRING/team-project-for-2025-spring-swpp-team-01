using UnityEngine;
using TMPro;
[RequireComponent(typeof(Rigidbody))]
public class LadybugMovement : MonoBehaviour, IRideableBug
{
    public float acceleration = 5f;
    public float maxSpeed = 4f;
    public float angularAcceleration = 1200f;
    public float maxAngularSpeed = 90f;
    public float obstacleCheckDist = 0.8f;
    public float rotationSpeed = 90f; // 필요 시 Inspector에서 수정
    public LayerMask obstacleMask;
    public GameObject FlyUI;
    public TMP_Text countdownText;
    private bool isMounted = false;
    private bool isApproaching = false;
    private Rigidbody rb;
    private Animator animator;
    private Coroutine approachRoutine;
    private WalkMovementStrategy walkStrategy;
    private FlyMovementStrategy flyStrategy;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        animator = GetComponent<Animator>();
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
        flyStrategy = new FlyMovementStrategy(this, countdownText, FlyUI, rb, animator);
    }
    void Update()
    {
        if (!isMounted) return;
        if (Input.GetKeyDown(KeyCode.Space) && flyStrategy.CanFly)
        {
            flyStrategy.StartFlight();
        }
    }
    void FixedUpdate()
    {
        if (!isMounted || isApproaching) return;
        // 걷기(가속도/각가속도 적용)
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        walkStrategy.HandleMovement(h, v);
        // 비행 핸들링(이 부분 유지)
        flyStrategy.HandleMovement(rb, animator, obstacleMask, maxSpeed, rotationSpeed, obstacleCheckDist);
    }
    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            flyStrategy.StopFlight();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            animator?.SetBool("is_walking", false);
            FlyUI?.SetActive(false);
            countdownText.text = "";
        }
        else
        {
            FlyUI?.SetActive(true);
            Destroy(GetComponent<MoveToTarget>());
        }
    }
    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }
    private System.Collections.IEnumerator MoveToTarget(Vector3 target)
    {
        SetMounted(false);
        isApproaching = true;
        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 next = rb.position + dir * maxSpeed * Time.fixedDeltaTime;
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
        if (col.gameObject.CompareTag("Obstacle"))
        {
            this.tag = "Wall"; // 다시 mount 방지
            animator?.SetTrigger("is_drop");
            flyStrategy.StopFlight();
            FlyUI?.SetActive(false);
            var player = GetComponentInChildren<PlayerMovement>();
            player?.ForceFallFromBug();
            SetMounted(false);
            Destroy(gameObject, 2f);
        }
    }
    public void SetUI(GameObject flyUI, TMP_Text countdown)
    {
        FlyUI = flyUI;
        countdownText = countdown;
    }
}