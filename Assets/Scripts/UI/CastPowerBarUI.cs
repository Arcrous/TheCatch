using UnityEngine;
using UnityEngine.UI;

public class CastPowerBarUI : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Hide bar initially
        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        if (player.isChargingCast)
        {
            canvasGroup.alpha = 1;
            powerSlider.value = player.GetCurrentCastPower();
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }
}