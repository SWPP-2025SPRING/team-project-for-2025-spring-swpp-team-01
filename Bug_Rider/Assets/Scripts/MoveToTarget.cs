using UnityEngine;
public class MoveToTarget : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private float elapsedTime;
    private Animator animator;
    [Header("Oscillation Settings")]
    [Tooltip("Max possible yaw amplitude in degrees")]
    public float maxYawAngle = 45f;
    [Tooltip("Max possible oscillation frequency (Hz)")]
    public float maxOscillationFrequency = 1f;
    [Header("Randomization & Smoothing")]
    [Tooltip("How often (seconds) to pick new random yaw/frequency")]
    public float changeInterval = 1f;
    [Tooltip("How fast to smooth toward the new values (per second)")]
    public float smoothingSpeed = 1f;
    // current & target params
    private float currentYawAmp;
    private float targetYawAmp;
    private float currentFreq;
    private float targetFreq;
    private float changeTimer;
    /// <summary>
    /// Call immediately after Instantiate().
    /// </summary>
    public void Initialize(Vector3 targetPos, float moveSpeed)
    {
        targetPosition = targetPos;
        speed = moveSpeed;
        elapsedTime = 0f;
        animator = GetComponent<Animator>();
        if (animator != null)
            animator.SetBool("is_walking", true);
        // Initialize both current and target to a random start
        targetYawAmp = Random.Range(0f, maxYawAngle);
        currentYawAmp = targetYawAmp;
        targetFreq = Random.Range(0f, maxOscillationFrequency);
        currentFreq = targetFreq;
        changeTimer = changeInterval;
        // 만약 탑승중인 벌레가 아니면 일정 시간 후에 파괴


        //Destroy(gameObject, 10f);
        
    }
    void Update()
    {
        float dt = Time.deltaTime;
        elapsedTime += dt;
        // 1) Handle random target update
        changeTimer -= dt;
        if (changeTimer <= 0f)
        {
            targetYawAmp = Random.Range(0f, maxYawAngle);
            targetFreq = Random.Range(0f, maxOscillationFrequency);
            changeTimer = changeInterval;
        }
        // 2) Smoothly interpolate current toward target
        currentYawAmp = Mathf.Lerp(currentYawAmp, targetYawAmp, smoothingSpeed * dt);
        currentFreq = Mathf.Lerp(currentFreq, targetFreq, smoothingSpeed * dt);
        // 3) Compute oscillating yaw
        float yawOffset = Mathf.Sin(elapsedTime * currentFreq * 2f * Mathf.PI)
                        * currentYawAmp;
        // 4) Compute base rotation toward target
        Vector3 toTarget = (targetPosition - transform.position).normalized;
        Quaternion baseRot = Quaternion.LookRotation(toTarget);
        // 5) Apply yaw oscillation and set rotation
        transform.rotation = baseRot * Quaternion.Euler(0f, yawOffset, 0f);
        // 6) Move forward along local forward
        transform.position += transform.forward * speed * dt;
    }
}