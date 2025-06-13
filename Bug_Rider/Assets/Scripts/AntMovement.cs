using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class AntMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.4f;
    public float dashCooldown = 1f;
    public float obstacleCheckDist = 0.8f;

    public LayerMask obstacleMask;
    public GameObject DashUI;
    public TMP_Text countdownText;

    private bool isMounted = false;
    private bool isApproaching = false;
    private bool isDashing = false;
    private bool canDash = true;

    private Rigidbody rb;
    private Animator antAnimator;
    private Coroutine approachRoutine;

    private IBugMovementStrategy walkStrategy;

    public AudioSource audioSource;

    public AudioConfig walkAudio;
    public AudioConfig dashAudio;
    public AudioConfig stunAudio;
    public AudioConfig dropAudio;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        antAnimator = GetComponent<Animator>();
        walkStrategy = new WalkMovementStrategy();

        // Set AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (!isMounted || isDashing || !canDash) return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(Dash());
            PlaySound(dashAudio); // Play audio loop
        }
    }

    void FixedUpdate()
    {
        if (!isMounted || isApproaching || isDashing) return;

        walkStrategy.HandleMovement(rb, antAnimator, obstacleMask, moveSpeed, rotationSpeed, obstacleCheckDist);

        if (!audioSource.isPlaying || audioSource.clip != walkAudio.clip)
        {
            PlaySound(walkAudio, true);  // loop 걷는 소리
        }
    }

    public IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;
        DashUI?.SetActive(false);

        antAnimator?.SetTrigger("is_dashing");

        Vector3 dashDir = rb.rotation * Vector3.forward;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            Vector3 next = rb.position + dashDir * dashSpeed * Time.fixedDeltaTime;
            rb.MovePosition(next);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        float countdown = dashCooldown;
        while (countdown > 0 && isMounted)
        {
            countdownText.text = $"You can dash after {Mathf.Ceil(countdown)}s";
            countdown -= Time.deltaTime;
            yield return null;
        }

        countdownText.text = "";
        DashUI?.SetActive(isMounted);
        canDash = true;
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            DashUI?.SetActive(false);
            countdownText.text = "";
            PlaySound(dropAudio, false);
        }
        else
        {
            DashUI?.SetActive(true);
            Destroy(GetComponent<MoveToTarget>());
        }
    }

    public void ApproachTo(Vector3 target)
    {
        if (approachRoutine != null) StopCoroutine(approachRoutine);
        approachRoutine = StartCoroutine(MoveToTarget(target));
    }

    public IEnumerator MoveToTarget(Vector3 target)
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

    public void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        var player = GetComponentInChildren<PlayerMovement>();
        antAnimator?.SetTrigger("is_dropping");
        player?.ForceFallFromBug();
        PlaySound(stunAudio);  // Play audio once
        SetMounted(false);
        Destroy(gameObject, 2f);
    }

    public void SetUI(GameObject dashUI, TMP_Text countdown)
    {
        DashUI = dashUI;
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
