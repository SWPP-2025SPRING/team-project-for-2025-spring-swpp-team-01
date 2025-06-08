using UnityEngine;

public class RespawnTriggerPoint : MonoBehaviour
{
    public RespawnManager manager;
    public int index;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Bug"))
        {
            manager.SetRespawnIndex(index);
        }
    }
}
