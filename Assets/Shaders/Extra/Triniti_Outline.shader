Shader "Triniti/Model/Triniti_Outline" {
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
	};
	
	uniform float _Outline;
	uniform float4 _OutlineColor;
	uniform sampler2D _MainTex;
	
	v2f vert(appdata v) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		//float2 offset = TransformViewToProjection(norm.xy);

		//o.pos.xy += offset * o.pos.z * _Outline;
		o.pos.xy +=  norm.xy * _Outline;
		o.color = _OutlineColor;
		o.uv = v.texcoord;
		return o;
	}

	half4 frag(v2f i) :COLOR 
	{
		half4 tex = tex2D(_MainTex,i.uv) * 0.5;
		tex.a = 1;
		return i.color*tex; 
	}

	ENDCG

	SubShader {
		Tags { "RenderType"="Opaque" }
		UsePass "Triniti/Model/ModelOneEdge/BASE"
		Pass {
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Front
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
		UsePass "Triniti/Model/ModelEdge/BASE"
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
