
Shader "Triniti/Scene/COL_LM_LMCOL_BR_BM"
{
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_BlendTex1 ("BlendMap (RGBA)", 2D) = "black" {}
		_MainTex1 ("BlendTexA (A)", 2D) = "black" {}
		_MainTex ("MainTexE", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
		_LightMapColor ("Light Map Color", Color) = (0,0,0,1)
	}
	
	SubShader {
		
		Pass {	
			CGPROGRAM
			
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half4 _MainTex_ST;
			sampler2D _ShadowTex;
			sampler2D _LightMap;

			sampler2D _BlendTex1;
			half4 _BlendTex1_ST;

			sampler2D _MainTex1;
			half4 _MainTex1_ST;

			fixed4 _Color;
			fixed4 _LightMapColor;

			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			
			struct VertexOutput
			{
				float4 position_ : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uvLM : TEXCOORD2;
				float2 uv2 : TEXCOORD3;
			};
			
			VertexOutput VertexProgram(VertexInput input)
			{
				VertexOutput output;
				output.position_ = mul(UNITY_MATRIX_MVP, input.vertex);

				output.uv = TRANSFORM_TEX(input.texcoord,_MainTex);
				output.uvLM = input.texcoord1;

				output.uv1 = TRANSFORM_TEX(input.texcoord,_BlendTex1);

				output.uv2 = TRANSFORM_TEX(input.texcoord,_MainTex1);

				return output;
			}
			
			fixed4 FragmentProgram(VertexOutput input) : COLOR0
			{
				fixed4 color = tex2D(_MainTex, input.uv) * _Color;
			    fixed4 colorRGB = tex2D(_MainTex1, input.uv2);
				fixed4 colorA = tex2D(_BlendTex1, input.uv1);
				color = color*(1 - colorA.a) + colorA.a*colorRGB;
				color *= 2.0;
				color *= lerp(tex2D(_LightMap, input.uvLM),1,_LightMapColor);
				return color;
			}
			
			ENDCG

		}
	}
}