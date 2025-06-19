using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class RideableBugBase : MonoBehaviour
{
    protected Rigidbody rb;
    protected Animator animator;

    public GameObject abilityUI;
    protected bool isMounted = false;
    public float acceleration = 15f;
    public float maxSpeed = 6f;
    public float angularAcceleration = 1200f;
    public float maxAngularSpeed = 90f;
    public float obstacleCheckDist = 0.0001f;
    protected bool isSkillActive = false;
    protected bool isSkillOnCooldown = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public virtual void SetMounted(bool mounted)
    {
        isMounted = mounted;
        if (!mounted)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            if (abilityUI) abilityUI?.SetActive(false);
            StopAllCoroutines();
        }
        else
        {
            if (abilityUI) abilityUI?.SetActive(true);
            Destroy(GetComponent<MoveToTarget>());
        }
    }

    protected virtual void OnCollisionEnter(Collision col)
    {
        if (!isMounted) return;
        if (!col.gameObject.CompareTag("Obstacle")) return;

        animator?.SetTrigger("is_dropping");
        // AudioManager.Instance?.StopBug();
        AudioManager.Instance?.PlayBug("Stun");
        GetComponentInChildren<PlayerMovement>()?.ForceFallFromBug();
        SetMounted(false);
        Destroy(transform.root.gameObject, 2f);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ForceFall"))
        {
            GetComponentInChildren<PlayerMovement>()?.Unmount();
            Destroy(transform.root.gameObject, 1f);
        }
    }

    protected IEnumerator SkillWithCooldown(
        float activeTime,
        float cooldownTime,
        System.Action onSkillStart,
        System.Action onSkillEnd)
    {
        isSkillActive = true;
        onSkillStart?.Invoke();
        Debug.Log("[SkillWithCooldown] Start Invoked");

        float timer = activeTime;
        while (timer > 0f)
        {
            UIManager.Instance.UpdateSkillActiveTime(timer);  
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
            UIManager.Instance.UpdateSkillCooldownTime(timer);  
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