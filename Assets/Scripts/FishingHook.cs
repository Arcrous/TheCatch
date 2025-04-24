using UnityEngine;

public class FishingHook : MonoBehaviour
{
    [SerializeField] private float descendSpeed = 3f;
    [SerializeField] private float defaultRetractSpeed = 5f;
    [SerializeField] private float maxLineLength = 15f;
    [SerializeField] private LineRenderer fishingLine;

    private PlayerController player;
    private Vector2 startPosition;
    private float castPower;
    private bool isDescending = true;
    private bool isRetracting = false;
    private bool hasCaughtFish = false;
    private FishData caughtFish;
    private float retractSpeed;

    public void Initialize(PlayerController playerRef, float power)
    {
        player = playerRef;
        startPosition = playerRef.transform.position;
        castPower = power;
        retractSpeed = defaultRetractSpeed;

        // Set up fishing line
        if (fishingLine != null)
        {
            fishingLine.positionCount = 2;
            fishingLine.SetPosition(0, startPosition);
            fishingLine.SetPosition(1, transform.position);
        }
    }

    private void Update()
    {
        UpdateLinePosition();

        if (isDescending)
        {
            // Move downward
            transform.Translate(Vector2.down * descendSpeed * Time.deltaTime);

            // Check if max length reached
            if (Vector2.Distance(startPosition, transform.position) >= maxLineLength)
            {
                StartRetracting();
            }
        }
        else if (isRetracting)
        {
            // Move back to player
            Vector2 playerPos = player.transform.position;
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerPos,
                retractSpeed * Time.deltaTime);

            // Check if reached player
            if (Vector2.Distance(playerPos, transform.position) < 0.1f)
            {
                CompleteFishing();
            }
        }
    }

    private void UpdateLinePosition()
    {
        if (fishingLine != null && player != null)
        {
            fishingLine.SetPosition(0, player.transform.position);
            fishingLine.SetPosition(1, transform.position);
        }
    }

    public void StartRetracting()
    {
        isDescending = false;
        isRetracting = true;
    }

    private void CompleteFishing()
    {
        player.OnFishingComplete(hasCaughtFish, caughtFish);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDescending && collision.CompareTag("Fish"))
        {
            /*  //Fish fish = collision.GetComponent<Fish>();
             if (fish != null)
             {
                 // Catch the fish
                 hasCaughtFish = true;
                 caughtFish = fish.fishData;

                 // Adjust retract speed based on fish weight
                 retractSpeed = defaultRetractSpeed / fish.fishData.weight;

                 // Attach fish to hook
                 collision.transform.SetParent(transform);

                 // Start retracting
                 StartRetracting();
             } */
        }
    }
}