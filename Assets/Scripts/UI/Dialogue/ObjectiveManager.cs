using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    public event Action<string> OnObjectiveCompleted;

    private HashSet<string> completedObjectives = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CompleteObjective(string objectiveKey)
    {
        Debug.Log("ObjectiveManager.CompleteObjective called with: " + objectiveKey);

        if (string.IsNullOrEmpty(objectiveKey))
        {
            Debug.LogWarning("CompleteObjective called with empty key");
            return;
        }

        if (completedObjectives.Contains(objectiveKey))
        {
            Debug.Log("Objective already completed: " + objectiveKey);
            return;
        }

        completedObjectives.Add(objectiveKey);

        Debug.Log("Invoking OnObjectiveCompleted for: " + objectiveKey);
        OnObjectiveCompleted?.Invoke(objectiveKey);
    }
}