using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCharacterEffectsManager : MonoBehaviour
{
    public static WorldCharacterEffectsManager instance;
    [Header("Damage")] public TakeDamageEffect TakeDamageEffect;
    

    [SerializeField] private List<InstantCharacterEffect> _instantCharacterEffects;
    // Start is called before the first frame update


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        GenerateEffectIDs();
    }

    private void GenerateEffectIDs()
    {
        for (int i = 0; i < _instantCharacterEffects.Count; i++)
        {
            _instantCharacterEffects[i].instantEffectID = i;
        }
    }
}
