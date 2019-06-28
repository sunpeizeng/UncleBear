Shader "Triniti/Particle/AB_COL_DO_FADE_P2_Dissolve_Rename"
{
	Properties
	{
		_Color2 ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex2 ("Particle Texture", 2D) = "white" {}
		_FadeTex2("Fade Texture",2D) = "white" {}
		_AlphaTex2 ("AlphaTex",2D) = "alpha" {}
		_AlphaValue2 ("AlphaValue",Range(-0.1,1)) = 0.0   
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
				
			uniform half4 _MainTex2_ST;
			uniform sampler2D _MainTex2;
			uniform half4 _FadeTex2_ST;
			uniform sampler2D _FadeTex2;
			uniform half4 _Color2;
			
			uniform sampler2D _AlphaTex2;
			uniform half4 _AlphaTex2_ST;

			uniform float _AlphaValue2;
			
			struct appdata_col {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2 : TEXCOORD1;
				half2 uv3 : TEXCOORD2;
				fixed4 col : COLOR;
			};
			
			v2f vert (appdata_col v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex2);
				
				o.uv2.xy = TRANSFORM_TEX(v.texcoord1,_FadeTex2);

				o.uv3.xy = TRANSFORM_TEX(v.texcoord1,_AlphaTex2);

				o.col = v.color;

				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{
				
				fixed4 tex = tex2D(_MainTex2, i.uv.xy) * _Color2 * tex2D(_FadeTex2, i.uv2.xy);

				fixed alpha = tex2D(_AlphaTex2, i.uv3.xy).r;

				fixed4 outColor;

				float Diff = alpha - _AlphaValue2;
				
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
				SetTexture [_MainTex2]
				{
					constantColor [_Color2]
					combine constant * primary
				}
				SetTexture [_MainTex2]
				{
					combine texture * previous double
				}
				SetTexture [_FadeTex2]
				{
					combine texture * previous
				}
			}
			
			Pass
			{
			    Cull Back
				SetTexture [_MainTex2]
				{
					constantColor [_Color2]
					combine constant * primary
				}
				SetTexture [_MainTex2]
				{
					combine texture * previous double
				}
				SetTexture [_FadeTex2]
				{
					combine texture * previous
				}
			}
		}
	}
}