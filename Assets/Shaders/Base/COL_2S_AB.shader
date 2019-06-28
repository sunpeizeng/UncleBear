
Shader "Triniti/Character/COL_2S_AB"
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
			Cull Off
			Lighting Off
			AlphaTest Greater 0.1
			SetTexture [_MainTex]
			{
				ConstantColor [_Color]
				combine texture * constant
			}
		}
	}
}

