using UnityEngine;
using System.Collections;

/// <summary>
/// 월드 좌표 기준으로 minHeight~maxHeight 사이를 왕복하는 장애물 (Rigidbody 없이)
/// 상승/낙하 속도는 위치 기반 시드로 개체마다 달라짐
/// </summary>
public class MusicRoomObstacle : MonoBehaviour
{
    [Header("Y 좌표 범위 (world units)")]
    public float minHeight = 6.57f;   // 절대 Y 좌표
    public float maxHeight = 8f;   // 절대 Y 좌표

    [Header("상승 / 낙하 시간 범위 (초)")]
    public Vector2 ascendTimeRange = new Vector2(1f, 3f);
    public Vector2 dropTimeRange = new Vector2(0.15f, 0.4f);

    [Header("꼭대기 / 바닥 정지 시간 (초)")]
    public float holdTimeTop = 0.2f;
    public float holdTimeBottom = 0.2f;

    private Vector3 basePosition;  // X, Z는 고정

    void Awake()
    {
        basePosition = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    void Start()
    {
        int seed = HashPosition(transform.position);
        System.Random prng = new System.Random(seed);

        float ascendDuration = Mathf.Lerp(ascendTimeRange.x, ascendTimeRange.y, (float)prng.NextDouble());
        float dropDuration = Mathf.Lerp(dropTimeRange.x, dropTimeRange.y, (float)prng.NextDouble());

        StartCoroutine(LiftDropLoop(ascendDuration, dropDuration));
    }

    private IEnumerator LiftDropLoop(float ascendDur, float dropDur)
    {
        // 시작 위치: minHeight
        transform.position = new Vector3(basePosition.x, minHeight, basePosition.z);

        while (true)
        {
            // 상승: minHeight → maxHeight
            for (float t = 0f; t < 1f; t += Time.deltaTime / ascendDur)
            {
                float y = Mathf.Lerp(minHeight, maxHeight, t);
                transform.position = new Vector3(basePosition.x, y, basePosition.z);
                yield return null;
            }
            transform.position = new Vector3(basePosition.x, maxHeight, basePosition.z);
            yield return new WaitForSeconds(holdTimeTop);

            // 낙하: maxHeight → minHeight
            for (float t = 0f; t < 1f; t += Time.deltaTime / dropDur)
            {
                float y = Mathf.Lerp(maxHeight, minHeight, t);
                transform.position = new Vector3(basePosition.x, y, basePosition.z);
                yield return null;
            }
            transform.position = new Vector3(basePosition.x, minHeight, basePosition.z);
            yield return new WaitForSeconds(holdTimeBottom);
        }
    }

    private int HashPosition(Vector3 pos)
    {
        int xi = Mathf.RoundToInt(pos.x * 1000f);
        int zi = Mathf.RoundToInt(pos.z * 1000f);
        return xi * 73856093 ^ zi * 83492791;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 baseXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(baseXZ + Vector3.up * minHeight, 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(baseXZ + Vector3.up * maxHeight, 0.2f);
    }
}
