﻿// Credit to Nobinator : https://github.com/Nobinator/Unity-UI-Rounded-Corners/blob/master/UiRoundedCorners/RoundedCorners.shader
Shader "Scuti/RoundedImage" {
	Properties{
		[HideInInspector] _MainTex("Texture", 2D) = "white" {}

	// --- Mask support ---
	[HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
	[HideInInspector] _Stencil("Stencil ID", Float) = 0
	[HideInInspector] _StencilOp("Stencil Operation", Float) = 0
	[HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
	[HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
	[HideInInspector] _ColorMask("Color Mask", Float) = 15
	[HideInInspector] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

		// Definition in Properties section is required to Mask works properly
		_WidthHeightRadius("WidthHeightRadius", Vector) = (0,0,0,0)
		// ---
	}

		SubShader{
			Tags {
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
			}

		// --- Mask support ---
		Stencil {
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
		Cull Off
		Lighting Off
		ZTest[unity_GUIZTestMode]
		ColorMask[_ColorMask]
		// ---

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass {
			CGPROGRAM

			#include "UnityCG.cginc"
			#include "SDFUtils.cginc"
			#include "ShaderSetup.cginc"
			#include "UnityUI.cginc"
			#pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

			#pragma vertex vert
			#pragma fragment frag

			float4 _WidthHeightRadius;
			sampler2D _MainTex;
			float4 _ClipRect;

			fixed4 frag(v2f i) : SV_Target {
				float alpha = CalcAlpha(i.uv, _WidthHeightRadius.xy, _WidthHeightRadius.z);
				half4 color = mixAlpha(tex2D(_MainTex, i.uv), i.color, alpha);
 				
				#ifdef UNITY_UI_CLIP_RECT
                	color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                	clip (color.a - 0.001);
                #endif

				return color;
			}

			ENDCG
		}
	}
}