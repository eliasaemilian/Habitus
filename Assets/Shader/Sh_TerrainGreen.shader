Shader "Terrain/Sh_TerrainGreen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Albedo( "Albedo", Color ) = ( 1,1,1,1 )

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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 debugCol : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Albedo;

            struct Vertex
            {
                float3 pos;
                int type; // = TerrainType
            };

            StructuredBuffer<Vertex> Vertices;

            v2f vert ( uint id : SV_VertexID, appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos( float4( Vertices[id].pos, 1.0f ) );
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                if (Vertices[id].type == 1) o.debugCol = float4( 0, 1, 0, 1 );
                else o.debugCol = float4( 1, 0, 0, 1 );
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return i.debugCol;
            }
            ENDCG
        }
    }
}
