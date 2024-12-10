using System;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    protected OSC osc;

    [Header("Wind Settings")]
    [Range(0f, 10f)]
    public float windIntensity = 1.0f;

    [SerializeField]
    private bool enableWind = true;

    [Header("Water Settings")]
    [SerializeField]
    private bool enableWater = true;

    [Header("Audio Settings")]
    public bool useBinauralSound = true;
    public string ambientSoundFile = "";

    [Header("Materials and Rendering")]
    [Tooltip("Assign the material that should have its wind properties updated.")]
    public Material windMaterial;

    // Events
    public event Action<bool> OnEnableWindChanged;
    public event Action<bool> OnEnableWaterChanged;

    // State tracking
    private bool lastEnableWind;
    private bool lastEnableWater;
    private bool lastBinauralSound;
    private float lastWindIntensity;

    // Cache of all MeshRenderers that use the windMaterial
    private MeshRenderer[] windMeshes;

    public bool EnableWind
    {
        get => enableWind;
        set
        {
            if (enableWind != value)
            {
                enableWind = value;
                OnEnableWindChanged?.Invoke(enableWind);
                UpdateWindMaterials();
            }
        }
    }

    public bool EnableWater
    {
        get => enableWater;
        set
        {
            if (enableWater != value)
            {
                enableWater = value;
                OnEnableWaterChanged?.Invoke(enableWater);
            }
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        lastEnableWind = enableWind;
        lastEnableWater = enableWater;
        lastBinauralSound = useBinauralSound;
        lastWindIntensity = windIntensity;

        // Find all MeshRenderers that use the specified windMaterial
        FindWindMaterialMeshes();

        // Update materials immediately at game start
        UpdateWindMaterials();

        // Setup OSC if present
        osc = FindObjectOfType<OSC>();
        SendMessage();
    }

    private void Update()
    {
        // Check if wind was toggled in inspector
        if (enableWind != lastEnableWind)
        {
            lastEnableWind = enableWind;
            OnEnableWindChanged?.Invoke(enableWind);
            UpdateWindMaterials();
        }

        // Check if water was toggled in inspector
        if (enableWater != lastEnableWater)
        {
            lastEnableWater = enableWater;
            OnEnableWaterChanged?.Invoke(enableWater);
        }

        // Check if binaural sound was toggled
        if (useBinauralSound != lastBinauralSound)
        {
            lastBinauralSound = useBinauralSound;
            SendMessage();
        }

        // Check if wind intensity changed
        if (Math.Abs(lastWindIntensity - windIntensity) > Mathf.Epsilon)
        {
            lastWindIntensity = windIntensity;
            UpdateWindMaterials();
            SendMessage();
        }
    }

    /// <summary>
    /// Finds all MeshRenderers in the scene that have the specified windMaterial.
    /// </summary>
    private void FindWindMaterialMeshes()
    {
        MeshRenderer[] allMeshes = FindObjectsOfType<MeshRenderer>();
        // Using a temp list to filter, then converting to array
        var meshList = new System.Collections.Generic.List<MeshRenderer>();

        foreach (var mesh in allMeshes)
        {
            Material[] mats = mesh.sharedMaterials;
            foreach (var mat in mats)
            {
                if (mat == windMaterial)
                {
                    meshList.Add(mesh);
                    break; // found the wind material in this mesh, no need to check further
                }
            }
        }

        windMeshes = meshList.ToArray();
    }

    /// <summary>
    /// Updates the wind value on all meshes that use the windMaterial.
    /// If wind is enabled, sets the _MotionSpeed property to the current windIntensity.
    /// If wind is disabled, sets it to 0.
    /// </summary>
    private void UpdateWindMaterials()
    {
        if (windMaterial == null || windMeshes == null) return;

        float intensity = enableWind ? windIntensity : 0f;

        // Update the _MotionSpeed property on all affected materials
        foreach (var mesh in windMeshes)
        {
            // Access the instance materials since we might be changing them at runtime
            Material[] mats = mesh.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].name.Contains(windMaterial.name))
                {
                    // Assuming the shader has a float property named "_MotionSpeed"
                    mats[i].SetFloat("_MotionSpeed", intensity);
                }
            }
        }
    }

    private void SendMessage()
    {
        if (osc != null)
        {
            OscMessage message = new OscMessage
            {
                address = "/source/windIntensity"
            };
            message.values.Add(windIntensity);
            osc.Send(message);

            message = new OscMessage
            {
                address = "/source/useBinauralSound"
            };
            message.values.Add(useBinauralSound);
            osc.Send(message);

            if (!string.IsNullOrEmpty(ambientSoundFile))
            {
                message = new OscMessage
                {
                    address = "/source/ambientSound"
                };
                message.values.Add(ambientSoundFile);
                osc.Send(message);
            }
        }
    }
}
