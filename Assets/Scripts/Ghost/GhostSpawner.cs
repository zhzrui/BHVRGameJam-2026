using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(AudioSource))]
public class GhostSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject ghostPrefab;

    [Header("Spawn Settings")]
    public int maxGhosts = 3;
    public float spawnIntervalMin = 6f;
    public float spawnIntervalMax = 14f;

    [Header("Spawn Area")]
    [Tooltip("Ghosts spawn randomly within this sprite's bounds. Falls back to manual min/max if null.")]
    public SpriteRenderer spawnAreaSprite;
    public float spawnXMin = -7f;
    public float spawnXMax =  7f;
    public float spawnYMin = -3f;
    public float spawnYMax =  3f;
    public float spawnZ = -1f;

    [Header("Audio")]
    public AudioClip[] spawnSounds;
    [SerializeField] VolumeProfile volume;
    [Header("Fear Settings")]
    [SerializeField] float smoothedFearDelta = 0.2f;
    [SerializeField] AnimationCurve phaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // based on time
    [SerializeField] float phaseLength = 5f;
    [SerializeField] AnimationCurve phaseStrengthCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // based on fear
    [SerializeField] float phaseMultiplier = 0.1f;
    [SerializeField] AnimationCurve vignetteCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] float vignetteMin = 0f;
    [SerializeField] float vignetteMax = 1f;
    [SerializeField] AnimationCurve abberationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] float abberationMin = 0f;
    [SerializeField] float abberationMax = 1f;
    UnityEngine.Rendering.Universal.ChromaticAberration chromaticAberration;
    UnityEngine.Rendering.Universal.Vignette vignette;

    private AudioSource audioSource;
    private List<GameObject> activeGhosts = new List<GameObject>();
    private float smoothedFear;

    void Awake()
    {
        volume.TryGet(out chromaticAberration);
        volume.TryGet(out vignette);
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        UpdateFear(GetGhostFear());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            ClearDeadGhosts();
            if (activeGhosts.Count < maxGhosts)
                SpawnGhost();
        }
    }

    void SpawnGhost()
    {
        float x, y;
        if (spawnAreaSprite != null)
        {
            Bounds b = spawnAreaSprite.bounds;
            x = Random.Range(b.min.x, b.max.x);
            y = Random.Range(b.min.y, b.max.y);
        }
        else
        {
            x = Random.Range(spawnXMin, spawnXMax);
            y = Random.Range(spawnYMin, spawnYMax);
        }
        Vector3 pos = new Vector3(x, y, spawnZ);

        GameObject ghost = Instantiate(ghostPrefab, pos, Quaternion.identity);
        activeGhosts.Add(ghost);

        if (spawnSounds != null && spawnSounds.Length > 0)
            audioSource.PlayOneShot(spawnSounds[Random.Range(0, spawnSounds.Length)]);
    }

    void ClearDeadGhosts()
    {
        activeGhosts.RemoveAll(g => g == null);
    }

    float GetGhostFear()
    {
        ClearDeadGhosts();
        return activeGhosts.Sum(g => g.GetComponent<Ghost>().GetGhostFearContribution()) / maxGhosts;
    }

    void UpdateFear(float fear)
    {
        // fear is a value between 0 and 1.
        // smooth fear such that it doesn't randomly jump.
        smoothedFear = Mathf.MoveTowards(smoothedFear, fear, smoothedFearDelta * Time.deltaTime);
        UpdateVisualFear(smoothedFear);
    }

    void UpdateVisualFear(float fear)
    {
        // fear is a value between 0 and 1.
        float phaseStrength = phaseStrengthCurve.Evaluate(fear) * phaseMultiplier;
        float phase = phaseCurve.Evaluate(Time.time / phaseLength) - 0.5f;

        float vignetteValue = Mathf.Lerp(vignetteMin, vignetteMax, vignetteCurve.Evaluate(fear)) + phase * phaseStrength;
        vignetteValue = Mathf.Clamp(vignetteValue, 0f, 1f);
        vignette.intensity.Override(vignetteValue);
        float abberationValue = Mathf.Lerp(abberationMin, abberationMax, abberationCurve.Evaluate(fear)) + phase * phaseStrength;
        abberationValue = Mathf.Clamp(abberationValue, 0f, 1f);
        chromaticAberration.intensity.Override(abberationValue);

        Debug.Log(string.Format("{0}, {1}, {2}, {3}", phase * phaseStrength, fear, vignetteValue, abberationValue));
    }
}
