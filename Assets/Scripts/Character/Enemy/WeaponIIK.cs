using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponIIK : MonoBehaviour
{
    private BossManager bossTransform;
    public Transform aimTransform;
    public Transform bone;
    public float angleLimit = 90f;
    public float distanceLimit = 1.5f;
    public int iterations = 10;
    [Range(0, 1)]
    public float weight = 1.0f;

    public bool canTurn;
    // Start is called before the first frame update
    void Start()
    {
        bossTransform = GetComponentInParent<BossManager>();
    }

    // Update is called once per frame
    Vector3 GetTargetPosition()
    {
        Vector3 targetDirection = bossTransform.target.position - aimTransform.position;
        Vector3 aimDirection = aimTransform.forward;
        float blendOut = 0.0f;
        float targetAngle = Vector3.Angle(targetDirection, aimDirection);
        if (targetAngle > angleLimit)
        {
            blendOut += (targetAngle - angleLimit) / 50.0f;
        }

        float targetDistance = targetDirection.magnitude; 
        if (targetDistance < distanceLimit)
        {
            blendOut += distanceLimit - targetDistance;
        }

        Vector3 direction = Vector3.Slerp(targetDirection, aimDirection, blendOut);
        return aimTransform.position + direction;
    }
    void LateUpdate()
    {

        Vector3 targetPosition = GetTargetPosition();
        for(int i = 0; i < iterations; i++)
            AimAtTarget(bone, targetPosition);
    
   
        
    }
    private void AimAtTarget(Transform bone,Vector3 targetPosition)
    {
        Vector3 aimDirection=aimTransform.forward;
        Vector3 targetDirection=targetPosition-aimTransform.position;
        Quaternion aimTowards=Quaternion.FromToRotation(aimDirection,targetDirection);
        bone.rotation = aimTowards*bone.rotation;
    }
}
