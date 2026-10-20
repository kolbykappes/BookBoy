Shader "BookBoy/ShelfSweep"
{
    // The gold overlay of a finished shelf (docs/unity-practices/31): an additive, unlit sheet over the row with a soft base glow that pulses, a bright band that sweeps from the
    // left end to the right end, and a glowing frame. _Progress runs 0 to 1 over the animation; _Alpha fades the whole thing. Values come from a MaterialPropertyBlock.
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 0.80, 0.30, 1)
        _Progress ("Progress", Range(0, 1)) = 0
        _Alpha ("Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+5" }

        Pass
        {
            Name "Sweep"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Progress;
                float _Alpha;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float p = _Progress;
                float x = i.uv.x, y = i.uv.y;
                float pulse = 0.5 - 0.5 * cos(p * 6.2831853 * 1.5);                      // one and a half breaths over the animation
                float centre = lerp(-0.25, 1.25, saturate(p * 1.15));
                float band = exp(-pow((x - centre) / 0.09, 2.0));                        // the sweeping band
                float trail = exp(-pow(max(0.0, centre - x) / 0.35, 1.0)) * step(x, centre) * 0.35;
                float ex = min(x, 1.0 - x), ey = min(y, 1.0 - y);
                float frame = exp(-min(ex, ey * 0.5) * 22.0);                            // glowing edge
                float vert = 0.65 + 0.35 * sin(y * 3.14159);
                float a = (0.07 * pulse + band * 0.8 + trail * 0.8 + frame * 0.6 * (0.4 + pulse)) * vert;
                return half4(_Color.rgb, saturate(a * _Alpha * _Color.a));
            }
            ENDHLSL
        }
    }
}
