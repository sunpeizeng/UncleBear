
Shader "Triniti/Character/COL_QUAD"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
	}

	SubShader
	{
		Tags { "Queue" = "Geometry" }
	
		Pass
		{
			Lighting Off
			Color [_Color]
			SetTexture [_MainTex]{ combine texture * primary quad }
		}
	}
}

