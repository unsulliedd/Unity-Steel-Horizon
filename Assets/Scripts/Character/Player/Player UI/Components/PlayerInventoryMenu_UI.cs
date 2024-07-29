using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the player's inventory UI, including weapon slots and crafting interface.
/// </summary>
public class PlayerInventoryMenu_UI : MonoBehaviour
{
    [Header("Inventory Weapons")]
    [SerializeField] private List<Button> weaponSlotsButton;
    [SerializeField] private List<Image> weaponSlotsImage;
    [SerializeField] private Sprite emptySlotSprite;

    [Header("Inventory Items")]
    [SerializeField] private TextMeshProUGUI junkAmountText;
    [SerializeField] private TextMeshProUGUI chipsAmountText;
    [SerializeField] private TextMeshProUGUI cablesAmountText;
    [SerializeField] private TextMeshProUGUI gearsAmountText;
    [SerializeField] private TextMeshProUGUI pipesAmountText;

    [Header("Crafting UI")]
    [SerializeField] private GameObject craftingCostPanel;
    [SerializeField] private GameObject decomposeJunkPanel;

    [Header("Crafting Costs")]
    [SerializeField] private TextMeshProUGUI craftingGearCostText;
    [SerializeField] private TextMeshProUGUI craftingCableCostText;
    [SerializeField] private TextMeshProUGUI craftingChipCostText;
    [SerializeField] private TextMeshProUGUI craftingPipeCostText;
    [SerializeField] private TextMeshProUGUI junkDecomposeResultText;

    [Header("Crafting Preview")]
    [SerializeField] private Image previewCurrentCraftingItemSprite;
    [SerializeField] private Image previewCraftedItemSprite;
    [SerializeField] private Sprite previewJunkSprite;
    [SerializeField] private Sprite emptyPreviewSprite;

    [Header("Crafting Buttons")]
    public Button craftWeaponButton;
    public Button decomposeJunkButton;

    /// <summary>
    /// Initializes the inventory UI on start.
    /// </summary>
    private void Start()
    {
        UpdateInventoryUI();
    }

    /// <summary>
    /// Subscribes to the inventory update event when the object is enabled.
    /// </summary>
    private void OnEnable()
    {
        PlayerUIManager.Instance.playerManager.InventoryManager.OnInventoryUpdated += UpdateInventoryUI;
    }

