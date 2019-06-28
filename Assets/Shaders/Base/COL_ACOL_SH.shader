// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'


Shader "Triniti/Character/COL_ACOL_SH"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "white" {}
		_AColor ("Additive Color", Color) = (0,0,0,0)
	}

	SubShader {
		Lighting On
		Tags { "RenderType"="Opaque" "Reflection" = "RenderReflectionOpaque" "Queue"="Geometry" "LightMode"="ForwardBase" }	
				
		Pass {
		
			CGPROGRAM
	
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			
			#include "UnityCG.cginc"
				
			uniform half4 _MainTex_ST;
			uniform sampler2D _MainTex;
			
			uniform half4 _Color;
			uniform half4 _AColor;
					
			struct v2f
			{
				half4 pos : POSITION;
				half2 uv : TEXCOORD0;
				half3 probColor : TEXCOORD2;
			};
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				
				half3 worldNormal = mul((half3x3)unity_ObjectToWorld, v.normal.xyz);
				
				o.uv.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
			
				o.probColor = ShadeSH9(float4(worldNormal,1));
				
				return o; 
			}
			
			fixed4 frag (v2f i) : COLOR 
			{				
				fixed4 tex = tex2D(_MainTex, i.uv.xy);
				fixed4 outColor = tex * _Color;
				outColor.rgb *= i.probColor;
				outColor.rgb += _AColor.xyz;
				return outColor;
			}
			
			ENDCG
		}
		

	}	

	SubShader
	{
		Tags { "Queue" = "Geometry" }
	
		Pass
		{
			Lighting Off
			SetTexture [_MainTex]
			{
			 ConstantColor [_Color]
			 combine texture * constant
			}
			
			SetTexture [_MainTex]
			{
			 ConstantColor [_AColor]
			 combine previous + constant,previous
			}
		}
	}
}

