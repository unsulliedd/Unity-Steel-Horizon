using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSockets : MonoBehaviour
{
    public enum SocketID
    {
        Spine,
        RightHand
    }

    private Dictionary<SocketID, MeshSocket> socketMap = new Dictionary<SocketID, MeshSocket>();
    // Start is called before the first frame update
    void Start()
    {
        MeshSocket[] sockets = GetComponentsInChildren<MeshSocket>();
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
