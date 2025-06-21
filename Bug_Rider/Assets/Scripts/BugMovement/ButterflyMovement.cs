using UnityEngine;
using TMPro;
using System.Collections;

public class ButterflyMovement : RideableBugBase
{
    public float flyMaxHeight = 50f;
    public float flyTimeLimit = 5f;
    public float skillCooldown = 7f;
    public LayerMask obstacleMask;
    private WalkMovementStrategy walkStrategy;
    private FlyMovementStrategy flyStrategy;

    protected override void Awake()
    {
        base.Awake();

        walkStrategy = new WalkMovementStrategy(
                            rb, animator, obstacleMask,
                            acceleration, maxSpeed,
                            angularAcceleration, maxAngularSpeed,
                            obstacleCheckDist,
                            "Butterfly"
                        );

        flyStrategy = new FlyMovementStrategy(
            rb, animator,
            maxSpeed, maxAngularSpeed,
            flyMaxHeight,
            "Butterfly"
        );
    }

    void Update()
    {
        if (!isMounted) return;
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(StartFlight());
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool s = Input.GetKey(KeyCode.Space);

        if (flyStrategy.IsFlying)
            flyStrategy.HandleMovement(h, v, s);
        else
            walkStrategy.HandleMovement(h, v);
    }

    IEnumerator StartFlight()
    {
        if (!CanUseSkill())
        {
            Debug.Log("Skill is not available (still active or cooling down).");
            yield break;
        }

        flyStrategy.SetFlying(true);

        yield return SkillWithCooldown(
            flyTimeLimit,
            skillCooldown,
            null,
            () => flyStrategy.SetFlying(false)
        );
    }

    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);

        if (!mounted)
        {
            flyStrategy.SetFlying(false);
            animator?.SetBool("is_walking", false);
            AudioManager.Instance?.StopObstacle(); // Turn off _Enter sound
        }
        // else // Mount되자마자 자동 비행 시작
        //     StartCoroutine(StartFlight());
        
    }
}
