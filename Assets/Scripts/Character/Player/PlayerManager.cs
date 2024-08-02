using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Netcode;

public class PlayerManager : CharacterManager
{
    [Header("DebugMenu")]
    [SerializeField] private bool respawnCharacter = false;
    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerAnimationManager PlayerAnimationManager;
    [HideInInspector] public PlayerNetworkManager PlayerNetworkManager;
    [HideInInspector] public PlayerStatsManager PlayerStatsManager;
    [HideInInspector] public PlayerCombatManager PlayerCombatManager;
    [HideInInspector] public InventoryManager InventoryManager;
    [HideInInspector] public CraftingManager CraftingManager;
    public PlayerClass playerClass;

    protected override void Awake()
    {
        base.Awake();

        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        PlayerAnimationManager = GetComponent<PlayerAnimationManager>();
        PlayerNetworkManager = GetComponent<PlayerNetworkManager>();
        PlayerStatsManager = GetComponent<PlayerStatsManager>();
        PlayerCombatManager = GetComponent<PlayerCombatManager>();
        InventoryManager = GetComponent<InventoryManager>();
        CraftingManager = GetComponent<CraftingManager>();
    }

    protected override void Start()
    {
        base.Start();

        if (IsOwner)
            InitializePlayerStats();
    }

    protected override void Update()
    {
        base.Update();

        // Prevent the player from controlling other players
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
            PlayerUIManager.Instance.playerManager = this;
            SaveGameManager.Instance.playerManager = this;

            // Update stats when they change
            PlayerNetworkManager.vitality.OnValueChanged += PlayerNetworkManager.SetNewHealthValue;
            PlayerNetworkManager.strength.OnValueChanged += PlayerNetworkManager.SetNewStaminaValue;

            // Health
            PlayerNetworkManager.health.OnValueChanged += PlayerUIManager.Instance.playerHUDManager.SetNewHealthValue;

            // Stamina
            PlayerNetworkManager.stamina.OnValueChanged += PlayerUIManager.Instance.playerHUDManager.SetNewStaminaValue;
            PlayerNetworkManager.stamina.OnValueChanged += PlayerStatsManager.ResetStaminaTimer;

            SaveGameCallbacks.OnSaveGame += SaveCurrentGameData;
            SaveGameCallbacks.OnLoadGame += LoadCurrentGameData;

            // Load the game data if the player is not the server
            if (IsOwner)
                LoadCurrentGameData(ref SaveGameManager.Instance.currentCharacterData);
        }

        PlayerNetworkManager.health.OnValueChanged += PlayerNetworkManager.CheckHPDeath;
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
            PlayerNetworkManager.isDead.Value = false;
            PlayerNetworkManager.health.Value = PlayerNetworkManager.maxHealth.Value;
            PlayerNetworkManager.stamina.Value = PlayerNetworkManager.maxStamina.Value;
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

    private void InitializePlayerStats()
    {
        PlayerNetworkManager.strength.Value = playerClass.baseStrength;
        PlayerNetworkManager.stamina.Value = playerClass.stamina;
        PlayerNetworkManager.maxStamina.Value = playerClass.stamina;
        PlayerNetworkManager.strength.Value = playerClass.baseStrength;
        PlayerNetworkManager.vitality.Value = playerClass.baseVitality;
        PlayerNetworkManager.health.Value = playerClass.health;
        PlayerNetworkManager.luck.Value = playerClass.baseLuck;
    }

    public override IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
    {
        return base.ProcessDeathEvent(manuallySelectDeathAnimation);
    }

    private void DebugMenu()
    {
        if (respawnCharacter)
        {
            respawnCharacter = false;
            ReviveCharacter();
        }
    }

    public void SetCharacterData(CharacterSaveData characterData)
    {
        InitializePlayerStats();
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

            currentCharacterData.characterClass = playerClass.className;

            currentCharacterData.vitality = PlayerNetworkManager.vitality.Value;
            currentCharacterData.strength = PlayerNetworkManager.strength.Value;

            currentCharacterData.currentHealth = PlayerNetworkManager.health.Value;
            currentCharacterData.currentStamina = PlayerNetworkManager.stamina.Value;

            currentCharacterData.ownedWeapons = InventoryManager.ownedWeapons;
            currentCharacterData.ownedAmmo = InventoryManager.ammo;
            currentCharacterData.ownedJunkAmount = InventoryManager.junkAmount;
            currentCharacterData.ownedChipAmount = InventoryManager.chips;
            currentCharacterData.ownedGearAmount = InventoryManager.gears;
            currentCharacterData.ownedCableAmount = InventoryManager.cables;
            currentCharacterData.ownedPipeAmount = InventoryManager.pipes;
        }
        else
            Debug.LogError("PlayerNetworkManager or its characterName is null");
    }

    public void LoadCurrentGameData(ref CharacterSaveData currentCharacterData)
    {
        PlayerNetworkManager.characterName.Value = currentCharacterData.characterName;
        transform.position = new Vector3(currentCharacterData.positionX, currentCharacterData.positionY, currentCharacterData.positionZ);

        PlayerNetworkManager.vitality.Value = currentCharacterData.vitality;
        PlayerNetworkManager.strength.Value = currentCharacterData.strength;

        PlayerNetworkManager.maxHealth.Value = PlayerStatsManager.CalculateHealthBasedOnVitalityLevel(PlayerNetworkManager.vitality.Value);
        PlayerNetworkManager.maxStamina.Value = PlayerStatsManager.CalculateStaminaBasedOnStrength(PlayerNetworkManager.strength.Value);

        PlayerNetworkManager.health.Value = PlayerNetworkManager.health.Value;
        PlayerNetworkManager.stamina.Value = PlayerNetworkManager.stamina.Value;

        PlayerUIManager.Instance.playerHUDManager.SetMaxHealthValue(PlayerNetworkManager.maxHealth.Value);
        PlayerUIManager.Instance.playerHUDManager.SetMaxStaminaValue(PlayerNetworkManager.maxStamina.Value);

        InventoryManager.ownedWeapons = currentCharacterData.ownedWeapons;
        InventoryManager.ammo = currentCharacterData.ownedAmmo;
        InventoryManager.junkAmount = currentCharacterData.ownedJunkAmount;
        InventoryManager.chips = currentCharacterData.ownedChipAmount;
        InventoryManager.gears = currentCharacterData.ownedGearAmount;
        InventoryManager.cables = currentCharacterData.ownedCableAmount;
        InventoryManager.pipes = currentCharacterData.ownedPipeAmount;
    }
}