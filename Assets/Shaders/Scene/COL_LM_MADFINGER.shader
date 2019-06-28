// - Unlit
// - Per-vertex (virtual) camera space specular light
// - SUPPORTS lightmap

Shader "Triniti/Scene/COL_LM_MADFINGER" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_LightMap ("Lightmap (RGB)", 2D) = "white" {}
	_SpecOffset ("Specular Offset from Camera", Vector) = (1, 10, 2, 0)
	_SpecRange ("Specular Range", Float) = 20
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
	_ScrollingSpeed("Scrolling speed", Vector) = (0,0,0,0)
}

SubShader {
	Tags { "RenderType"="Opaque" "LightMode"="ForwardBase"}
	LOD 100
	
	
	
	CGINCLUDE
	#include "UnityCG.cginc"
	
	uniform half4 _MainTex_ST;
	uniform sampler2D _MainTex;
	uniform half4 _LightMap_ST;
	uniform sampler2D _LightMap;

	float3 _SpecOffset;
	float _SpecRange;
	float3 _SpecColor;
	float _Shininess;
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 lmap : TEXCOORD1;
		fixed3 spec : TEXCOORD2;
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		
		o.uv = TRANSFORM_TEX(v.texcoord,_MainTex);
		
		float3 viewNormal = mul((float3x3)UNITY_MATRIX_MV, v.normal);
		float4 viewPos = mul(UNITY_MATRIX_MV, v.vertex);
		float3 viewDir = float3(0,0,1);
		float3 viewLightPos = _SpecOffset * float3(1,1,-1);
		
		float3 dirToLight = viewPos.xyz - viewLightPos;
		
		float3 h = (viewDir + normalize(-dirToLight)) * 0.5;
		float atten = 1.0 - saturate(length(dirToLight) / _SpecRange);

		o.spec = _SpecColor * pow(saturate(dot(viewNormal, normalize(h))), _Shininess * 128) * 2 * atten;
		
		o.lmap = TRANSFORM_TEX(v.texcoord1,_LightMap);

		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest		
		fixed4 frag (v2f i) : COLOR
		{
			fixed4 c = tex2D (_MainTex, i.uv);
			
			fixed3 spec = i.spec.rgb * c.a;
			
			#if 1
			c.rgb += spec;
			#else			
			c.rgb = c.rgb + spec - c.rgb * spec;
			#endif
			
			fixed3 lm = DecodeLightmap (tex2D(_LightMap, i.lmap));
			c.rgb *= lm;
			
			return c;
		}
		ENDCG
	}	
}
}

