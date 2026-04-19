using UnityEngine;

public class ShakeUI : MonoBehaviour
{
    [SerializeField] private float intensity = 5f;  // max pixels of displacement
    [SerializeField] private float speed = 15f;     // how fast it shakes

    private RectTransform rt;
    private Vector2 originPosition;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        originPosition = rt.anchoredPosition;
    }

    private void Update()
    {
        float offsetX = Mathf.Sin(Time.time * speed)                * intensity;
        float offsetY = Mathf.Sin(Time.time * speed * 0.7f + 1.3f) * intensity;
        rt.anchoredPosition = originPosition + new Vector2(offsetX, offsetY);
    }
}
