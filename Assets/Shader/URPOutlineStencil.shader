Shader "URP/URPOutlineStencil"
{
	Properties{
		_MainTex("MainTex",2D) = "white"{}
		_OutlineWidth("Outline Width", Range(0, 1.0)) = 1.0
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags{
			"RenderPipeLine"="UniversalRenderPipeline"
		}

		HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float _OutlineWidth;
			float4 _OutlineColor;
		CBUFFER_END

		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);

		struct a2v{
			float4 positionOS: POSITION;
			float3 normalOS: NORMAL;
		};
		struct v2f{
			float4 positionCS: SV_POSITION;
		};
		ENDHLSL

		Pass
		{
			ZWrite Off
			ZTest Always
			ColorMask 0
			Stencil
			{
				Ref 1
				Pass Replace
			}
		}
		Pass
		{
			ZWrite Off
			ZTest Always
			Cull Off
			Stencil
			{
				Ref 1
				Comp NotEqual
			}
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			v2f vert(a2v v)
			{
				v2f o;
				float3 normalVS = mul((float3x3)UNITY_MATRIX_IT_MV, v.normalOS);
				normalVS.z = -0.5;
				float4 positionVS = mul(UNITY_MATRIX_MV,v.positionOS);

				positionVS.xyz += normalVS * _OutlineWidth;
				o.positionCS = mul(UNITY_MATRIX_P,positionVS);
				return o;
			}

			real4 frag(v2f i):SV_TARGET
			{
				return _OutlineColor;
			}
			ENDHLSL
		}
	}
} 