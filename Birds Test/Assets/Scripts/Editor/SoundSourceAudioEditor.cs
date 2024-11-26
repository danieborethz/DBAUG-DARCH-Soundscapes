using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(SoundSourceAudio))]
public class SoundSourceAudioEditor : Editor
{
    private SoundSourceAudio component;
    private string[] categoryNames;
    private string[] audioNames;
    private string[] sourceOptions;

    void OnEnable()
    {
        component = (SoundSourceAudio)target;

        // Load the audio library using the component's method
        component.LoadAudioLibrary();

        UpdateCategoryNames();

        if (!string.IsNullOrEmpty(component.selectedCategoryName))
        {
            component.selectedCategoryIndex = Array.FindIndex(categoryNames, name => name == component.selectedCategoryName);
            if (component.selectedCategoryIndex == -1)
                component.selectedCategoryIndex = 0;
        }

        UpdateAudioNames();

        if (!string.IsNullOrEmpty(component.selectedAudioName))
        {
            component.selectedAudioIndex = Array.FindIndex(audioNames, name => name == component.selectedAudioName);
            if (component.selectedAudioIndex == -1)
                component.selectedAudioIndex = 0;
        }

        UpdateParameterValues();
        UpdateSourceOptions();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Space(10);

        if (component.audioLibrary != null)
        {
            if (categoryNames == null || categoryNames.Length == 0)
            {
                UpdateCategoryNames();
            }

            int newCategoryIndex = EditorGUILayout.Popup("Category", component.selectedCategoryIndex, categoryNames);
            if (newCategoryIndex != component.selectedCategoryIndex)
            {
                component.selectedCategoryIndex = newCategoryIndex;
                component.selectedCategoryName = categoryNames[component.selectedCategoryIndex];
                UpdateAudioNames();
                if (component.selectedAudioIndex >= audioNames.Length)
                    component.selectedAudioIndex = 0;
                component.selectedAudioName = audioNames.Length > 0 ? audioNames[component.selectedAudioIndex] : "";
                UpdateParameterValues();

                // Mark the component as dirty to save changes
                EditorUtility.SetDirty(component);
            }

            GUILayout.Space(10);

            if (component.currentAudioItems != null && component.currentAudioItems.Count > 0)
            {
                if (audioNames == null || audioNames.Length == 0)
                {
                    UpdateAudioNames();
                }

                int newAudioIndex = EditorGUILayout.Popup("Audio", component.selectedAudioIndex, audioNames);
                if (newAudioIndex != component.selectedAudioIndex)
                {
                    component.selectedAudioIndex = newAudioIndex;
                    component.selectedAudioName = audioNames[component.selectedAudioIndex];
                    UpdateParameterValues();

                    // Mark the component as dirty to save changes
                    EditorUtility.SetDirty(component);
                }

                if (component.parameterValues != null && component.parameterValues.Count > 0)
                {
                    EditorGUILayout.LabelField("Parameters:", EditorStyles.boldLabel);
                    foreach (var param in component.parameterValues)
                    {
                        float newValue = EditorGUILayout.Slider(param.key, param.currentValue, param.minValue, param.maxValue);
                        if (newValue != param.currentValue)
                        {
                            param.currentValue = newValue;

                            // Mark the component as dirty to save changes
                            EditorUtility.SetDirty(component);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No audio files in this category.");
            }

            GUILayout.Space(10);

            if (component.sourceTypes != null && component.sourceTypes.Length > 0)
            {
                int newSourceSelection = EditorGUILayout.Popup("Source Type", component.sourceTypeSelection, component.sourceTypes);
                if (newSourceSelection != component.sourceTypeSelection)
                {
                    component.sourceTypeSelection = newSourceSelection;
                    UpdateSourceOptions();

                    // Mark the component as dirty to save changes
                    EditorUtility.SetDirty(component);
                }
            }

            GUILayout.Space(10);

            if (sourceOptions == null || sourceOptions.Length == 0)
            {
                UpdateSourceOptions();
            }

            if (sourceOptions != null && sourceOptions.Length > 0)
            {
                int newSourceSelection = EditorGUILayout.Popup("Source", component.sourceSelection, sourceOptions);
                if (newSourceSelection != component.sourceSelection)
                {
                    component.sourceSelection = newSourceSelection;

                    // Mark the component as dirty to save changes
                    EditorUtility.SetDirty(component);
                }
            }

            if (component.sourceTypes[component.sourceTypeSelection] == "multi")
            {
                float newMultiSize = EditorGUILayout.Slider("Multi Size", component.multiSize, 1.0f, 10.0f);
                if (newMultiSize != component.multiSize)
                {
                    component.multiSize = newMultiSize;

                    // Mark the component as dirty to save changes
                    EditorUtility.SetDirty(component);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No audio library loaded. Please load the audio library.", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void LoadAudioLibrary()
    {
        if (File.Exists(component.cacheFilePath))
        {
            string json = File.ReadAllText(component.cacheFilePath);
            component.audioLibrary = JsonUtility.FromJson<AudioLibrary>(json);
            component.categories = component.audioLibrary.categories;
            component.monoSources = component.audioLibrary.monoSources;
            component.stereoSources = component.audioLibrary.stereoSources;
            component.multiSources = component.audioLibrary.ambisonicSources;

            // Mark the component as dirty to save changes
            EditorUtility.SetDirty(component);
        }
        else
        {
            Debug.LogError("Cache file not found at: " + component.cacheFilePath);
        }
    }

    private void UpdateCategoryNames()
    {
        if (component.categories != null)
        {
            categoryNames = new string[component.categories.Count];
            for (int i = 0; i < categoryNames.Length; i++)
            {
                categoryNames[i] = component.categories[i].categoryName;
            }

            if (!string.IsNullOrEmpty(component.selectedCategoryName))
            {
                component.selectedCategoryIndex = Array.FindIndex(categoryNames, name => name == component.selectedCategoryName);
                if (component.selectedCategoryIndex == -1)
                    component.selectedCategoryIndex = 0;
            }
        }
        else
        {
            categoryNames = new string[0];
        }
    }

    private void UpdateAudioNames()
    {
        if (component.categories != null && component.categories.Count > 0)
        {
            if (component.selectedCategoryIndex >= component.categories.Count)
                component.selectedCategoryIndex = 0;

            AudioCategory selectedCategory = component.categories[component.selectedCategoryIndex];
            component.currentAudioItems = selectedCategory.audioItems;

            if (component.currentAudioItems != null)
            {
                audioNames = new string[component.currentAudioItems.Count];
                for (int i = 0; i < audioNames.Length; i++)
                {
                    audioNames[i] = component.currentAudioItems[i].displayName;
                }

                if (!string.IsNullOrEmpty(component.selectedAudioName))
                {
                    component.selectedAudioIndex = Array.FindIndex(audioNames, name => name == component.selectedAudioName);
                    if (component.selectedAudioIndex == -1)
                        component.selectedAudioIndex = 0;
                }
            }
            else
            {
                audioNames = new string[0];
            }
        }
        else
        {
            component.currentAudioItems = null;
            audioNames = new string[0];
        }
    }

    private void UpdateParameterValues()
    {
        var existingParams = new Dictionary<string, float>();
        foreach (var param in component.parameterValues)
        {
            existingParams[param.key] = param.currentValue;
        }

        component.parameterValues.Clear();

        if (component.currentAudioItems != null && component.currentAudioItems.Count > component.selectedAudioIndex)
        {
            AudioItem selectedItem = component.currentAudioItems[component.selectedAudioIndex];

            foreach (var param in selectedItem.parameters)
            {
                float currentValue = param.minValue;
                if (existingParams.TryGetValue(param.key, out float savedValue))
                {
                    currentValue = savedValue;
                }

                SoundSource.ParameterValue paramValue = new SoundSource.ParameterValue
                {
                    key = param.key,
                    minValue = param.minValue,
                    maxValue = param.maxValue,
                    currentValue = currentValue
                };
                component.parameterValues.Add(paramValue);
            }
        }

        // Mark the component as dirty to save changes
        EditorUtility.SetDirty(component);
    }

    private void UpdateSourceOptions()
    {
        int sourceCount = 0;
        string sourceType = component.sourceTypes[component.sourceTypeSelection];
        switch (component.sourceTypeSelection)
        {
            case 0:
                sourceCount = component.monoSources;
                break;
            case 1:
                sourceCount = component.stereoSources;
                break;
            case 2:
                sourceCount = component.multiSources;
                break;
        }

        sourceOptions = new string[sourceCount];
        for (int i = 0; i < sourceCount; i++)
        {
            sourceOptions[i] = $"{sourceType} source {i + 1}";
        }
    }
}
