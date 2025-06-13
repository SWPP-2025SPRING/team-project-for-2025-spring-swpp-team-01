using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class LadybugMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 4f;
    public float rotationSpeed = 180f;
    public float obstacleCheckDist = 0.8f;
    public float flightHeight = 2f;

    public LayerMask obstacleMask;
    public GameObject FlyUI;
    public TMP_Text countdownText;

    private bool isMounted = false;
    private bool isApproaching = false;

    private Rigidbody rb;
    private Animator animator;

    private Coroutine approachRoutine;

    private IBugMovementStrategy walkStrategy;
    private FlyMovementStrategy flyStrategy;


    public AudioSource audioSource;
    public AudioConfig walkAudio;
    public AudioConfig flyAudio;
    public AudioConfig stunAudio;
    public AudioConfig dropAudio;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponent<Animator>();
        walkStrategy = new WalkMovementStrategy();
        flyStrategy = new FlyMovementStrategy(this, countdownText, FlyUI, rb, animator);

        // FlyUI?.SetActive(false);
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

        // Handle walking
        walkStrategy.HandleMovement(rb, animator, obstacleMask, moveSpeed, rotationSpeed, obstacleCheckDist);

        // Handle flight
        flyStrategy.HandleMovement(rb, animator, obstacleMask, moveSpeed, rotationSpeed, obstacleCheckDist);
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;

        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            animator?.SetBool("is_walking", false);
            FlyUI?.SetActive(false);
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
        if (!isMounted) return;

        if (col.gameObject.CompareTag("Obstacle"))
        {
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
    
    private void PlaySound(AudioConfig config, bool loop = false)
    {
        if (config == null || config.clip == null || audioSource == null) return;

        audioSource.loop = false;  // Unity 기본 loop는 쓰지 않는다
        audioSource.clip = config.clip;
        audioSource.volume = config.volume;
        audioSource.pitch = config.pitch;
        audioSource.time = config.startTime;
        audioSource.Play();

        float duration = (config.endTime > 0)
            ? Mathf.Clamp(config.endTime - config.startTime, 0f, config.clip.length - config.startTime)
            : config.clip.length - config.startTime;

        if (loop)
        {
            StartCoroutine(CustomLoop(config, duration));
        }
        else
        {
            StartCoroutine(StopAfter(duration));
        }
    }

    private IEnumerator CustomLoop(AudioConfig config, float duration)
    {
        while (true)
        {
            yield return new WaitForSeconds(duration);
            audioSource.time = config.startTime;
            audioSource.Play();
        }
    }

    private IEnumerator StopAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        audioSource.Stop();
    }
}
