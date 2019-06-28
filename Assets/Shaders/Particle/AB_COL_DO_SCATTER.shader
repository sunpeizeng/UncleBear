Shader "Triniti/Particle/AB_COL_DO_SCATTER"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_SecondTex ("Texture",2D) = "white" {}
		_Extend ("AlphaValue",Range(0,1)) = 0.0   
	}

	Category
	{
		Tags { "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		Fog { Color (0,0,0,0) }
		
		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType" = "Transparent"}
		LOD 200
		
		Pass {
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"	

			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			
			uniform fixed4 _Color;

			uniform half4 _SecondTex_ST;
			uniform sampler2D _SecondTex;
			
			uniform float _Extend;
			
			//uniform half4 _AtmoColor;
			//uniform half _TH;

			
			struct v2i
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				half4 pos : POSITION;
				half2 uv0 : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
			};
			
			v2f vert (v2i v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv0.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv1.xy = TRANSFORM_TEX(v.texcoord1,_SecondTex);
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				
				fixed4 tex2 = tex2D(_SecondTex, i.uv1.xy);	
				
				fixed4 tex1 = tex2D(_MainTex, i.uv0.xy + fixed2(0,(tex2.x -  i.uv0.y)*_Extend)) * _Color;
				
				return tex1*2.0;
			}
			
			ENDCG
			
		}
	} 
		
		SubShader
		{
			Pass
			{
				SetTexture [_MainTex]
				{
					constantColor [_Color]
					combine constant * primary
				}
				SetTexture [_MainTex]
				{
					combine texture * previous double
				}
			}
		}
	}
}

