Shader "Triniti/TUI/TUIGrayStyle" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
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
			fixed4 _Color;
			
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
			   half4 c = tex2D (_MainTex, input.uv) * _Color;
			   half Gray = c.r*0.1f + c.g*0.6f + c.b*0.3f;
			   c.rgb = Gray;
			   return c;
			 
			}
			
		ENDCG
		}
	} 
}
