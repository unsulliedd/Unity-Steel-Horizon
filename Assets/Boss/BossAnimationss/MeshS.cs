using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshS : MonoBehaviour
{
    public enum SocketID
    {
        Spine,
        RightHand
    }

    private Dictionary<SocketID, MeshSocketBoss> socketMap = new Dictionary<SocketID, MeshSocketBoss>();
    // Start is called before the first frame update
    void Start()
    {
        MeshSocketBoss[] sockets = GetComponentsInChildren<MeshSocketBoss>();
        foreach (var socket in sockets)
        {
            socketMap[socket.socketID] = socket;
        }
    }

    // Update is called once per frame
    public void Attach(Transform objectTransform, SocketID socketID)
    {
        socketMap[socketID].Attach(objectTransform);
    }
}
