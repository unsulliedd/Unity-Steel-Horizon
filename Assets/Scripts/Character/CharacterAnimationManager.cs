using UnityEngine;

public class CharacterAnimationManager : MonoBehaviour
{
    private CharacterManager characterManager;
    float horizontal;
    float vertical;

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

    public void MovementAnimations(float horizontalValue, float verticalValue)
    {
        characterManager.Animator.SetFloat("Horizontal", horizontalValue, 0.1f, Time.deltaTime);
        characterManager.Animator.SetFloat("Vertical", verticalValue, 0.1f, Time.deltaTime);
    }

    public virtual void PlayTargetAnimation(string targetAnimation, 
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
    }
}
