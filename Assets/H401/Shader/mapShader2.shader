Shader "Unlit/mapShader2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HighLightCol("HighLightCol",Color) = (1,1,1,1)
		_HightLightPos("HighLightPos",vector) = (0.0,0.0,0.0)

		_RateCnt("RateCount",Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			/*struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};*/

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _HighLightCol;
			float3 _HightLightPos;
			float _Rate;
			float _RateCnt;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				_Rate = 90.0f;

				float distRate = distance(i.vertex ,mul(UNITY_MATRIX_MVP,_HightLightPos)) % _Rate;
				distRate /= _Rate;


				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);

				distRate += _RateCnt;
				distRate > 1.0f ? distRate - 1.0f : distRate;
				
					
				col = col * (1.0f -distRate)+ _HighLightCol * distRate; 

				return col;
			}
			ENDCG
		}
	}
}
