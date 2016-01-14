Shader "Custom/DisplacedUV" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_UVGap ("UV Gap", Range(0.0, 1.0)) = 0.5
		_BrightnessHighThreshold ("Brightness Threshold (High)", Range(0.0, 1.0)) = 0.5
		_BrightnessLowThreshold ("Brightness Threshold (Low)", Range(0.0, 1.0)) = 0.5
		_DisplaceL1 ("DisplaceL1", Range(0.0, 1.0)) = 0.5
		_DisplaceR1 ("DisplaceR1", Range(0.0, 1.0)) = 0.5
		_DisplaceL2 ("DisplaceL2", Range(0.0, 1.0)) = 0.5
		_DisplaceR2 ("DisplaceR2", Range(0.0, 1.0)) = 0.5
		_DisplaceL3 ("DisplaceL3", Range(0.0, 1.0)) = 0.5
		_DisplaceR3 ("DisplaceR3", Range(0.0, 1.0)) = 0.5
		_DisplaceL4 ("DisplaceL4", Range(0.0, 1.0)) = 0.5
		_DisplaceR4 ("DisplaceR4", Range(0.0, 1.0)) = 0.5
		_DisplaceL5 ("DisplaceL5", Range(0.0, 1.0)) = 0.5
		_DisplaceR5 ("DisplaceR5", Range(0.0, 1.0)) = 0.5
	}
	SubShader {
		Pass {
//			Blend SrcAlpha One
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			uniform fixed4 _Color;
			uniform sampler2D _MainTex;
			uniform float _UVGap;
			uniform float _BrightnessHighThreshold;
			uniform float _BrightnessLowThreshold;
			uniform float _DisplaceL1;
			uniform float _DisplaceR1;
			uniform float _DisplaceL2;
			uniform float _DisplaceR2;
			uniform float _DisplaceL3;
			uniform float _DisplaceR3;
			uniform float _DisplaceL4;
			uniform float _DisplaceR4;
			uniform float _DisplaceL5;
			uniform float _DisplaceR5;

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
				fixed4 tex;

				if ((i.uv.x > _DisplaceL1 && i.uv.x < _DisplaceR1) ||
					(i.uv.x > _DisplaceL2 && i.uv.x < _DisplaceR2) ||
					(i.uv.x > _DisplaceL3 && i.uv.x < _DisplaceR3) ||
					(i.uv.x > _DisplaceL4 && i.uv.x < _DisplaceR4) ||
					(i.uv.x > _DisplaceL5 && i.uv.x < _DisplaceR5)) {

					float4 uv = i.uv;
					uv.y += _UVGap;
					tex = tex2D(_MainTex, uv);

					if(uv.y > _BrightnessHighThreshold)
						uv.y = _BrightnessHighThreshold;

					if(uv.y < _BrightnessLowThreshold)
						uv.y = _BrightnessLowThreshold;

					tex.rgb /= _Color.rgb * (1.0f - uv.y);
					tex.a /= _Color.a;
				} else {
					tex = tex2D(_MainTex, i.uv);
				}

				return tex;
			}

			ENDCG
		}
	}
}
