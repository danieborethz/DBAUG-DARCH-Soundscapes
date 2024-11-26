using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundSourceGenerator : SoundSource
{
    public string[] generatorTypes = { "Wind", "Water" };
    public int selectedGeneratorTypeIndex = 0;

    // For Wind
    public string[] foliageTypes = { "Leaves", "Needles", "Mixed" };
    public int selectedFoliageTypeIndex = 0;
    public float leavesTreeSize = 1.0f;

    // For Water
    public string[] waterTypes = { "River", "Cascade", "Drinking Fountain", "Monumental Fountain", "Spray Fountain", "Shore" };
    public int selectedWaterTypeIndex = 0;
    public float size = 1.0f;

    // Override properties to provide data to the base class
    protected override string SourceType => "generator";
    protected override int SourceSelection => 0; // Assuming only one generator source
    protected override float MultiSize => selectedGeneratorTypeIndex == 0 ? leavesTreeSize : size;
    protected override List<ParameterValue> ParameterValues => parameterValues;

    [SerializeField]
    [HideInInspector]
    public List<ParameterValue> parameterValues = new List<ParameterValue>();

    protected override void Awake()
    {
        base.Awake();
        // Additional initialization if needed
    }

    protected override void Start()
    {
        base.Start();
        // Additional start logic if needed
    }

    protected override void Update()
    {
        base.Update();
        // Additional update logic if needed
    }

    protected override void SendMessages()
    {
        base.SendMessages();

        if (osc != null)
        {
            string source = $"/source/{SourceType}/1";

            // Send generator type
            OscMessage message = new OscMessage
            {
                address = $"{source}/generatorType"
            };
            string generatorType = generatorTypes[selectedGeneratorTypeIndex];
            message.values.Add(generatorType);
            osc.Send(message);

            if (selectedGeneratorTypeIndex == 0) // Wind
            {
                // Send foliage type
                message = new OscMessage
                {
                    address = $"{source}/foliageType"
                };
                string foliageType = foliageTypes[selectedFoliageTypeIndex];
                message.values.Add(foliageType);
                osc.Send(message);

                // Send leavesTreeSize
                message = new OscMessage
                {
                    address = $"{source}/leavesTreeSize"
                };
                message.values.Add(leavesTreeSize);
                osc.Send(message);
            }
            else if (selectedGeneratorTypeIndex == 1) // Water
            {
                // Send water type
                message = new OscMessage
                {
                    address = $"{source}/waterType"
                };
                string waterType = waterTypes[selectedWaterTypeIndex];
                message.values.Add(waterType);
                osc.Send(message);

                // Send size
                message = new OscMessage
                {
                    address = $"{source}/size"
                };
                message.values.Add(size);
                osc.Send(message);
            }
        }
    }
}
