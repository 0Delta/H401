Shader "Custom/Mobile/MaskChangeColor" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MaskTex ("Mask", 2D) = "white" {}
	}
	SubShader {
		Pass {
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			uniform fixed4 _Color;
			uniform sampler2D _MainTex;
			uniform sampler2D _MaskTex;

			struct v2f {
				float4 position : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.position = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;

				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				fixed4 mainTex;
				fixed4 maskTex;

				mainTex = tex2D(_MainTex, i.uv);
				maskTex = tex2D(_MaskTex, i.uv);

				if(maskTex.r >= 1.0f) {
					mainTex.rgb /= _Color.rgb;
					mainTex.a /= _Color.a;
				}

				return mainTex;
			}

			ENDCG
		}
	}
}
