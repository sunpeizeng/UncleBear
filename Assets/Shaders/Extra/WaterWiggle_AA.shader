Shader "Triniti/Extra/WaterWiggle_AA"
{
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_WiggleTex ("Base (RGB)", 2D) = "white" {}
	_WiggleStrength ("Wiggle Strength", Range (0.01, 0.1)) = 0.01
}
SubShader {
		LOD 190

		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" "Queue"="Geometry+1" }	
				
		Pass {
		
			Blend SrcAlpha One
		
			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
			}
		
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			
			uniform float4 _Color;
			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			
			uniform half4 _WiggleTex_ST;
			uniform sampler2D _WiggleTex; 
			
			uniform float _WiggleStrength;
			
			struct appdata_water
			{
			    float4 vertex : POSITION;
			    float4 texcoord : TEXCOORD0;
			    float4 texcoord1 : TEXCOORD1;
			};
			
			struct v2f
			{
				half4 pos : POSITION;
				fixed4 color : COLOR;
				half2 uv_MainTex : TEXCOORD0;
				half2 uv_WiggleTex : TEXCOORD1;
			};
			
			v2f vert (appdata_water v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.uv_MainTex   = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv_WiggleTex = TRANSFORM_TEX(v.texcoord1,_WiggleTex);
			
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				half2 tc2 = i.uv_WiggleTex;
				tc2.x -= _SinTime;
				tc2.y += _CosTime;
				float4 wiggle = tex2D(_WiggleTex, tc2);
				
				i.uv_MainTex.x -= wiggle.r * _WiggleStrength;
				i.uv_MainTex.y += wiggle.b * _WiggleStrength*1.5f;
				
				fixed4 c = tex2D(_MainTex, i.uv_MainTex) * _Color;
				return c;
			}
			
			ENDCG
		}
		//end pass

	}

Fallback "VertexLit"
}
