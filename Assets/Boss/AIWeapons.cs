using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWeapons : MonoBehaviour
{
    private MeshSockets sockets;
    [SerializeField] private Transform Weapon;
    private WeaponManage _weapon;
    public float sphereRadius;
    [SerializeField] private float damage;
    public float castDistance;
    [Header("Characters Damaged")] protected List<CharacterManager> CharacterManagers = new List<CharacterManager>();
    public LayerMask targetLayer;
    private bool canControl;
    // Start is called before the first frame update
    void Start()
    {
        sockets = GetComponent<MeshSockets>();
        sockets.Attach(_weapon.transform,MeshSockets.SocketID.Spine);
        sockets.Attach(_weapon.transform,MeshSockets.SocketID.RightHand);
    }

    // Update is called once per frame
    void Update()
    {
        if (canControl)
        {
            Vector3 origin = Weapon.position;
            Vector3 direction =Weapon.transform.up;

            // SphereCast ile belirli bir mesafeye kadar çarpışma kontrolü yap
            if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, castDistance, targetLayer))
            {
                Debug.Log(hit.transform.name);
                CharacterManager damageTarget = hit.transform.GetComponent<CharacterManager>();
                if (damageTarget != null)
                {
                    Debug.Log("Hasar başarılı");
                    DamageTarget(damageTarget);
                }
                
                canControl = false;

            }

           
        }
    }
    
    public void CheckWeaponDamage()
    {
        
        canControl = true;
        
    }

    public void ResetFlag()
    {
        canControl = false;
        
        
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Weapon.position + Weapon.transform.up * castDistance, sphereRadius);
    }
    public virtual void DamageTarget(CharacterManager damageTarget)
    {
        if (CharacterManagers.Contains(damageTarget))
        return;
    
        Debug.Log("DamageTarget: " + damageTarget.name);
        CharacterManagers.Add(damageTarget);
        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.TakeDamageEffect);
        damageEffect.physicalDamage = damage;
        damageTarget.CharacterEffectsManager.ProcessInstantEffect(damageEffect);
        CharacterManagers.Remove(damageTarget);
    }
}