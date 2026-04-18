using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartObjective(string objectiveId)
    {
        Debug.Log("Starting objective: " + objectiveId);

        // Put your gameplay activation logic here
        // Example:
        // if (objectiveId == "CleanDesk") { ... }
    }

    public void CompleteObjective(string objectiveId)
    {
        Debug.Log("Completed objective: " + objectiveId);

        DialogueManager dialogue = FindFirstObjectByType<DialogueManager>();
        if (dialogue != null)
        {
            dialogue.ResumeAfterObjective(objectiveId);
        }
    }
}