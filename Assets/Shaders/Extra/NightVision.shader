Shader "Triniti/Extra/NightVision" 
{
	Properties 
	{
		_MainTex ("MainTex", 2D) = "" {}
	}
	 
	SubShader 
	{
		Pass 
		{
		 
			//ZTest Always 
			Cull Off
			//ZWrite Off 
			Lighting Off
			 
			Fog { Mode off }
			 
			CGPROGRAM
			 
			#pragma vertex vert_img
			 
			#pragma fragment frag
			 
			#pragma fragmentoption ARB_precision_hint_fastest 
			 
			#include "UnityCG.cginc"
			 
			uniform sampler2D _MainTex;
			 
			float4 frag (v2f_img i) : COLOR 
			{
			 
				float4 c = tex2D(_MainTex, i.uv);
				 
				c.b = c.r*2;
				 
				c.g = c.b*2;
				
				return c;
			}
			ENDCG
		}
	}
	Fallback off
}
