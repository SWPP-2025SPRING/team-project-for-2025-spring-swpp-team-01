using UnityEngine;
public class RespawnTriggerPoint : MonoBehaviour
{
    public RespawnManager manager;
    public int index;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Bug"))
        {
            manager.SetRespawnIndex(index);
            Debug.Log("[RespawnManager] SetRespawnIndex " + index);
        }
    }
}