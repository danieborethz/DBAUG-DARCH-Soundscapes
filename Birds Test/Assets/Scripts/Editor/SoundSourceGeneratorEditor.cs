using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoundSourceGenerator))]
public class SoundSourceGeneratorEditor : Editor
{
    private SoundSourceGenerator component;

    private string[] generatorTypes = { "Wind", "Water" };
    private string[] foliageTypes = { "Leaves", "Needles", "Mixed" };
    private string[] waterTypes = { "River", "Cascade", "Drinking Fountain", "Monumental Fountain", "Spray Fountain", "Shore" };

    void OnEnable()
    {
        component = (SoundSourceGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Generator Settings", EditorStyles.boldLabel);

        // Generator Type dropdown
        int newGeneratorTypeIndex = EditorGUILayout.Popup("Generator Type", component.selectedGeneratorTypeIndex, generatorTypes);
        if (newGeneratorTypeIndex != component.selectedGeneratorTypeIndex)
        {
            component.selectedGeneratorTypeIndex = newGeneratorTypeIndex;
            EditorUtility.SetDirty(component);
        }

        GUILayout.Space(10);

        if (component.selectedGeneratorTypeIndex == 0) // Wind
        {
            // Foliage Type dropdown
            int newFoliageTypeIndex = EditorGUILayout.Popup("Foliage Type", component.selectedFoliageTypeIndex, foliageTypes);
            if (newFoliageTypeIndex != component.selectedFoliageTypeIndex)
            {
                component.selectedFoliageTypeIndex = newFoliageTypeIndex;
                EditorUtility.SetDirty(component);
            }

            // Leaves/Tree Size slider
            float newLeavesTreeSize = EditorGUILayout.Slider("Leaves/Tree Size", component.leavesTreeSize, 0.1f, 10.0f);
            if (newLeavesTreeSize != component.leavesTreeSize)
            {
                component.leavesTreeSize = newLeavesTreeSize;
                EditorUtility.SetDirty(component);
            }
        }
        else if (component.selectedGeneratorTypeIndex == 1) // Water
        {
            // Water Type dropdown
            int newWaterTypeIndex = EditorGUILayout.Popup("Water Type", component.selectedWaterTypeIndex, waterTypes);
            if (newWaterTypeIndex != component.selectedWaterTypeIndex)
            {
                component.selectedWaterTypeIndex = newWaterTypeIndex;
                EditorUtility.SetDirty(component);
            }

            // Size slider
            float newSize = EditorGUILayout.Slider("Size", component.size, 0.1f, 10.0f);
            if (newSize != component.size)
            {
                component.size = newSize;
                EditorUtility.SetDirty(component);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
