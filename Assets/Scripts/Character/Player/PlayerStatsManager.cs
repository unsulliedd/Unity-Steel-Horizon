using UnityEngine;

public class PlayerStatsManager : CharacterStatsManager
{
    [SerializeField] private float staminaRegenTimer = 0f;
    [SerializeField] private float staminaRegenDelay = 2f;
    [SerializeField] private float staminaRegenAmount = 2f;

    private PlayerManager playerManager;

    // Awake is called when the script instance is being loaded.
    override protected void Awake()
    {
        base.Awake();
        playerManager = GetComponent<PlayerManager>();
    }

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        RegenerateStamina();
    }

    // Calculates stamina based on the strength attribute
    public int CalculateStaminaBasedOnStrength(int strength)
    {
        int stamina = 100 + (strength * 10); // Base stamina is 100 plus 10 per strength point
        return Mathf.RoundToInt(stamina);
    }

    // Handles stamina regeneration logic
    public void RegenerateStamina()
    {
        // Check if the player is the owner of this character instance
        if (!playerManager.IsOwner) return;

        // Check if the player is sprinting or performing an action
        if (playerManager.PlayerNetworkManager.isSprinting.Value) return;
        if (playerManager.isPerformingAction) return;

        // Increment the regeneration timer
        staminaRegenTimer += Time.deltaTime;

        // Check if the delay for regeneration has passed
        if (staminaRegenTimer >= staminaRegenDelay)
        {
            // Regenerate stamina if it is below the maximum value
            if (playerManager.PlayerNetworkManager.stamina.Value < playerManager.PlayerNetworkManager.maxStamina.Value)
            {
                playerManager.PlayerNetworkManager.stamina.Value += staminaRegenAmount;
                // Clamp the stamina to ensure it doesn't exceed the maximum value
                playerManager.PlayerNetworkManager.stamina.Value = Mathf.Clamp(playerManager.PlayerNetworkManager.stamina.Value, 0, playerManager.PlayerNetworkManager.maxStamina.Value);
            }
            // Reset the regeneration timer after stamina has been regenerated
            staminaRegenTimer = 0f;
        }
    }

    public void ResetStaminaTimer(float previousStamina, float currentStamina)
    {         
        if (previousStamina < currentStamina)
            staminaRegenTimer = 0;
    }
}
