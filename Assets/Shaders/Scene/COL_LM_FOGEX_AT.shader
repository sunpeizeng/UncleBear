
Shader "Triniti/Scene/COL_LM_FOGEX_AT"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("MainTex(RGB)", 2D) = "" {}
		_LightMap ("Lightmap (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }

		Pass
		{
			Lighting Off
			
			AlphaTest Greater 0.1
			
			BindChannels
			{
				Bind "vertex", vertex
				Bind "texcoord", texcoord0
				Bind "texcoord1", texcoord1
			}
			
			Color [_Color]
			SetTexture [_MainTex]{ combine texture * primary }
			SetTexture [_LightMap] { combine texture * previous }
		}
	}
}

