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

    // The channel index assigned to this generator:
    // Wind: 0-3 -> Channels 1-4
    // Water: 4-7 -> Channels 5-8
    [SerializeField, HideInInspector]
    private int sourceSelection = 0;

    protected override string SourceType => "stereo";
    protected override int SourceSelection => sourceSelection;
    protected override float MultiSize => 1.0f;
    protected override List<ParameterValue> ParameterValues => null;

    private GameObject debugMesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Bounds combinedBounds;
    private float forestWidth;
    private Vector3 forestCenter;

    // Last sent values for OSC
    private bool lastSentEnableGenerator;
    private int lastSentSelectedGeneratorTypeIndex;
    private int lastSentSelectedFoliageTypeIndex;
    private float lastSentLeavesTreeSize;
    private int lastSentSelectedWaterTypeIndex;
    private float lastSentSize;
    private float lastSentForestWidth;
    private Vector3 lastSentForestCenter;

    public int SourceSelectionIndex
    {
        get => sourceSelection;
        set => sourceSelection = value;
    }

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
        lastSentForestWidth = 0f;
        lastSentForestCenter = Vector3.zero;

        // Initial send of messages
        SendMessages();
    }

    private void OnDestroy()
    {
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.OnEnableWindChanged -= HandleEnableWindChanged;
        }
    }

    private void HandleEnableWindChanged(bool isEnabled)
    {
        if (selectedGeneratorTypeIndex == 0 && isEnabled)
        {
            CalculateBounds();
            SendMessages();
        }
    }

    protected override void CalculateRelativePosition()
    {
        if (selectedGeneratorTypeIndex == 0 && SceneManager.Instance.EnableWind)
        {
            relativePosition = forestCenter;
        }
        else
        {
           base.CalculateRelativePosition();
        }
    }

    private void CalculateBounds()
    {
        // Check if there are child elements and calculate the bounding box
        if (transform.childCount > 1)
        {
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

            forestWidth = Mathf.Max(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
            forestCenter = combinedBounds.center;
        }
        else
        {
            forestWidth = 0f;
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
        base.SendMessages();

        if (osc == null) return;

        string source = $"/source/generator";

        var genType = (selectedGeneratorTypeIndex == 0) ? "wind" : "water";
        var customSourceValue = (selectedGeneratorTypeIndex == 0) ? SourceSelection + 1 : SourceSelection - 3;

        // Always send status if changed
        if (lastSentEnableGenerator != enableGenerator)
        {
            var statusMessage = new OscMessage
            {
                address = $"{source}/{genType}/{customSourceValue}/status"
            };
            statusMessage.values.Add(enableGenerator ? 1 : 0);
            osc.Send(statusMessage);
            lastSentEnableGenerator = enableGenerator;
        }

        if (!enableGenerator) return;

        // Wind
        if (selectedGeneratorTypeIndex == 0)
        {
            // Foliage type
            if (selectedFoliageTypeIndex != lastSentSelectedFoliageTypeIndex || selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
            {
                var message = new OscMessage
                {
                    address = $"{source}/wind/{customSourceValue}/type"
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
                    address = $"{source}/wind/{customSourceValue}/size"
                };
                message.values.Add(leavesTreeSize);
                osc.Send(message);
                lastSentLeavesTreeSize = leavesTreeSize;
            }

            // Single/forest
            if (!Mathf.Approximately(forestWidth, lastSentForestWidth) || selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
            {
                var message = new OscMessage
                {
                    address = $"{source}/wind/{customSourceValue}/width"
                };
                message.values.Add((forestWidth > 0) ? MapValue(forestWidth, 1f, 100f, 1f, 10f) : 0);
                osc.Send(message);
                lastSentForestWidth = forestWidth;
            }
        }
        else if (selectedGeneratorTypeIndex == 1)
        {
            // Water
            if (selectedWaterTypeIndex != lastSentSelectedWaterTypeIndex || selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
            {
                var message = new OscMessage
                {
                    address = $"{source}/water/{customSourceValue}/type"
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
                    address = $"{source}/water/{customSourceValue}/size"
                };
                message.values.Add(size);
                osc.Send(message);
                lastSentSize = size;
            }
        }

        // Update last sent generator type
        if (selectedGeneratorTypeIndex != lastSentSelectedGeneratorTypeIndex)
        {
            lastSentSelectedGeneratorTypeIndex = selectedGeneratorTypeIndex;
        }
    }

    private float MapValue(float value, float inMin, float inMax, float outMin, float outMax)
    {
        if (value > inMax)
        {
            value = inMax;
        }
        else if (value < inMin)
        {
            value = inMin;
        }
        return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
    }
}
