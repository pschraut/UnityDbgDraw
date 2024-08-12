// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

// DbgDraw for Unity. Copyright (c) 2019-2024 Peter Schraut (www.console-dev.de). See LICENSE.md
// DbgDraw-Shaded implements some kind of fake lighting to mimic how the Unity Gizmos and Handles renders models.
// https://github.com/pschraut/UnityDbgDraw

Shader "Hidden/DbgDraw-Shaded" {
Properties {
	_Color("Color", Color) = (1,1,1,1)
	_SrcBlend("SrcBlend", Int) = 5.0 // SrcAlpha
	_DstBlend("DstBlend", Int) = 10.0 // OneMinusSrcAlpha
	_ZWrite("ZWrite", Int) = 1.0 // On
	_ZTest("ZTest", Int) = 4.0 // LEqual
	_Cull("Cull", Int) = 0.0 // Off
	_ZBias("ZBias", Float) = 0.0
}

SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 100

	Blend[_SrcBlend][_DstBlend]
	ZWrite[_ZWrite]
	ZTest[_ZTest]
	Cull [_Cull]
	Offset[_ZBias],[_ZBias]

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
			#pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

			float4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);

				float3 fakeLightNormal = mul((float3x3)unity_WorldToObject, normalize(float3(0, 1, 1)));
				float4 shadedColor = saturate(dot(normalize(v.normal), fakeLightNormal)) * 0.35;

				o.color.rgb = v.color.rgb - shadedColor.rgb;
				o.color.a = v.color.a;
				o.color *= _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
        ENDCG
    }
}

}
