using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(SoundSourceGenerator))]
[CanEditMultipleObjects]
public class SoundSourceGeneratorEditor : Editor
{
    private SerializedProperty enableGenerator;
    private SerializedProperty selectedGeneratorTypeIndex;
    private SerializedProperty selectedFoliageTypeIndex;
    private SerializedProperty selectedWaterTypeIndex;
    private SerializedProperty leavesTreeSize;
    private SerializedProperty size;
    private SerializedProperty sourceSelectionProp;

    // Properties for splashing fountain
    private SerializedProperty splashingTime;
    private SerializedProperty splashingBreak;

    private string[] generatorTypes = { "Wind", "Water" };
    private string[] foliageTypes = { "Needles", "Leaves" };
    private string[] waterTypes = { "Flow", "Drinking Fountain", "Splashing Fountain" };

    // We now have only 4 possible channels (no separate wind/water arrays).
    private string[] channelOptions =
    {
        "Stereo channel 1",
        "Stereo channel 2",
        "Stereo channel 3",
        "Stereo channel 4"
    };

    private SoundSourceGenerator[] targetsList;

    private void OnEnable()
    {
        enableGenerator = serializedObject.FindProperty("enableGenerator");
        selectedGeneratorTypeIndex = serializedObject.FindProperty("selectedGeneratorTypeIndex");
        selectedFoliageTypeIndex = serializedObject.FindProperty("selectedFoliageTypeIndex");
        selectedWaterTypeIndex = serializedObject.FindProperty("selectedWaterTypeIndex");
        leavesTreeSize = serializedObject.FindProperty("leavesTreeSize");
        size = serializedObject.FindProperty("size");
        sourceSelectionProp = serializedObject.FindProperty("sourceSelectionIndex");

        splashingTime = serializedObject.FindProperty("splashingTime");
        splashingBreak = serializedObject.FindProperty("splashingBreak");

        targetsList = targets.Cast<SoundSourceGenerator>().ToArray();

        // Register each SoundSourceGenerator so SourceSelectionManager
        // knows which channels are taken, etc.
        foreach (var t in targetsList)
        {
            string sourceType = "stereo";
            int sel = t.SourceSelectionIndex;

            if (!SourceSelectionManager.IsSourceTaken(sourceType, sel))
            {
                SourceSelectionManager.AssignSource(sourceType, sel, t);
            }
            else
            {
                var assignedObj = SourceSelectionManager.GetAssignedObject(sourceType, sel);
                if (assignedObj != t)
                {
                    // conflict handling logic if needed
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Generator Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(enableGenerator, new GUIContent("Enable Generator"));

        // Select Wind or Water
        DrawGeneratorTypeDropdown();
        GUILayout.Space(10);

        // Show relevant fields depending on Wind or Water
        if (selectedGeneratorTypeIndex.hasMultipleDifferentValues)
        {
            EditorGUILayout.HelpBox("Generator type varies across selected objects.", MessageType.Info);
        }
        else if (selectedGeneratorTypeIndex.intValue == 0) // Wind
        {
            DrawFoliageTypeDropdown();
            DrawLeavesTreeSizeSlider();
        }
        else if (selectedGeneratorTypeIndex.intValue == 1) // Water
        {
            DrawWaterTypeDropdown();
            DrawSizeSlider();

            // If "Splashing Fountain" is selected
            if (!selectedWaterTypeIndex.hasMultipleDifferentValues &&
                selectedWaterTypeIndex.intValue == 2)
            {
                // Show splashingTime
                EditorGUI.showMixedValue = splashingTime.hasMultipleDifferentValues;
                float newSplashingTime = EditorGUILayout.Slider("Splashing Time", splashingTime.floatValue, 0.0f, 10.0f);
                if (!Mathf.Approximately(newSplashingTime, splashingTime.floatValue))
                {
                    splashingTime.floatValue = newSplashingTime;
                }
                EditorGUI.showMixedValue = false;

                // Show splashingBreak
                EditorGUI.showMixedValue = splashingBreak.hasMultipleDifferentValues;
                float newSplashingBreak = EditorGUILayout.Slider("Splashing Break", splashingBreak.floatValue, 0.0f, 10.0f);
                if (!Mathf.Approximately(newSplashingBreak, splashingBreak.floatValue))
                {
                    splashingBreak.floatValue = newSplashingBreak;
                }
                EditorGUI.showMixedValue = false;
            }
        }

        // Draw the unified channel selection (always 4 channels).
        DrawChannelSelection();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGeneratorTypeDropdown()
    {
        EditorGUI.showMixedValue = selectedGeneratorTypeIndex.hasMultipleDifferentValues;
        int newGeneratorTypeIndex = EditorGUILayout.Popup("Generator Type", selectedGeneratorTypeIndex.intValue, generatorTypes);
        if (newGeneratorTypeIndex != selectedGeneratorTypeIndex.intValue)
        {
            selectedGeneratorTypeIndex.intValue = newGeneratorTypeIndex;
        }
        EditorGUI.showMixedValue = false;
    }

    private void DrawFoliageTypeDropdown()
    {
        EditorGUI.showMixedValue = selectedFoliageTypeIndex.hasMultipleDifferentValues;
        int newFoliageTypeIndex = EditorGUILayout.Popup("Foliage Type", selectedFoliageTypeIndex.intValue, foliageTypes);
        if (newFoliageTypeIndex != selectedFoliageTypeIndex.intValue)
        {
            selectedFoliageTypeIndex.intValue = newFoliageTypeIndex;
        }
        EditorGUI.showMixedValue = false;
    }

    private void DrawWaterTypeDropdown()
    {
        EditorGUI.showMixedValue = selectedWaterTypeIndex.hasMultipleDifferentValues;
        int newWaterTypeIndex = EditorGUILayout.Popup("Water Type", selectedWaterTypeIndex.intValue, waterTypes);
        if (newWaterTypeIndex != selectedWaterTypeIndex.intValue)
        {
            selectedWaterTypeIndex.intValue = newWaterTypeIndex;
        }
        EditorGUI.showMixedValue = false;
    }

    private void DrawLeavesTreeSizeSlider()
    {
        EditorGUI.showMixedValue = leavesTreeSize.hasMultipleDifferentValues;
        float newLeavesTreeSize = EditorGUILayout.Slider("Leaves/Tree Size", leavesTreeSize.floatValue, 0.1f, 10.0f);
        if (!Mathf.Approximately(newLeavesTreeSize, leavesTreeSize.floatValue))
        {
            leavesTreeSize.floatValue = newLeavesTreeSize;
        }
        EditorGUI.showMixedValue = false;
    }

    private void DrawSizeSlider()
    {
        EditorGUI.showMixedValue = size.hasMultipleDifferentValues;
        float newSize = EditorGUILayout.Slider("Size", size.floatValue, 0.1f, 10.0f);
        if (!Mathf.Approximately(newSize, size.floatValue))
        {
            size.floatValue = newSize;
        }
        EditorGUI.showMixedValue = false;
    }

    /// <summary>
    /// Now we always present the same 4 channels for selection, regardless of Wind or Water.
    /// </summary>
    private void DrawChannelSelection()
    {
        int[] selections = targetsList.Select(t => t.SourceSelectionIndex).ToArray();
        bool multipleValues = selections.Distinct().Count() > 1;

        EditorGUI.showMixedValue = multipleValues;
        int currentSelection = selections[0];

        // Clamp if out of range
        if (currentSelection < 0 || currentSelection >= channelOptions.Length)
            currentSelection = 0;

        int newSelection = EditorGUILayout.Popup("Channel", currentSelection, channelOptions);
        EditorGUI.showMixedValue = false;

        // If user picks the same channel, do nothing
        if (newSelection == currentSelection) return;

        // Otherwise, reassign all selected objects to newSelection
        foreach (var t in targetsList)
        {
            // first unassign old
            if (SourceSelectionManager.IsSourceTaken("stereo", t.SourceSelectionIndex))
            {
                var assignedObj = SourceSelectionManager.GetAssignedObject("stereo", t.SourceSelectionIndex);
                if (assignedObj == t)
                {
                    SourceSelectionManager.UnassignSource("stereo", t.SourceSelectionIndex);
                }
            }

            // assign new
            if (SourceSelectionManager.IsSourceTaken("stereo", newSelection))
            {
                var otherObj = SourceSelectionManager.GetAssignedObject("stereo", newSelection);
                if (otherObj != null && otherObj != t && otherObj is SoundSourceGenerator otherGen)
                {
                    // swap channels with the other generator
                    SourceSelectionManager.UnassignSource("stereo", newSelection);

                    t.SourceSelectionIndex = newSelection;
                    SourceSelectionManager.AssignSource("stereo", newSelection, t);

                    // place the other generator in old selection
                    if (otherGen.SourceSelectionIndex != t.SourceSelectionIndex)
                    {
                        otherGen.SourceSelectionIndex = currentSelection;
                        SourceSelectionManager.AssignSource("stereo", currentSelection, otherGen);
                    }
                    EditorUtility.SetDirty(otherGen);
                }
                else
                {
                    t.SourceSelectionIndex = newSelection;
                    SourceSelectionManager.AssignSource("stereo", newSelection, t);
                }
            }
            else
            {
                t.SourceSelectionIndex = newSelection;
                SourceSelectionManager.AssignSource("stereo", newSelection, t);
            }

            EditorUtility.SetDirty(t);
        }
    }
}
