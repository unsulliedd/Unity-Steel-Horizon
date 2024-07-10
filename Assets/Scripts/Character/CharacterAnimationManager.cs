using Unity.Netcode;
using UnityEngine;

public class CharacterAnimationManager : MonoBehaviour
{
    private CharacterManager characterManager;

    protected virtual void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    public void MovementAnimations(float horizontalValue, float verticalValue, bool isSprinting)
    {
        float horizontal = horizontalValue;
        float vertical = verticalValue;

        if (isSprinting)
            vertical = 2;

        characterManager.Animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        characterManager.Animator.SetFloat("Vertical", vertical, 0.1f, Time.deltaTime);
    }

    public virtual void PlayTargetAnimation(
        string targetAnimation, 
        bool isPerformingAction, 
        bool applyRootMotion = true,
        bool canMove = false,
        bool canRotate = false)
    {
        characterManager.Animator.applyRootMotion = applyRootMotion;
        characterManager.canMove = canMove;
        characterManager.canRotate = canRotate;
        characterManager.Animator.CrossFade(targetAnimation, 0.2f);
        characterManager.isPerformingAction = isPerformingAction;

        characterManager.CharacterNetworkManager.NotifyServerOfActionAnimationsRpc(
            NetworkManager.Singleton.LocalClientId,
            targetAnimation,
            applyRootMotion);
    }
}
