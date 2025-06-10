using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CaterpillarMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;
    public float obstacleCheckDist = 1f;
    public float groundHeight = 0.5f;

    public LayerMask obstacleMask;
    public TMP_Text countdownText;

    public GameObject mothPrefab;
    public GameObject butterflyPrefab;
    public GameObject beePrefab;

    private bool isMounted = false;
    private bool isApproaching = false;

    private Rigidbody rb;
    private Animator animator;

    private Coroutine approachRoutine;
    private Coroutine transformationRoutine;

    private IBugMovementStrategy walkStrategy;
    private PlayerMovement mountedPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponent<Animator>();
        walkStrategy = new WalkMovementStrategy();
    }

    void Update()
    {
        if (!isMounted) return;
    }

    void FixedUpdate()
    {
        if (!isMounted || isApproaching) return;

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            walkStrategy.HandleMovement(rb, animator, obstacleMask, moveSpeed, rotationSpeed, obstacleCheckDist);
        }
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;

        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            if (animator != null && animator.runtimeAnimatorController != null)
                animator.SetBool("is_walking", false);

            if (transformationRoutine != null)
                StopCoroutine(transformationRoutine);
        }
        else
        {
            mountedPlayer = GetComponentInChildren<PlayerMovement>();
            transformationRoutine = StartCoroutine(DelayedTransformation());
        }
    }

    private IEnumerator DelayedTransformation()
    {
        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        TransformIntoRandomBug();
    }
    private void TransformIntoRandomBug()
    {
        GameObject[] candidates = { mothPrefab, butterflyPrefab, beePrefab };
        int idx = Random.Range(0, candidates.Length);
        GameObject chosen = candidates[idx];

        GameObject newBug = Instantiate(chosen, transform.position, transform.rotation);

        mountedPlayer?.Mount(newBug.transform);

        Destroy(gameObject); // 애벌레 제거
    }


    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

    private IEnumerator MoveToTarget(Vector3 target)
    {
        SetMounted(false);
        isApproaching = true;

        while (Vector3.Distance(transform.position, target) > 1.5f)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 next = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
            next.y = groundHeight;
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
            if (animator != null && animator.runtimeAnimatorController != null)
                animator.SetTrigger("is_drop");

            var player = GetComponentInChildren<PlayerMovement>();
            player?.ForceFallFromBug();
            SetMounted(false);
        }
    }
}
