Shader "Triniti/Particle/AA_COL_DO_FADE_P2_Dissolve_Rename"
{
	Properties
	{
		_Color1 ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex1 ("Particle Texture", 2D) = "white" {}
		_FadeTex1 ("Fade Texture",2D) = "white" {}
		_AlphaTex1 ("AlphaTex",2D) = "alpha" {}
		_AlphaValue1 ("AlphaValue",Range(-0.1,1)) = 0.0   
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
				
			uniform half4 _MainTex1_ST;
			uniform sampler2D _MainTex1;
			uniform half4 _FadeTex1_ST;
			uniform sampler2D _FadeTex1;
			uniform half4 _Color1;
			
			uniform sampler2D _AlphaTex1;
			uniform half4 _AlphaTex1_ST;

			uniform float _AlphaValue1;
			
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
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex1);
				
				o.uv2.xy = TRANSFORM_TEX(v.texcoord1,_FadeTex1);

				o.uv3.xy = TRANSFORM_TEX(v.texcoord1,_AlphaTex1);

				o.col = v.color;

				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{
				
				fixed4 tex = tex2D(_MainTex1, i.uv.xy) * _Color1 * tex2D(_FadeTex1, i.uv2.xy);

				fixed alpha = tex2D(_AlphaTex1, i.uv3.xy).r;

				fixed4 outColor;

				float Diff = alpha - _AlphaValue1;
				
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
				SetTexture [_MainTex1]
				{
					constantColor [_Color1]
					combine constant * primary
				}
				SetTexture [_MainTex1]
				{
					combine texture * previous double
				}
				SetTexture [_FadeTex1]
				{
					combine texture * previous
				}
			}
			
			Pass
			{
			    Cull Back
				SetTexture [_MainTex1]
				{
					constantColor [_Color1]
					combine constant * primary
				}
				SetTexture [_MainTex1]
				{
					combine texture * previous double
				}
				SetTexture [_FadeTex1]
				{
					combine texture * previous
				}
			}
		}
	}
}