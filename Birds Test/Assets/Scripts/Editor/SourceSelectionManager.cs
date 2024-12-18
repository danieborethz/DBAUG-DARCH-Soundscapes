using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class SourceSelectionManager
{
    // Now we keep track of which object is assigned to each (type,index)
    private static Dictionary<string, Dictionary<int, SoundSourceAudio>> assignedMap
        = new Dictionary<string, Dictionary<int, SoundSourceAudio>>()
    {
        { "mono", new Dictionary<int, SoundSourceAudio>() },
        { "stereo", new Dictionary<int, SoundSourceAudio>() },
        { "multi", new Dictionary<int, SoundSourceAudio>() }
    };

    public static bool IsSourceTaken(string type, int index)
    {
        return assignedMap.ContainsKey(type) && assignedMap[type].ContainsKey(index);
    }

    public static SoundSourceAudio GetAssignedObject(string type, int index)
    {
        if (IsSourceTaken(type, index))
        {
            return assignedMap[type][index];
        }
        return null;
    }

    public static void AssignSource(string type, int index, SoundSourceAudio obj)
    {
        if (!assignedMap[type].ContainsKey(index))
            assignedMap[type].Add(index, obj);
        else
            assignedMap[type][index] = obj;
    }

    public static void UnassignSource(string type, int index)
    {
        if (assignedMap[type].ContainsKey(index))
            assignedMap[type].Remove(index);
    }
}
