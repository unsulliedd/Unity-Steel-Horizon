using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerManager : CharacterManager
{
    [Header("DebugMenu")] [SerializeField] private bool respawnCharacter = false;
    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerAnimationManager PlayerAnimationManager;
    [HideInInspector] public PlayerNetworkManager PlayerNetworkManager;
    [HideInInspector] public PlayerStatsManager PlayerStatsManager;
    [HideInInspector] public PlayerCombatManager PlayerCombatManager;
    [HideInInspector] public InventoryManager InventoryManager;

    protected override void Awake()
    {
        base.Awake();

        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        PlayerAnimationManager = GetComponent<PlayerAnimationManager>();
        PlayerNetworkManager = GetComponent<PlayerNetworkManager>();
        PlayerStatsManager = GetComponent<PlayerStatsManager>();
        PlayerCombatManager = GetComponent<PlayerCombatManager>();
        InventoryManager = GetComponent<InventoryManager>();
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {

            StartCoroutine(InitializeSingletons());
            // Set the camera to follow the player
            PlayerCamera.Instance.SetCameraFollowAndLookAt(transform);
            PlayerCamera.Instance.Enable3rdPersonCamera();

            // Register the player manager to the singletons
            PlayerCamera.Instance.playerManager = this;
            PlayerInputManager.Instance.playerManager = this;
            SaveGameManager.Instance.playerManager = this;
            

            // Health
            PlayerNetworkManager.vitality.OnValueChanged += PlayerNetworkManager.SetNewHealthValue;
            PlayerNetworkManager.currentHealth.OnValueChanged += PlayerUIManager.Instance.playerHUDManager.SetNewHealthValue;
            PlayerNetworkManager.maxHealth.Value = PlayerStatsManager.CalculateHealthBasedOnVitalityLevel(PlayerNetworkManager.vitality.Value);
            PlayerNetworkManager.currentHealth.Value = PlayerStatsManager.CalculateHealthBasedOnVitalityLevel(PlayerNetworkManager.vitality.Value);
            PlayerUIManager.Instance.playerHUDManager.SetMaxHealthValue(PlayerNetworkManager.maxHealth.Value);
            PlayerNetworkManager.currentHealth.OnValueChanged += PlayerNetworkManager.CheckHPDeath;
            // Stamina
            PlayerNetworkManager.strength.OnValueChanged += PlayerNetworkManager.SetNewStaminaValue;
            PlayerNetworkManager.stamina.OnValueChanged += PlayerUIManager.Instance.playerHUDManager.SetNewStaminaValue;
            PlayerNetworkManager.stamina.OnValueChanged += PlayerStatsManager.ResetStaminaTimer;
            PlayerNetworkManager.maxStamina.Value = PlayerStatsManager.CalculateStaminaBasedOnStrength(PlayerNetworkManager.strength.Value);
            PlayerNetworkManager.stamina.Value = PlayerStatsManager.CalculateStaminaBasedOnStrength(PlayerNetworkManager.strength.Value);
          
            PlayerUIManager.Instance.playerHUDManager.SetMaxStaminaValue(PlayerNetworkManager.maxStamina.Value);

            SaveGameCallbacks.OnSaveGame += SaveCurrentGameData;
            SaveGameCallbacks.OnLoadGame += LoadCurrentGameData;

            // Load the game data if the player is not the server
            if (IsOwner && !IsServer)
                LoadCurrentGameData(ref SaveGameManager.Instance.currentCharacterData);
        }
    }

    private IEnumerator InitializeSingletons()
    {
        yield return new WaitUntil(() => PlayerCamera.Instance != null && PlayerInputManager.Instance != null && PlayerUIManager.Instance != null && SaveGameManager.Instance != null);
    }

    public override void ReviveCharacter()
    {
        base.ReviveCharacter();
        if (IsOwner)
        {
            isDead.Value=false;
            PlayerNetworkManager.currentHealth.Value = PlayerNetworkManager.maxHealth.Value;
            PlayerNetworkManager.stamina.Value = PlayerNetworkManager.maxStamina.Value;
            Debug.Log("doğdum ab");
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

    public override IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
    {
        return base.ProcessDeathEvent(manuallySelectDeathAnimation);
        if (IsOwner)
        {
      
        }
    }

    private void DebugMenu()
    {
        if (respawnCharacter)
        {
            respawnCharacter = false;
            ReviveCharacter();
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
            currentCharacterData.vitality = PlayerNetworkManager.vitality.Value;
            currentCharacterData.strenght = PlayerNetworkManager.strength.Value;
            currentCharacterData.currentHealth=PlayerNetworkManager.currentHealth.Value ;
            currentCharacterData.currentStamina= PlayerNetworkManager.stamina.Value;
        }
        else
            Debug.LogError("PlayerNetworkManager or its characterName is null");
    }

    public void LoadCurrentGameData(ref CharacterSaveData currentCharacterData)
    {
        PlayerNetworkManager.characterName.Value = currentCharacterData.characterName;
        transform.position = new Vector3(currentCharacterData.positionX, currentCharacterData.positionY, currentCharacterData.positionZ);
        PlayerNetworkManager.vitality.Value = currentCharacterData.vitality;
        PlayerNetworkManager.strength.Value = currentCharacterData.strenght;
        PlayerNetworkManager.maxHealth.Value =
            PlayerStatsManager.CalculateHealthBasedOnVitalityLevel(PlayerNetworkManager.vitality.Value);
        PlayerNetworkManager.maxStamina.Value =
            PlayerStatsManager.CalculateStaminaBasedOnStrength(PlayerNetworkManager.strength.Value);
        PlayerNetworkManager.currentHealth.Value = currentCharacterData.currentHealth;
        PlayerNetworkManager.stamina.Value =
            currentCharacterData.currentStamina;
        PlayerUIManager.Instance.playerHUDManager.SetMaxStaminaValue(PlayerNetworkManager.maxStamina.Value);
    }
}
