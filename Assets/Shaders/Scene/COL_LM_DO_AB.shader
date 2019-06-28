
Shader "Triniti/Scene/COL_LM_DO_AB"
{
	Properties
	{
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Lighting Off
			Fog{ Mode Off }
			
			BindChannels
			{
				Bind "vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
			}
			
			SetTexture [_MainTex]
			{
				ConstantColor [_Color]
				combine texture * constant double
			}
			SetTexture [_LightMap] { combine texture * previous }
		}
	}
}

