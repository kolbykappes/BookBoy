Shader "BookBoy/BookOutline"
{
    // An inverted-hull outline (docs/unity-practices/29): the mesh drawn again, back faces only, pushed out along its SMOOTHED normals in clip space so the line is
    // _WidthPx pixels wide whatever the distance. Meshes need smoothed normals (BookOutline.HullMesh builds them; the book's own normals are split at the box edges
    // and would leave gaps at the corners). _DepthPull (default 0) can move the vertex toward the camera along its view ray (same place on screen, nearer depth); do NOT
    // use it on a book: the back faces then come in front of the book's own front faces and cover it (tried 2026-10-19). Color, alpha and width come from a
    // MaterialPropertyBlock per hull.
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 0.95, 0.85, 1)
        _WidthPx ("Width (pixels at 1080p)", Float) = 2
        _DepthPull ("Depth pull toward the camera (m)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _WidthPx;
                float _DepthPull;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);   // handles the book's non-uniform scale

                float3 toCamera = _WorldSpaceCameraPos - positionWS;
                float dist = max(length(toCamera), 1e-4);
                positionWS += toCamera / dist * min(_DepthPull, dist * 0.5);

                float4 positionCS = TransformWorldToHClip(positionWS);
                float2 dirCS = mul((float3x3)UNITY_MATRIX_VP, normalWS).xy;
                float len = length(dirCS);
                dirCS = len > 1e-4 ? dirCS / len : float2(0, 0);
                positionCS.xy += dirCS * (_WidthPx * 2.0 / _ScreenParams.xy) * positionCS.w;   // 1 unit = 1 pixel
                output.positionCS = positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return half4(_Color.rgb, _Color.a);
            }
            ENDHLSL
        }
    }
}
