using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance { get; private set; }

    [HideInInspector] public PlayerManager playerManager;
    public PlayerHUDManager playerHUDManager;
    public PlayerMenuManager playerMenuManager;
    public PlayerInventoryMenu_UI playerInventoryUI;
    public PlayerInfoMenu_UI playerInfoUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
