Shader "Unlit/MapGridShader"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _EmptyTileColor("Empty Tile Color", Color) = (0, 0, 0, 1)
        _FoodTileColor("Food Tile Color", Color) = (0, 1, 0, 1)
        _WallTileColor("Wall Tile Color", Color) = (0.3, 0.3, 0.3, 1)
        _HomePheromonColor("Home Pheromon Color", Color) = (0, 0, 1, 1)
        _ResourcePheromonColor("Resource Pheromon Color", Color) = (1, 0, 0, 1)
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _EmptyTileColor;
            fixed4 _FoodTileColor;
            fixed4 _WallTileColor;
            fixed4 _HomePheromonColor;
            fixed4 _ResourcePheromonColor;

            StructuredBuffer<int> grid;
            StructuredBuffer<float2> pheromons;
            uniform int width;
            uniform int height;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                int x = int(floor(i.uv.x * width));
                int y = int(floor(i.uv.y * height));
                int type = grid[x * height + y];

                if (type >= 2)
                {
                    return _WallTileColor;
                }
                else if (type >= 1)
                {
                    return _FoodTileColor;
                }
                else
                {
                    return _EmptyTileColor + (_HomePheromonColor * pheromons[x * height + y].x) + (_ResourcePheromonColor * pheromons[x * height + y].y);
                }
            }
            ENDCG
        }
    }
}
