
Shader "Triniti/Character/COL_AB_ZWO"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			Lighting Off
			ZWrite Off
			SetTexture [_MainTex]
			{
				ConstantColor [_Color]
				combine texture * constant
			}
		}
	}
}

