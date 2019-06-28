
Shader "Triniti/Character/COL_3000"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }
	
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

