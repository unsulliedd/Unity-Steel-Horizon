using UnityEngine;

public class PlayerAnimationManager : CharacterAnimationManager
{
    private void OnAnimatorMove()
    {
        if (characterManager.applyRootMotion)
        {
            Vector3 velocity = characterManager.Animator.deltaPosition;
            characterManager.CharacterController.Move(velocity);
            characterManager.transform.rotation *= characterManager.Animator.deltaRotation;
        }
    }
}