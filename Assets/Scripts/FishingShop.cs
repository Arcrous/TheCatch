using UnityEngine;
using System;

public class FishingShop : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public string description;
        public int basePrice;
        public int maxLevel = 5;
        public float priceMultiplierPerLevel = 1.5f;

        [HideInInspector]
        public int currentLevel = 0;

        public int GetCurrentPrice()
        {
            return Mathf.RoundToInt(basePrice * Mathf.Pow(priceMultiplierPerLevel, currentLevel));
        }

        public bool IsMaxLevel()
        {
            return currentLevel >= maxLevel;
        }
    }

    [Header("Available Upgrades")]
    [SerializeField] private ShopItem hookStrengthUpgrade;
    [SerializeField] private ShopItem reelSpeedUpgrade;
    [SerializeField] private ShopItem inventoryUpgrade;
    [SerializeField] private ShopItem baitQualityUpgrade;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] GameObject sellButton;
    [SerializeField] GameObject shopButton;
    // Events
    public event Action OnShopPurchase;

    private void Start()
    {
        // Find references if not set
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sellButton.SetActive(true);
            shopButton.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sellButton.SetActive(false);
            shopButton.SetActive(false);
        }
    }

    public void PurchaseHookUpgrade()
    {
        if (PurchaseItem(hookStrengthUpgrade))
        {
            // Update player's hook level
            // You'll need to add a method to PlayerController to handle this
            playerController.UpgradeHook();

            Debug.Log($"Hook upgraded to level {hookStrengthUpgrade.currentLevel}");
        }
    }

    public void PurchaseReelSpeedUpgrade()
    {
        if (PurchaseItem(reelSpeedUpgrade))
        {
            // Update player's reel speed
            // You'll need to add a method to PlayerController to handle this
            playerController.UpgradeReelSpeed();

            Debug.Log($"Reel Speed upgraded to level {reelSpeedUpgrade.currentLevel}");
        }
    }

    public void PurchaseInventoryUpgrade()
    {
        if (PurchaseItem(inventoryUpgrade))
        {
            // Increase inventory capacity
            InventoryManager.Instance.IncreaseCapacity(2); // +2 slots per upgrade

            Debug.Log($"Inventory upgraded to level {inventoryUpgrade.currentLevel}");
        }
    }

    public void PurchaseBaitUpgrade()
    {
        if (PurchaseItem(baitQualityUpgrade))
        {
            /// Update player's bait level
            playerController.UpgradeBait();

            Debug.Log($"Bait upgraded to level {baitQualityUpgrade.currentLevel}");
        }
    }

    private bool PurchaseItem(ShopItem item)
    {
        if (item.IsMaxLevel())
        {
            Debug.Log($"{item.itemName} is already at max level!");
            return false;
        }

        int price = item.GetCurrentPrice();
        if (InventoryManager.Instance.SpendMoney(price))
        {
            item.currentLevel++;
            OnShopPurchase?.Invoke();
            return true;
        }

        return false;
    }

    // Getter methods for UI display
    public ShopItem GetHookUpgrade() => hookStrengthUpgrade;
    public ShopItem GetReelUpgrade() => reelSpeedUpgrade;
    public ShopItem GetInventoryUpgrade() => inventoryUpgrade;
    public ShopItem GetBaitUpgrade() => baitQualityUpgrade;
}