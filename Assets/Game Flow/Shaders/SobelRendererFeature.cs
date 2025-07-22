using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SobelRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private SobelSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private SobelRenderPass sobelRenderPass;
    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        sobelRenderPass = new SobelRenderPass(material, settings);  // Pass entire settings object
        
        sobelRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (sobelRenderPass == null)
        { 
            return;
        }    
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(sobelRenderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }
}

[Serializable]
public class SobelSettings
{
    [Range(0, 2f)] public float sobelStrength = 1f;
    [Range(1, 5)] public float outlineThickness = 3f;
    public Color outlineColor = Color.black;
    [Range(0, 100f)] public float depthMultiplier = 25f;
    [Range(1, 100f)] public float noiseScale = 50f;
    [Range(0, 5f)] public float noiseStrength = 0.1f;
    [Range(0, 2f)] public float timeScale = 0.5f;
    [Range(0, 1f)] public float Frequency = 0.1f;
    public Color bokashiColor = new Color(0.6f, 0.1f, 0.4f, 1f);
    [Range(0, 1f)] public float bokashiStrength = 0.3f;
    [Range(0.01f, 0.2f)] public float edgeThreshold = 0.05f;
    [Range(0, 1f)] public float noiseEdgeOnly = 0.8f;
    [Range(0, 0.05f)] public float solidNoiseStrength = 0.005f;
    [Range(0.01f, 10f)] public float depthContrast = 2.0f;
}