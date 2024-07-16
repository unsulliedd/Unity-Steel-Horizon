using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private PlayerManager playerManager;

    void Awake() => playerManager = GetComponent<PlayerManager>();

    private void ApplyRootMotion()
    {
        playerManager.applyRootMotion = true;
    }

    private void DisableMoveAndRotate()
    {
        playerManager.canMove = false;
        playerManager.canRotate = false;
    }

    public void PreventMovementWhenLanding()
    {
        DisableMoveAndRotate();
    }

    public void FallToRoll() 
    {
        ApplyRootMotion();
        DisableMoveAndRotate();
    }
}
