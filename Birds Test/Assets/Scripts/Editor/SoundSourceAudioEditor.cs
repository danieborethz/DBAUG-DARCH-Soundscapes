using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System;

[CustomEditor(typeof(SoundSourceAudio), true)]
[CanEditMultipleObjects] // Enables multi-object editing
public class SoundSourceAudioEditor : Editor
{
    private SerializedProperty selectedCategoryIndex;
    private SerializedProperty selectedAudioIndex;
    private SerializedProperty sourceTypeSelection;
    private SerializedProperty sourceSelection;
    private SerializedProperty multiSize;

    private string[] categoryNames;
    private string[] audioNames;
    private string[] sourceOptions;

    private List<SoundSourceAudio> targetsList;

    private void OnEnable()
    {
        selectedCategoryIndex = serializedObject.FindProperty("selectedCategoryIndex");
        selectedAudioIndex = serializedObject.FindProperty("selectedAudioIndex");
        sourceTypeSelection = serializedObject.FindProperty("sourceTypeSelection");
        sourceSelection = serializedObject.FindProperty("sourceSelection");
        multiSize = serializedObject.FindProperty("multiSize");

        targetsList = targets.Cast<SoundSourceAudio>().ToList();

        UpdateCategoryNames();
        UpdateAudioNames();
        UpdateSourceOptions();

        // Assign current sources to the SourceSelectionManager
        // This ensures the manager knows which sources are already taken.
        foreach (var t in targetsList)
        {
            string st = t.sourceTypes[t.sourceTypeSelection];
            // Only assign if it's not already taken (avoid duplicates)
            if (!SourceSelectionManager.IsSourceTaken(st, t.sourceSelection))
            {
                SourceSelectionManager.AssignSource(st, t.sourceSelection);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Multi-Object Editing", EditorStyles.boldLabel);

        if (targetsList.All(t => t.audioLibrary != null))
        {
            DrawCategoryDropdown();
            DrawAudioDropdown();
            DrawSourceTypeDropdown();
            DrawSourceDropdown();
            DrawMultiSizeSlider();
        }
        else
        {
            EditorGUILayout.HelpBox("One or more selected objects have no audio library loaded.", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCategoryDropdown()
    {
        int commonCategoryIndex = selectedCategoryIndex.intValue;
        if (targetsList.Select(t => t.selectedCategoryIndex).Distinct().Count() > 1)
            commonCategoryIndex = -1;

        int newCategoryIndex = EditorGUILayout.Popup("Category", commonCategoryIndex, categoryNames);
        if (newCategoryIndex != commonCategoryIndex && newCategoryIndex >= 0)
        {
            foreach (var target in targetsList)
            {
                target.selectedCategoryIndex = newCategoryIndex;
                target.selectedCategoryName = categoryNames[newCategoryIndex];

                // Update currentAudioItems based on the newly selected category
                if (target.categories != null && target.categories.Count > newCategoryIndex)
                {
                    target.currentAudioItems = target.categories[newCategoryIndex].audioItems;
                }

                EditorUtility.SetDirty(target);
            }

            // Now that currentAudioItems have been updated, refresh the audioNames array
            UpdateAudioNames();
        }
    }


    private void DrawAudioDropdown()
    {
        int commonAudioIndex = selectedAudioIndex.intValue;
        if (targetsList.Select(t => t.selectedAudioIndex).Distinct().Count() > 1)
            commonAudioIndex = -1;

        int newAudioIndex = EditorGUILayout.Popup("Audio", commonAudioIndex, audioNames);
        if (newAudioIndex != commonAudioIndex && newAudioIndex >= 0)
        {
            foreach (var target in targetsList)
            {
                target.selectedAudioIndex = newAudioIndex;
                target.selectedAudioName = audioNames[newAudioIndex];
                EditorUtility.SetDirty(target);
            }
        }
    }

    private void DrawSourceTypeDropdown()
    {
        int commonSourceType = sourceTypeSelection.intValue;
        if (targetsList.Select(t => t.sourceTypeSelection).Distinct().Count() > 1)
            commonSourceType = -1;

        int newSourceType = EditorGUILayout.Popup("Source Type", commonSourceType, targetsList[0].sourceTypes);
        if (newSourceType != commonSourceType && newSourceType >= 0)
        {
            // Before changing the type, unassign current sources
            foreach (var t in targetsList)
            {
                string oldType = t.sourceTypes[t.sourceTypeSelection];
                SourceSelectionManager.UnassignSource(oldType, t.sourceSelection);
            }

            foreach (var target in targetsList)
            {
                target.sourceTypeSelection = newSourceType;
                // Reset source selection when type changes (to 0 for example)
                target.sourceSelection = 0;
                EditorUtility.SetDirty(target);

                // Assign the new default source
                string newTypeName = target.sourceTypes[newSourceType];
                SourceSelectionManager.AssignSource(newTypeName, target.sourceSelection);
            }

            UpdateSourceOptions();
        }
    }

    private void DrawSourceDropdown()
    {
        // Determine the "common" source selection among multiple objects
        int commonSourceSelection = sourceSelection.intValue;
        if (targetsList.Select(t => t.sourceSelection).Distinct().Count() > 1)
            commonSourceSelection = -1;

        string currentType = targetsList[0].sourceTypes[targetsList[0].sourceTypeSelection];

        // We need to map the actual selected source index (commonSourceSelection)
        // to the index in sourceOptions. If we have multiple objects with different selections,
        // we can just default to 0 or handle differently.
        int displayIndex = 0;
        if (commonSourceSelection >= 0)
        {
            string currentLabel = $"{currentType} source {commonSourceSelection + 1}";
            int foundIndex = System.Array.IndexOf(sourceOptions, currentLabel);
            if (foundIndex >= 0)
                displayIndex = foundIndex;
        }
        else
        {
            // If multiple objects are selected with different sources,
            // you may choose to leave it at displayIndex = 0,
            // or handle it differently if desired.
        }

        int newIndexInArray = EditorGUILayout.Popup("Source", displayIndex, sourceOptions);

        // If the user selects a new option
        if (newIndexInArray != displayIndex)
        {
            // Parse the selected option to get the actual source index
            // sourceOptions entries are in the format "mono source 3"
            string selectedOption = sourceOptions[newIndexInArray];
            string[] splitOption = selectedOption.Split(' ');
            int newIndex = int.Parse(splitOption[splitOption.Length - 1]) - 1;

            // Unassign old sources
            foreach (var t in targetsList)
            {
                string oldType = t.sourceTypes[t.sourceTypeSelection];
                SourceSelectionManager.UnassignSource(oldType, t.sourceSelection);
            }

            // Assign the new source selection
            foreach (var target in targetsList)
            {
                target.sourceSelection = newIndex;
                SourceSelectionManager.AssignSource(currentType, newIndex);
                EditorUtility.SetDirty(target);
            }
        }
    }


    private void DrawMultiSizeSlider()
    {
        float commonMultiSize = multiSize.floatValue;
        if (targetsList.Select(t => t.multiSize).Distinct().Count() > 1)
            commonMultiSize = -1;

        float newMultiSize = EditorGUILayout.Slider("Multi Size", commonMultiSize, 1.0f, 10.0f);
        if (newMultiSize != commonMultiSize && newMultiSize >= 1.0f)
        {
            foreach (var target in targetsList)
            {
                target.multiSize = newMultiSize;
                EditorUtility.SetDirty(target);
            }
        }
    }

    private void UpdateCategoryNames()
    {
        if (targetsList.All(t => t.categories != null))
        {
            categoryNames = targetsList[0].categories.Select(c => c.categoryName).ToArray();
        }
        else
        {
            categoryNames = new string[0];
        }
    }

    private void UpdateAudioNames()
    {
        if (targetsList.All(t => t.currentAudioItems != null))
        {
            audioNames = targetsList[0].currentAudioItems.Select(a => a.displayName).ToArray();
        }
        else
        {
            audioNames = new string[0];
        }
    }

    private void UpdateSourceOptions()
    {
        int sourceCount = 0;
        string sourceType = targetsList[0].sourceTypes[targetsList[0].sourceTypeSelection];
        switch (targetsList[0].sourceTypeSelection)
        {
            case 0:
                sourceCount = targetsList[0].monoSources;
                break;
            case 1:
                sourceCount = targetsList[0].stereoSources;
                break;
            case 2:
                sourceCount = targetsList[0].multiSources;
                break;
        }

        List<string> availableOptions = new List<string>();
        for (int i = 0; i < sourceCount; i++)
        {
            // Only add this source if it's not already taken
            if (!SourceSelectionManager.IsSourceTaken(sourceType, i) ||
                targetsList.Any(t => t.sourceSelection == i && t.sourceTypes[t.sourceTypeSelection] == sourceType))
            {
                availableOptions.Add($"{sourceType} source {i + 1}");
            }
        }

        sourceOptions = availableOptions.ToArray();
    }
}
