using System;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    [SerializeField]
    private bool enableWind = true;

    [SerializeField]
    private bool enableWater = true;

    public event Action<bool> OnEnableWindChanged;
    public event Action<bool> OnEnableWaterChanged;

    private bool lastEnableWind;
    private bool lastEnableWater;

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
    }
}
