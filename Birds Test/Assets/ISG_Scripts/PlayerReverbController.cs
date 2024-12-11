using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReverbController : MonoBehaviour
{

    // List of ReverbZones the player is currently inside
    private List<ReverbZone> overlappingZones = new List<ReverbZone>();

    protected OSC osc;

    private float accumulatedRoomSize = 0f;
    private float lastSentAccumulatedRoomSize = 0f;

    private float accumulatedDecayTime = 0f;
    private float lastSentAccumulatedDecayTime = 0f;

    private float accumulatedWetDryMix = 0f;
    private float lastSentAccumulatedWetDryMix = 0f;

    private float accumulatedEq = 0f;
    private float lastAccumulatedEQ = 0f;

    private void Start()
    {
        osc = FindObjectOfType<OSC>();
    }

    void Update()
    {
        if (overlappingZones.Count > 0)
        {
            // Calculate interpolated parameters
            InterpolateAndLogParameters();
            SendChangedMessages();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ReverbZone zone = other.GetComponent<ReverbZone>();
        if (zone != null && !overlappingZones.Contains(zone))
        {
            overlappingZones.Add(zone);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ReverbZone zone = other.GetComponent<ReverbZone>();
        if (zone != null && overlappingZones.Contains(zone))
        {
            overlappingZones.Remove(zone);
        }
    }

    private void InterpolateAndLogParameters()
    {
        // Initialize accumulators
        float totalWeight = 0f;
        accumulatedRoomSize = 0f;
        accumulatedDecayTime = 0f;     
        accumulatedWetDryMix = 0f;
        accumulatedEq = 0f;

        // Loop through overlapping zones
        foreach (ReverbZone zone in overlappingZones)
        {
            // Calculate weight based on player's position within the zone
            float weight = CalculateZoneWeight(zone);

            totalWeight += weight;
            accumulatedRoomSize += zone.RoomSize * weight;
            accumulatedDecayTime += zone.DecayTime * weight;
            accumulatedWetDryMix += zone.WetDryMix * weight;
            accumulatedEq += zone.Eq * weight;
        }

        if (totalWeight > 0f)
        {
            // Normalize accumulations
            accumulatedRoomSize = accumulatedRoomSize / totalWeight;
            accumulatedDecayTime = accumulatedDecayTime / totalWeight;
            accumulatedWetDryMix = accumulatedWetDryMix / totalWeight;
            accumulatedEq = accumulatedEq / totalWeight;

            // Log the interpolated values
            Debug.Log($"Interpolated Parameters - RoomSize: {accumulatedRoomSize}, DecayTime: {accumulatedDecayTime}, WetDryMix: {accumulatedWetDryMix}, Eq: {accumulatedEq}");
        }
    }

    private float CalculateZoneWeight(ReverbZone zone)
    {
        // Transform the player's position into the zone's local space
        Vector3 localPosition = zone.transform.InverseTransformPoint(transform.position);

        // Normalize the local position based on the zone's radii
        Vector3 normalizedPosition = new Vector3(
            localPosition.x / zone.radii.x,
            localPosition.y / zone.radii.y,
            localPosition.z / zone.radii.z
        );

        // Calculate the distance from the center in normalized space
        float distance = normalizedPosition.magnitude;

        // Calculate weight based on distance (closer to center = higher weight)
        float weight = Mathf.Clamp01(1f - distance);

        return weight;
    }

    private void SendChangedMessages()
    {
        if (osc == null) return;

        if (Math.Abs(lastSentAccumulatedRoomSize - accumulatedRoomSize) > Mathf.Epsilon)
        {
            OscMessage message = new OscMessage
            {
                address = "/reverb/roomsize"
            };
            message.values.Add(accumulatedRoomSize);
            osc.Send(message);
            Debug.Log("Message send with value " + accumulatedRoomSize);
            lastSentAccumulatedRoomSize = accumulatedRoomSize;
        }

        if (Math.Abs(lastSentAccumulatedDecayTime - accumulatedDecayTime) > Mathf.Epsilon)
        {
            OscMessage message = new OscMessage
            {
                address = "/reverb/decaytime"
            };
            message.values.Add(accumulatedDecayTime);
            osc.Send(message);
            lastSentAccumulatedDecayTime = accumulatedDecayTime;
        }

        if (Math.Abs(lastSentAccumulatedWetDryMix - accumulatedWetDryMix) > Mathf.Epsilon)
        {
            OscMessage message = new OscMessage
            {
                address = "/reverb/mix"
            };
            message.values.Add(accumulatedWetDryMix);
            osc.Send(message);
            lastSentAccumulatedWetDryMix = accumulatedWetDryMix;
        }

        if (Math.Abs(lastAccumulatedEQ - accumulatedEq) > Mathf.Epsilon)
        {
            OscMessage message = new OscMessage
            {
                address = "/reverb/eq"
            };
            message.values.Add(accumulatedEq);
            osc.Send(message);
            lastAccumulatedEQ = accumulatedEq;
        }
    }

}
