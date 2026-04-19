using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableIngredient : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] public int orderIndex;    // position in the required sequence (0-based)
    [SerializeField] public Sprite bowlSprite;            // image to spawn in the bowl on correct drop
    [SerializeField] public Vector2 bowlSpriteSize = new Vector2(80f, 80f);
    [SerializeField] public Vector2 bowlSpritePosition = Vector2.zero;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // Reparent to canvas root so it renders on top of everything
        transform.SetParent(GetComponentInParent<Canvas>().transform, true);
        canvasGroup.blocksRaycasts = false; // let raycasts pass through to the bowl
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        // If the bowl's IDropHandler didn't consume this, snap back
        SnapBack();
    }

    public void SnapBack()
    {
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = originalPosition;
    }

}
