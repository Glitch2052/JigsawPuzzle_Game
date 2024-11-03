Shader "Jigsaw/PuzzlePiece"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}   // The main texture
        _ShadowColor ("Shadow Color", Color) = (0,0,0,0.5)  // Shadow color
        _ShadowOffset ("Shadow Offset", Vector) = (0.1, -0.1, 0, 0)  // Shadow offset (x, y)
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        
        // First pass: Draw the shadow
        Pass
        {
            Name "ShadowPass"
            ZWrite On          // Writes to the depth buffer
            Cull Off
            Offset 0,0
            Blend SrcAlpha OneMinusSrcAlpha  // Blend the shadow based on alpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _ShadowOffset;
            float4 _ShadowColor;

            v2f vert(appdata v)
            {
                v2f o;
                // Offset the vertex for the shadow
                float4 shadowPos = v.vertex + float4(_ShadowOffset.xy, 0.0, 0.0);
                o.vertex = UnityObjectToClipPos(shadowPos);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Return the shadow color
                return _ShadowColor;
            }
            ENDCG
        }

        // Second pass: Draw the texture on top of the shadow
        Pass
        {
            Name "TexturePass"
            ZWrite Off         // Don't write to depth buffer for this pass
            Cull Off
            Offset 0,0
            Blend SrcAlpha OneMinusSrcAlpha  // Standard alpha blending for the texture

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragTex
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                v2f o;
                // Standard position (no offset for the texture pass)
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 fragTex(v2f i) : SV_Target
            {
                // Sample the texture and return its color
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
