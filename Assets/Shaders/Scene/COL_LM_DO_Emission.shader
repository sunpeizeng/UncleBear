
Shader "Triniti/Scene/COL_LM_DO_Emission"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
		_EmissionColor ("Emission Color", Color) = (0.5,0.5,0.5,1)
		_EmissionScale ("Emission Scale",Range(0,1)) = 1
		_Emission ("Emission",2D) = "black" {}
		
	}

		SubShader {
		Lighting off
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" "Queue"="Geometry" }	
				
		Pass {
			Lighting off
			Cull Off
			Fog{ Mode Off }
			
		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			uniform half4 _LightMap_ST;
			uniform sampler2D _LightMap;
			
			uniform half4 _Color;
			uniform half4 _EmissionColor;
			
			uniform half _EmissionScale;

			uniform half4 _Emission_ST;
			uniform sampler2D _Emission;

			struct V2In
			{
				half4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
			};
									
			struct v2f
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD1;
				half2 uv1 : TEXCOORD2;
				half2 uv2 : TEXCOORD3;
			};
			
			v2f vert (V2In v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
				
				o.uv1 = TRANSFORM_TEX(v.texcoord1,_LightMap);

				o.uv2 = TRANSFORM_TEX(v.texcoord,_Emission);
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy);
				
				tex *= tex2D(_LightMap, i.uv1.xy);

				return _Color * tex + tex2D(_Emission, i.uv2.xy) * _EmissionColor * _EmissionScale;
			}
			
			ENDCG
		}
		

	}
	

}