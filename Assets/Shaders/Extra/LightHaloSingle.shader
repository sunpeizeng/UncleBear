Shader "Triniti/Extra/LightHaloSingle"
{
	Properties
	{
		_Color ("Main Color", Color) = (0, 0, 0, 0)
		_Halo ("Halo (RGBA)", 2D) = "black" {}
	}

	SubShader
	{
		Tags { "Queue"="Transparent+1" }

		

		Pass
		{
			Blend DstColor One
			
			Fog{ Mode Off }

			Color [_Color]
			SetTexture [_Halo]{ combine texture}
			SetTexture [_Halo]{ combine texture * previous double ,texture}
		}
	}

}

