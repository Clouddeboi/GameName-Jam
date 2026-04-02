Shader "Custom/BlackholeEffect"
{
    Properties
    {
        _EventHorizon ("Black Center Size", Range(0, 1)) = 0.5
        _DistortionRange ("Distortion Softness", Range(0, 1)) = 0.2
        _Strength ("Warp Strength", Range(0, 20)) = 5.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+500" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float4 objScreenPos : TEXCOORD1;
            };

            float _EventHorizon;
            float _DistortionRange;
            float _Strength;

            Varyings vert (Attributes v) {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.screenPos = ComputeScreenPos(o.positionHCS);
                
                // Get the center of the object in screen space
                float3 worldPos = TransformObjectToWorld(float3(0,0,0));
                o.objScreenPos = ComputeScreenPos(TransformWorldToHClip(worldPos));
                return o;
            }

            half4 frag (Varyings i) : SV_Target {
                float2 uv = i.screenPos.xy / i.screenPos.w;
                float2 centerUV = i.objScreenPos.xy / i.objScreenPos.w;

                //Distance calculation with aspect ratio correction
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 dir = uv - centerUV;
                dir.x *= aspect;
                
                //Scale the distance relative to the object's perspective size
                //This makes the '0 to 1' range match the sphere mesh
                float dist = length(dir) * (i.objScreenPos.w / (length(GetObjectToWorldMatrix()[0].xyz) * 0.5));

                //Event horizon check - if the pixel is within the black hole's center, just return black
                if (dist < _EventHorizon) {
                    return half4(0, 0, 0, 1);
                }

                //Warping and distortion calculations
                //Smoothstep creates the 'ring' area where distortion happens
                float mask = smoothstep(1.5, _EventHorizon, dist);
                
                //Warp power
                float warpAmount = (_Strength * 0.01) / (dist - _EventHorizon + 0.05);
                warpAmount = clamp(warpAmount, 0, 0.1); // Prevent the pixelated 'bites'

                float2 warpedUV = uv - (normalize(dir) * warpAmount * mask);
                
                //Final background sample
                half3 sceneColor = SampleSceneColor(warpedUV);

                //Softer inner edges
                float edgeDarkening = smoothstep(_EventHorizon, _EventHorizon + _DistortionRange, dist);
                
                return half4(sceneColor * edgeDarkening, 1.0);
            }
            ENDHLSL
        }
    }
}