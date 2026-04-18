using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GhostSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject ghostPrefab;

    [Header("Spawn Settings")]
    public int maxGhosts = 3;
    public float spawnIntervalMin = 6f;
    public float spawnIntervalMax = 14f;

    [Header("Spawn Area (world space X/Y)")]
    public float spawnXMin = -7f;
    public float spawnXMax =  7f;
    public float spawnYMin = -3f;
    public float spawnYMax =  3f;
    public float spawnZ = -1f;

    [Header("Audio")]
    public AudioClip[] spawnSounds;

    private AudioSource audioSource;
    private List<GameObject> activeGhosts = new List<GameObject>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            activeGhosts.RemoveAll(g => g == null);
            if (activeGhosts.Count < maxGhosts)
                SpawnGhost();
        }
    }

    void SpawnGhost()
    {
        Vector3 pos = new Vector3(
            Random.Range(spawnXMin, spawnXMax),
            Random.Range(spawnYMin, spawnYMax),
            spawnZ
        );

        GameObject ghost = Instantiate(ghostPrefab, pos, Quaternion.identity);
        activeGhosts.Add(ghost);

        if (spawnSounds != null && spawnSounds.Length > 0)
            audioSource.PlayOneShot(spawnSounds[Random.Range(0, spawnSounds.Length)]);
    }

}
