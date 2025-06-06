using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RespawnManager : MonoBehaviour
{
    [Header("전체 리스폰 포인트")]
    [SerializeField] Transform[] respawnPoints;

    [Header("추락 Y 한계값")]
    [SerializeField] float fallY = -10f;

    CharacterController cc;

    void Awake() => cc = GetComponent<CharacterController>();

    void Update()
    {
        // 1) Y 좌표가 너무 낮으면
        if (transform.position.y < fallY)
            RespawnToNearest();
    }

    // 2) KillZone 트리거와 부딪히면
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KillZone"))
            RespawnToNearest();
    }

    // ───────── 핵심 로직 ─────────
    public void RespawnToNearest()
    {
        Transform nearest = GetNearestPoint();

        cc.enabled = false;                   // CharacterController 잠깐 끔
        transform.position = nearest.position;
        transform.rotation = nearest.rotation; // 회전값도 초기화하고 싶으면
        cc.enabled = true;
    }

    Transform GetNearestPoint()
    {
        Transform best = respawnPoints[0];
        float bestSqr = (transform.position - best.position).sqrMagnitude;

        for (int i = 1; i < respawnPoints.Length; i++)
        {
            float sqr = (transform.position - respawnPoints[i].position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                best = respawnPoints[i];
                bestSqr = sqr;
            }
        }
        return best;
    }
}
