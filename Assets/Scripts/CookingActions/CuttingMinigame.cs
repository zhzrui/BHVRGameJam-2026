using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class CuttingMinigame : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    //private void Start() => StartMinigame();

    [Header("Ingredient")]
    [SerializeField] private Texture2D ingredientTexture;
    [SerializeField] private RectTransform ingredientContainer; // fixed rect representing the full ingredient bounds

    [Header("Cut Settings")]
    [SerializeField] private float[] cutPositions = { 0.33f, 0.66f }; // normalized X positions (0 = left edge, 1 = right edge)
    [SerializeField] private float cutLineTolerance = 20f;             // pixels either side of the line that accept input
    [SerializeField] private float requiredCutDistance = 80f;          // vertical pixels to drag to complete a cut

    [Header("Visuals")]
    [SerializeField] private RectTransform cutLineIndicator;  // thin Image child of ingredientContainer
    [SerializeField] private float sliceSeparation = 8f;     // pixels each completed slice drifts left
    [SerializeField] private GameObject minigamePanel;

    public event Action OnSuccess;

    private int currentCutIndex;
    private bool isCutting;
    private float cuttingProgress;
    private float currentRightStart; // normalized X where the still-uncut portion begins
    private RawImage mainDisplay;
    private List<RawImage> completedSlices = new();

    public void StartMinigame()
    {
        currentCutIndex = 0;
        currentRightStart = 0f;
        isCutting = false;
        cuttingProgress = 0f;

        foreach (var slice in completedSlices)
            if (slice != null) Destroy(slice.gameObject);
        completedSlices.Clear();

        if (mainDisplay == null)
        {
            mainDisplay = new GameObject("IngredientMain").AddComponent<RawImage>();
            mainDisplay.transform.SetParent(ingredientContainer, false);
        }

        mainDisplay.texture = ingredientTexture;
        SetAnchoredSlice(mainDisplay.rectTransform, 0f, 1f, 0f);
        mainDisplay.uvRect = new Rect(0, 0, 1, 1);

        minigamePanel.SetActive(true);
        RefreshCutLine();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentCutIndex >= cutPositions.Length) return;

        float normX = PointerToNormalizedX(eventData.position, eventData.pressEventCamera);
        float tolerance = cutLineTolerance / ingredientContainer.rect.width;

        if (Mathf.Abs(normX - cutPositions[currentCutIndex]) <= tolerance)
        {
            isCutting = true;
            cuttingProgress = 0f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isCutting) return;

        cuttingProgress += Mathf.Abs(eventData.delta.y);
        if (cuttingProgress >= requiredCutDistance)
            CompleteCut();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isCutting = false;
        cuttingProgress = 0f;
    }

    private void CompleteCut()
    {
        isCutting = false;

        float cutX = cutPositions[currentCutIndex];
        int sliceNumber = completedSlices.Count + 1;

        // Freeze the left portion as a separated slice that drifts left
        RawImage slice = new GameObject($"Slice_{sliceNumber}").AddComponent<RawImage>();
        slice.transform.SetParent(ingredientContainer, false);
        slice.texture = ingredientTexture;
        slice.uvRect = new Rect(currentRightStart, 0, cutX - currentRightStart, 1);
        SetAnchoredSlice(slice.rectTransform, currentRightStart, cutX, 0f);
        completedSlices.Add(slice);

        // Shift all slices (including the new one) one step further left
        for (int i = 0; i < completedSlices.Count; i++)
            SetAnchoredSlice(completedSlices[i].rectTransform,
                completedSlices[i].uvRect.x,
                completedSlices[i].uvRect.x + completedSlices[i].uvRect.width,
                -sliceSeparation * (completedSlices.Count - i));

        // Shift main display to start at the new cut
        currentRightStart = cutX;
        mainDisplay.uvRect = new Rect(cutX, 0, 1f - cutX, 1);
        SetAnchoredSlice(mainDisplay.rectTransform, cutX, 1f, 0f);

        currentCutIndex++;

        if (currentCutIndex >= cutPositions.Length)
            Complete();
        else
            RefreshCutLine();
    }

    // Places a RectTransform as a horizontal band between fromNorm and toNorm, offset by xPixels
    private void SetAnchoredSlice(RectTransform rt, float fromNorm, float toNorm, float xPixels)
    {
        rt.anchorMin = new Vector2(fromNorm, 0f);
        rt.anchorMax = new Vector2(toNorm, 1f);
        rt.offsetMin = new Vector2(xPixels, 0f);
        rt.offsetMax = new Vector2(xPixels, 0f);
    }

    private float PointerToNormalizedX(Vector2 screenPos, Camera cam)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ingredientContainer, screenPos, cam, out Vector2 local);
        float width = ingredientContainer.rect.width;
        return (local.x + width * 0.5f) / width;
    }

    private void RefreshCutLine()
    {
        if (cutLineIndicator == null) return;

        if (currentCutIndex >= cutPositions.Length)
        {
            cutLineIndicator.gameObject.SetActive(false);
            return;
        }

        float cutX = cutPositions[currentCutIndex];
        cutLineIndicator.anchorMin = new Vector2(cutX, 0f);
        cutLineIndicator.anchorMax = new Vector2(cutX, 1f);
        cutLineIndicator.sizeDelta = new Vector2(4f, 0f); // 4px wide dashed line
        cutLineIndicator.anchoredPosition = Vector2.zero;
        cutLineIndicator.SetAsLastSibling();
        cutLineIndicator.gameObject.SetActive(true);
    }

    private void Complete()
    {
        cutLineIndicator?.gameObject.SetActive(false);
        OnSuccess?.Invoke();
    }
}
