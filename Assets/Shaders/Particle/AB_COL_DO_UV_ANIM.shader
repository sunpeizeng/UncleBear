Shader "Triniti/Particle/AB_COL_DO_UV_ANIM"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_width_height_max ("UV Width Height Max", vector) = (2,2,4,0)
		_frame ("Frame",float) = 15
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

			//暂时没有用
			//uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			
			uniform fixed4 _Color;
			
			uniform half4 _width_height_max;
			//帧数
			uniform float _frame;

			
			struct v2i
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				half4 pos : POSITION;
				half2 uv0 : TEXCOORD0;
			};
			
			v2f vert (v2i v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				half temp = _Time.y/_width_height_max.x * _frame;
				
				// 行
				half col = floor(temp) + v.texcoord.y;
				
				// 列
				half row = floor(frac(temp) * _width_height_max.y) + v.texcoord.x;
				
				o.uv0.xy = half2(col/_width_height_max.x,row/_width_height_max.y);
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{
				
				fixed4 tex = tex2D(_MainTex, i.uv0.xy);	
				
				//return fixed4(i.uv0.x,i.uv0.y,0,1);
				return tex*2.0;
			}
			
			ENDCG
			
		}
	} 
		
	}
}

