using UnityEngine;

public interface IBugMovementStrategy
{
    void HandleMovement(Rigidbody rb, Animator animator, LayerMask obstacleMask, float moveSpeed, float rotationSpeed, float obstacleCheckDist);
}
