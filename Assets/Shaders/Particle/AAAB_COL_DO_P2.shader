Shader "Triniti/Particle/AAAB_COL_DO_P2"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		
		_Color2 ("Main Color 2", Color) = (0.5,0.5,0.5,0.5)
		_MainTex2 ("Particle Texture 2", 2D) = "white" {}
	}

	Category
	{
		Tags { "Queue"="Transparent"}
		Cull Off
		ZWrite Off
		Fog { Color (0,0,0,0) }
		
		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		
			CGINCLUDE

			#include "UnityCG.cginc"
				
			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			uniform half4 _MainTex2_ST;
			uniform sampler2D _MainTex2;
			uniform half4 _FadeTex_ST;
			uniform sampler2D _FadeTex;
			uniform half4 _Color;
			uniform half4 _Color2;
			
			struct appdata_col {
				half4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				fixed4 col : COLOR;
			};
			
			v2f vert (appdata_col v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord.xy,_MainTex);

				o.col = v.color;

				return o; 
			}

			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy) * _Color * 2.0f;
				return tex * i.col;
			}

			v2f vert2 (appdata_col v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord.xy,_MainTex2);

				o.col = v.color;

				return o; 
			}

			fixed4 frag2 (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex2, i.uv.xy) * _Color2 * 2.0f;
				return tex * i.col;
			}
			
			ENDCG

		SubShader
		{
		//AA
			Pass
			{
				Blend SrcAlpha One
			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest 
				ENDCG
			}
		//AB
			Pass
			{
			    Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM
				#pragma vertex vert2
				#pragma fragment frag2
				#pragma fragmentoption ARB_precision_hint_fastest 
				ENDCG
			}
		}
	}
}

