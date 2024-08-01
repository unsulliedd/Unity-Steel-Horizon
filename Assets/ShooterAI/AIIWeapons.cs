using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIIWeapons : MonoBehaviour
{
    private MeshSockets sockets;

    private RaycastWeapon _weapon;
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
        
    }
}
