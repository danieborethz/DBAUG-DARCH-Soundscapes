using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SoundSource : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    public string[] sourceTypes = { "mono", "stereo", "multi" };

    [SerializeField]
    [HideInInspector]
    public string cacheFilePath;

    [SerializeField]
    [HideInInspector]
    public int selectedCategoryIndex = 0;

    [SerializeField]
    [HideInInspector]
    public string selectedCategoryName;

    [SerializeField]
    [HideInInspector]
    public int selectedAudioIndex = 0;

    [SerializeField]
    [HideInInspector]
    public string selectedAudioName;

    [SerializeField]
    [HideInInspector]
    public int sourceTypeSelection = 0;

    [SerializeField]
    [HideInInspector]
    public int sourceSelection = 0;

    [SerializeField]
    [HideInInspector]
    public float multiSize = 1.0f;

    [SerializeField]
    [HideInInspector]
    public int monoSources;

    [SerializeField]
    [HideInInspector]
    public int stereoSources;

    [SerializeField]
    [HideInInspector]
    public int multiSources;

    [SerializeField]
    [HideInInspector]
    public AudioLibrary audioLibrary;

    [SerializeField]
    [HideInInspector]
    public List<AudioCategory> categories;

    [SerializeField]
    [HideInInspector]
    public List<AudioItem> currentAudioItems;

    [SerializeField]
    [HideInInspector]
    public List<ParameterValue> parameterValues = new List<ParameterValue>();

    [System.Serializable]
    public class ParameterValue
    {
        public string key;
        public float minValue;
        public float maxValue;
        public float currentValue;
    }

    private OSC osc;
    private Transform mainCameraTransform;
    private Vector3 relativePosition;
    private float occlusion;
    private Camera mainCamera;

    void Awake()
    {
        cacheFilePath = Path.Combine(Application.dataPath, "AudioDataCache.json");
    }

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
            mainCameraTransform = mainCamera.transform;

        osc = FindObjectOfType<OSC>();
        UpdateSoundFile();
    }

    private void Update()
    {
        CalculateRelativePosition();
        CalculateOcclusion();
        SendMessages();
    }

    void CalculateRelativePosition()
    {
        if (mainCameraTransform != null)
        {
            Vector3 soundSourcePosition = transform.position;
            Vector3 cameraPosition = mainCameraTransform.position;
            relativePosition = soundSourcePosition - cameraPosition;
        }
    }

    void CalculateOcclusion()
    {
        if (mainCameraTransform != null)
        {
            Vector3 direction = transform.position - mainCameraTransform.position;
            float distance = direction.magnitude;
            Ray ray = new Ray(mainCamera.transform.position, direction);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, distance))
            {
                if (hitInfo.collider.gameObject != gameObject)
                    occlusion = 1.0f;
                else
                    occlusion = 0.0f;
            }
            else
            {
                occlusion = 0.0f;
            }
        }
    }

void UpdateSoundFile()
{
    if (osc != null && currentAudioItems != null && selectedAudioIndex < currentAudioItems.Count)
    {
        OscMessage message = new OscMessage
        {
            address = $"/source/{sourceTypes[sourceTypeSelection]}/{sourceSelection + 1}/soundpath"
        };

        string filePath = currentAudioItems[selectedAudioIndex].audioFilePath;

        // Check if the operating system is Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            filePath = filePath.Replace("\\", "/");
        }

        message.values.Add(filePath);
        osc.Send(message);

        Debug.Log($"Current audio item is: {filePath}");
    }
}


void SendMessages()
    {
        if (osc != null)
        {
            string source = $"/source/{sourceTypes[sourceTypeSelection]}/{sourceSelection + 1}";
            OscMessage message = new OscMessage
            {
                address = $"{source}/xyz"
            };
            message.values.Add(relativePosition.x);
            message.values.Add(relativePosition.z);
            message.values.Add(relativePosition.y);
            osc.Send(message);

            message = new OscMessage
            {
                address = $"/occlusion/{sourceTypes[sourceTypeSelection]}/{sourceSelection + 1}"
            };
            message.values.Add(occlusion);
            osc.Send(message);

            if (sourceTypes[sourceTypeSelection] == "multi")
            {
                message = new OscMessage
                {
                    address = $"{source}/size"
                };
                message.values.Add(multiSize);
                osc.Send(message);
            }

            foreach (var parameter in parameterValues)
            {
                message = new OscMessage
                {
                    address = $"{source}/{parameter.key}"
                };
                message.values.Add(parameter.currentValue);
                osc.Send(message);
            }
        }
    }
}
