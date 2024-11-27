using System;
using UnityEngine;
using static SoundSource;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    protected OSC osc;

    [Range(0f, 10f)]
    public float windIntensity = 1.0f;

    [SerializeField]
    private bool enableWind = true;

    [SerializeField]
    private bool enableWater = true;

    public bool useBinauralSound = true;

    public event Action<bool> OnEnableWindChanged;
    public event Action<bool> OnEnableWaterChanged;

    private bool lastEnableWind;
    private bool lastEnableWater;
    private bool lastBinauralSound;
    private float lastWindIntensity;

    public bool EnableWind
    {
        get => enableWind;
        set
        {
            if (enableWind != value)
            {
                enableWind = value;
                OnEnableWindChanged?.Invoke(enableWind);
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

        osc = FindObjectOfType<OSC>();
        SendMessage();
    }

    private void Update()
    {
        // Check for Inspector changes in play mode
        if (enableWind != lastEnableWind)
        {
            lastEnableWind = enableWind;
            OnEnableWindChanged?.Invoke(enableWind);
        }

        if (enableWater != lastEnableWater)
        {
            lastEnableWater = enableWater;
            OnEnableWaterChanged?.Invoke(enableWater);
        }

        if (useBinauralSound != lastBinauralSound)
        {
            SendMessage();
        }

        if (Math.Abs(lastWindIntensity - windIntensity) > Mathf.Epsilon)
        {
            lastWindIntensity = windIntensity;
            SendMessage();
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
        }
    }
}
