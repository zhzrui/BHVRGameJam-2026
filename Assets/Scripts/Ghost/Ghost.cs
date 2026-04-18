using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ghost : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Variables")]
    [SerializeField] float clickTime = 3.0f;
    [SerializeField] float explodeTime = 2.0f;
    [Header("Fear")]
    [SerializeField] AnimationCurve fearCurve;
    [SerializeField] float minFearTime = 10.0f;
    [SerializeField] float maxFearTime = 40.0f;
    private float timeLeft;
    private float explodeTimeLeft;
    private bool clicked = false;
    private Renderer rend;
    private float spawnTime;
    [Header("Assignables")]
    [SerializeField] new private ParticleSystem particleSystem;
    [SerializeField] private ParticleSystem explodeParticles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rend = GetComponent<Renderer>();
        timeLeft = clickTime;
        explodeTimeLeft = explodeTime;
        spawnTime = Time.time;
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
                StartCoroutine(Explode());
            }
        }
    }

    IEnumerator Explode()
    {
        rend.material.SetInt("_isExploding", 1);
        rend.material.SetInt("_isClicking", 0);
        particleSystem.Stop();
        explodeParticles.Stop();
        // TODO: need to update ghost array or counter or something here
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
        explodeParticles.Play();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (timeLeft > 0)
        {
            clicked = false;
            timeLeft = clickTime;
            explodeParticles.Stop();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (clicked && timeLeft > 0)
        {
            clicked = false;
            timeLeft = clickTime;
            explodeParticles.Stop();
        }
    }

    public float GetGhostFearContribution()
    {
        float lifespan = Time.time - spawnTime;
        // no fear until minFearTime in and no fear after it's popped
        if (lifespan < minFearTime || timeLeft <= 0)
        {
            return 0;
        } else
        {
            return fearCurve.Evaluate((lifespan - minFearTime) / (maxFearTime - minFearTime)) * timeLeft / clickTime;
        }
    }
}
