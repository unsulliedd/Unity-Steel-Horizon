using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [Header("Damage")] public float physicalDamage = 0;
    [Header("Contact Point")] private Vector3 contactPoint;

    [Header("Characters Damaged")] protected List<CharacterManager> CharacterManagers = new List<CharacterManager>();
    private void OnTriggerEnter(Collider other)
    {
        CharacterManager damageTarget = other.GetComponent<CharacterManager>();
        if (damageTarget != null)
        {
            Debug.Log("DamageCollider: " + damageTarget.name);
            contactPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            DamageTarget(damageTarget);
        }
        
    }

    protected virtual void DamageTarget(CharacterManager damageTarget)
    {
        //if (CharacterManagers.Contains(damageTarget))
        //return;
    
        Debug.Log("DamageTarget: " + damageTarget.name);
        CharacterManagers.Add(damageTarget);
        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.TakeDamageEffect);
        damageEffect.physicalDamage = physicalDamage;
        damageTarget.CharacterEffectsManager.ProcessInstantEffect(damageEffect);
        CharacterManagers.Remove(damageTarget);
    }
}
