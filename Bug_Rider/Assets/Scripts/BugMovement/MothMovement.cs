using UnityEngine;
using TMPro;
using System.Collections;

public class MothMovement : RideableBugBase
{
    public float flyMaxHeight = 50f;
    public float flyTimeLimit = 5f;
    public float skillCooldown = 7f;
    public float backwardSpeed = 3f;
    public float retreatDuration = 3f;
    public LayerMask obstacleMask;

    private FlyMovementStrategy flyStrategy;
    private bool isInitialRetreat = false;
    private float retreatTimer = 0f;

    protected override void Awake()
    {
        base.Awake();

        flyStrategy = new FlyMovementStrategy(
            rb, animator,
            maxSpeed, maxAngularSpeed,
            flyMaxHeight
        );
    }

    void Update()
    {
        if (!isMounted) return;

        if (Input.GetKeyDown(KeyCode.Space) && !flyStrategy.IsFlying && !isInitialRetreat)
        {
            StartCoroutine(StartFlight());
        }
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        if (isInitialRetreat)
        {
            retreatTimer += Time.fixedDeltaTime;
            if (retreatTimer >= retreatDuration)
            {
                isInitialRetreat = false;
                flyStrategy.SetFlying(true);
            }
            else
            {
                rb.velocity = -transform.forward * backwardSpeed;
                return;
            }
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool s = Input.GetKey(KeyCode.Space);

        if (flyStrategy.IsFlying)
            flyStrategy.HandleMovement(h, v, s);
    }

    IEnumerator StartFlight()
    {
        isInitialRetreat = true;
        retreatTimer = 0f;

        yield return SkillWithCooldown(
            flyTimeLimit,
            skillCooldown,
            () => { /* 비행 전 아무것도 안 함 (retreat 진행 중) */ },
            () => flyStrategy.SetFlying(false)
        );
    }

    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);

        if (!mounted)
        {
            isInitialRetreat = false;
            flyStrategy.SetFlying(false);
            animator?.SetBool("is_walking", false);
        }
    }
    protected override void OnCollisionEnter(Collision col)
    {
        if (!isMounted || isInitialRetreat) return;
        base.OnCollisionEnter(col);
    }

}
