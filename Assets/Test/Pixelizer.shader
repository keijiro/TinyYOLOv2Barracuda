Shader "Hidden/TinyYOLOv2/Pixelizer"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "../Shader/Common.hlsl"

    //
    // Pass 0 - Mask construction
    //

    StructuredBuffer<BoundingBox> _Boxes;

    void VertexMask(uint vid : SV_VertexID,
                    uint iid : SV_InstanceID,
                    out float4 position : SV_Position)
    {
        BoundingBox box = _Boxes[iid];

        // Bounding box vertex
        float x = box.x + box.w * lerp(-0.5, 0.5, vid & 1);
        float y = box.y + box.h * lerp(-0.5, 0.5, vid < 2 || vid == 5);

        // Clip space coordinates
        float2 p = float2(x, y) * 2 - 1;

        // Mask (14 == Person)
        p *= box.classIndex == 14;

        position = float4(p, 1, 1);
    }

    float4 FragmentMask(float4 position : SV_Position) : SV_Target
    {
        return 0;
    }

    //
    // Pass 1 - Pixelization image effect
    //

    sampler2D _MainTex;

    float4 FragmentPost(float4 position : SV_Position,
                        float2 uv : TEXCOORD0) : SV_Target
    {
        float2 reso = float2(_ScreenParams.x / _ScreenParams.y, 1) * 40;
        float2 uv2 = floor(uv * reso) / reso;

        float4 c1 = tex2D(_MainTex, uv);
        float4 c2 = tex2D(_MainTex, uv2);

        return lerp(c2, c1, c1.a);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off ColorMask A
            CGPROGRAM
            #pragma vertex VertexMask
            #pragma fragment FragmentMask
            ENDCG
        }
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment FragmentPost
            ENDCG
        }
    }
}
