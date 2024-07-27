using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCamManager : MonoBehaviour
{
    [SerializeField] Transform vehiclePosition;
    private void LateUpdate()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + vehiclePosition.localPosition.z);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
