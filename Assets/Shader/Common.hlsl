#ifndef _TINYYOLOV2BARRACUDA_COMMON_H_
#define _TINYYOLOV2BARRACUDA_COMMON_H_

// Pre-defined constants from our Tiny YOLOv2 model
#define CELLS_IN_ROW 13
#define ANCHOR_COUNT 5
#define CLASS_COUNT 20

#define MAX_DETECTION (CELLS_IN_ROW * CELLS_IN_ROW * ANCHOR_COUNT)

// Anchor box definitions from our Tiny YOLOv2 model
static float2 anchors[] = { float2(1.08, 1.19),
                            float2(3.42, 4.41),
                            float2(6.63, 11.38),
                            float2(9.42, 5.11),
                            float2(16.62, 10.52) };

// Bounding box structure used for storing object detection results
struct BoundingBox
{
    float x, y, w, h;
    uint classIndex;
    float score;
};

// Common math functions

float CalculateIOU(BoundingBox box1, BoundingBox box2)
{
    float x0 = max(box1.x - box1.w / 2, box2.x - box2.w / 2);
    float x1 = min(box1.x + box1.w / 2, box2.x + box2.w / 2);
    float y0 = max(box1.y - box1.h / 2, box2.y - box2.h / 2);
    float y1 = min(box1.y + box1.h / 2, box2.y + box2.h / 2);

    float area0 = box1.w * box1.h;
    float area1 = box2.w * box2.h;
    float areaInner = max(0, x1 - x0) * max(0, y1 - y0);

    return areaInner / (area0 + area1 - areaInner);
}

float Sigmoid(float x)
{
    return 1 / (1 + exp(-x));
}

#endif
