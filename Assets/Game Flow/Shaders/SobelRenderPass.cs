using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
public class SobelRenderPass : ScriptableRenderPass
{
  private static readonly int sobelStrengthId = Shader.PropertyToID("_SobelStrength");
    private static readonly int outlineThicknessId = Shader.PropertyToID("_OutlineThickness");
    private static readonly int outlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int depthMultiplierId = Shader.PropertyToID("_DepthMultiplier");
    private static readonly int noiseStrengthId = Shader.PropertyToID("_NoiseStrength");
    private static readonly int noiseTexId = Shader.PropertyToID("_NoiseTex");
    private static readonly int noiseScaleId = Shader.PropertyToID("_NoiseScale");
    private static readonly int timeScaleId = Shader.PropertyToID("_TimeScale");
    private static readonly int frequency = Shader.PropertyToID("_Frequency");
    private static readonly int bokashiColorId = Shader.PropertyToID("_BokashiColor");
    private static readonly int bokashiStrengthId = Shader.PropertyToID("_BokashiStrength");
    private static readonly int edgeThresholdId = Shader.PropertyToID("_EdgeThreshold");
    private static readonly int noiseEdgeOnlyId = Shader.PropertyToID("_NoiseEdgeOnly");
    private static readonly int solidNoiseStrengthId = Shader.PropertyToID("_SolidNoiseStrength");
    private static readonly int depthContrastId = Shader.PropertyToID("_DepthContrast");

    private const string k_SobelTextureName = "_SobelTexture";
    private const string k_SobelPassName = "SobelRenderPass";

    private SobelSettings settings;
    private Material material;
    private RenderTextureDescriptor sobelTextureDescriptor;

    public SobelRenderPass(Material material, SobelSettings settings)
    {
        ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal);
        
        this.material = material;
        this.settings = settings;

        sobelTextureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
    }

    private void UpdateSobelSettings()
    {
        if (material == null) return;
        
        material.SetFloat(sobelStrengthId, settings.sobelStrength);
        material.SetFloat(outlineThicknessId, settings.outlineThickness);
        material.SetColor(outlineColorId, settings.outlineColor);
        material.SetFloat(depthMultiplierId, settings.depthMultiplier);
        material.SetFloat(noiseStrengthId, settings.noiseStrength);
        material.SetFloat(noiseScaleId, settings.noiseScale);
        material.SetFloat(timeScaleId, settings.timeScale);
        material.SetColor(bokashiColorId, settings.bokashiColor);
        material.SetFloat(bokashiStrengthId, settings.bokashiStrength);
        material.SetFloat(edgeThresholdId, settings.edgeThreshold);
        material.SetFloat(noiseEdgeOnlyId, settings.noiseEdgeOnly);
        material.SetFloat(solidNoiseStrengthId, settings.solidNoiseStrength);
        material.SetFloat(depthContrastId, settings.depthContrast);
        // if (settings.noiseTexture != null)
        //     material.SetTexture(noiseTexId, settings.noiseTexture);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph,
    ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer)
        {
            return;
        }

        sobelTextureDescriptor.width = cameraData.cameraTargetDescriptor.width;
        sobelTextureDescriptor.height = cameraData.cameraTargetDescriptor.height;
        sobelTextureDescriptor.depthBufferBits = 0;

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        TextureHandle intermediate = UniversalRenderer.CreateRenderGraphTexture(renderGraph,
            sobelTextureDescriptor, k_SobelTextureName + "_Temp", false);
        
        UpdateSobelSettings();

        if (!srcCamColor.IsValid() || !intermediate.IsValid())
            return;

        // Process the effect in the intermediate texture
        RenderGraphUtils.BlitMaterialParameters paraSobel = new(srcCamColor, intermediate, material, 0);
        renderGraph.AddBlitPass(paraSobel, k_SobelPassName);

        // Copy back to source
        RenderGraphUtils.BlitMaterialParameters paraCopy = new(intermediate, srcCamColor, material, 0);
        renderGraph.AddBlitPass(paraCopy, k_SobelPassName + "_Copy");
    }
}