using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MothMovement : MonoBehaviour, IRideableBug
{
    public float moveSpeed = 5f;
    public float flightHeight = 3f;

    public GameObject FlyUI;
    public TMP_Text countdownText;

    private bool isMounted = false;
    private Rigidbody rb;
    private Animator animator;

    private FlyMovementStrategy flyStrategy;
    private PlayerMovement mountedPlayer;

    public AudioSource audioSource;
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

        flyStrategy = new FlyMovementStrategy(this, countdownText, FlyUI, rb, animator, true);

        FlyUI?.SetActive(false);
    }

    public void SetMounted(bool mounted)
    {
        isMounted = mounted;

        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            animator?.SetBool("is_flying", false);
            FlyUI?.SetActive(false);
            PlaySound(dropAudio);
        }
        else
        {
            FlyUI?.SetActive(true);
            mountedPlayer = GetComponentInChildren<PlayerMovement>();

            // 비행 애니메이션 시작
            animator?.SetBool("is_flying", true);
            flyStrategy.StartFlight();
            PlaySound(flyAudio);
            StartCoroutine(AutoFlyBackwardRoutine());
        }
    }

    private IEnumerator AutoFlyBackwardRoutine()
    {
        float duration = 5f;
        float timer = 0f;

        while (timer < duration)
        {
            Vector3 backward = -transform.forward;
            Vector3 nextPos = rb.position + backward * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);

            timer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 비행 종료 애니메이션
        animator?.SetBool("is_flying", false);
        animator?.SetTrigger("is_drop");

        // 강제 낙하
        mountedPlayer?.ForceFallFromBug();
    }

    public void ApproachTo(Vector3 target)
    {
        // Moth는 자동 접근 안 씀
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
            PlaySound(stunAudio);
        }
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
