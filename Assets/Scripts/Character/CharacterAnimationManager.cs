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
}
