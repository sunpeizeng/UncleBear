//小猪哥友情提供
//2012,8,29
Shader "Triniti/Extra/LightHalo_Color"
{
	Properties
	{
		_HaloColor ("Halo Color", Color) = (1,1,1,1)
		_Halo ("Halo (RGBA)", 2D) = "black" {}
		_Color ("Halo Color", Color) = (1,1,1,1)
		_MainTex ("MainTex (RGB)", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}

	}

	SubShader {
		LOD 190
		Lighting on
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" "Queue"="Geometry+1" }	
				
		Pass {
		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			uniform float4 _Color;
			uniform float4 _HaloColor;
			uniform sampler2D _MainTex;
			uniform half4 _MainTex_ST;
			uniform sampler2D _Halo;
			uniform half4 _Halo_ST;
			uniform sampler2D _LightMap;
			uniform half4 _LightMap_ST;
			
			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			
			struct v2f
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
				half2 uv2 : TEXCOORD2;
			};
			
			v2f vert (VertexInput v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv1.xy = TRANSFORM_TEX(v.texcoord1,_Halo);
				o.uv2.xy = TRANSFORM_TEX(v.texcoord1,_LightMap);
			
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 texMain = tex2D(_MainTex, i.uv);
				fixed4 texHalo = tex2D(_Halo, i.uv1);
				fixed4 texLightMap = tex2D(_LightMap, i.uv2);
				
				texHalo = texHalo*texHalo*2*_HaloColor;
				texMain = texMain*(texHalo*2 + 1);
				fixed4 outColor = texMain * texLightMap*_Color;
				return outColor;
			}
			
			ENDCG
		}
		

	}	

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		BindChannels
		{
			Bind "texcoord1", texcoord0
			Bind "texcoord", texcoord1
			Bind "texcoord", texcoord2
			Bind "texcoord", texcoord3
			Bind "texcoord1", texcoord4
		}

		Pass
		{
			Fog{ Mode Off }
			
			SetTexture [_Halo]{ 
			combine texture * texture double

			}
			
			SetTexture [_Halo]{ 
			constantColor [_HaloColor]
			combine constant * previous
			}

			SetTexture [_MainTex]{ combine texture * previous double }
			SetTexture [_MainTex]{ combine previous + texture }

			SetTexture [_LightMap]{ combine texture * previous }
		}
	}

}

