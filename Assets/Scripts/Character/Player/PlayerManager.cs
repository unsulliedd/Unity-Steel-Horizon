using UnityEngine;

public class PlayerManager : CharacterManager
{
    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerAnimationManager PlayerAnimationManager;
    [HideInInspector] public PlayerNetworkManager PlayerNetworkManager;
    [HideInInspector] public PlayerStatsManager PlayerStatsManager;

    protected override void Awake()
    {
        base.Awake();

        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        PlayerAnimationManager = GetComponent<PlayerAnimationManager>();
        PlayerNetworkManager = GetComponent<PlayerNetworkManager>();
        PlayerStatsManager = GetComponent<PlayerStatsManager>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // Prevent the player controlling other players
        if (!IsOwner) return;

        playerLocomotionManager.HandleAllMovement();
        PlayerStatsManager.RegenerateStamina();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (!IsOwner) return;

        PlayerCamera.Instance.HandleAllCameraAction();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            PlayerCamera.Instance.playerManager = this;
            PlayerInputManager.Instance.playerManager = this;
            SaveGameManager.Instance.playerManager = this;

            PlayerNetworkManager.stamina.OnValueChanged += PlayerUIManager.Instance.playerHUDManager.SetNewStaminaValue;
            PlayerNetworkManager.stamina.OnValueChanged += PlayerStatsManager.ResetStaminaTimer;
            
            PlayerNetworkManager.maxStamina.Value = PlayerStatsManager.CalculateStaminaBasedOnStrength(PlayerNetworkManager.strength.Value);
            PlayerNetworkManager.stamina.Value = PlayerStatsManager.CalculateStaminaBasedOnStrength(PlayerNetworkManager.strength.Value);
            PlayerUIManager.Instance.playerHUDManager.SetMaxStaminaValue(PlayerNetworkManager.maxStamina.Value);

            SaveGameCallbacks.OnSaveGame += SaveCurrentGameData;
            SaveGameCallbacks.OnLoadGame += LoadCurrentGameData;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner)
        {
            SaveGameCallbacks.OnSaveGame -= SaveCurrentGameData;
            SaveGameCallbacks.OnLoadGame -= LoadCurrentGameData;
        }
    }

    public void SaveCurrentGameData(ref CharacterSaveData currentCharacterData)
    {
        // Ensure PlayerNetworkManager and its characterName are not null
        if (PlayerNetworkManager != null)
        {
            //currentCharacterData.characterName = PlayerNetworkManager.characterName.Value.ToString();
            currentCharacterData.positionX = transform.position.x;
            currentCharacterData.positionY = transform.position.y;
            currentCharacterData.positionZ = transform.position.z;
        }
        else
            Debug.LogError("PlayerNetworkManager or its characterName is null");
    }

    public void LoadCurrentGameData(ref CharacterSaveData currentCharacterData)
    {
        PlayerNetworkManager.characterName.Value = currentCharacterData.characterName;
        transform.position = new Vector3(currentCharacterData.positionX, currentCharacterData.positionY, currentCharacterData.positionZ);
    }
}
