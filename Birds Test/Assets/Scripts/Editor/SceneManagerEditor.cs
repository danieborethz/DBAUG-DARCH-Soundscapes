using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(SceneManager))]
public class SceneManagerEditor : Editor
{
    private SerializedProperty windIntensityProp;
    private SerializedProperty distanceScaleProp;
    private SerializedProperty enableWindProp;
    private SerializedProperty enableWaterProp;

    private SerializedProperty selectedAmbisonicIndexProp;
    private SerializedProperty ambientSoundFileProp;
    private SceneManager sceneManager;
    private string[] ambisonicAudioNames;

    private void OnEnable()
    {
        sceneManager = (SceneManager)target;

        // Find properties you want to show
        windIntensityProp = serializedObject.FindProperty("windIntensity");
        distanceScaleProp = serializedObject.FindProperty("distanceScale");
        enableWindProp = serializedObject.FindProperty("enableWind");
        enableWaterProp = serializedObject.FindProperty("enableWater");
        selectedAmbisonicIndexProp = serializedObject.FindProperty("selectedAmbisonicIndex");

        ambientSoundFileProp = serializedObject.FindProperty("ambientSoundFile");

        sceneManager.LoadAudioLibrary();

        UpdateAmbisonicAudioNames();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(enableWindProp);
        EditorGUILayout.PropertyField(enableWaterProp);
        EditorGUILayout.PropertyField(windIntensityProp);
        EditorGUILayout.PropertyField(distanceScaleProp);

        EditorGUILayout.Space();
        // Draw the ambisonic dropdown if available
        if (sceneManager.ambisonicAudioItems != null && sceneManager.ambisonicAudioItems.Count > 0)
        {
            EditorGUILayout.LabelField("Ambisonic Audio Selection", EditorStyles.boldLabel);
            int currentIndex = selectedAmbisonicIndexProp.intValue;
            int newIndex = EditorGUILayout.Popup("Ambisonic File", currentIndex, ambisonicAudioNames);

            if (newIndex != currentIndex && newIndex >= 0 && newIndex < ambisonicAudioNames.Length)
            {
                selectedAmbisonicIndexProp.intValue = newIndex;
                // Set via the serialized property
                ambientSoundFileProp.stringValue = sceneManager.ambisonicAudioItems[newIndex].audioFilePath;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No ambisonic category or audio files found. Make sure your JSON file contains a 'ambisonics' category.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void UpdateAmbisonicAudioNames()
    {
        if (sceneManager.ambisonicAudioItems != null && sceneManager.ambisonicAudioItems.Count > 0)
        {
            ambisonicAudioNames = sceneManager.ambisonicAudioItems.Select(a => a.displayName).ToArray();
        }
        else
        {
            ambisonicAudioNames = new string[0];
        }
    }
}
