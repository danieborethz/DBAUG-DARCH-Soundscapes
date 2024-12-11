using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundSourceGenerator : SoundSource
{
    public bool enableGenerator = true;

    public string[] generatorTypes = { "Wind", "Water" };
    public int selectedGeneratorTypeIndex = 0;

    // For Wind
    public string[] foliageTypes = { "Needles", "Leaves" };
    public int selectedFoliageTypeIndex = 0;
    public float leavesTreeSize = 1.0f;

    // For Water
    public string[] waterTypes = { "Flow", "Drinking Fountain", "Splashing Fountain" };
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
    private Bounds combinedBounds;
    private bool isForest;

    // Last sent values for OSC
    private bool lastSentEnableGenerator;
    private int lastSentSelectedGeneratorTypeIndex;
    private int lastSentSelectedFoliageTypeIndex;
    private float lastSentLeavesTreeSize;
    private int lastSentSelectedWaterTypeIndex;
    private float lastSentSize;
    private bool lastSentIsForest;

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

        SceneManager.Instance.OnEnableWindChanged += HandleEnableWindChanged;

        // Initial calculation for wind if enabled
        if (selectedGeneratorTypeIndex == 0 && SceneManager.Instance.EnableWind)
        {
            CalculateBounds();
        }

        // Initialize last sent values so they differ from current (forcing initial send)
        lastSentEnableGenerator = !enableGenerator;
        lastSentSelectedGeneratorTypeIndex = -1;
        lastSentSelectedFoliageTypeIndex = -1;
        lastSentLeavesTreeSize = float.MinValue;
        lastSentSelectedWaterTypeIndex = -1;
        lastSentSize = float.MinValue;
        lastSentIsForest = !isForest;

        // Initial send of messages
        SendMessages();
    }

    private void HandleEnableWindChanged(bool isEnabled)
    {
        if (selectedGeneratorTypeIndex == 0 && isEnabled)
        {
            CalculateBounds();
            SendMessages();
        }
    }

    private void OnDestroy()
    {
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.OnEnableWindChanged -= HandleEnableWindChanged;
        }
    }

    private void CalculateBounds()
    {
        // Check if there are child elements and calculate the bounding box
        if (transform.childCount > 1)
        {
            isForest = true;
            bool boundsInitialized = false;
            combinedBounds = new Bounds(transform.position, Vector3.zero);
            foreach (Transform child in transform)
            {
                Renderer childRenderer = child.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    if (!boundsInitialized)
                    {
                        combinedBounds = childRenderer.bounds;
                        boundsInitialized = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(childRenderer.bounds);
                    }
                }
            }

            if (debugMesh != null)
            {
                debugMesh.transform.position = combinedBounds.center;
                debugMesh.transform.localScale = combinedBounds.size;
            }
        }
        else
        {
            isForest = false;
        }
    }

    private void AddOrReplaceMeshCollider()
    {
        MeshCollider existingCollider = GetComponent<MeshCollider>();
        if (existingCollider != null)
        {
            Destroy(existingCollider);
        }

        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        meshCollider.convex = true;
    }

    protected override void Update()
    {
        base.Update();

        if (selectedGeneratorTypeIndex == 1 && (selectedWaterTypeIndex == 0 || selectedWaterTypeIndex == 5) && SceneManager.Instance.EnableWater)
        {
            if (meshCollider != null && mainCamera != null && debugMesh != null)
            {
                Vector3 cameraPosition = mainCamera.transform.position;
                Vector3 closestPoint = Physics.ClosestPoint(cameraPosition, meshCollider, meshCollider.transform.position, meshCollider.transform.rotation);
                debugMesh.transform.position = closestPoint;
            }
        }
    }

    protected override void SendMessages()
    {
        //base.SendMessages();

        if (osc == null) return;

        string source = $"/source/{SourceType}/";

        // Always send status if changed
        if (lastSentEnableGenerator != enableGenerator)
        {
            var genType = (selectedGeneratorTypeIndex == 0) ? "wind" : "water";
            var statusMessage = new OscMessage
            {
                address = $"{source}/{genType}/1/status"
            };
            statusMessage.values.Add(enableGenerator ? 1 : 0);
            osc.Send(statusMessage);
            lastSentEnableGenerator = enableGenerator;
        }

        // Only send other messages if generator is enabled
        if (!enableGenerator)
        {
            return;
        }

        // If generator type is Wind
        if (selectedGeneratorTypeIndex == 0)
        {
            // Foliage type
            if (selectedFoliageTypeIndex != lastSentSelectedFoliageTypeIndex || selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
            {
                var message = new OscMessage
                {
                    address = $"{source}/wind/1/type"
                };
                message.values.Add(selectedFoliageTypeIndex);
                osc.Send(message);
                lastSentSelectedFoliageTypeIndex = selectedFoliageTypeIndex;
            }

            // Leaves tree size
            if (!Mathf.Approximately(leavesTreeSize, lastSentLeavesTreeSize) || selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
            {
                var message = new OscMessage
                {
                    address = $"{source}/wind/1/size"
                };
                message.values.Add(leavesTreeSize);
                osc.Send(message);
                lastSentLeavesTreeSize = leavesTreeSize;
            }

            // Single/forest
            if (isForest != lastSentIsForest || selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
            {
                var message = new OscMessage
                {
                    address = $"{source}/wind/1/single"
                };
                message.values.Add(isForest ? 0 : 1);
                osc.Send(message);
                lastSentIsForest = isForest;
            }
        }
        else if (selectedGeneratorTypeIndex == 1)
        {
            // Water type
            if (selectedWaterTypeIndex != lastSentSelectedWaterTypeIndex || selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
            {
                var message = new OscMessage
                {
                    address = $"{source}/water/1/type"
                };
                message.values.Add(selectedWaterTypeIndex);
                osc.Send(message);
                lastSentSelectedWaterTypeIndex = selectedWaterTypeIndex;
            }

            // Water size
            if (!Mathf.Approximately(size, lastSentSize) || selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
            {
                var message = new OscMessage
                {
                    address = $"{source}/water/1/size"
                };
                message.values.Add(size);
                osc.Send(message);
                lastSentSize = size;
            }
        }

        // Update the last sent generator type
        if (selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
        {
            lastSentSelectedGeneratorTypeIndex = selectedGeneratorTypeIndex;
        }
    }
}
