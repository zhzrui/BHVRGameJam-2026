using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ghost : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Variables")]
    [SerializeField] float clickTime = 3.0f;
    [SerializeField] float explodeTime = 2.0f;
    private float timeLeft;
    private float explodeTimeLeft;
    private bool clicked = false;
    private Renderer rend;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rend = GetComponent<Renderer>();
        timeLeft = clickTime;
        explodeTimeLeft = explodeTime;
    }

    void Update()
    {
        if (clicked && timeLeft > 0 && explodeTimeLeft == explodeTime)
        {
            rend.material.SetInt("_isClicking", 1);
            timeLeft -= Time.deltaTime;
            rend.material.SetFloat("_clickTime", timeLeft);
            if (timeLeft <= 0.0f)
            {
                // explode. only occurs once because afterwards this is inaccessible.
                // TODO: need to update ghost array or counter or something here
                StartCoroutine(Explode());
            }
        }
    }

    IEnumerator Explode()
    {
        rend.material.SetInt("_isExploding", 1);
        rend.material.SetInt("_isClicking", 0);
        while (explodeTimeLeft > 0)
        {
            yield return new WaitForEndOfFrame();
            explodeTimeLeft -= Time.deltaTime;
            rend.material.SetFloat("_explodeTime", explodeTimeLeft);
        }

        Destroy(this.gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        clicked = true;
        Debug.Log("clicked");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        clicked = false;
        timeLeft = clickTime;
        Debug.Log("unclicked");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (clicked)
        {
            clicked = false;
            timeLeft = clickTime;
            Debug.Log("unclicked");
        }
    }
}
