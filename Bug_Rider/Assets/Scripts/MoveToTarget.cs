using UnityEngine;
using System.Collections;
public class MoveToTarget : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private float elapsedTime;
    private Animator animator;
    private bool isMounted = false;
    private Coroutine selfDestructRoutine;
    [Header("Oscillation Settings")]
    public float maxYawAngle = 45f;
    public float maxOscillationFrequency = 1f;
    [Header("Randomization & Smoothing")]
    public float changeInterval = 1f;
    public float smoothingSpeed = 1f;
    private float currentYawAmp;
    private float targetYawAmp;
    private float currentFreq;
    private float targetFreq;
    private float changeTimer;
    public void Initialize(Vector3 targetPos, float moveSpeed)
    {
        targetPosition = targetPos;
        speed = moveSpeed;
        elapsedTime = 0f;
        animator = GetComponent<Animator>();
        if (animator != null)
            animator.SetBool("is_walking", true);
        targetYawAmp = Random.Range(0f, maxYawAngle);
        currentYawAmp = targetYawAmp;
        targetFreq = Random.Range(0f, maxOscillationFrequency);
        currentFreq = targetFreq;
        changeTimer = changeInterval;
        // :별: 자동 파괴 예약
        selfDestructRoutine = StartCoroutine(SelfDestructIfNotMounted(10f));
    }
    private IEnumerator SelfDestructIfNotMounted(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isMounted)
        {
            Destroy(gameObject); // 벌레 통째로 삭제
        }
    }
    // :별: 외부에서 호출: 플레이어가 탑승하면 예약 취소
    public void CancelSelfDestruct()
    {
        isMounted = true;
        if (selfDestructRoutine != null)
            StopCoroutine(selfDestructRoutine);
    }
    void Update()
    {
        float dt = Time.deltaTime;
        elapsedTime += dt;
        changeTimer -= dt;
        if (changeTimer <= 0f)
        {
            targetYawAmp = Random.Range(0f, maxYawAngle);
            targetFreq = Random.Range(0f, maxOscillationFrequency);
            changeTimer = changeInterval;
        }
        currentYawAmp = Mathf.Lerp(currentYawAmp, targetYawAmp, smoothingSpeed * dt);
        currentFreq = Mathf.Lerp(currentFreq, targetFreq, smoothingSpeed * dt);
        float yawOffset = Mathf.Sin(elapsedTime * currentFreq * 2f * Mathf.PI) * currentYawAmp;
        Vector3 toTarget = (targetPosition - transform.position).normalized;
        Quaternion baseRot = Quaternion.LookRotation(toTarget);
        transform.rotation = baseRot * Quaternion.Euler(0f, yawOffset, 0f);
        transform.position += transform.forward * speed * dt;
    }
}