// Upgrade NOTE: commented out 'float4x4 _CameraToWorld', a built-in variable
// Upgrade NOTE: replaced '_CameraToWorld' with 'unity_CameraToWorld'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Triniti/Scene/COL_DO_LM_VL_TOON"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
				
	// uniform float4x4 _CameraToWorld;
	uniform half4 _MainTex_ST;
	uniform sampler2D _MainTex;
	uniform half4 _LightMap_ST;
	uniform sampler2D _LightMap;
	uniform sampler2D _Ramp;
	
				
	half VertexLightsWorldSpace (half3 WP, half3 WN)
	{
		//half3 lightColor = half3(0.0,0.0,0.0);

		// preface & optimization
		half3 toLight0 = mul(unity_CameraToWorld, unity_LightPosition[0] * half4(1,1,-1,1)).xyz - WP;
		
		half lengthSq = dot(toLight0, toLight0);

		half atten = 1.0 + lengthSq * unity_LightAtten[0].z;
		
		atten = 1.0 / atten;
					
		// light #0
		half diff = saturate (dot (WN, normalize(toLight0)));
		//lightColor += unity_LightColor[0].rgb * (diff * atten);

		return (diff*0.5 + 0.5)*atten;//lightColor;
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
				half color : TEXCOORD0;
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
				
				o.uv1 = TRANSFORM_TEX(v.texcoord,_LightMap);
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy);
				half3 ramp = tex2D (_Ramp, float2(i.color,i.color)).rgb;
				fixed4 outColor = tex * _Color;
				outColor.rgb *= ramp * unity_LightColor[0].rgb;
				return outColor * tex2D(_LightMap, i.uv1.xy) * 2;
			}
			
			ENDCG
		}
		

	}	
}

