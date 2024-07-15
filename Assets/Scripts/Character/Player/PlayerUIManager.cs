using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance { get; private set; }
    [HideInInspector] public PlayerHUDManager playerHUDManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        playerHUDManager = GetComponentInChildren<PlayerHUDManager>();
    }
}
