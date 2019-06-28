Shader "Triniti/Model/Triniti_Outline_Edge" {
	Properties {
		_Color ("Main Color", Color) = (.5,.5,.5,1)
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (0, 5)) = 2
		_MainTex ("Base (RGB)", 2D) = "white" { }
		_AtmoColor("Atmosphere Color", Color) = (0.5, 0.5, 1.0, 1)
		_TH ("Range TH",Range (0, 1))  = 0
		_ZP ("Range Tp",Range (0, 2))  = 1
		
		_LightDirect ("Light Direct",Vector) = (0,1,0)
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	
	struct appdata {
		half4 vertex : POSITION;
		half3 normal : NORMAL;
		half2 texcoord : TEXCOORD0;
	};

	struct v2f {
		half4 pos : POSITION;
		half4 color : COLOR;
		half2 uv : TEXCOORD0;
		half3 pos1 : TEXCOORD2;
		half3 pos2 : TEXCOORD3;
		half  weight : TEXCOORD1;
	};
	
	uniform float _Outline;
	uniform float4 _OutlineColor;
	uniform sampler2D _MainTex;
	
	v2f vert(appdata v) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		//float2 offset = TransformViewToProjection(norm.xy);

        o.pos1 = o.pos;

		//o.pos.xy += offset * o.pos.z * _Outline;
		o.pos.xy +=  norm.xy * _Outline;
		
		o.pos2 = o.pos;
		
		o.color = _OutlineColor;
		o.uv = v.texcoord;
		o.weight = length(norm.xy);
		return o;
	}

	half4 frag(v2f i) :COLOR 
	{
	    half2 offset = i.pos2.xy - i.pos1.xy;
	    offset *= offset;
	    offset *= offset;
	    offset *= offset;
		half4 tex = tex2D(_MainTex,i.uv) * 0.5;
		tex.a = 1;
		return 1 - i.color * length(offset.xy) * 0.00001f; 
	}

	ENDCG

	SubShader {
		Tags { "RenderType"="Opaque" }
		//UsePass "Triniti/Model/ModelEdge_Additive/BASE"
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			//Cull Front
			ZWrite On
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			ENDCG
		}
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" }
		UsePass "Triniti/Model/ModelEdge_Additive/BASE"
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Front
			ZWrite On
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma exclude_renderers shaderonly
			ENDCG
			SetTexture [_MainTex] { combine primary }
		}
	}
	
	Fallback "Toon/Basic"
}
