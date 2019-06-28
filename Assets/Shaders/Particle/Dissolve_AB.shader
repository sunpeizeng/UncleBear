Shader "Triniti/Particle/Dissolve_AB" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Color",Color) = (1,1,1,1)
		_SecondTex ("Texture",2D) = "black" {}
		_ColorS ("Color",Color) = (1,1,1,1)
		_FadeTex ("Fade Texture",2D) = "white" {}
		_AlphaTex ("AlphaTex",2D) = "alpha" {}
		_AlphaValue ("AlphaValue",Range(-0.1,1)) = 0.0   
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType" = "Transparent"}
		LOD 200
		
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
		    
			Zwrite On
			
			Cull off
			
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#include "UnityCG.cginc"	

			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			
			uniform fixed4 _Color;
			uniform fixed4 _ColorS;

			uniform half4 _SecondTex_ST;
			uniform sampler2D _SecondTex;

            uniform sampler2D _AlphaTex;
			uniform half4 _AlphaTex_ST;

			uniform sampler2D _FadeTex;
			uniform half4 _FadeTex_ST;

			uniform float _AlphaValue;
			
			struct v2i
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				half2 texcoord1 : TEXCOORD1;
			};

			struct v2f
			{
				half4 pos : POSITION;
				half2 uv_main : TEXCOORD0;
				half2 uv_second : TEXCOORD1;
				half2 uv_fade : TEXCOORD2;
				half2 uv_alpha : TEXCOORD3;
			};
			
			v2f vert (v2i v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

				o.uv_main.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.uv_second.xy = TRANSFORM_TEX(v.texcoord,_SecondTex);
				o.uv_fade.xy = TRANSFORM_TEX(v.texcoord,_FadeTex);
				o.uv_alpha.xy = TRANSFORM_TEX(v.texcoord,_AlphaTex);

				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex1 = tex2D(_MainTex, i.uv_main.xy) * _Color;
				fixed4 tex2 = tex2D(_SecondTex, i.uv_second.xy) * _ColorS;
				fixed alpha = tex2D(_AlphaTex, i.uv_alpha.xy).r;
				fixed4 fade = tex2D(_FadeTex, i.uv_fade.xy);
				
				fixed4 outColor;

				float Diff = alpha - _AlphaValue;
       		
       			if( Diff >= 0.1 )
       			{
       			   outColor = tex1;
       			}
       			else if( Diff <= 0 )
       			{
       			   outColor = tex2;
       			}
       			else 
       			{
       			   outColor = tex1*Diff*10 + tex2*(1 - Diff)*10;
       			}		


				return outColor*2.0*fade;
			}
			
			ENDCG
			
		}
	} 
	FallBack "Diffuse"
}
