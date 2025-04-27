using UnityEngine;

[CreateAssetMenu(fileName = "New Fish", menuName = "Fishing Game/Fish Data")]
public class FishData : ScriptableObject
{
    [Header("Basic Info")]
    public string fishName;
    public Sprite fishSprite;
    public int value;
    public float weight = 1f;
    public float baseSpeed = 2f;
    public float skittishness = 0.3f; // Chance to flee from hook

    [Header("Special Properties")]
    public bool isSpecial = false;
    public bool isAggressive = false;
    public int requiredHookLevel = 1;

    [Header("Visual")]
    public Color fishColor = Color.white;
    public Vector2 sizeRange = new Vector2(0.8f, 1.2f);

    [Header("Spawning")]
    public float spawnChance = 1f;
    public Vector2 depthRange = new Vector2(-5f, -15f);
}