    /// <summary>
    /// Unsubscribes from the inventory update event when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        PlayerUIManager.Instance.playerManager.InventoryManager.OnInventoryUpdated -= UpdateInventoryUI;
    }

    /// <summary>
    /// Updates the inventory UI elements.
    /// </summary>
    public void UpdateInventoryUI()
    {
        var inventoryManager = PlayerUIManager.Instance.playerManager.InventoryManager;

        for (int i = 0; i < weaponSlotsButton.Count; i++)
        {
            if (i < inventoryManager.ownedWeapons.Count)
            {
                Weapon weapon = inventoryManager.ownedWeapons[i];
                weaponSlotsImage[i].sprite = weapon.weaponIcon;
                weaponSlotsImage[i].color = Color.white;
                weaponSlotsButton[i].onClick.RemoveAllListeners();
                weaponSlotsButton[i].onClick.AddListener(() => OnWeaponSlotClicked(weapon));
                AddRightClickEvent(weaponSlotsButton[i], weapon);
            }
            else
            {
                weaponSlotsImage[i].sprite = emptySlotSprite;
                weaponSlotsImage[i].color = new Color(1, 1, 1, 0);
                weaponSlotsButton[i].onClick.RemoveAllListeners();
            }
        }

        junkAmountText.text = inventoryManager.junkAmount.ToString();
        chipsAmountText.text = inventoryManager.chips.ToString();
        cablesAmountText.text = inventoryManager.cables.ToString();
        gearsAmountText.text = inventoryManager.gears.ToString();
        pipesAmountText.text = inventoryManager.pipes.ToString();
    }

    /// <summary>
    /// Handles weapon slot click events.
    /// </summary>
    private void OnWeaponSlotClicked(Weapon weapon)
    {
        var craftingManager = PlayerUIManager.Instance.playerManager.CraftingManager;
        craftingManager.SelectWeaponForCrafting(weapon);
        UpdateCraftingUI(weapon);
        craftWeaponButton.interactable = true;
        decomposeJunkButton.interactable = false;
    }

    /// <summary>
    /// Handles junk slot click events.
    /// </summary>
    public void OnJunkSlotClicked()
    {
        UpdateJunkDecomposeUI();
        craftWeaponButton.interactable = false;

        var inventoryManager = PlayerUIManager.Instance.playerManager.InventoryManager;
        decomposeJunkButton.interactable = inventoryManager.junkAmount > 0;
    }

    /// <summary>
    /// Handles the craft button click event.
    /// </summary>
    public void OnCraftButtonClick()
    {
        PlayerUIManager.Instance.playerManager.CraftingManager.TryCraftWeaponUpgrade();
        UpdateInventoryUI();
        ResetCraftingUI();
    }

    /// <summary>
    /// Handles the decompose junk button click event.
    /// </summary>
    public void OnDecomposeJunkButtonClick()
    {
        PlayerUIManager.Instance.playerManager.CraftingManager.DecomposeJunk();
        UpdateInventoryUI();
        decomposeJunkButton.interactable = false;
    }

    /// <summary>
    /// Updates the crafting UI elements.
    /// </summary>
    public void UpdateCraftingUI(Weapon selectedWeapon)
    {
        craftingCostPanel.SetActive(true);
        decomposeJunkPanel.SetActive(false);

        if (selectedWeapon != null)
        {
            previewCurrentCraftingItemSprite.sprite = selectedWeapon.weaponIcon;
            previewCurrentCraftingItemSprite.color = Color.white;

            var inventoryManager = PlayerUIManager.Instance.playerManager.InventoryManager;
            WeaponRarity nextRarity = inventoryManager.GetNextRarity(selectedWeapon.rarity);
            Weapon nextWeapon = FindWeaponByRarity(nextRarity);

            if (nextWeapon != null)
            {
                previewCraftedItemSprite.sprite = nextWeapon.weaponIcon;
                previewCraftedItemSprite.color = Color.white;
                var cost = selectedWeapon.GetUpgradeCost(selectedWeapon.rarity);
                UpdateCraftingCostText(cost);
            }
        }
        else
            ResetCraftingUI();
    }

    /// <summary>
    /// Updates the UI for junk decomposition.
    /// </summary>
    public void UpdateJunkDecomposeUI()
    {
        craftingCostPanel.SetActive(false);
        decomposeJunkPanel.SetActive(true);
        previewCurrentCraftingItemSprite.sprite = previewJunkSprite;
        previewCurrentCraftingItemSprite.color = Color.white;
        previewCraftedItemSprite.sprite = emptyPreviewSprite;
        previewCraftedItemSprite.color = new Color(1, 1, 1, 0);
        junkDecomposeResultText.text = "Junk will be decomposed into random materials.";
    }

    /// <summary>
    /// Shows a message indicating insufficient materials for crafting.
    /// </summary>
    public void ShowNotEnoughMaterialsMessage()
    {
        craftingCostPanel.SetActive(false);
        decomposeJunkPanel.SetActive(true);
        junkDecomposeResultText.text = "Not enough materials to craft the weapon.";
    }

    /// <summary>
    /// Finds a weapon by its rarity.
    /// </summary>
    private Weapon FindWeaponByRarity(WeaponRarity rarity) => PlayerUIManager.Instance.playerManager.WeaponManager.weapons.Find(w => w.rarity == rarity);

    /// <summary>
    /// Handles right-click events on weapon slots to equip the weapon.
    /// </summary>
    private void OnWeaponSlotRightClicked(Weapon weapon)
    {
        PlayerUIManager.Instance.playerManager.WeaponManager.EquipWeapon(weapon.weaponID);
    }

    /// <summary>
    /// Adds a right-click event listener to a button.
    /// </summary>
    private void AddRightClickEvent(Button button, Weapon weapon)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener((eventData) =>
        {
            if ((eventData as PointerEventData).button == PointerEventData.InputButton.Right)
                OnWeaponSlotRightClicked(weapon);
        });
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// Updates the crafting cost text fields.
    /// </summary>
    private void UpdateCraftingCostText(MaterialCost cost)
    {
        var inventoryManager = PlayerUIManager.Instance.playerManager.InventoryManager;
        craftingCableCostText.text = $"Cables: {cost.cables} / {inventoryManager.cables}";
        craftingChipCostText.text = $"Chips: {cost.chips} / {inventoryManager.chips}";
        craftingGearCostText.text = $"Gears: {cost.gears} / {inventoryManager.gears}";
        craftingPipeCostText.text = $"Pipes: {cost.pipes} / {inventoryManager.pipes}";
    }

    /// <summary>
    /// Resets the crafting UI to default state.
    /// </summary>
    private void ResetCraftingUI()
    {
        previewCurrentCraftingItemSprite.sprite = emptyPreviewSprite;
        previewCurrentCraftingItemSprite.color = new Color(1, 1, 1, 0);
        previewCraftedItemSprite.sprite = emptyPreviewSprite;
        previewCraftedItemSprite.color = new Color(1, 1, 1, 0);
        craftingCableCostText.text = "Cables: ";
        craftingChipCostText.text = "Chips: ";
        craftingGearCostText.text = "Gears: ";
        craftingPipeCostText.text = "Pipes: ";
    }
}