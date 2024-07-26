using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class PlayerCamera : MonoBehaviour
{
    #region Singleton
    public static PlayerCamera Instance { get; private set; }
    #endregion
    [HideInInspector] public PlayerManager playerManager;

    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera virtual3rdPersonCamera;
    public CinemachineVirtualCamera virtual3rdPersonAimCamera;

    [Header("Main Camera")]
    public Camera mainCamera;

    public Image crosshair;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetCameraFollowAndLookAt(Transform target)
    {
        virtual3rdPersonCamera.Follow = target;
        virtual3rdPersonCamera.LookAt = target;
        virtual3rdPersonAimCamera.Follow = target;
        virtual3rdPersonAimCamera.LookAt = target;
    }

    public void Enable3rdPersonCamera()
    {
        virtual3rdPersonAimCamera.gameObject.SetActive(false);
        virtual3rdPersonCamera.gameObject.SetActive(true);
    }

    public void Enable3rdPersonAimCamera() 
    {
        virtual3rdPersonCamera.gameObject.SetActive(false);
        virtual3rdPersonAimCamera.gameObject.SetActive(true);
    }
}
