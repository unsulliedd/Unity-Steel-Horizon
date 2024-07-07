using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance { get; private set; }
    public Camera playerCamera;
    private void Awake()
    {
         if (Instance == null)
              Instance = this;
         else
              Destroy(gameObject);
    }
}
