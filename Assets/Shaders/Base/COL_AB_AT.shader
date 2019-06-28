
Shader "Triniti/Character/COL_AB_AT"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
	}

	SubShader
	{
		Tags { "Queue" = "Transparent+1" }
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0.1
		Offset -10,-1
		Pass
		{
			Lighting Off
			SetTexture [_MainTex]
			{
				ConstantColor [_Color]
				combine texture * constant
			}
		}
	}
}

