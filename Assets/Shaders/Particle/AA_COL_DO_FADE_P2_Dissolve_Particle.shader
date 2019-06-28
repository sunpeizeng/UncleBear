Shader "Triniti/Particle/AA_COL_DO_FADE_P2_Dissolve_Particle"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_FadeTex ("Fade Texture",2D) = "white" {}
		_AlphaTex ("AlphaTex",2D) = "alpha" {}
		_AlphaValue ("AlphaValue",Range(-0.1,1)) = 0.0   
	}

	Category
	{
		Tags { "Queue"="Transparent"}
		Blend SrcAlpha One
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
			
			uniform sampler2D _AlphaTex;
			uniform float _AlphaValue;
			
			struct appdata_col {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
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
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);

				o.col = v.color;

				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{
				
				fixed4 tex = tex2D(_MainTex, i.uv.xy) * _Color * tex2D(_FadeTex, i.uv.xy);

				fixed alpha = tex2D(_AlphaTex, i.uv.xy).r;

				fixed4 outColor;

				float Diff = alpha - _AlphaValue;
				
				if( Diff >= 0.1 )
				{
					outColor = tex;
				}
				else if( Diff <= 0 )
				{
					outColor = 0;
				}
				else 
				{
					outColor = tex*Diff*10;
				}

				return outColor*2.0;
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