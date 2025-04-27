using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory UI")]
    [SerializeField] private Transform fishContainer;
    [SerializeField] private GameObject fishItemPrefab;
    [SerializeField] private TextMeshProUGUI capacityText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Button sellButton;

    [Header("Shop UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button openShopButton;
    [SerializeField] private Button closeShopButton;

    // Shop item buttons
    [SerializeField] private Button hookUpgradeButton;
    [SerializeField] private Button reelUpgradeButton;
    [SerializeField] private Button inventoryUpgradeButton;
    [SerializeField] private Button baitUpgradeButton;

    // Shop item texts
    [SerializeField] private TextMeshProUGUI hookUpgradeText;
    [SerializeField] private TextMeshProUGUI reelUpgradeText;
    [SerializeField] private TextMeshProUGUI inventoryUpgradeText;
    [SerializeField] private TextMeshProUGUI baitUpgradeText;

    // References
    private InventoryManager inventoryManager;
    private FishingShop fishingShop;

    private void Start()
    {
        inventoryManager = InventoryManager.Instance;
        fishingShop = FindObjectOfType<FishingShop>();

        sellButton.gameObject.SetActive(false);
        openShopButton.gameObject.SetActive(false);

        capacityText.text = $"Inventory: {inventoryManager.CurrentCapacity}/{inventoryManager.MaxCapacity}";
        moneyText.text = $"Money: ${inventoryManager.Money}";

        // Subscribe to events
        inventoryManager.OnInventoryChanged += UpdateInventoryUI;
        inventoryManager.OnMoneyChanged += UpdateMoneyUI;

        if (fishingShop != null)
            fishingShop.OnShopPurchase += UpdateShopUI;

        // Button listeners
        sellButton.onClick.AddListener(SellAllFish);
        openShopButton.onClick.AddListener(() => ToggleShop(true));
        closeShopButton.onClick.AddListener(() => ToggleShop(false));

        // Shop buttons
        if (fishingShop != null)
        {
            hookUpgradeButton.onClick.AddListener(fishingShop.PurchaseHookUpgrade);
            reelUpgradeButton.onClick.AddListener(fishingShop.PurchaseReelSpeedUpgrade);
            inventoryUpgradeButton.onClick.AddListener(fishingShop.PurchaseInventoryUpgrade);
            baitUpgradeButton.onClick.AddListener(fishingShop.PurchaseBaitUpgrade);
        }

        // Initial shop state (closed)
        ToggleShop(false);

        // Initial UI update
        UpdateInventoryUI();
        UpdateMoneyUI();
        UpdateShopUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= UpdateInventoryUI;
            inventoryManager.OnMoneyChanged -= UpdateMoneyUI;
        }

        if (fishingShop != null)
            fishingShop.OnShopPurchase -= UpdateShopUI;
    }

    private void UpdateInventoryUI()
    {
        // Clear existing fish items
        foreach (Transform child in fishContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new fish items
        foreach (FishData fish in inventoryManager.CaughtFish)
        {
            GameObject fishItem = Instantiate(fishItemPrefab, fishContainer);
            FishItemUI fishItemUI = fishItem.GetComponent<FishItemUI>();

            if (fishItemUI != null)
                fishItemUI.SetFishData(fish);
        }

        // Update capacity text
        capacityText.text = $"Inventory: {inventoryManager.CurrentCapacity}/{inventoryManager.MaxCapacity}";

        // Enable/disable sell button based on inventory content
        sellButton.interactable = inventoryManager.CurrentCapacity > 0;
    }

    private void UpdateMoneyUI()
    {
        moneyText.text = $"Money: ${inventoryManager.Money}";
    }

    private void UpdateShopUI()
    {
        if (fishingShop == null) return;

        // Update hook upgrade
        var hookUpgrade = fishingShop.GetHookUpgrade();
        hookUpgradeText.text = hookUpgrade.IsMaxLevel() ?
            $"{hookUpgrade.itemName} (MAX)" :
            $"{hookUpgrade.itemName} Lv.{hookUpgrade.currentLevel + 1}   Cost: ${hookUpgrade.GetCurrentPrice()}";
        hookUpgradeButton.interactable = !hookUpgrade.IsMaxLevel() &&
                                         inventoryManager.Money >= hookUpgrade.GetCurrentPrice();

        // Update reel upgrade
        var reelUpgrade = fishingShop.GetReelUpgrade();
        reelUpgradeText.text = reelUpgrade.IsMaxLevel() ?
            $"{reelUpgrade.itemName} (MAX)" :
            $"{reelUpgrade.itemName} Lv.{reelUpgrade.currentLevel + 1}   Cost: ${reelUpgrade.GetCurrentPrice()}";
        reelUpgradeButton.interactable = !reelUpgrade.IsMaxLevel() &&
                                        inventoryManager.Money >= reelUpgrade.GetCurrentPrice();

        // Update inventory upgrade
        var invUpgrade = fishingShop.GetInventoryUpgrade();
        inventoryUpgradeText.text = invUpgrade.IsMaxLevel() ?
            $"{invUpgrade.itemName} (MAX)" :
            $"{invUpgrade.itemName} Lv.{invUpgrade.currentLevel + 1}   Cost: ${invUpgrade.GetCurrentPrice()}";
        inventoryUpgradeButton.interactable = !invUpgrade.IsMaxLevel() &&
                                             inventoryManager.Money >= invUpgrade.GetCurrentPrice();

        // Update bait upgrade
        var baitUpgrade = fishingShop.GetBaitUpgrade();
        baitUpgradeText.text = baitUpgrade.IsMaxLevel() ?
            $"{baitUpgrade.itemName} (MAX)" :
            $"{baitUpgrade.itemName} Lv.{baitUpgrade.currentLevel + 1}   Cost: ${baitUpgrade.GetCurrentPrice()}";
        baitUpgradeButton.interactable = !baitUpgrade.IsMaxLevel() &&
                                        inventoryManager.Money >= baitUpgrade.GetCurrentPrice();
    }

    private void SellAllFish()
    {
        inventoryManager.SellAllFish();
    }

    private void ToggleShop(bool isOpen)
    {
        shopPanel.SetActive(isOpen);

        // Optional: pause the game while shop is open
        // Time.timeScale = isOpen ? 0 : 1;
    }
}