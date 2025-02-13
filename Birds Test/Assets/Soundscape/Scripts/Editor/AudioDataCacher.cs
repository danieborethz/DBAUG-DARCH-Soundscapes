using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class AudioDataCacher : EditorWindow
{
    private string rootFolderPath = "";
    private string cacheFilePath = "Assets/AudioDataCache.json";

    // New fields for additional parameters
    private int monoSources = 16;
    private int stereoSources = 8;
    private int multiSources = 3;

    [MenuItem("Soundscape/Audio Data Updater")]
    public static void ShowWindow()
    {
        GetWindow<AudioDataCacher>("Audio Data Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("Audio Data Cacher", EditorStyles.boldLabel);

        // Input for Root Folder Path
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Root Folder Path", GUILayout.Width(100));
        rootFolderPath = EditorGUILayout.TextField(rootFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedFolder = EditorUtility.OpenFolderPanel("Select Root Folder", "", "");
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                rootFolderPath = selectedFolder;
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Input for Cache File Path
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Cache File Path", GUILayout.Width(100));
        cacheFilePath = EditorGUILayout.TextField(cacheFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedFile = EditorUtility.SaveFilePanel("Select Cache File", Application.dataPath, "AudioDataCache", "json");
            if (!string.IsNullOrEmpty(selectedFile))
            {
                cacheFilePath = selectedFile;
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Input fields for Mono, Stereo, and Ambisonic sources
        monoSources = EditorGUILayout.IntField("Mono Sources", monoSources);
        stereoSources = EditorGUILayout.IntField("Stereo Sources", stereoSources);
        multiSources = EditorGUILayout.IntField("Multi Sources", multiSources);

        GUILayout.Space(10);

        if (GUILayout.Button("Update Audio Library"))
        {
            CacheAudioData();
        }
    }

    private void CacheAudioData()
    {
        if (string.IsNullOrEmpty(rootFolderPath) || !Directory.Exists(rootFolderPath))
        {
            Debug.LogError("Root folder does not exist: " + rootFolderPath);
            return;
        }

        // Create audio library object and add the new parameters
        AudioLibrary audioLibrary = new AudioLibrary
        {
            monoSources = monoSources,
            stereoSources = stereoSources,
            ambisonicSources = multiSources,
            categories = new List<AudioCategory>()
        };

        ScanFolder(rootFolderPath, audioLibrary.categories, "");

        // Serialize to JSON
        string json = JsonUtility.ToJson(audioLibrary, true);
        File.WriteAllText(cacheFilePath, json);
        AssetDatabase.Refresh();
        Debug.Log("Audio data cached successfully at: " + cacheFilePath);

        // Now update all SoundSourceAudio components
        UpdateAllSoundSourceAudioComponents();
    }

    private void UpdateAllSoundSourceAudioComponents()
    {
        // Get all loaded scenes
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);

            if (scene.isLoaded)
            {
                var rootObjects = scene.GetRootGameObjects();

                foreach (var rootObject in rootObjects)
                {
                    var components = rootObject.GetComponentsInChildren<SoundSourceAudio>(true);

                    foreach (var component in components)
                    {
                        // Update the cacheFilePath in case it's different
                        component.cacheFilePath = cacheFilePath;

                        // Reload the audio library
                        component.LoadAudioLibrary();

                        // Mark the component and scene as dirty
                        EditorUtility.SetDirty(component);
                        EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
                    }
                }
            }
        }

        // Save all open scenes
        EditorSceneManager.SaveOpenScenes();

        // Refresh the editor views
        InternalEditorUtility.RepaintAllViews();
    }

    private void ScanFolder(string folderPath, List<AudioCategory> categories, string relativePath)
    {
        foreach (var directory in Directory.GetDirectories(folderPath))
        {
            string dirName = Path.GetFileName(directory);
            string newRelativePath = string.IsNullOrEmpty(relativePath) ? dirName : Path.Combine(relativePath, dirName);

            AudioCategory category = new AudioCategory
            {
                categoryName = newRelativePath.Replace("\\", "/"),
                audioItems = new List<AudioItem>()
            };

            // Check for Parameters.txt in this directory
            string parametersFilePath = Path.Combine(directory, "Parameters.txt");
            List<Parameter> parameters = null;

            if (File.Exists(parametersFilePath))
            {
                parameters = ParseParametersFile(parametersFilePath);
            }

            // Get all audio files in this directory
            var audioFiles = Directory.GetFiles(directory, "*.mp3")
                                      .Concat(Directory.GetFiles(directory, "*.wav"))
                                      .ToArray();

            foreach (var audioFile in audioFiles)
            {
                AudioItem item = new AudioItem
                {
                    displayName = Path.GetFileNameWithoutExtension(audioFile),
                    audioFilePath = audioFile, // Absolute path
                    parametersFilePath = File.Exists(parametersFilePath) ? parametersFilePath : null,
                    parameters = parameters ?? new List<Parameter>()
                };
                category.audioItems.Add(item);
            }

            // Only add category if it contains audio items
            if (category.audioItems.Count > 0)
            {
                categories.Add(category);
            }

            // Recursively scan subdirectories
            ScanFolder(directory, categories, newRelativePath);
        }
    }

    private List<Parameter> ParseParametersFile(string filePath)
    {
        var parameters = new List<Parameter>();
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            var keyValue = line.Split(':');
            if (keyValue.Length == 2)
            {
                string key = keyValue[0].Trim();
                string range = keyValue[1].Trim();

                var minMax = range.Split('-');
                if (minMax.Length == 2)
                {
                    if (float.TryParse(minMax[0], out float minValue) && float.TryParse(minMax[1], out float maxValue))
                    {
                        Parameter param = new Parameter
                        {
                            key = key,
                            minValue = minValue,
                            maxValue = maxValue,
                            defaultValue = minValue
                        };
                        parameters.Add(param);
                    }
                    else
                    {
                        Debug.LogError($"Invalid range values for parameter '{key}' in file '{filePath}'.");
                    }
                }
                else
                {
                    Debug.LogError($"Invalid range format for parameter '{key}' in file '{filePath}'. Expected format 'minValue-maxValue'.");
                }
            }
            else
            {
                Debug.LogError($"Invalid parameter line format in file '{filePath}': '{line}'. Expected format 'ParameterName: minValue-maxValue'.");
            }
        }
        return parameters;
    }
}
