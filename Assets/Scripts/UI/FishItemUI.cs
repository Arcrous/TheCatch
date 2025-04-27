using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FishItemUI : MonoBehaviour
{
    [SerializeField] private Image fishIcon;
    [SerializeField] private TextMeshProUGUI fishNameText;
    [SerializeField] private TextMeshProUGUI fishValueText;
    [SerializeField] private TextMeshProUGUI fishWeightText;

    public void SetFishData(FishData fish)
    {
        if (fish == null) return;

        // Set fish sprite
        if (fishIcon != null && fish.fishSprite != null)
        {
            fishIcon.sprite = fish.fishSprite;

            // Set color tint for special fish
            fishIcon.color = fish.isSpecial ? Color.yellow : Color.white;
        }

        // Set text fields
        if (fishNameText != null)
            fishNameText.text = fish.fishName;

        if (fishValueText != null)
            fishValueText.text = $"${fish.value}";

        if (fishWeightText != null)
            fishWeightText.text = $"{fish.weight:F1}kg";
    }
}