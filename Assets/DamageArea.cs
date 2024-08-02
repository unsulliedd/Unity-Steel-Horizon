using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageArea : MonoBehaviour
{
    [SerializeField] private float damage;
    public float castDistance;
    [Header("Characters Damaged")] protected List<CharacterManager> CharacterManagers = new List<CharacterManager>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
           
            CharacterManager damageTarget = other.transform.GetComponent<CharacterManager>();
            if (damageTarget != null)
            {
                Debug.Log("Hasar başarılı");
                DamageTarget(damageTarget);
            }
        }
    }

    public virtual void DamageTarget(CharacterManager damageTarget)
    {
        if (CharacterManagers.Contains(damageTarget))
            return;
    
        Debug.Log("DamageTarget: " + damageTarget.name);
        CharacterManagers.Add(damageTarget);
        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.TakeDamageEffect);
        damageEffect.physicalDamage = 1;
        damageTarget.CharacterEffectsManager.ProcessInstantEffect(damageEffect);
        CharacterManagers.Remove(damageTarget);
    }
}
