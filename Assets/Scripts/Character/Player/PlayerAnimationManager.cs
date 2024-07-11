using UnityEngine;

public class PlayerAnimationManager : CharacterAnimationManager
{
    private void OnAnimatorMove()
    {
        if (characterManager.applyRootMotion)
        {
            Vector3 velocty = characterManager.Animator.deltaPosition;
            characterManager.CharacterController.Move(velocty);
            characterManager.transform.rotation *= characterManager.Animator.deltaRotation;
        }
    }
}
