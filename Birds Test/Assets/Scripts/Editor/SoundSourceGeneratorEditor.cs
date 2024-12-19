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

    // Note: We keep the variable names the same, but now "wind" uses waterChannelOptions and "water" uses windChannelOptions
    private string[] generatorTypes = { "Wind", "Water" };
    private string[] foliageTypes = { "Needles", "Leaves" };
    private string[] waterTypes = { "Flow", "Drinking Fountain", "Splashing Fountain" };

    // Original definitions kept the same, but logic below is changed.
    private string[] windChannelOptions = { "Stereo channel 1", "Stereo channel 2", "Stereo channel 3", "Stereo channel 4" };
    private string[] waterChannelOptions = { "Stereo channel 5", "Stereo channel 6", "Stereo channel 7", "Stereo channel 8" };

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

        targetsList = targets.Cast<SoundSourceGenerator>().ToArray();

        // Register current assignments in SourceSelectionManager (if not already)
        foreach (var t in targetsList)
        {
            string st = "stereo";
            int sel = t.SourceSelectionIndex;

            if (!SourceSelectionManager.IsSourceTaken(st, sel))
            {
                SourceSelectionManager.AssignSource(st, sel, t);
            }
            else
            {
                var assignedObj = SourceSelectionManager.GetAssignedObject(st, sel);
                if (assignedObj != t)
                {
                    // Conflict handling if needed
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Generator Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(enableGenerator, new GUIContent("Enable Generator"));
        DrawGeneratorTypeDropdown();

        GUILayout.Space(10);

        if (selectedGeneratorTypeIndex.hasMultipleDifferentValues)
        {
            EditorGUILayout.HelpBox("Generator type varies across selected objects.", MessageType.Info);
        }
        else if (selectedGeneratorTypeIndex.intValue == 0) // Wind
        {
            DrawFoliageTypeDropdown();
            DrawLeavesTreeSizeSlider();
            // Now Wind uses water channel mapping
            DrawChannelSelection(true);
        }
        else if (selectedGeneratorTypeIndex.intValue == 1) // Water
        {
            DrawWaterTypeDropdown();
            DrawSizeSlider();
            // Now Water uses wind channel mapping
            DrawChannelSelection(false);
        }

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

    private void DrawChannelSelection(bool isWind)
    {
        string sourceType = "stereo";

        // Gather current selections
        int[] selections = targetsList.Select(t => t.SourceSelectionIndex).ToArray();
        bool multipleValues = selections.Distinct().Count() > 1;
        EditorGUI.showMixedValue = multipleValues;

        int currentSelection = selections[0];

        // Swapped logic: 
        // Wind now uses what used to be water channels (5–8)
        // Water now uses what used to be wind channels (1–4)
        string[] options = isWind ? waterChannelOptions : windChannelOptions;

        int displayIndex;
        if (isWind)
        {
            // If currently wind: channels start at 5, so subtract 4
            displayIndex = currentSelection - 4;
        }
        else
        {
            // If currently water: channels start at 1, no offset
            displayIndex = currentSelection;
        }

        // Ensure display index is within valid range
        if (displayIndex < 0 || displayIndex >= options.Length) displayIndex = 0;

        int newDisplayIndex = EditorGUILayout.Popup("Channel", displayIndex, options);
        EditorGUI.showMixedValue = false;

        // If user didn't change selection, do nothing
        if (newDisplayIndex == displayIndex) return;

        int newSelection = isWind ? (newDisplayIndex + 4) : newDisplayIndex;

        // Process each selected target
        foreach (var targetObj in targetsList)
        {
            var t = targetObj;
            int oldSelection = t.SourceSelectionIndex;

            // Unassign old selection
            if (SourceSelectionManager.IsSourceTaken(sourceType, oldSelection))
            {
                var assignedObj = SourceSelectionManager.GetAssignedObject(sourceType, oldSelection);
                if (assignedObj == t)
                {
                    SourceSelectionManager.UnassignSource(sourceType, oldSelection);
                }
            }

            // If new slot is taken, attempt a swap
            if (SourceSelectionManager.IsSourceTaken(sourceType, newSelection))
            {
                var otherObj = SourceSelectionManager.GetAssignedObject(sourceType, newSelection);
                if (otherObj != null && otherObj != t && otherObj is SoundSourceGenerator otherGen)
                {
                    // Unassign the currently taken slot
                    SourceSelectionManager.UnassignSource(sourceType, newSelection);

                    // Assign current object to new slot
                    t.SourceSelectionIndex = newSelection;
                    SourceSelectionManager.AssignSource(sourceType, newSelection, t);

                    // The other object takes the old slot
                    if (oldSelection != newSelection)
                    {
                        otherGen.SourceSelectionIndex = oldSelection;
                        SourceSelectionManager.AssignSource(sourceType, oldSelection, otherGen);
                    }

                    EditorUtility.SetDirty(otherGen);
                    EditorUtility.SetDirty(t);
                }
                else
                {
                    // If taken by null or the same object, just reassign
                    t.SourceSelectionIndex = newSelection;
                    SourceSelectionManager.AssignSource(sourceType, newSelection, t);
                    EditorUtility.SetDirty(t);
                }
            }
            else
            {
                // Not taken: simply assign
                t.SourceSelectionIndex = newSelection;
                SourceSelectionManager.AssignSource(sourceType, newSelection, t);
                EditorUtility.SetDirty(t);
            }
        }
    }

}
