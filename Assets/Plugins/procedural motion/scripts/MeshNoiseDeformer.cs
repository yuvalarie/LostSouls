using UnityEngine;
using System.Collections.Generic;

public class MeshNoiseDeformer : MonoBehaviour
{
    public float intensity = 0.1f; // Strength of the noise deformation
    public float noiseScale = 1.0f; // Scale of the Perlin noise
    public float frequency = 1.0f; // Animation speed of the noise
    public bool planarDeformation = true; // Toggle between planar and vertex deformation
    public Vector3 planarDirection = Vector3.up; // Custom planar deformation direction
    public Vector3 normalOriginOffset = Vector3.zero; // Adjusts the perceived origin of the normals
    public bool use3DNoise = false; // Use 3D Perlin noise if true, 2D if false

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] deformedVertices;
    private Vector3[] smoothedNormals;
    private Dictionary<int, List<int>> sharedVertexMap;

    void Start()
    {
        // Try to get SkinnedMeshRenderer or fallback to MeshFilter
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();

        if (skinnedMeshRenderer != null)
        {
            mesh = Instantiate(skinnedMeshRenderer.sharedMesh); // Create a new instance of the mesh
            skinnedMeshRenderer.sharedMesh = mesh; // Assign it back to the renderer
        }
        else if (meshFilter != null)
        {
            mesh = Instantiate(meshFilter.sharedMesh); // Create a new instance of the mesh
            meshFilter.sharedMesh = mesh; // Assign it back to the filter
        }
        else
        {
            Debug.LogError("No SkinnedMeshRenderer or MeshFilter found on this GameObject.");
            return;
        }

        // Store the original vertices of the mesh
        originalVertices = mesh.vertices;
        deformedVertices = new Vector3[originalVertices.Length];

        // Precompute smoothed normals for consistent vertex direction deformation
        smoothedNormals = CalculateSmoothedNormals(mesh);

        // Map shared vertices to ensure deformation consistency
        sharedVertexMap = BuildSharedVertexMap(mesh);
    }

    void Update()
    {
        // Normalize planar direction to keep consistent deformation
        Vector3 planarDir = planarDirection.normalized;

        // Temporary array to store new positions before applying them
        Vector3[] tempDeformedVertices = (Vector3[])originalVertices.Clone();

        // Track the total offset for centering the deformation
        Vector3 totalOffset = Vector3.zero;

        // Apply noise (2D or 3D based on use3DNoise toggle)
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 originalPosition = originalVertices[i];

            // Adjust normal direction based on the normal origin offset
            Vector3 adjustedNormal = (originalPosition - normalOriginOffset).normalized;

            // Generate noise based on world position and time
            float noise = 0f;
            if (use3DNoise)
            {
                // Use 3D Perlin noise
                noise = PerlinNoise3D(
                    originalPosition.x * noiseScale + Time.time * frequency,
                    originalPosition.y * noiseScale + Time.time * frequency,
                    originalPosition.z * noiseScale + Time.time * frequency
                );
            }
            else
            {
                // Use 2D Perlin noise
                noise = PerlinNoise(
                    originalPosition.x * noiseScale + Time.time * frequency,
                    originalPosition.z * noiseScale + Time.time * frequency
                );
            }

            Vector3 offset;
            if (planarDeformation)
            {
                // Apply noise along the specified planar direction
                offset = planarDir * noise * intensity;
            }
            else
            {
                // Apply noise along the adjusted normal direction
                offset = adjustedNormal * noise * intensity;
            }

            // Apply the offset to the vertex
            tempDeformedVertices[i] = originalPosition + offset;

            // Accumulate the offset for centering calculation
            totalOffset += offset;
        }

        // Calculate the average offset to recentralize the mesh
        Vector3 averageOffset = totalOffset / originalVertices.Length;

        // Apply the average offset to each vertex to keep the mesh centered
        for (int i = 0; i < tempDeformedVertices.Length; i++)
        {
            tempDeformedVertices[i] -= averageOffset;
        }

        // Ensure shared vertices move consistently
        foreach (var sharedVertices in sharedVertexMap.Values)
        {
            Vector3 averagePosition = Vector3.zero;

            // Calculate the average position for this group
            foreach (int index in sharedVertices)
            {
                averagePosition += tempDeformedVertices[index];
            }

            averagePosition /= sharedVertices.Count;

            // Assign the average position back to all shared vertices
            foreach (int index in sharedVertices)
            {
                deformedVertices[index] = averagePosition;
            }
        }

        // Update the mesh with the deformed vertices
        if (skinnedMeshRenderer != null)
        {
            // Modify the skinned mesh's vertices directly without altering sharedMesh
            Mesh skinnedMeshInstance = skinnedMeshRenderer.sharedMesh;
            skinnedMeshInstance.vertices = deformedVertices;
            skinnedMeshInstance.RecalculateNormals(); // Update normals for correct lighting
        }
        else if (meshFilter != null)
        {
            // Modify the meshFilter's mesh vertices directly without altering sharedMesh
            meshFilter.sharedMesh.vertices = deformedVertices;
            meshFilter.sharedMesh.RecalculateNormals(); // Update normals for correct lighting
        }
    }

    void OnDisable()
    {
        // Restore the original mesh when the script is disabled
        if (mesh != null && originalVertices != null)
        {
            mesh.vertices = originalVertices;
            mesh.RecalculateNormals();
        }
    }

    // Calculate smoothed normals to ensure consistent deformation
    private Vector3[] CalculateSmoothedNormals(Mesh mesh)
    {
        Vector3[] normals = new Vector3[mesh.vertexCount];
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        // Initialize normals to zero
        for (int i = 0; i < normals.Length; i++)
            normals[i] = Vector3.zero;

        // Accumulate face normals for each vertex
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            Vector3 v0 = vertices[i0];
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];

            Vector3 faceNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            normals[i0] += faceNormal;
            normals[i1] += faceNormal;
            normals[i2] += faceNormal;
        }

        // Normalize accumulated normals
        for (int i = 0; i < normals.Length; i++)
            normals[i] = normals[i].normalized;

        return normals;
    }

    // Build a map of shared vertices
    private Dictionary<int, List<int>> BuildSharedVertexMap(Mesh mesh)
    {
        Dictionary<int, List<int>> sharedVertexMap = new Dictionary<int, List<int>>();
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 position = vertices[i];

            // Find or create a group for this vertex
            bool foundGroup = false;
            foreach (var key in sharedVertexMap.Keys)
            {
                if (Vector3.Distance(vertices[key], position) < 0.0001f)
                {
                    sharedVertexMap[key].Add(i);
                    foundGroup = true;
                    break;
                }
            }

            if (!foundGroup)
            {
                sharedVertexMap[i] = new List<int> { i };
            }
        }

        return sharedVertexMap;
    }

    // Generate 3D Perlin noise
    private float PerlinNoise3D(float x, float y, float z)
    {
        return Mathf.PerlinNoise(x, Mathf.PerlinNoise(y, z));
    }

    // Generate 2D Perlin noise
    private float PerlinNoise(float x, float z)
    {
        return Mathf.PerlinNoise(x, z);
    }
}