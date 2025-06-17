using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public abstract class RideableBugBase : MonoBehaviour
{
    protected Rigidbody rb;
    protected Animator animator;

    public GameObject abilityUI;
    protected bool isMounted = false;
    public float acceleration = 5f;
    public float maxSpeed = 5f;
    public float angularAcceleration = 600f;
    public float maxAngularSpeed = 45f;
    public float obstacleCheckDist = 0.8f;

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
        GetComponentInChildren<PlayerMovement>()?.ForceFallFromBug();
        SetMounted(false);
        Destroy(transform.root.gameObject, 2f);
    }

    protected IEnumerator SkillWithCooldown(
        float activeTime,
        float cooldownTime,
        System.Action onSkillStart,
        System.Action onSkillEnd)
    {
        onSkillStart?.Invoke();

        float timer = activeTime;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        onSkillEnd?.Invoke();

        timer = cooldownTime;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
    }
}
