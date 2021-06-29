Shader "Grid/Sh_Border"
{
	Properties
	{
		_MainTex( "Texture", 2D ) = "white" {}
		_Albedo( "Albedo", Color ) = ( 1,1,1,1 )

	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#pragma target 4.0

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
					float4 debugCol : TEXCOORD1;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _Albedo;

				struct Vertex
				{
					float4 pos; // xyz pos, w = terrain type
				};

				uniform StructuredBuffer<Vertex> BVertices;

				v2f vert( uint id : SV_VertexID, appdata v )
				{
					v2f o;
					float type = BVertices[id].pos.w;

					o.vertex = UnityObjectToClipPos( float4( BVertices[id].pos.xyz, 1.0f ) );
					o.uv = TRANSFORM_TEX( v.uv, _MainTex );


					if (type == 0) o.debugCol = float4( 0, 1, 0, 1 );
					else if (type == 1) o.debugCol = float4( 1, 0, 0, 1 );
					else o.debugCol = float4( 0, 0, 0, 1 );


					return o;
				}

				fixed4 frag( v2f i ) : SV_Target
				{
					// sample the texture
					fixed4 col = tex2D( _MainTex, i.uv );
					return i.debugCol;
				}
				ENDCG
			}
		}
}
