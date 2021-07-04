Shader "Uber/UberSh_Terrain"
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
				#include "TMountains.cginc"
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
					float3 normal : NORMAL;
					float4 debugCol : TEXCOORD1;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				fixed4 _Albedo;

				struct Vertex
				{
					float4 pos; // xyz pos, w = terrain type
				};

				uniform StructuredBuffer<Vertex> Vertices;

				v2f vert( uint id : SV_VertexID, appdata v )
				{
					v2f o;
					float3 pos = Vertices[id].pos.xyz;
					float type = Vertices[id].pos.w;

					if (type == 0) o.debugCol = float4( 0, 1, 0, 1 );
					else if (type == 1) o.debugCol = float4( 1, 0, 0, 1 );
					else if (type == 3) o.debugCol = float4( 0, 0, 1, 1 );
					else o.debugCol = float4( 0, 0, 0, 1 );

					o.vertex = UnityObjectToClipPos( float4( pos, 1.0f ) );
					o.uv = TRANSFORM_TEX( v.uv, _MainTex );
					o.normal = float3( 0, 1, 0 );



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
