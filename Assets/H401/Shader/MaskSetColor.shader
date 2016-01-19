Shader "Custom/Sprites/MaskSetColor"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_Threshold ("Mask Threshold (R)", Range(0, 1)) = 0.5
		_Emission ("Emission", Color) = (1,1,1,1)
		[MaterialToggle] UseEmission ("Use Emission", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			fixed4 _Color;
			float _Threshold;
			fixed4 _Emission;
			float UseEmission;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color = tex2D (_MainTex, IN.texcoord);

				if(color.r >= _Threshold) {
					if(UseEmission >= 1) {
						color.rgb *= _Color.rgb;
						color.rgb /= _Emission.rgb;
						color.a *= _Color.a;
						color.a /= _Emission.a;
					} else {
						color.rgb = _Color.rgb;
						color.a = _Color.a;
					}
				} else {
					color.rgb = 0.0f;
					color.a = 0.0f;
				}

				return color;
			}
		ENDCG
		}
	}
}
