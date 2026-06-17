Shader "Custom/Portal"
{
    Properties
    {
        _LeftEyeTexture ("Texture", 2D) = "white" {}
        _RightEyeTexture("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
                float2 nearPlanePos : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _LeftEyeTexture;
            sampler2D _RightEyeTexture;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float fovRad = atan(1.0f / unity_CameraProjection._m11) * 2.0;
                float aspect = unity_CameraProjection._m11 / unity_CameraProjection._m00;
                float nearPlaneHeight = 2.0f * _ProjectionParams.y * tan(fovRad * 0.5f);
                float nearPlaneWidth = aspect * nearPlaneHeight;

                float4 pos4 = mul(unity_MatrixMV, v.vertex);
                float3 pos = float3(-pos4.x/pos4.w,-pos4.y/pos4.w,pos4.z/pos4.w);
                float3 rayVector = pos / length(pos);
                float3 rayPoint = pos;
                float3 planeNormal = float3(0,0,1);
                float3 planePoint = float3(0,0,_ProjectionParams.y);
                float3 diff = rayPoint - planePoint;
                float prod1 = dot(diff,planeNormal);
                float prod2 = dot(rayVector,planeNormal);
                float prod3 = prod1 / prod2;
                float3 nearPlancePos = rayPoint - rayVector * prod3;
                o.nearPlanePos = float2((nearPlancePos.x / nearPlaneWidth +1)/2,(nearPlancePos.y / nearPlaneHeight +1)/2);
                o.screenPos = ComputeScreenPos(o.vertex); // use the screen position coordinates of the portal to sample the render texture (which is our screen)
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 uv = i.screenPos.xy / i.screenPos.w; // clip space -> normalized texture
                float2 uv2 = float2((i.vertex.x +1 )/2 ,(i.vertex.y +1 )/2);
                // sample the texture
                fixed4 col = unity_StereoEyeIndex == 0 ? tex2D(_LeftEyeTexture, uv) : tex2D(_RightEyeTexture, uv);
 
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
