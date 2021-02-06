Shader "Hidden/TinyYOLOv2/Visualizer"
{
    Properties
    {
        _CameraFeed("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "BoundingBox.hlsl"

    StructuredBuffer<BoundingBox> _Boxes;

    void VertexKeyPoints(uint vid : SV_VertexID,
                         uint iid : SV_InstanceID,
                         out float4 position : SV_Position,
                         out float4 color : COLOR)
    {
        BoundingBox box = _Boxes[iid];

        // Bounding box vertex
        float x = box.x + box.w * lerp(-0.5, 0.5, vid >> 1);
        float y = box.y + box.h * lerp(-0.5, 0.5, (vid & 1) ^ (vid >> 1));

        // Clip space to screen space
        float2 p = float2(x, y) * 2 - 1;

        // Aspect ratio compensation
        p.x = p.x * _ScreenParams.y / _ScreenParams.x;

        // Vertex attributes
        position = float4(p, 1, 1);
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
            ZTest Always ZWrite Off Cull Off Blend One One
            CGPROGRAM
            #pragma vertex VertexKeyPoints
            #pragma fragment FragmentKeyPoints
            ENDCG
        }
    }
}
