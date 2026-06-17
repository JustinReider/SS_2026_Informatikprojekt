Shader "Custom/Fade"
{
    Properties
    {
        _LeftEyeTexture ("Texture", 2D) = "white" {}
        _RightEyeTexture("Texture", 2D) = "white" {}
        _Progress ("Progress", Float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile __ AR_TARGET

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 screenPos : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _LeftEyeTexture;
            sampler2D _RightEyeTexture;
            float _Progress;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex); // use the screen position coordinates of the portal to sample the render texture (which is our screen)
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 uv = i.screenPos.xy / i.screenPos.w; // clip space -> normalized texture
                fixed4 col;
                fixed alpha = _Progress <= 0.5f ? 1 - (_Progress * 2) : 2- (_Progress*2);
#if defined(AR_TARGET)
                if (_Progress <= 0.5f)
#else
                alpha = 1-alpha;
                if (_Progress > 0.5f)
#endif
                {
                    col = unity_StereoEyeIndex == 0 ? tex2D(_LeftEyeTexture, uv) : tex2D(_RightEyeTexture, uv);
                    col *= alpha;
                    col.a = 1;
                } else
                {
                    col.r = 0;
                    col.g = 0;
                    col.b = 0;
                    col.a = alpha;
                }
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
