// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Triniti/Scene/shadowProject_VC_DO_VL4" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Lightmap ("Lightmap (RGBA)", 2D) = "white" {}
		_ShadowTex ("Base (RGBA)", 2D) = "black" {}
		}

	SubShader {
		Tags { "Queue"="Geometry" "ShadowType"="Receive"}
		
		Pass {

			Lighting On 
			Fog{ Mode Off }
			
			BindChannels
			{
				Bind "vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
				Bind "Color", color
			}
			
			CGPROGRAM
			
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _ShadowTex;
			sampler2D _Lightmap;
			float4 _Color;
			float4x4 _ShadowMatrix;

			// uniform float4x4 _CameraToWorld;

			half3 VertexLightsWorldSpace (half3 WP, half3 WN)
			{
				half3 lightColor = half3(0.0,0.0,0.0);

				// preface & optimization
				
				half3 toLight0 = mul(unity_CameraToWorld, unity_LightPosition[0] * half4(1,1,-1,1)).xyz - WP * unity_LightPosition[0].w;
				half3 toLight1 = mul(unity_CameraToWorld, unity_LightPosition[1] * half4(1,1,-1,1)).xyz - WP * unity_LightPosition[1].w;
				half3 toLight2 = mul(unity_CameraToWorld, unity_LightPosition[2] * half4(1,1,-1,1)).xyz - WP * unity_LightPosition[2].w;
				half3 toLight3 = mul(unity_CameraToWorld, unity_LightPosition[3] * half4(1,1,-1,1)).xyz - WP * unity_LightPosition[3].w;

				half4 lengthSq4 = half4(dot(toLight0, toLight0), dot(toLight1, toLight1), dot(toLight2, toLight2), dot(toLight3, toLight3));

				half4 atten4 = half4(1.0,1.0,1.0,1.0) + lengthSq4 * half4(unity_LightAtten[0].z, unity_LightAtten[1].z,unity_LightAtten[2].z, unity_LightAtten[3].z);
				atten4 = 1.0 / atten4;
				
				//w == 0的时候为方向光
				// light #0
				half diff = saturate (dot (WN, normalize(toLight0)));
				lightColor += unity_LightColor[0].rgb * (diff * atten4.x) * unity_LightPosition[0].w;

				// light #1
				diff = saturate (dot (WN, normalize(toLight1)));
				lightColor += unity_LightColor[1].rgb * (diff * atten4.y) * unity_LightPosition[1].w;
				
				// light #2
				diff = saturate (dot (WN, normalize(toLight2)));
				lightColor += unity_LightColor[2].rgb * (diff * atten4.z) * unity_LightPosition[2].w;
				
				// light #3
				diff = saturate (dot (WN, normalize(toLight3)));
				lightColor += unity_LightColor[3].rgb * (diff * atten4.w) * unity_LightPosition[3].w;

				return lightColor;
			}

			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
				half4 color : COLOR;
				half3 normal : NORMAL;
			};
			
			struct VertexOutput
			{
				float4 position_ : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uvLM : TEXCOORD2;
				half4 color : COLOR;
				half4 vcolor : TEXCOORD3;
			};
			
			VertexOutput VertexProgram(VertexInput input)
			{
				VertexOutput output;
				output.position_ = mul(UNITY_MATRIX_MVP, input.vertex);
				float4 shadowPos = mul(_ShadowMatrix, input.vertex);
				float2 shadowUV = float2(shadowPos.x/shadowPos.w,-shadowPos.y/shadowPos.w)*0.5 + 0.5;
				output.uv = input.texcoord;
				output.uv1 = shadowUV;
				output.uvLM = input.texcoord1;
				output.color = input.color;
				
				half3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
				half3 worldNormal = mul((half3x3)unity_ObjectToWorld, input.normal.xyz);
				
				output.vcolor = half4(VertexLightsWorldSpace(worldPos, worldNormal),0);
				
				return output;
			}
			
			float4 FragmentProgram(VertexOutput input) : COLOR0
			{
			    float4 shadowColor = tex2D(_ShadowTex, input.uv1);
			    float4 color = tex2D(_MainTex, input.uv) *(1 - shadowColor.a*0.5);
				//color *= 2.0;
				color *= tex2D(_Lightmap, input.uvLM);
				return color*_Color*(1 + input.color) * 2.0 * (1 + input.vcolor);
			}
			
			ENDCG

		}
	}
}