using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundSourceGenerator : SoundSource
{
    public string[] generatorTypes = { "Wind", "Water" };
    public int selectedGeneratorTypeIndex = 0;

    // For Wind
    public string[] foliageTypes = { "Leaves", "Needles", "Mixed" };
    public int selectedFoliageTypeIndex = 0;
    public float leavesTreeSize = 1.0f;

    // For Water
    public string[] waterTypes = { "River", "Cascade", "Drinking Fountain", "Monumental Fountain", "Spray Fountain", "Shore" };
    public int selectedWaterTypeIndex = 0;
    public float size = 1.0f;

    protected override string SourceType => "generator";
    protected override int SourceSelection => 0;
    protected override float MultiSize => selectedGeneratorTypeIndex == 0 ? leavesTreeSize : size;
    protected override List<ParameterValue> ParameterValues => parameterValues;

    [SerializeField]
    [HideInInspector]
    public List<ParameterValue> parameterValues = new List<ParameterValue>();

    private GameObject debugMesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == 3) // Assuming layer 3 is the debug mesh
            {
                debugMesh = obj;
            }
        }

        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            AddOrReplaceMeshCollider();
        }
    }

    private void AddOrReplaceMeshCollider()
    {
        // Check if a collider already exists and remove it
        MeshCollider existingCollider = GetComponent<MeshCollider>();
        if (existingCollider != null)
        {
            Destroy(existingCollider);
        }

        // Add a new MeshCollider and set it to convex
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        meshCollider.convex = true; // Enable convex mode for compatibility with ClosestPoint
    }

    protected override void Update()
    {
        base.Update();

        if (selectedGeneratorTypeIndex == 1 && (selectedWaterTypeIndex == 0 || selectedWaterTypeIndex == 5))
        {
            if (meshCollider != null && mainCamera != null)
            {
                Vector3 cameraPosition = mainCamera.transform.position;
                Vector3 closestPoint = Physics.ClosestPoint(cameraPosition, meshCollider, meshCollider.transform.position, meshCollider.transform.rotation);

                // Ensure the debug mesh stays as close as possible to the camera while on the surface
                debugMesh.transform.position = closestPoint;
            }
        }
    }

    protected override void SendMessages()
    {
        base.SendMessages();

        if (osc != null)
        {
            string source = $"/source/{SourceType}/1";

            OscMessage message = new OscMessage
            {
                address = $"{source}/generatorType"
            };
            string generatorType = generatorTypes[selectedGeneratorTypeIndex];
            message.values.Add(generatorType);
            osc.Send(message);

            if (selectedGeneratorTypeIndex == 0)
            {
                message = new OscMessage
                {
                    address = $"{source}/foliageType"
                };
                string foliageType = foliageTypes[selectedFoliageTypeIndex];
                message.values.Add(foliageType);
                osc.Send(message);

                message = new OscMessage
                {
                    address = $"{source}/leavesTreeSize"
                };
                message.values.Add(leavesTreeSize);
                osc.Send(message);
            }
            else if (selectedGeneratorTypeIndex == 1)
            {
                message = new OscMessage
                {
                    address = $"{source}/waterType"
                };
                string waterType = waterTypes[selectedWaterTypeIndex];
                message.values.Add(waterType);
                osc.Send(message);

                message = new OscMessage
                {
                    address = $"{source}/size"
                };
                message.values.Add(size);
                osc.Send(message);
            }
        }
    }
}
