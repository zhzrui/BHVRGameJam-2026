using System.Collections.Generic;
using UnityEngine;

public static class DialogueFlags
{
    private static HashSet<string> flags = new HashSet<string>();

    public static void SetFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        flags.Add(flag);
        Debug.Log("Flag set: " + flag);
    }

    public static bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public static void ClearFlag(string flag)
    {
        if (string.IsNullOrEmpty(flag)) return;
        flags.Remove(flag);
    }

    public static void ClearAll()
    {
        flags.Clear();
    }
}