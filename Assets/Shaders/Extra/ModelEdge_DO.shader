Shader "Triniti/Model/ModelEdge_DO"
{
    Properties
    {
        _MainTex("Texture (RGB)", 2D) = "black" {}
        _Color("Color", Color) = (1, 1, 1, 1)
        _AtmoColor("Atmosphere Color", Color) = (0.5, 0.5, 1.0, 1)
		_TH ("Range TH",Range (0, 1))  = 1
    }
   
 SubShader
    {
    
        Pass
        {
            Cull back
			Name "BASE"
			BindChannels 
			{
				Bind "Vertex", vertex
				Bind "texcoord", texcoord0
				Bind "normal", normal
			}
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
               
                #pragma fragmentoption ARB_fog_exp2
                #pragma fragmentoption ARB_precision_hint_fastest
               
                #include "UnityCG.cginc"
               
                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform float4 _Color;
                uniform float4 _AtmoColor;
				uniform half _TH;
               
                struct v2f
                {
                    float4 pos : SV_POSITION;
                    float3 normal : TEXCOORD0;
                    float3 worldvertpos : TEXCOORD1;
                    float2 texcoord : TEXCOORD2;
					float amount : TEXCOORD3;
                };

                v2f vert(appdata_base v)
                {
                    v2f o;
                   
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    o.normal = mul (UNITY_MATRIX_MV, float4(v.normal,0));//mul (UNITY_MATRIX_MVP,v.normal.xyzz).xyz;
					o.normal = normalize(o.normal);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.amount =  saturate((o.normal.x*o.normal.x + o.normal.y*o.normal.y - o.normal.z*o.normal.z)) * _TH;
					o.amount *= o.amount;
                    return o;
                }
              
                float4 frag(v2f i) : COLOR
                {

                    float4 color = tex2D(_MainTex, i.texcoord) * _Color;
                    color.rgba = lerp(color.rgba, _AtmoColor, i.amount);
                    color.rgb *= 2;
                    
                    return color;
                }
            ENDCG
        }
   
    }
}

