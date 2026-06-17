Shader "TransitionPackage/EnvironmentTransitionShader"
{
	Properties
	{
		_FirstTex ("FirstTexture", 2D) = "black"  {}
		_SecondTex("SecondTexture", 2D) = "black" {}
		[NoScaleOffset]
		_TransitionTex("TransitionForm", 2D) = "white" {}
		_Progress("TransitionProgress", Range(0, 1)) = 0
		_MaxProgress("MaximumProgress", Range(0, 1)) = 1
	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 screenSpaceUV2 : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _FirstTex;
			sampler2D _SecondTex;
			float4 _FirstTex_ST;
			float4 _SecondTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _FirstTex);
				o.screenSpaceUV2 = ComputeScreenPos(o.vertex);
				return o;
			}
			
			float _Progress;
			sampler2D _TransitionTex;

			fixed4 frag(v2f i) : SV_Target
			{;
				float transValue = tex2D(_TransitionTex, i.uv).b;
				float edgeProgress = _Progress;
				half4 color;
				
				if (transValue <= _Progress)
				{
					color = tex2Dproj(_SecondTex, UNITY_PROJ_COORD(i.screenSpaceUV2));
				}
				else 
				{
					color = tex2D(_FirstTex, i.uv);
				}


				return color;
			}
			ENDCG
		}
	}
}
