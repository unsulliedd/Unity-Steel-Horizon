using UnityEngine;

public class PlayerManager : CharacterManager
{
    [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
    [HideInInspector] public PlayerAnimationManager PlayerAnimationManager;
    [HideInInspector] public PlayerNetworkManager PlayerNetworkManager;

    protected override void Awake()
    {
        base.Awake();

        playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        PlayerAnimationManager = GetComponent<PlayerAnimationManager>();
        PlayerNetworkManager = GetComponent<PlayerNetworkManager>();
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

            SaveGameEvent.OnSaveGame += OnSaveGameEvent;
            SaveGameEvent.OnLoadGame += OnLoadGameEvent;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsOwner)
        {
            SaveGameEvent.OnSaveGame -= OnSaveGameEvent;
            SaveGameEvent.OnLoadGame -= OnLoadGameEvent;
        }
    }

    // Wrapper methods to handle ref parameters
    private void OnSaveGameEvent(CharacterSaveData data)
    {
        SaveCurrentGameData(ref data);
    }

    private void OnLoadGameEvent(CharacterSaveData data)
    {
        LoadCurrentGameData(ref data);
    }

    public void SaveCurrentGameData(ref CharacterSaveData currentCharacterData)
    {
        currentCharacterData.characterName = PlayerNetworkManager.characterName.Value.ToString();
        currentCharacterData.positionX = transform.position.x;
        currentCharacterData.positionY = transform.position.y;
        currentCharacterData.positionZ = transform.position.z;
    }

    public void LoadCurrentGameData(ref CharacterSaveData currentCharacterData)
    {
        PlayerNetworkManager.characterName.Value = currentCharacterData.characterName;
        transform.position = new Vector3(currentCharacterData.positionX, currentCharacterData.positionY, currentCharacterData.positionZ);
    }
}
