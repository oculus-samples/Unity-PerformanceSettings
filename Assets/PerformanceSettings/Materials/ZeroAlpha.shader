Shader "Unlit/ZeroAlpha"
{
    Properties
    {
        [ToggleUI] _ZWrite("Write Depth", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Blend One Zero, One Zero
        ZWrite [_ZWrite]
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 frag = fixed4(0,0,0,0);
                return frag;
            }
            ENDCG
        }
    }
}
