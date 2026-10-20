Shader "BookBoy/BookGhost"
{
    // The see-through body of the placement ghost (docs/unity-practices/29): unlit, additive, almost transparent in the middle with a bright fresnel rim and a slow
    // light band that rises through the book, so it reads as a hologram of where the book will stand. The crisp outline comes from BookBoy/BookOutline on a hull.
    Properties
    {
        [HDR] _Color ("Color", Color) = (0.25, 1, 0.35, 1)
        _Fill ("Fill", Range(0, 1)) = 0.10
        _Rim ("Rim strength", Range(0, 2)) = 0.9
        _Band ("Band strength", Range(0, 1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Ghost"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On   // so the outline hull (drawn after it, back faces only) is hidden inside the ghost's silhouette and shows only around it
            ZTest LEqual
            Blend SrcAlpha One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Fill;
                float _Rim;
                float _Band;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float height : TEXCOORD2;   // 0 at the bottom of the book, 1 at the top (the mesh is a unit box)
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.height = input.positionOS.y + 0.5;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
                float rim = pow(1.0 - saturate(dot(normalize(input.normalWS), viewDir)), 2.5);
                float centre = frac(_Time.y * 0.45) * 1.5 - 0.25;           // a band sweeping from below the book to above it
                float band = exp(-pow((input.height - centre) / 0.08, 2.0));
                float a = _Color.a * (_Fill + _Rim * rim + _Band * band);
                return half4(_Color.rgb, saturate(a));
            }
            ENDHLSL
        }
    }
}
