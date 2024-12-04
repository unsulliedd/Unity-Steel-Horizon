using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float health;
    public float maxHealth;
    private float blinkTimer = 0f;
    public float blinkDuration;
    public float blinkIntensity;
    SkinnedMeshRenderer skinnedMeshRenderer;
    AIAgent agent;
    public bool useDamage=false;
    // Start is called before the first frame update
    void Start()
    {
        health=maxHealth;
        agent=GetComponent<AIAgent>();
        skinnedMeshRenderer=GetComponentInChildren<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
        blinkTimer = blinkDuration;
           
    }
    private void Die() {
        AIDeathState deathState = new AIDeathState();
        agent.StateMachine.ChangeState(AIStateID.Death);
    }

    private void Update()
    {
        blinkTimer-=Time.deltaTime;
        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity=lerp*blinkIntensity;
        skinnedMeshRenderer.material.color = Color.white * intensity;
        if (useDamage) {
            useDamage = false;
            TakeDamage(10);
        }

    }
}
