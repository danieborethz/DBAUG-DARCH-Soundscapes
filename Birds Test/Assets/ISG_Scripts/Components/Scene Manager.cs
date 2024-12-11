using System;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    protected OSC osc;

    [Header("Wind Settings")]
    [Range(1.0f, 10f)]
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

    // State tracking for inspector-driven changes
    private bool lastEnableWind;
    private bool lastEnableWater;
    private bool lastBinauralSound;
    private float lastWindIntensity;

    // State tracking for OSC message sending
    private bool lastSentEnableWind;
    private bool lastSentEnableWater;
    private float lastSentWindIntensity;
    private bool lastSentBinauralSound;
    private string lastSentAmbientSoundFile;

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
                SendChangedMessages();
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
                SendChangedMessages();
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

        // Initialize the "lastSent" values so that they are guaranteed different
        // from the current settings to ensure initial sending.
        lastSentEnableWind = !enableWind; // Opposite boolean value
        lastSentEnableWater = !enableWater; // Opposite boolean value
        lastSentWindIntensity = float.MinValue; // A value that can't be equal to windIntensity unless windIntensity is also min value
        lastSentBinauralSound = !useBinauralSound; // Opposite boolean
        lastSentAmbientSoundFile = null; // Null ensures difference if ambientSoundFile is not empty

        FindWindMaterialMeshes();
        UpdateWindMaterials();

        osc = FindObjectOfType<OSC>();
        SendChangedMessages();
    }

    private void Update()
    {
        // Check if wind was toggled in inspector
        if (enableWind != lastEnableWind)
        {
            lastEnableWind = enableWind;
            OnEnableWindChanged?.Invoke(enableWind);
            UpdateWindMaterials();
            SendChangedMessages();
        }

        // Check if water was toggled in inspector
        if (enableWater != lastEnableWater)
        {
            lastEnableWater = enableWater;
            OnEnableWaterChanged?.Invoke(enableWater);
            SendChangedMessages();
        }

        // Check if binaural sound was toggled
        if (useBinauralSound != lastBinauralSound)
        {
            lastBinauralSound = useBinauralSound;
            SendChangedMessages();
        }

        // Check if wind intensity changed
        if (Math.Abs(lastWindIntensity - windIntensity) > Mathf.Epsilon)
        {
            lastWindIntensity = windIntensity;
            UpdateWindMaterials();
            SendChangedMessages();
        }

        // Check if ambient sound file changed
        if (ambientSoundFile != lastSentAmbientSoundFile)
        {
            SendChangedMessages();
        }
    }

    /// <summary>
    /// Finds all MeshRenderers in the scene that have the specified windMaterial.
    /// </summary>
    private void FindWindMaterialMeshes()
    {
        MeshRenderer[] allMeshes = FindObjectsOfType<MeshRenderer>();
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

    /// <summary>
    /// Sends OSC messages only if the relevant values have changed since the last send.
    /// - Only send windIntensity if enableWind is true.
    /// - Only send messages if their corresponding value changed from the last sent value.
    /// </summary>
    private void SendChangedMessages()
    {
        if (osc == null) return;

        // Enable Wind
        if (lastSentEnableWind != enableWind)
        {
            OscMessage message = new OscMessage
            {
                address = "/master/windstatus"
            };
            message.values.Add(enableWind ? 1 : 0);
            osc.Send(message);
            lastSentEnableWind = enableWind;
        }

        // Enable Water
        if (lastSentEnableWater != enableWater)
        {
            OscMessage message = new OscMessage
            {
                address = "/master/waterstatus"
            };
            message.values.Add(enableWater ? 1 : 0);
            osc.Send(message);
            lastSentEnableWater = enableWater;
        }

        // Wind Intensity (only if enableWind is true)
        if (enableWind && Math.Abs(lastSentWindIntensity - windIntensity) > Mathf.Epsilon)
        {
            OscMessage message = new OscMessage
            {
                address = "/master/windintensity"
            };
            message.values.Add(windIntensity);
            osc.Send(message);
            lastSentWindIntensity = windIntensity;
        }

        // Binaural sound
        if (lastSentBinauralSound != useBinauralSound)
        {
            OscMessage message = new OscMessage
            {
                address = "/source/useBinauralSound"
            };
            message.values.Add(useBinauralSound);
            osc.Send(message);
            lastSentBinauralSound = useBinauralSound;
        }

        // Ambient sound file
        if (ambientSoundFile != lastSentAmbientSoundFile && !string.IsNullOrEmpty(ambientSoundFile))
        {
            OscMessage message = new OscMessage
            {
                address = "/source/ambientSound"
            };
            message.values.Add(ambientSoundFile);
            osc.Send(message);
            lastSentAmbientSoundFile = ambientSoundFile;
        }
    }
}
