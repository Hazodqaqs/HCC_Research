// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BackGround" {
	Properties{
		_MainTex("Base (RGBA)", 2D) = "white" {}
	}
		SubShader{
		Tags{ "Queue" = "geometry-11" "RenderType" = "opaque" }

		Pass{
		CGPROGRAM
#pragma exclude_renderers gles flash
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct fout {
		half4 color : COLOR;
		float depth : DEPTH;
	};

	uniform sampler2D _MainTex;

	v2f vert(appdata_base v) {
		v2f vo;
		vo.pos = UnityObjectToClipPos(v.vertex);
		vo.uv = float2(-v.texcoord.x,v.texcoord.y);
		return vo;
	}

	fout frag(v2f i) {
		fout fo;
		fo.color = float4(tex2D(_MainTex, i.uv).rgb, 1);
		//2000 = far plane; 0.05 = near plane
		fo.depth = 0.999999;
		return fo;
	}
	ENDCG
	}

	}
		FallBack "Diffuse"
}
