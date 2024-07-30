using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Effects/Instant Effects/Take Damage")]
public class TakeDamageEffect : InstantCharacterEffect
{
    [Header("Character Causing Damage")] public CharacterManager CharacterManager;

    [Header("Damage")] public float physicalDamage = 0;
    [Header("Animation")] 
    public bool playDamageAnimation = true;
    public bool manuallySelectDamageAnimation = false;
    public string damageAnimation;
    [Header("SoundFX")] public bool willPlayDamageSFX = true;
    public AudioClip elementalSoundFX;
    [Header("DirectionDamageTakeFrom")] public float angleHitFrom;
    public Vector3 contactPoint;

    [Header("FinalDamage")] private int finalDamageDealt = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public override void ProcessEffect(CharacterManager character)
    {
     
        base.ProcessEffect(character);
       if(character.isDead.Value)
           return;
        CalculateDamage(character);
    }

    private void CalculateDamage(CharacterManager character)
    {
      
        if (!character.IsOwner)
        {
            return;
        }
        if (CharacterManager != null)
        {
            finalDamageDealt = Mathf.RoundToInt(physicalDamage);

            if (finalDamageDealt <= 0)
            {
                finalDamageDealt = 1;
            }

            character.CharacterNetworkManager.currentHealth.Value -= finalDamageDealt;
        }
    }
}
