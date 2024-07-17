using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private CharacterManager characterManager;

    void Awake() => characterManager = GetComponent<CharacterManager>();

    private void ApplyRootMotion()
    {
        characterManager.applyRootMotion = true;
    }

    public void EnableMoveAndRotate()
    {
        characterManager.canMove = true;
        characterManager.canRotate = true;
    }

    public void DisableMoveAndRotate()
    {
        characterManager.canMove = false;
        characterManager.canRotate = false;
    }

    public void FallToRoll() 
    {
        ApplyRootMotion();
        DisableMoveAndRotate();
    }
}
