using UnityEngine;
using UnityEngine.UI;

public class TimingMinigame : CookingMinigameBase
{
    //private void Start() => StartMinigame();

    [Header("Meter")]
    [SerializeField] private RectTransform meterBar;
    [SerializeField] private RectTransform arrowIndicator;
    [SerializeField] private float arrowSpeed = 0.4f;

    [Header("Green Zone")]
    [SerializeField] private RectTransform greenZone;
    [SerializeField] private float greenZoneCenter = 0.5f;
    [SerializeField] private float greenZoneWidth = 0.25f;

    [SerializeField] private string objectiveKey = "cooking";

    private float arrowPosition;
    private float arrowDirection = 1f;
    private bool isActive;

    public override void StartMinigame()
    {
        arrowPosition = 0f;
        arrowDirection = 1f;
        isActive = true;
        minigamePanel.SetActive(true);
        PositionGreenZone();
    }

    private void Update()
    {
        if (!isActive) return;

        arrowPosition += arrowDirection * arrowSpeed * Time.deltaTime;

        if (arrowPosition >= 1f) { arrowPosition = 1f; arrowDirection = -1f; }
        else if (arrowPosition <= 0f) { arrowPosition = 0f; arrowDirection = 1f; }

        PositionArrow();
    }

    public void OnButtonClick()
    {
        if (!isActive) return;

        float greenMin = greenZoneCenter - greenZoneWidth * 0.5f;
        float greenMax = greenZoneCenter + greenZoneWidth * 0.5f;
        bool inGreen = arrowPosition >= greenMin && arrowPosition <= greenMax;

        isActive = false;
        minigamePanel.SetActive(false);

        if (inGreen)
        {
            Debug.Log("Successful cooking");
            ObjectiveManager.Instance?.CompleteObjective(objectiveKey);
            RaiseSuccess();
        }
        else
        {
            Debug.Log("Cooking Failed");
            RaiseFail();
        }
    }

    private void PositionArrow()
    {
        if (arrowIndicator == null) return;
        float barWidth = meterBar.rect.width;
        float x = Mathf.Lerp(-barWidth * 0.5f, barWidth * 0.5f, arrowPosition);
        arrowIndicator.anchoredPosition = new Vector2(x, arrowIndicator.anchoredPosition.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (meterBar == null) return;

        Vector3[] corners = new Vector3[4];
        meterBar.GetWorldCorners(corners);

        float barWorldWidth = corners[2].x - corners[0].x;
        float greenMin = greenZoneCenter - greenZoneWidth * 0.5f;
        float greenMax = greenZoneCenter + greenZoneWidth * 0.5f;

        Vector3 botLeft = new Vector3(corners[0].x + barWorldWidth * greenMin, corners[0].y, corners[0].z);
        Vector3 topRight = new Vector3(corners[0].x + barWorldWidth * greenMax, corners[1].y, corners[1].z);
        Vector3 center = (botLeft + topRight) * 0.5f;
        Vector3 size = topRight - botLeft;

        Gizmos.color = new Color(0f, 1f, 0f, 0.35f);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }

    private void PositionGreenZone()
    {
        if (greenZone == null) return;
        greenZone.anchorMin = new Vector2(greenZoneCenter - greenZoneWidth * 0.5f, 0f);
        greenZone.anchorMax = new Vector2(greenZoneCenter + greenZoneWidth * 0.5f, 1f);
        greenZone.offsetMin = Vector2.zero;
        greenZone.offsetMax = Vector2.zero;
    }
}
