using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    // Singleton instance
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Settings")]
    [SerializeField] private int maxInventoryCapacity = 10;
    [SerializeField] private int currentMoney = 0;

    // Event system for UI updates
    public event Action OnInventoryChanged;
    public event Action OnMoneyChanged;

    // Inventory content
    private List<FishData> caughtFish = new List<FishData>();

    // Properties
    public int MaxCapacity => maxInventoryCapacity;
    public int CurrentCapacity => caughtFish.Count;
    public bool IsFull => CurrentCapacity >= MaxCapacity;
    public int Money => currentMoney;
    public List<FishData> CaughtFish => caughtFish;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddFish(FishData fish)
    {
        if (IsFull)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        caughtFish.Add(fish);
        OnInventoryChanged?.Invoke();

        Debug.Log($"Added {fish.fishName} to inventory. Value: {fish.value}");
        return true;
    }

    public void SellAllFish()
    {
        int totalValue = 0;

        foreach (FishData fish in caughtFish)
        {
            totalValue += fish.value;
        }

        AddMoney(totalValue);
        caughtFish.Clear();

        OnInventoryChanged?.Invoke();
        Debug.Log($"Sold all fish for {totalValue} coins. Total money: {currentMoney}");
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke();
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke();
            return true;
        }

        Debug.Log("Not enough money!");
        return false;
    }

    public void IncreaseCapacity(int amount)
    {
        maxInventoryCapacity += amount;
        OnInventoryChanged?.Invoke();
    }

    // Method to get the total weight of caught fish
    public float GetTotalWeight()
    {
        float totalWeight = 0f;
        foreach (FishData fish in caughtFish)
        {
            totalWeight += fish.weight;
        }
        return totalWeight;
    }

    // Method to get the heaviest fish
    public FishData GetHeaviestFish()
    {
        if (caughtFish.Count == 0)
            return null;

        FishData heaviest = caughtFish[0];
        foreach (FishData fish in caughtFish)
        {
            if (fish.weight > heaviest.weight)
                heaviest = fish;
        }

        return heaviest;
    }

    // Method to get all caught fish of a specific type
    public List<FishData> GetFishByName(string fishName)
    {
        return caughtFish.FindAll(fish => fish.fishName == fishName);
    }
}