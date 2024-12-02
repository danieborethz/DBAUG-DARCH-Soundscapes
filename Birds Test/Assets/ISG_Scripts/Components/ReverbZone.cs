using UnityEngine;

[ExecuteInEditMode]
public class ReverbZone : MonoBehaviour
{
    [Header("Audio Parameters")]

    [Range(0f, 10f)]
    public float RoomSize = 1.0f;

    [Range(0f, 10f)]
    public float DecayTime = 1.0f;

    [Range(0f, 10f)]
    public float WetDryMix = 1.0f;

    [Range(0f, 10f)]
    public float Eq = 1.0f;

    [Header("Zone Settings")]
    public Vector3 radii = Vector3.one;

    [Header("Visualization Settings")]
    public bool showInGame = false;

    private Mesh sphereMesh;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Material zoneMaterial;

    private void Awake()
    {
        InitializeComponents();
        UpdateVisuals();
    }

    private void OnValidate()
    {
        InitializeComponents();
        UpdateCollider();
        UpdateVisuals();
    }

    private void InitializeComponents()
    {
        // Load the sphere mesh if not already loaded
        if (sphereMesh == null)
        {
            sphereMesh = Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx");
            if (sphereMesh == null)
            {
                // Fallback: create a primitive sphere and extract its mesh
                GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
                DestroyImmediate(tempSphere);
            }
        }

        // Ensure there is a MeshFilter and MeshRenderer for visual representation
        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>().sharedMesh = sphereMesh;
        else
            GetComponent<MeshFilter>().sharedMesh = sphereMesh;

        if (GetComponent<MeshRenderer>() == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        else
            meshRenderer = GetComponent<MeshRenderer>();

        // Set up the material if not already set
        if (zoneMaterial == null)
        {
            zoneMaterial = new Material(Shader.Find("Standard"));
            zoneMaterial.color = new Color(0, 1, 1, 0.3f); // Semi-transparent cyan
            zoneMaterial.SetFloat("_Mode", 2); // Fade mode
            zoneMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            zoneMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            zoneMaterial.SetInt("_ZWrite", 0);
            zoneMaterial.DisableKeyword("_ALPHATEST_ON");
            zoneMaterial.EnableKeyword("_ALPHABLEND_ON");
            zoneMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            zoneMaterial.renderQueue = 3000;
        }
        meshRenderer.sharedMaterial = zoneMaterial;

        // Ensure there is a MeshCollider for the trigger zone
        if (GetComponent<MeshCollider>() == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = sphereMesh;
            meshCollider.convex = true;
            meshCollider.isTrigger = true;
        }
        else
        {
            meshCollider = GetComponent<MeshCollider>();
            meshCollider.sharedMesh = sphereMesh;
            meshCollider.convex = true;
            meshCollider.isTrigger = true;
        }
    }

    private void UpdateCollider()
    {
        // MeshCollider will automatically use the transform's scale
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = sphereMesh;
            meshCollider.convex = true;
            meshCollider.isTrigger = true;
        }
    }

    public void UpdateVisuals()
    {
        // Update the transform scale based on the radii
        transform.localScale = Vector3.Max(radii * 2f, Vector3.one * 0.01f); // Prevent zero or negative scale

        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = showInGame || !Application.isPlaying;
        }
    }

    private void Update()
    {
        // Keep updating the visuals in Edit Mode
        if (!Application.isPlaying)
        {
            UpdateVisuals();
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a wireframe sphere to represent the zone
        Gizmos.color = new Color(0, 1, 1, 0.5f); // Semi-transparent cyan
        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, 0.5f); // Sphere of radius 0.5

        Gizmos.matrix = oldMatrix;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Implement logic to apply audio parameters when an object enters the zone
        if (other.CompareTag("Player"))
        {
            // Apply audio effects to the player's audio source
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Implement logic to revert audio parameters when an object exits the zone
        if (other.CompareTag("Player"))
        {
            // Revert audio effects on the player's audio source
        }
    }
}
