using System.Collections.Generic;
using UnityEngine;

public class PlayerReverbController : MonoBehaviour
{

    // List of ReverbZones the player is currently inside
    private List<ReverbZone> overlappingZones = new List<ReverbZone>();

    void Update()
    {
        if (overlappingZones.Count > 0)
        {
            // Calculate interpolated parameters
            InterpolateAndLogParameters();
        }
        else
        {
            // Player is not inside any zone
            Debug.Log("Player is not inside any ReverbZone.");
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
        float accumulatedRoomSize = 0f;
        float accumulatedDecayTime = 0f;
        float accumulatedWetDryMix = 0f;
        float accumulatedEq = 0f;

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
            float normalizedRoomSize = accumulatedRoomSize / totalWeight;
            float normalizedDecayTime = accumulatedDecayTime / totalWeight;
            float normalizedWetDryMix = accumulatedWetDryMix / totalWeight;
            float normalizedEq = accumulatedEq / totalWeight;

            // Log the interpolated values
            Debug.Log($"Interpolated Parameters - RoomSize: {normalizedRoomSize}, DecayTime: {normalizedDecayTime}, WetDryMix: {normalizedWetDryMix}, Eq: {normalizedEq}");
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
}