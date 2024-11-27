using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

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
                EditorUtility.SetDirty(target);
            }
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
            foreach (var target in targetsList)
            {
                target.sourceTypeSelection = newSourceType;
                EditorUtility.SetDirty(target);
            }
            UpdateSourceOptions();
        }
    }

    private void DrawSourceDropdown()
    {
        int commonSourceSelection = sourceSelection.intValue;
        if (targetsList.Select(t => t.sourceSelection).Distinct().Count() > 1)
            commonSourceSelection = -1;

        int newSourceSelection = EditorGUILayout.Popup("Source", commonSourceSelection, sourceOptions);
        if (newSourceSelection != commonSourceSelection && newSourceSelection >= 0)
        {
            foreach (var target in targetsList)
            {
                target.sourceSelection = newSourceSelection;
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
        if (newMultiSize != commonMultiSize)
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

        sourceOptions = new string[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            sourceOptions[i] = $"{sourceType} source {i + 1}";
        }
    }
}
