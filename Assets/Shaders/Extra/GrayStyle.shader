Shader "Triniti/Extra/GrayStyle" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_GrayFactor ("Gray Factor", Range (0, 1)) = 0
		_ColorF ("Gray Color", Range (0, 2)) = 1
	}
	

	
	SubShader {
		Tags { "Queue" = "Transparent" }
		LOD 200
				
	Pass {
	    Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#pragma vertex VertexProgram
	    #pragma fragment FragmentProgram
			
	    #pragma fragmentoption ARB_precision_hint_fastest
			
			sampler2D _MainTex;
			float _GrayFactor;
			float _ColorF;
			
			struct VertexInput
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			
			struct VertexOutput
			{
				float4 position_ : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			VertexOutput VertexProgram(VertexInput input)
			{
				VertexOutput output;
				
				output.position_ = mul(UNITY_MATRIX_MVP, input.vertex);
				output.uv = input.texcoord;
				
				return output;
			}
			
			float4 FragmentProgram(VertexOutput input) : COLOR0
			{
			   half4 c = tex2D (_MainTex, input.uv);
			   half Gray = (c.r + c.g + c.b)/3*_ColorF;
			   c.r = lerp(c.r, Gray, _GrayFactor);
			   c.g = lerp(c.g, Gray, _GrayFactor);
			   c.b = lerp(c.b, Gray, _GrayFactor);
			   return c;
			 
			}
			
		ENDCG
		}
	} 
}
