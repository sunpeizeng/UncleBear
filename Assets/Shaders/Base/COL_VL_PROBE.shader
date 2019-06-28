// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Triniti/Character/COL_VL_PROBE"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
		_ColorB ("Light Bright Color", Color) = (1,1,1,1)
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
				
	//uniform float4x4 _CameraToWorld;
	uniform half4 _MainTex_ST;
	uniform sampler2D _MainTex;
	
				
	half3 VertexLightsWorldSpace (half3 WP, half3 WN)
	{
		half3 lightColor = half3(0.0,0.0,0.0);

		// preface & optimization
		//half3 toLight0 = mul(_CameraToWorld, unity_LightPosition[0] * half4(1,1,-1,1)).xyz - WP;
		half3 toLight0 = _WorldSpaceLightPos0 - WP;
		//half3 toLight0 = half3(0,0,1);
		
		half lengthSq = dot(toLight0, toLight0);

		half atten = 1.0 + lengthSq * unity_LightAtten[0].z;
		
		atten = 1.0 / atten;
					
		// light #0
		half diff = saturate (dot (WN, normalize(toLight0)));
		lightColor += unity_LightColor[0].rgb * (diff * atten);

		return lightColor;
	}	
	
	ENDCG
		
	
	SubShader {
		LOD 190
		Lighting On
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" "Queue"="Geometry+1" "LightMode"="ForwardBase" }	
				
		Pass {
		
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			uniform half4 _Color;
			uniform half4 _ColorB;
					
			struct v2f
			{
				half4 pos : POSITION;
				half3 color : TEXCOORD0;
				half2 uv : TEXCOORD1;
				half3 probColor : TEXCOORD2;
			};
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				half3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				half3 worldNormal = mul((half3x3)unity_ObjectToWorld, v.normal.xyz);
				
				o.color = VertexLightsWorldSpace(worldPos, worldNormal) * _ColorB;
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
			
				o.probColor = ShadeSH9(float4(worldNormal,1));
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy);
				fixed4 outColor = tex * _Color;
				outColor.rgb *= i.probColor;
				outColor.rgb += i.color;
				return outColor;//fixed4(i.color,1); 
			}
			
			ENDCG
		}
		

	}	
}

