// RideableBugBase.cs 수정
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class RideableBugBase : MonoBehaviour
{
    protected Rigidbody rb;
    protected Animator animator;
    protected bool isMounted = false;
    public float acceleration = 15f;
    public float maxSpeed = 6f;
    public float angularAcceleration = 1200f;
    public float maxAngularSpeed = 90f;
    public float obstacleCheckDist = 0.0001f;
    protected bool isSkillActive = false;
    protected bool isSkillOnCooldown = false;
    protected Quaternion meshRot = Quaternion.Euler(0f, 0f, 0f);
    protected Transform mesh;
    protected bool isGrounded = true;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }
    
    public virtual void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            AudioManager.Instance?.StopBugAll();
            
            StopAllCoroutines();
        }
        else
        {
            Destroy(GetComponent<MoveToTarget>());
        }
    }
    
    protected virtual void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;
        
        animator?.SetTrigger("is_dropping");

        AudioManager.Instance?.StopBugAll();
        AudioManager.Instance?.PlayObstacle("Stun");
        
        UIManager.Instance?.HideAllSkillUI();
        GetComponentInChildren<PlayerMovement>()?.ForceFallFromBug();
        SetMounted(false);
        Destroy(transform.root.gameObject, 2f);
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ForceFall"))
        {
            AudioManager.Instance?.StopBugAll();   
            GetComponentInChildren<PlayerMovement>()?.Unmount();
            Destroy(transform.root.gameObject, 1f);
        }
    }
    // 파괴 중인지 확인하는 헬퍼 함수
    private bool IsDestroying()
    {
        return this == null || gameObject == null || !gameObject.activeInHierarchy;
    }
    
    // OnDestroy에서 안전한 정리
    protected virtual void OnDestroy()
    {
        try
        {
            // 게임이 종료 중이면 즉시 리턴
            if (!Application.isPlaying)
            {
                return;
            }
            
            // AudioManager 존재 및 상태 확인
            if (AudioManager.Instance != null && !IsDestroying())
            {
                // 즉시 정지 메소드 사용 (코루틴 시작 안함)
                AudioManager.Instance.StopBugAll();
            }
        }
        catch (MissingReferenceException)
        {
            // AudioManager가 이미 파괴된 경우 - 조용히 무시
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[OnDestroy] Error during audio cleanup: {e.Message}");
        }
    }
    
    protected virtual void RotateMesh()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(
            rb.position + Vector3.up * 0.05f,
            Vector3.down,
            out hit,
            0.3f
        );
        
        if (isGrounded && mesh != null)
        {
            Debug.Log("[] Ground normal: " + hit.normal);
            Vector3 normal = hit.normal;
            Vector3 projectedForward = Vector3.ProjectOnPlane(rb.transform.forward, normal).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, normal);
            Quaternion finalRotation = targetRotation * meshRot;
            mesh.rotation = Quaternion.Slerp(mesh.rotation, finalRotation, Time.fixedDeltaTime * 10f);
        }
    }
    
    protected IEnumerator SkillWithCooldown(
        float activeTime,
        float cooldownTime,
        System.Action onSkillStart,
        System.Action onSkillEnd, 
        string skillType)
    {
        isSkillActive = true;
        onSkillStart?.Invoke();
        Debug.Log("[SkillWithCooldown] Start Invoked");
        
        float timer = activeTime;
        while (timer > 0f)
        {
            UIManager.Instance.UpdateSkillActiveTime(skillType, timer, activeTime);
            timer -= Time.deltaTime;
            yield return null;
        }
        
        isSkillActive = false;
        onSkillEnd?.Invoke();
        Debug.Log("[SkillWithCooldown] End Invoked");
        isSkillOnCooldown = true;
        
        timer = cooldownTime;
        while (timer > 0f)
        {
            UIManager.Instance.UpdateSkillCooldownTime(skillType, timer, cooldownTime);
            timer -= Time.deltaTime;
            yield return null;
        }
        
        isSkillOnCooldown = false;
    }
    
    protected bool CanUseSkill()
    {
        return !isSkillActive && !isSkillOnCooldown;
    }
}