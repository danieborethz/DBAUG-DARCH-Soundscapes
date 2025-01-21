using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class SourceSelectionManager
{
    // The dictionary now stores any SoundSource object regardless of specific subclass
    private static Dictionary<string, Dictionary<int, SoundSource>> assignedMap
        = new Dictionary<string, Dictionary<int, SoundSource>>()
    {
        { "mono", new Dictionary<int, SoundSource>() },
        { "stereo", new Dictionary<int, SoundSource>() },
        { "multi", new Dictionary<int, SoundSource>() }
    };

    public static bool IsSourceTaken(string type, int index)
    {
        return assignedMap.ContainsKey(type) && assignedMap[type].ContainsKey(index);
    }

    public static SoundSource GetAssignedObject(string type, int index)
    {
        if (IsSourceTaken(type, index))
        {
            return assignedMap[type][index];
        }
        return null;
    }

    public static void AssignSource(string type, int index, SoundSource obj)
    {
        if (!assignedMap.ContainsKey(type))
        {
            assignedMap[type] = new Dictionary<int, SoundSource>();
        }

        assignedMap[type][index] = obj;
    }

    public static void UnassignSource(string type, int index)
    {
        if (assignedMap.ContainsKey(type) && assignedMap[type].ContainsKey(index))
        {
            assignedMap[type].Remove(index);
        }
    }
}
