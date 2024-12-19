using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviour
{

    [SerializeField]
    [HideInInspector]
    public string[] sourceTypes = { "mono", "stereo", "multi" };

    protected OSC osc;
    protected Transform mainCameraTransform;
    protected Vector3 relativePosition;
    protected float occlusion;
    protected Camera mainCamera;

    // Properties required for SendMessages
    protected virtual string SourceType { get; }
    protected virtual int SourceSelection { get; }
    protected virtual float MultiSize { get; }
    protected virtual List<ParameterValue> ParameterValues { get; }

    [System.Serializable]
    public class ParameterValue
    {
        public string key;
        public float minValue;
        public float maxValue;
        public float currentValue;
    }

    protected virtual void Awake()
    {
        // Base class Awake logic
    }

    protected virtual void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
            mainCameraTransform = mainCamera.transform;

        osc = FindObjectOfType<OSC>();
    }

    protected virtual void Update()
    {
        CalculateRelativePosition();
        CalculateOcclusion();
        SendMessages();
    }

    protected virtual void CalculateRelativePosition()
    {
        if (mainCameraTransform != null)
        {
            Vector3 soundSourcePosition = transform.position;
            Vector3 cameraPosition = mainCameraTransform.position;
            relativePosition = soundSourcePosition - cameraPosition;
        }
    }

    protected void CalculateOcclusion()
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

    protected virtual void SendMessages()
    {
        if (osc != null)
        {
            string sourceType = SourceType;
            int sourceSelection = SourceSelection;
            float multiSize = MultiSize;
            List<ParameterValue> parameterValues = ParameterValues;

            string source = $"/source/{sourceType}/{sourceSelection + 1}";
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
                address = $"/occlusion/{sourceType}/{sourceSelection + 1}"
            };
            message.values.Add(occlusion);
            osc.Send(message);

            if (sourceType == "multi")
            {
                message = new OscMessage
                {
                    address = $"{source}/size"
                };
                message.values.Add(multiSize);
                osc.Send(message);
            }

            if (parameterValues != null)
            {
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
}
