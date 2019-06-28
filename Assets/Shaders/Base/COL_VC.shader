
Shader "Triniti/Character/COL_VC"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex", 2D) = "" {}
	}

	SubShader {
		Lighting off
		Tags { "Queue"="Geometry" }	
				
		Pass {
		
			Lighting off 
			Fog{ Mode Off }
			
			BindChannels
			{
				Bind "vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
				Bind "Color", color
			}
		
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			
			uniform float4 _Color;
			
			struct V2In
			{
				half4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				half4 color : COLOR;
			};
									
			struct v2f
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD1;
				half4 color : COLOR;
			};
			
			v2f vert (V2In v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
				
				o.color = v.color;
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy);

				return _Color * tex * i.color;
			}
			
			ENDCG
		}
		

	}


}

