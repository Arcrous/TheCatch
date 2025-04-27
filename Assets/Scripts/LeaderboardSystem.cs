using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardSystem : MonoBehaviour
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
    }

    [Header("Leaderboard Settings")]
    [SerializeField] private List<LeaderboardEntry> fakeEntries = new List<LeaderboardEntry>();
    [SerializeField] private string playerName = "YOU";
    [SerializeField] private int entriesCount = 10;
    [SerializeField] private int playerInitialRank = 8; // Start at 8th place

    [Header("UI")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform entriesContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Button toggleLeaderboardButton;
    [SerializeField] private Button closeLeaderboardButton;

    private List<LeaderboardEntry> sortedEntries = new List<LeaderboardEntry>();
    private LeaderboardEntry playerEntry;

    private void Start()
    {
        // Add button listeners
        if (toggleLeaderboardButton != null)
            toggleLeaderboardButton.onClick.AddListener(() => ToggleLeaderboard(true));

        if (closeLeaderboardButton != null)
            closeLeaderboardButton.onClick.AddListener(() => ToggleLeaderboard(false));

        // Initialize player entry
        playerEntry = new LeaderboardEntry { playerName = playerName, score = 0 };

        // Set initial fake scores
        InitializeFakeScores();

        // Initial state (closed)
        ToggleLeaderboard(false);

        // Subscribe to money change events to update player score
        InventoryManager.Instance.OnMoneyChanged += UpdatePlayerScore;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnMoneyChanged -= UpdatePlayerScore;
    }

    private void InitializeFakeScores()
    {
        // Generate better fake entries if not enough
        while (fakeEntries.Count < entriesCount)
        {
            string fakeName = GetRandomName();
            int fakeScore = Random.Range(50, 500) * 10;

            fakeEntries.Add(new LeaderboardEntry { playerName = fakeName, score = fakeScore });
        }

        // Sort fake entries
        fakeEntries.Sort((a, b) => b.score.CompareTo(a.score));

        // Set initial player score to be just below the target rank
        if (playerInitialRank < fakeEntries.Count)
        {
            playerEntry.score = Mathf.Max(0, fakeEntries[playerInitialRank].score - 50);
        }

        // Update the leaderboard
        UpdateLeaderboard();
    }

    private void UpdatePlayerScore()
    {
        // Update player score based on money
        playerEntry.score = InventoryManager.Instance.Money;

        // Update leaderboard
        UpdateLeaderboard();
    }

    private void UpdateLeaderboard()
    {
        // Clear existing sort list
        sortedEntries.Clear();

        // Add all entries including player
        sortedEntries.AddRange(fakeEntries);
        sortedEntries.Add(playerEntry);

        // Sort by score (descending)
        sortedEntries.Sort((a, b) => b.score.CompareTo(a.score));

        // Limit number of entries
        if (sortedEntries.Count > entriesCount)
            sortedEntries.RemoveRange(entriesCount, sortedEntries.Count - entriesCount);

        // Update UI
        UpdateLeaderboardUI();
    }

    private void UpdateLeaderboardUI()
    {
        // Skip if panel is inactive
        if (!leaderboardPanel.activeSelf) return;

        // Clear existing entries
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new entries
        for (int i = 0; i < sortedEntries.Count; i++)
        {
            LeaderboardEntry entry = sortedEntries[i];
            GameObject entryObject = Instantiate(entryPrefab, entriesContainer);

            // Get Text components
            TextMeshProUGUI[] texts = entryObject.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 3)
            {
                texts[0].text = (i + 1).ToString(); // Rank
                texts[1].text = entry.playerName;
                texts[2].text = entry.score.ToString();

                // Highlight player's entry
                if (entry == playerEntry)
                {
                    foreach (TextMeshProUGUI text in texts)
                    {
                        text.color = Color.yellow;
                        text.fontStyle = FontStyles.Bold;
                    }
                }
            }
        }
    }

    private void ToggleLeaderboard(bool isOpen)
    {
        leaderboardPanel.SetActive(isOpen);

        if (isOpen)
            UpdateLeaderboardUI();
    }

    // Helper method to generate random names
    private string GetRandomName()
    {
        string[] firstNames = { "Alex", "Sam", "Jordan", "Taylor", "Casey", "Riley", "Morgan", "Jamie", "Quinn", "Avery" };
        string[] lastNames = { "Smith", "Jones", "Garcia", "Chen", "Patel", "Kim", "Singh", "Brown", "Lopez" };

        return firstNames[Random.Range(0, firstNames.Length)] + " " +
               lastNames[Random.Range(0, lastNames.Length)];
    }
}