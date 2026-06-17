Shader "Custom/Morph"
{
    Properties
    {
        _MorphMask ("MorphMask", 2D) = "white" {}
        _LeftEyeTexture ("LeftEyeTexture", 2D) = "white" {}
        _RightEyeTexture("RightEyeTexture", 2D) = "white" {}
        _Progress ("Progress", Float) = 0
        _MaxProgress ("MaxProgress", Float) = 1
        _Count ("Count", Int) =30
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
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
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex); // use the screen position coordinates of the portal to sample the render texture (which is our screen)
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            sampler2D _MorphMask;
            sampler2D _LeftEyeTexture;
            sampler2D _RightEyeTexture;
            int _Count;
            float _Progress;
            
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float transValue = tex2D(_MorphMask, i.uv*_Count).b;
                fixed4 color = (1,1,1,0);
#if defined(AR_TARGET)
                if(transValue > _Progress)
#else
                if(transValue <= _Progress)
#endif
                {
                    color = unity_StereoEyeIndex == 0 ? tex2Dproj(_LeftEyeTexture, UNITY_PROJ_COORD(i.screenPos)) : tex2Dproj(_RightEyeTexture, UNITY_PROJ_COORD(i.screenPos));
                }
                UNITY_APPLY_FOG(i.fogCoord, color );
                return color;
            }
            ENDCG
        }
    }
}