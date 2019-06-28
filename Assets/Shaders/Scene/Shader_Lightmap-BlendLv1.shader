
Shader "Triniti/Scene/Shader_Lightmap-BlendLv1"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_BlendTex1 ("BlendMap (RGBA)", 2D) = "black" {}
		_MainTex1 ("BlendTexA (R)", 2D) = "black" {}
		_MainTex ("MainTexE", 2D) = "" {}
		//_LightMapBright ("Bright (RGB)", 2D) = "white" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
	}

		SubShader {

		Tags { "RenderType"="Opaque" }
				
		Pass {
		
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			uniform float4 _Color;

			uniform half4 _BlendTex1_ST;
			uniform sampler2D _BlendTex1;

			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;

			uniform half4 _MainTex1_ST;
			uniform sampler2D _MainTex1;

			uniform half4 _LightMap_ST;
			uniform sampler2D _LightMap;
			
			struct V2In
			{
				half4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
			};
									
			struct v2f
			{
				half4 pos : POSITION;
				half2 uv_BlendTex1 : TEXCOORD0;
				half2 uv_MainTex : TEXCOORD1;
				half2 uv_MainTex1 : TEXCOORD2;
				half2 uv_LightMap : TEXCOORD3;
			};
			
			v2f vert (V2In v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.uv_BlendTex1 = TRANSFORM_TEX(v.texcoord,_BlendTex1);
				o.uv_MainTex = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv_MainTex1 = TRANSFORM_TEX(v.texcoord,_MainTex1);
				o.uv_LightMap = TRANSFORM_TEX(v.texcoord1,_LightMap);
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{		
				fixed4 BlendTex = tex2D(_BlendTex1, i.uv_BlendTex1.xy);
				fixed4 MainTex = tex2D(_MainTex, i.uv_MainTex.xy);
				fixed4 MainTex1 = tex2D(_MainTex1, i.uv_MainTex1.xy);
				fixed4 LightMap = tex2D(_LightMap, i.uv_LightMap.xy);

				fixed4 outColor = MainTex1 * BlendTex.a + MainTex * (1 - BlendTex.a);
				outColor *= LightMap;

				return outColor * 2.0 * _Color;
			}
			
			ENDCG
		}
		

	}	

	SubShader
	{
		//Tags { "Queue" = "Transparent" }
		Tags { "RenderType"="Opaque" }

		BindChannels
		{
			Bind "vertex", vertex
			Bind "texcoord", texcoord0
			Bind "texcoord", texcoord1
			Bind "texcoord", texcoord2
			Bind "texcoord1", texcoord3
			Bind "texcoord1", texcoord4
		}
		
		Pass
		{
			Lighting Off
			AlphaTest Off

			Fog{ Mode Off }

			Color [_Color]
			SetTexture [_MainTex]{ combine texture * primary }
			SetTexture [_BlendTex1]{ combine previous, texture }
			SetTexture [_MainTex1]{ combine texture lerp (previous) previous }
			SetTexture [_LightMapBright]{ combine texture * previous double }
			SetTexture [_LightMap]{ combine texture * previous }
		}
	}
}
