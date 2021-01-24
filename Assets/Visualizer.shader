Shader "Hidden/TinyYOLOv2/Visualizer"
{
    Properties
    {
        _CameraFeed("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    // Camera feed blit

    sampler2D _CameraFeed;

    void VertexBlit(uint vid : SV_VertexID,
                    out float4 position : SV_Position,
                    out float2 uv : TEXCOORD0)
    {
        float x = vid >> 1;
        float y = (vid & 1) ^ (vid >> 1);

        position = float4(float2(x, y) * 2 - 1, 1, 1);
        uv = float2(x, y);
    }

    float4 FragmentBlit(float4 position : SV_Position,
                        float2 uv : TEXCOORD0) : SV_Target
    {
        return tex2D(_CameraFeed, uv);
    }

    // Bounding box visualizer

    #include "BoundingBox.hlsl"

    StructuredBuffer<BoundingBox> _Boxes;

    void VertexKeyPoints(uint vid : SV_VertexID,
                         uint iid : SV_InstanceID,
                         out float4 position : SV_Position,
                         out float4 color : COLOR)
    {
        BoundingBox box = _Boxes[iid];

        float x = box.x + box.w * lerp(-0.5, 0.5, vid >> 1);
        float y = box.y + box.h * lerp(-0.5, 0.5, (vid & 1) ^ (vid >> 1));

        x = -2 * x + 1;
        y =  2 * y - 1;

        position = float4(x, y, 1, 1);
        color = box.confidence > 0.3;
    }

    float4 FragmentKeyPoints(float4 position : SV_Position,
                             float4 color : COLOR) : SV_Target
    {
        return color * 0.3;
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex VertexBlit
            #pragma fragment FragmentBlit
            ENDCG
        }
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend One One
            CGPROGRAM
            #pragma vertex VertexKeyPoints
            #pragma fragment FragmentKeyPoints
            ENDCG
        }
    }
}
