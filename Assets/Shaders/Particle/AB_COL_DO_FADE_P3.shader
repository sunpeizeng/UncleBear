Shader "Triniti/Particle/AB_COL_DO_FADE_P3"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_FadeTex ("Fade Texture",2D) = "white" {}
	}

	Category
	{
		Tags { "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
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
			uniform half4 _FadeTex_ST;
			uniform sampler2D _FadeTex;
			uniform float4 _Color;
			
			struct appdata_col {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 texcoord2 : TEXCOORD1;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2 : TEXCOORD1;
				fixed4 col : COLOR;
			};
			
			v2f vert (appdata_col v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
				
				o.uv2.xy = TRANSFORM_TEX(v.texcoord2,_FadeTex);

				o.col = v.color;

				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy) * tex2D(_FadeTex, i.uv2.xy) * _Color * 2.0f;
				return tex * i.col;
			}
			
			ENDCG
		
		SubShader
		{
		  Pass
		  {
		    Cull Front
		  	CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		  }
		  
		  Pass
		  {
		    Cull Back
		  	CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			ENDCG
		  }
		  
		}
		
		SubShader
		{
			Pass
			{
			    Cull Front
				SetTexture [_MainTex]
				{
					constantColor [_Color]
					combine constant * primary
				}
				SetTexture [_MainTex]
				{
					combine texture * previous double
				}
				SetTexture [_FadeTex]
				{
					combine texture * previous
				}
			}
			
			Pass
			{
			    Cull Back
				SetTexture [_MainTex]
				{
					constantColor [_Color]
					combine constant * primary
				}
				SetTexture [_MainTex]
				{
					combine texture * previous double
				}
				SetTexture [_FadeTex]
				{
					combine texture * previous
				}
			}
		}
	}
}

