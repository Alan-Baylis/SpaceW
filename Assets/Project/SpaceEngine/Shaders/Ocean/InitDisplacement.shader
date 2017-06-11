Shader "SpaceEngine/Ocean/InitDisplacement" 
{
	SubShader 
	{
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma target 3.0

			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _Buffer1;
			uniform sampler2D _Buffer2;
			uniform float4 _InverseGridSizes;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			struct f2a
			{
				float4 col0 : COLOR0;
				float4 col1 : COLOR1;
			};
			
			void vert(in a2v i, out v2f o)
			{
				o.pos = mul(UNITY_MATRIX_MVP, i.vertex);
				o.uv = i.texcoord;
			}

			void frag(in v2f IN, out f2a OUT)
			{ 
				float2 uv = IN.uv.xy;
			
				float2 st = float2(0.0, 0.0);
				st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
				st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;
				
				float2 k1 = st * _InverseGridSizes.x;
				float2 k2 = st * _InverseGridSizes.y;
				float2 k3 = st * _InverseGridSizes.z;
				float2 k4 = st * _InverseGridSizes.w;
				
				float K1 = length(k1);
				float K2 = length(k2);
				float K3 = length(k3);
				float K4 = length(k4);
			
				float IK1 = K1 == 0.0 ? 0.0 : 1.0 / K1;
				float IK2 = K2 == 0.0 ? 0.0 : 1.0 / K2;
				float IK3 = K3 == 0.0 ? 0.0 : 1.0 / K3;
				float IK4 = K4 == 0.0 ? 0.0 : 1.0 / K4;
				
				OUT.col0 = tex2D(_Buffer1, IN.uv) * float4(IK1, IK1, IK2, IK2);
				OUT.col1 = tex2D(_Buffer2, IN.uv) * float4(IK3, IK3, IK4, IK4);
			}
			
			ENDCG
		}
	}
}