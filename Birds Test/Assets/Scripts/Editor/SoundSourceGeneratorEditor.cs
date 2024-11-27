using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(SoundSourceGenerator))]
[CanEditMultipleObjects] // Enables multi-object editing
public class SoundSourceGeneratorEditor : Editor
{
    private SerializedProperty selectedGeneratorTypeIndex;
    private SerializedProperty selectedFoliageTypeIndex;
    private SerializedProperty selectedWaterTypeIndex;
    private SerializedProperty leavesTreeSize;
    private SerializedProperty size;

    private string[] generatorTypes = { "Wind", "Water" };
    private string[] foliageTypes = { "Leaves", "Needles", "Mixed" };
    private string[] waterTypes = { "River", "Cascade", "Drinking Fountain", "Monumental Fountain", "Spray Fountain", "Shore" };

    private void OnEnable()
    {
        selectedGeneratorTypeIndex = serializedObject.FindProperty("selectedGeneratorTypeIndex");
        selectedFoliageTypeIndex = serializedObject.FindProperty("selectedFoliageTypeIndex");
        selectedWaterTypeIndex = serializedObject.FindProperty("selectedWaterTypeIndex");
        leavesTreeSize = serializedObject.FindProperty("leavesTreeSize");
        size = serializedObject.FindProperty("size");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Generator Settings", EditorStyles.boldLabel);

        // Generator Type dropdown
        DrawGeneratorTypeDropdown();

        GUILayout.Space(10);

        // Specific settings for Wind or Water
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
        if (newLeavesTreeSize != leavesTreeSize.floatValue)
        {
            leavesTreeSize.floatValue = newLeavesTreeSize;
        }

        EditorGUI.showMixedValue = false;
    }

    private void DrawSizeSlider()
    {
        EditorGUI.showMixedValue = size.hasMultipleDifferentValues;

        float newSize = EditorGUILayout.Slider("Size", size.floatValue, 0.1f, 10.0f);
        if (newSize != size.floatValue)
        {
            size.floatValue = newSize;
        }

        EditorGUI.showMixedValue = false;
    }
}
