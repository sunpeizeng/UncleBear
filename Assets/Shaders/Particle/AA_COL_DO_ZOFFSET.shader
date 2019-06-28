Shader "Triniti/Particle/AA_COL_DO_ZOFFSET"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_ZOffset ("Z Offset",Float) = 0
	}

	Category
	{
		Tags { "Queue"="Transparent"}
		Blend SrcAlpha One
		Cull Off
		ZWrite Off
		//ZTest Always
		Fog { Color (0,0,0,0) }
		
		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		
		SubShader
		{
		Pass {
		
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"
				
			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			uniform float4 _Color;
			uniform float _ZOffset;
			
			struct appdata_col {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				fixed4 col : TEXCOORD1;
			};
			
			v2f vert (appdata_col v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MV, v.vertex);

				o.pos.z += _ZOffset;

				o.pos = mul(UNITY_MATRIX_P, o.pos);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);

				o.col = v.color;

				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy) * _Color * 2.0f;

				return tex * i.col;
			}
			
			ENDCG
		}
		}
	}
}

