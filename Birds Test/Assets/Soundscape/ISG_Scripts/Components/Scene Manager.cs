using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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

    // Add these parameters for when the player is not inside any local zone
    [Range(10f, 50000f)]
    public float globalRoomSize = 50f;

    [Range(0f, 3f)]
    public float globalDecayTime = 2f;

    [Range(0f, 1f)]
    public float globalWetDryMix = 1f;

    [Range(0f, -20f)]
    public float globalEq = -10f;

    [Header("Audio Settings")]
    [SerializeField]
    [HideInInspector] // Hide from default inspector so we can show a dropdown instead
    public string ambientSoundFile = "";

    // NEW: Toggle to enable/disable ambisonic sound
    [SerializeField]
    private bool enableAmbisonic = true;

    [Range(0f, 5f)]
    public float distanceScale = 0.5f;

    [SerializeField]
    [HideInInspector]
    private string _cacheFilePath;

    public string cacheFilePath
    {
        get
        {
            
            _cacheFilePath = Path.Combine(Application.dataPath, "AudioDataCache.json");
            return _cacheFilePath;
        }
        set
        {
            _cacheFilePath = value;
        }
    }

    [SerializeField, HideInInspector]
    public AudioLibrary audioLibrary;
    [SerializeField, HideInInspector]
    public List<AudioItem> ambisonicAudioItems = new List<AudioItem>();
    [SerializeField, HideInInspector]
    public int selectedAmbisonicIndex = 0;

    [Header("Materials and Rendering")]
    public Material windMaterial;

    public event Action<bool> OnEnableWindChanged;
    public event Action<bool> OnEnableWaterChanged;

    private bool lastEnableWind;
    private bool lastEnableWater;
    private float lastWindIntensity;
    private float lastDistanceScale;

    private bool lastEnableAmbisonic;

    private bool lastSentEnableWind;
    private bool lastSentEnableWater;
    private float lastSentWindIntensity;
    private string lastSentAmbientSoundFile;
    private float lastSentDistanceScale;

    private bool lastSentEnableAmbisonic;

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

    // NEW: Property to access the ambisonic toggle (if you need to reference it in code)
    public bool EnableAmbisonic
    {
        get => enableAmbisonic;
        set
        {
            if (enableAmbisonic != value)
            {
                enableAmbisonic = value;
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
        lastWindIntensity = windIntensity;
        lastDistanceScale = distanceScale;
        lastEnableAmbisonic = enableAmbisonic;

        lastSentEnableWind = !enableWind;
        lastSentEnableWater = !enableWater;
        lastSentWindIntensity = float.MinValue;
        lastSentDistanceScale = float.MinValue;
        lastSentAmbientSoundFile = null;
        lastSentEnableAmbisonic = !enableAmbisonic;

        FindWindMaterialMeshes();
        UpdateWindMaterials();

        osc = FindObjectOfType<OSC>();
        SendChangedMessages();
    }

    private void Update()
    {
        if (enableWind != lastEnableWind)
        {
            lastEnableWind = enableWind;
            OnEnableWindChanged?.Invoke(enableWind);
            UpdateWindMaterials();
            SendChangedMessages();
        }

        if (enableWater != lastEnableWater)
        {
            lastEnableWater = enableWater;
            OnEnableWaterChanged?.Invoke(enableWater);
            SendChangedMessages();
        }

        if (Math.Abs(lastWindIntensity - windIntensity) > Mathf.Epsilon)
        {
            lastWindIntensity = windIntensity;
            UpdateWindMaterials();
            SendChangedMessages();
        }

        if (Math.Abs(lastDistanceScale - distanceScale) > Mathf.Epsilon)
        {
            lastDistanceScale = distanceScale;
            SendChangedMessages();
        }

        if (ambientSoundFile != lastSentAmbientSoundFile)
        {
            SendChangedMessages();
        }

        if (enableAmbisonic != lastSentEnableAmbisonic)
        {
            lastEnableAmbisonic = enableAmbisonic;
            SendChangedMessages();
        }
    }

    private void FindWindMaterialMeshes()
    {
        MeshRenderer[] allMeshes = FindObjectsOfType<MeshRenderer>();
        var meshList = new List<MeshRenderer>();

        foreach (var mesh in allMeshes)
        {
            Material[] mats = mesh.sharedMaterials;
            foreach (var mat in mats)
            {
                if (mat == windMaterial)
                {
                    meshList.Add(mesh);
                    break;
                }
            }
        }

        windMeshes = meshList.ToArray();
    }

    private void UpdateWindMaterials()
    {
        if (windMaterial == null || windMeshes == null) return;

        float intensity = enableWind ? windIntensity : 0f;
        foreach (var mesh in windMeshes)
        {
            Material[] mats = mesh.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].name.Contains(windMaterial.name))
                {
                    mats[i].SetFloat("_MotionSpeed", intensity);
                }
            }
        }
    }

    private void SendChangedMessages()
    {
        if (osc == null) return;

        if (lastSentEnableWind != enableWind)
        {
            OscMessage message = new OscMessage { address = "/master/windstatus" };
            message.values.Add(enableWind ? 1 : 0);
            osc.Send(message);
            lastSentEnableWind = enableWind;
        }

        if (lastSentEnableWater != enableWater)
        {
            OscMessage message = new OscMessage { address = "/master/waterstatus" };
            message.values.Add(enableWater ? 1 : 0);
            osc.Send(message);
            lastSentEnableWater = enableWater;
        }

        if (enableWind && Math.Abs(lastSentWindIntensity - windIntensity) > Mathf.Epsilon)
        {
            OscMessage message = new OscMessage { address = "/master/windintensity" };
            message.values.Add(windIntensity);
            osc.Send(message);
            lastSentWindIntensity = windIntensity;
        }

        if (Math.Abs(lastSentDistanceScale - distanceScale) > Mathf.Epsilon)
        {
            OscMessage message = new OscMessage { address = "/master/scaledist" };
            message.values.Add(distanceScale);
            osc.Send(message);
            lastSentDistanceScale = distanceScale;
        }

        if (lastSentEnableAmbisonic != enableAmbisonic)
        {
            OscMessage message = new OscMessage { address = "/source/bformat/1/status" };
            message.values.Add(enableAmbisonic ? 1 : 0);
            osc.Send(message);
            lastSentEnableAmbisonic = enableAmbisonic;
        }

        // NEW: Only send the ambisonic OSC message if enabled
        if (enableAmbisonic)
        {
            if (ambientSoundFile != lastSentAmbientSoundFile && !string.IsNullOrEmpty(ambientSoundFile))
            {
                OscMessage message = new OscMessage { address = "/source/bformat/1/soundpath" };
                message.values.Add(ambientSoundFile);
                osc.Send(message);
                lastSentAmbientSoundFile = ambientSoundFile;
            }
        }
        else
        {
            // Optionally, reset the cached ambient sound so it will send again when re-enabled.
            lastSentAmbientSoundFile = "";
        }
    }

    public void LoadAudioLibrary()
    {
        if (File.Exists(cacheFilePath))
        {
            string json = File.ReadAllText(cacheFilePath);
            audioLibrary = JsonUtility.FromJson<AudioLibrary>(json);

            var ambisonicsCategory = audioLibrary.categories.FirstOrDefault(c =>
                string.Equals(c.categoryName, "ambisonics", StringComparison.OrdinalIgnoreCase));
            if (ambisonicsCategory != null)
            {
                ambisonicAudioItems = ambisonicsCategory.audioItems;
            }
            else
            {
                ambisonicAudioItems = new List<AudioItem>();
            }
        }
        else
        {
            Debug.LogError("Cache file not found at: " + cacheFilePath);
        }
    }

    private void OnApplicationQuit()
    {
        if (osc == null) return;
        OscMessage message = new OscMessage { address = "/master/stop" };
        message.values.Add(0);
        osc.Send(message);
    }
}
