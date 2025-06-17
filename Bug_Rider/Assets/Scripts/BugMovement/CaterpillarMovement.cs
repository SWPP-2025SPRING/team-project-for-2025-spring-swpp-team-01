using UnityEngine;
using TMPro;
using System.Collections;

public class CaterpillarMovement : RideableBugBase
{
    public GameObject mothPrefab;
    public GameObject butterflyPrefab;
    public GameObject beePrefab;

    public LayerMask obstacleMask;
    public float transformTime = 5f;

    private WalkMovementStrategy walkStrategy;
    private Coroutine transformRoutine;
    private PlayerMovement mountedPlayer;

    protected override void Awake()
    {
        base.Awake();

        walkStrategy = new WalkMovementStrategy(
            rb, animator, obstacleMask,
            acceleration, maxSpeed,
            angularAcceleration, maxAngularSpeed,
            obstacleCheckDist,
            false,  // useAcceleration
            false   // useAngularAcceleration
        );
    }

    void FixedUpdate()
    {
        if (!isMounted) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        walkStrategy.HandleMovement(h, v);
    }

    public override void SetMounted(bool mounted)
    {
        base.SetMounted(mounted);

        if (!mounted)
        {
            animator?.SetBool("is_walking", false);
            if (transformRoutine != null)
                StopCoroutine(transformRoutine);
        }
        else
        {
            mountedPlayer = GetComponentInChildren<PlayerMovement>();
            transformRoutine = StartCoroutine(DelayedTransformation());
        }
    }

    private IEnumerator DelayedTransformation()
    {
        float timer = transformTime;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        TransformIntoRandomBug();
    }

    private void TransformIntoRandomBug()
    {
        GameObject[] candidates = { mothPrefab, butterflyPrefab, beePrefab };
        int idx = Random.Range(0, candidates.Length);
        GameObject chosen = candidates[idx];

        GameObject newBug = Instantiate(chosen, transform.position, transform.rotation);
        if (mountedPlayer != null)
        {
            mountedPlayer.transform.SetParent(newBug.transform);
            mountedPlayer.transform.localPosition = new Vector3(0, -1.2f, 0);
            mountedPlayer.transform.localRotation = Quaternion.identity;
            mountedPlayer.isMounted = true;
            mountedPlayer.mountedBug = newBug.transform;
        }

        Destroy(gameObject);
    }
}
