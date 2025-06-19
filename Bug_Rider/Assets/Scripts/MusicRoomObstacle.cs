using UnityEngine;
using System.Collections;

/// <summary>
/// ���� ��ǥ �������� minHeight~maxHeight ���̸� �պ��ϴ� ��ֹ� (Rigidbody ����)
/// ���/���� �ӵ��� ��ġ ��� �õ�� ��ü���� �޶���
/// </summary>
public class MusicRoomObstacle : MonoBehaviour
{
    [Header("Y ��ǥ ���� (world units)")]
    public float minHeight = 6.57f;   // ���� Y ��ǥ
    public float maxHeight = 8f;   // ���� Y ��ǥ

    [Header("��� / ���� �ð� ���� (��)")]
    public Vector2 ascendTimeRange = new Vector2(1f, 3f);
    public Vector2 dropTimeRange = new Vector2(0.15f, 0.4f);

    [Header("����� / �ٴ� ���� �ð� (��)")]
    public float holdTimeTop = 0.2f;
    public float holdTimeBottom = 0.2f;

    private Vector3 basePosition;  // X, Z�� ����

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
        // ���� ��ġ: minHeight
        transform.position = new Vector3(basePosition.x, minHeight, basePosition.z);

        while (true)
        {
            // ���: minHeight �� maxHeight
            for (float t = 0f; t < 1f; t += Time.deltaTime / ascendDur)
            {
                float y = Mathf.Lerp(minHeight, maxHeight, t);
                transform.position = new Vector3(basePosition.x, y, basePosition.z);
                yield return null;
            }
            transform.position = new Vector3(basePosition.x, maxHeight, basePosition.z);
            yield return new WaitForSeconds(holdTimeTop);

            // ����: maxHeight �� minHeight
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
