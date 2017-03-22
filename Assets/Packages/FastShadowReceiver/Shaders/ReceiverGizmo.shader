Shader "FastShadowReceiver/Receiver/Gizmo" {
    Properties { _Color ("Main Color", Color) = (0,1,0,0.5) }
    SubShader {
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off Fog { Mode Off }
        Offset -1, -1
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            float4 vert (float4 vertex : POSITION) : SV_POSITION { return mul(UNITY_MATRIX_MVP, vertex); }
            fixed4 frag () : COLOR { return _Color; }
            ENDCG
        }
    }
}
