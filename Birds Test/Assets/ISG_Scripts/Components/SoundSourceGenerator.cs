using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundSourceGenerator : SoundSource
{
    // Example properties specific to SoundSourceGenerator
    [SerializeField]
    private string sourceType = "generator";

    [SerializeField]
    private int sourceSelection = 0;

    [SerializeField]
    private float multiSize = 1.0f;

    [SerializeField]
    private List<ParameterValue> parameterValues = new List<ParameterValue>();

    // Override properties to provide data to the base class
    protected override string SourceType => sourceType;
    protected override int SourceSelection => sourceSelection;
    protected override float MultiSize => multiSize;
    protected override List<ParameterValue> ParameterValues => parameterValues;

    protected override void Awake()
    {
        base.Awake();
        // Additional Awake logic
    }

    protected override void Start()
    {
        base.Start();
        // Additional Start logic
    }

    protected override void Update()
    {
        base.Update();
        // Additional Update logic
    }

    // No need to override SendMessages unless you have additional logic
}
