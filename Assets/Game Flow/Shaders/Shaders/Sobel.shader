Shader "CustomEffects/UkiyoeSobel"
{
    Properties
    {
        _OutlineThickness("Outline Thickness", Range(1, 5)) = 3
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _DepthMultiplier("Depth Multiplier", Float) = 25.0
        _NoiseScale("Noise Scale", Range(1, 100)) = 50
        _NoiseStrength("Noise Strength", Range(0, 1)) = 0.1
        _TimeScale("Time Scale", Range(0, 2)) = 0.5
        _BokashiColor("Bokashi Color", Color) = (0.6,0.1,0.4,1)
        _BokashiStrength("Bokashi Strength", Range(0, 1)) = 0.3
        _EdgeThreshold("Edge Threshold", Range(0.01, 0.2)) = 0.05
        _SolidNoiseStrength("Solid Area Noise", Range(0, 0.05)) = 0.005
        _NoiseEdgeOnly("Noise on Edges Only", Range(0, 1)) = 0.8
        _DepthContrast("Depth Contrast", Range(0.01, 10)) = 2.0
    }

    HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

        float _OutlineThickness;
        float4 _OutlineColor;
        float _DepthMultiplier;
        float _NoiseScale;
        float _NoiseStrength;
        float _TimeScale;
        float4 _BokashiColor;
        float _BokashiStrength;
        float _EdgeThreshold;
        float _SolidNoiseStrength;
        float _NoiseEdgeOnly;
        float _DepthContrast;

        // Define Sobel kernels as separate floats instead of matrices
        static const float Sx[9] = {
            -1, -2, -1,
             0,  0,  0,
             1,  2,  1
        };

        static const float Sy[9] = {
            -1, 0, 1,
            -2, 0, 2,
            -1, 0, 1
        };

        // Hash function
        float2 hash22(float2 p)
        {
            float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
            p3 += dot(p3, p3.yzx + 33.33);
            return frac((p3.xx+p3.yz)*p3.zy);
        }

        float perlin2D(float2 p)
        {
            float2 pi = floor(p);
            float2 pf = frac(p);
            
            float2 w = pf * pf * (3.0 - 2.0 * pf);
            
            float2 n00 = hash22(pi + float2(0.0, 0.0));
            float2 n01 = hash22(pi + float2(0.0, 1.0));
            float2 n10 = hash22(pi + float2(1.0, 0.0));
            float2 n11 = hash22(pi + float2(1.0, 1.0));
            
            float2 n0 = lerp(n00, n01, w.y);
            float2 n1 = lerp(n10, n11, w.y);
            
            return lerp(n0.x, n1.x, w.x);
        }

        float fbm(float2 p)
        {
            float f = 0.0;
            float w = 0.5;
            float time = _Time.y * _TimeScale;
            
            for (int i = 0; i < 4; i++)
            {
                f += w * perlin2D(p);
                p *= 2.0;
                p += time;
                w *= 0.5;
            }
            
            return f;
        }

        float Luma(float3 color)
        {
            return dot(color, float3(0.2125, 0.7154, 0.0721));
        }

        float GetDepth(float2 uv)
        {
            float rawDepth = SampleSceneDepth(uv);
            #if UNITY_REVERSED_Z
                rawDepth = 1.0 - rawDepth;
            #endif
            return LinearEyeDepth(rawDepth, _ZBufferParams);
        }

        float4 UkiyoeSobel(Varyings input) : SV_Target
        {
            float2 uv = input.texcoord;
            float2 texelSize = 1.0 / _ScreenParams.xy * _OutlineThickness;
                            
            // Sample original color
            float4 originalColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);

            // Get the current pixel's depth for scaling
            float centerDepth = GetDepth(uv);
            // return float4(centerDepth,centerDepth,centerDepth,1);
            // Normalize depth and enhance contrast for indoor scene distances
            float normalizedDepth2 = saturate(centerDepth / 0.2);
            // Apply power function to enhance contrast
            normalizedDepth2 = pow(normalizedDepth2, _DepthContrast);
            // centerDepth =1.0- normalizedDepth2;
            // return float4(normalizedDepth2, normalizedDepth2, normalizedDepth2, 1);
            // Depth-based adjustments for line rendering
            float depthFactor = saturate(1.0 - centerDepth / 5.0);
            // return float4(normalizedDepth2, normalizedDepth2, normalizedDepth2, 1);
            depthFactor = normalizedDepth2;
            float adaptiveOutlineThickness = _OutlineThickness * lerp(0, 8.8, normalizedDepth2);
            // return float4(adaptiveOutlineThickness, adaptiveOutlineThickness, adaptiveOutlineThickness, 1);
            texelSize = 1.0 / _ScreenParams.xy * adaptiveOutlineThickness;
            // return float4(texelSize,0.0, 1);
            
            // Create stable noise coordinates for temporal coherence
            float2 worldPos = uv * _ScreenParams.xy / _NoiseScale;
            // Remove the grid snap for smoother noise
            // worldPos = floor(worldPos * 100) / 100; // Grid snap for stability
            float adaptiveNoiseScale = _NoiseScale * (1.0 + (1.0 - depthFactor) * 0.5);
            
            // Calculate noise
            float2 noiseUV = uv * adaptiveNoiseScale;
            float2 stableNoiseUV = worldPos;
            float brushStrokeEffect = fbm(stableNoiseUV * 3.0 + _Time.y * _TimeScale * 0.2);
            brushStrokeEffect = pow(brushStrokeEffect, 2.0) * 0.3;
            // return float4(brushStrokeEffect, brushStrokeEffect, brushStrokeEffect, 1);
            // Calculate noise offset for hand-drawn feel
            float2 noiseOffset = float2(fbm(noiseUV), fbm(noiseUV + 3.14));
            float depthScale = saturate(centerDepth / 10.0);
            noiseOffset *= _NoiseStrength * texelSize * (1.0 + depthScale);
            // return float4(noiseOffset,0.0, 1);

            // Handle transitions for very distant objects
            float normalizedDepth = saturate(centerDepth / 0.5);
            float transitionFactor = smoothstep(0.90, 0.98, normalizedDepth);
            
            // Smoothly transition to original color when too close
            if (transitionFactor > 0.0) {
                return lerp(originalColor, originalColor, transitionFactor);
            }                

            // Sample depths with noise offset
            float depth[9];
            depth[0] = GetDepth(uv + texelSize * float2(-1,  1) + noiseOffset);
            depth[1] = GetDepth(uv + texelSize * float2(-1,  0) + noiseOffset);
            depth[2] = GetDepth(uv + texelSize * float2(-1, -1) + noiseOffset);
            depth[3] = GetDepth(uv + texelSize * float2( 0, -1) + noiseOffset);
            depth[4] = GetDepth(uv + noiseOffset);
            depth[5] = GetDepth(uv + texelSize * float2( 0,  1) + noiseOffset);
            depth[6] = GetDepth(uv + texelSize * float2( 1, -1) + noiseOffset);
            depth[7] = GetDepth(uv + texelSize * float2( 1,  0) + noiseOffset);
            depth[8] = GetDepth(uv + texelSize * float2( 1,  1) + noiseOffset);

            // Calculate Sobel
            float depthGx = 0, depthGy = 0;
            for(int i = 0; i < 9; i++)
            {
                depthGx += depth[i] * Sx[i];
                depthGy += depth[i] * Sy[i];
            }

            float gradientDepth = sqrt(depthGx * depthGx + depthGy * depthGy);

            // Sample normals with same noise offset
            float normal[9];
            normal[0] = Luma(SampleSceneNormals(uv + texelSize * float2(-1,  1) + noiseOffset));
            normal[1] = Luma(SampleSceneNormals(uv + texelSize * float2(-1,  0) + noiseOffset));
            normal[2] = Luma(SampleSceneNormals(uv + texelSize * float2(-1, -1) + noiseOffset));
            normal[3] = Luma(SampleSceneNormals(uv + texelSize * float2( 0, -1) + noiseOffset));
            normal[4] = Luma(SampleSceneNormals(uv + noiseOffset));
            normal[5] = Luma(SampleSceneNormals(uv + texelSize * float2( 0,  1) + noiseOffset));
            normal[6] = Luma(SampleSceneNormals(uv + texelSize * float2( 1, -1) + noiseOffset));
            normal[7] = Luma(SampleSceneNormals(uv + texelSize * float2( 1,  0) + noiseOffset));
            normal[8] = Luma(SampleSceneNormals(uv + texelSize * float2( 1,  1) + noiseOffset));

            // Calculate Sobel for normals
            float normalGx = 0, normalGy = 0;
            for(int i = 0; i < 9; i++)
            {
                normalGx += normal[i] * Sx[i];
                normalGy += normal[i] * Sy[i];
            }

            float gradientNormal = sqrt(normalGx * normalGx + normalGy * normalGy);

            // Calculate depth discontinuity for line priority
            float depthDiscontinuity = abs(depthGx) + abs(depthGy);
            float normalDiscontinuity = abs(normalGx) + abs(normalGy);
            
            // Combine depth and normal edges
            float outline = gradientDepth * _DepthMultiplier + gradientNormal;
            
            // Add brush-like variation to edge thickness and appearance
            outline *= (1.0 + brushStrokeEffect);
            
            // Emphasize edges with high depth discontinuity (object silhouettes)
            float priorityFactor = smoothstep(0.1, 0.3, depthDiscontinuity);
            outline = lerp(outline, outline * 1.5, priorityFactor);
            
            // Make lines more deliberate with a sharper falloff
            outline = smoothstep(0, _EdgeThreshold, outline) * outline;
            
            // Apply non-linear transformation for more distinctive lines
            outline = pow(outline, 1.5);
            
            // Add stable noise to outline intensity for hand-drawn feel
            outline *= (0.8 + fbm(stableNoiseUV) * 0.4);
            
            // Create bokashi-style color gradation
            float4 baseColor = originalColor;
            
            // Create vertical gradation (mimicking bokashi technique)
            float verticalGradient = pow(uv.y, 1.2);
            float4 gradientColor = lerp(originalColor, _BokashiColor, verticalGradient);
            
            // Apply gradation with noise for that hand-printed look
            float gradientNoise = fbm(uv * _NoiseScale * 0.5) * 0.15;
            baseColor = lerp(baseColor, gradientColor, _BokashiStrength + gradientNoise);
            
            // Adjust outline color with brush-like variations
            float4 finalOutlineColor = _OutlineColor;
            finalOutlineColor.rgb *= (0.85 + brushStrokeEffect * 0.3);
            
            // Combine everything for final result
            return lerp(baseColor, finalOutlineColor, saturate(outline));
        }
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off 
        Cull Off
        
        Pass
        {
            Name "UkiyoeSobel"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment UkiyoeSobel
            ENDHLSL
        }
    }
}