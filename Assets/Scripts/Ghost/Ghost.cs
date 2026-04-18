using System.Collections;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] float timeLeft = 10.0f;
    [SerializeField] float explodeTime = 2.0f;
    [SerializeField] bool clicked = false;
    private Renderer rend;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (clicked && timeLeft > 0)
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
        while (explodeTime > 0)
        {
            yield return new WaitForEndOfFrame();
            explodeTime -= Time.deltaTime;
            rend.material.SetFloat("_explodeTime", explodeTime);
        }

        Destroy(this.gameObject);
    }
}
