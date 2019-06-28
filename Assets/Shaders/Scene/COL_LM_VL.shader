// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Triniti/Scene/COL_LM_VL"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
				
	// uniform float4x4 _CameraToWorld;
	uniform half4 _MainTex_ST;
	uniform sampler2D _MainTex;
	uniform half4 _LightMap_ST;
	uniform sampler2D _LightMap;
	
				
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
	
	ENDCG
		
	
	SubShader {
		LOD 190
		Lighting on
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" "Queue"="Geometry+1" }	
				
		Pass {
		
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			uniform float4 _Color;
			
			struct V2In
			{
				half4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
				half3 normal : NORMAL;
			};
									
			struct v2f
			{
				half4 pos : POSITION;
				half3 color : TEXCOORD0;
				half2 uv : TEXCOORD1;
				half2 uv1 : TEXCOORD2;
			};
			
			v2f vert (V2In v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				half3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				half3 worldNormal = mul((half3x3)unity_ObjectToWorld, v.normal.xyz);
				
				o.color = VertexLightsWorldSpace(worldPos, worldNormal);
				
				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
				
				o.uv1 = TRANSFORM_TEX(v.texcoord1,_LightMap);
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy);
				fixed4 outColor = tex * _Color;
				outColor.rgb += i.color;
				return outColor * tex2D(_LightMap, i.uv1.xy);;
			}
			
			ENDCG
		}
		

	}	
}

