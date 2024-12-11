using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class SourceSelectionManager
{
    // This holds a list of assigned sources. You might need a more complex 
    // structure if you differentiate mono/stereo/multi.
    private static Dictionary<string, List<int>> assignedSources = new Dictionary<string, List<int>>()
    {
        { "mono", new List<int>() },
        { "stereo", new List<int>() },
        { "multi", new List<int>() }
    };

    public static bool IsSourceTaken(string type, int index)
    {
        return assignedSources.ContainsKey(type) && assignedSources[type].Contains(index);
    }

    public static void AssignSource(string type, int index)
    {
        if (!assignedSources[type].Contains(index))
            assignedSources[type].Add(index);
    }

    public static void UnassignSource(string type, int index)
    {
        if (assignedSources[type].Contains(index))
            assignedSources[type].Remove(index);
    }
}
