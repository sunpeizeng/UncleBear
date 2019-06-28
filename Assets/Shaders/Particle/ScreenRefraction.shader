
Shader "Triniti/Particle/ScreenRefraction" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _NoiseTex ("NoiseTex", 2D) = "white" {}
		_Amplitude ("Amplitude",Range (0, 2)) = 0
		_Offset ("Offset",Range (0, 1)) = 0
    }
	
	CGINCLUDE

		#include "UnityCG.cginc"
		 
		sampler2D _MainTex;
		sampler2D _NoiseTex;
		sampler2D _ScreenImage;
		
		half4 _MainTex_ST;
		half4 _NoiseTex_ST;
		
		float _Amplitude;
		float _Offset;
						
		struct v2f {
			half4 pos : SV_POSITION;
			half4 uv : TEXCOORD0;
		};

		v2f vert(appdata_full v)
		{
			v2f o;
			
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);	
			o.uv.xy = o.pos.xy/o.pos.w*0.5+0.5;//TRANSFORM_TEX(v.texcoord, _MainTex);
			o.uv.zw = TRANSFORM_TEX(v.texcoord, _NoiseTex);
					
			return o; 
		}
		
		fixed4 frag( v2f i ) : COLOR
		{	
		    fixed4 noiseTex = (tex2D (_NoiseTex, i.uv.zw) - 0.5f) * _Amplitude;
			//noiseTex.y += _Offset;
			i.uv.y += _Offset;
			return tex2D (_ScreenImage, i.uv.xy + noiseTex.xy);
		}
	
	ENDCG
	
	SubShader {
		Tags { "RenderType" = "Transparent" "Reflection" = "LaserScope" "Queue" = "Transparent"}
		Cull Off
		ZWrite Off
		Blend SrcAlpha One
		
	Pass {
	
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		
		ENDCG
		 
		}
				
	} 
	FallBack Off
}
