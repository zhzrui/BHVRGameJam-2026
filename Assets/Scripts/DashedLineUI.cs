using UnityEngine;
using UnityEngine.UI;

// Attach to the cutLineIndicator RectTransform.
public class DashedLineUI : MonoBehaviour
{
    [SerializeField] private float dashHeight = 12f;
    [SerializeField] private float gapHeight  = 8f;
    [SerializeField] private Color dashColor  = Color.white;
    [SerializeField] private float totalHeight = 200f; // match your cutLineIndicator height

    private void Start() => Rebuild();

    public void Rebuild()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        float y = totalHeight * 0.5f;

        while (y > -totalHeight * 0.5f)
        {
            GameObject dash = new GameObject("Dash");
            dash.transform.SetParent(transform, false);

            Image img = dash.AddComponent<Image>();
            img.color = dashColor;
            img.raycastTarget = false;

            RectTransform drt = dash.GetComponent<RectTransform>();
            drt.anchorMin = new Vector2(0, 0.5f);
            drt.anchorMax = new Vector2(1, 0.5f);
            drt.sizeDelta = new Vector2(0, dashHeight);
            drt.anchoredPosition = new Vector2(0, y - dashHeight * 0.5f);

            y -= dashHeight + gapHeight;
        }
    }
}
