TinyYOLOv2Barracuda
===================

![gif](https://i.imgur.com/fmYy8os.gif)
![gif](https://i.imgur.com/82Tekyj.gif)

**TinyYOLOv2Barracuda** is a Unity sample project that shows how to run the
[YOLO] object detection system on the Unity [Barracuda] neural network inference
library.

[YOLO]: https://pjreddie.com/darknet/yolo/
[Barracuda]: https://docs.unity3d.com/Packages/com.unity.barracuda@latest

This project uses a [Tiny YOLOv2 model] from [ONNX Model Zoo]. See the model
description page for details.

[Tiny YOLOv2 model]:
  https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/tiny-yolov2
[ONNX Model Zoo]: https://github.com/onnx/models

System requirements
-------------------

- Unity 2021.3
- Barracuda 3.0.0

How to run
----------

This repository doesn't contain the ONNX model file to avoid hitting the storage
quota. [Download the model file] from the ONNX Model Zoo page and put it in the
`Assets/ONNX` directory.

[Download the model file]:
  https://github.com/onnx/models/blob/master/vision/object_detection_segmentation/tiny-yolov2/model/tinyyolov2-7.onnx

Sample scenes
-------------

All these samples use the [ImageSource] class to feed images to the model. You
can change the image source type from the drop down list.

[ImageSource]: https://github.com/keijiro/TestTools

### VisualizerGpu

**VisualizerGpu** runs all the object detection & visualization processes
(preprocess, inference, post-process, overlap removal, and visualization) solely
on GPU. It minimizes the CPU load and visualization latency, but you can't do
anything more complicated than simple visualization like drawing rectangles or
something on detected objects.

### VisualizerCpu

**VisualizerCpu** runs the object detection on GPU and then reads the detection
results back to the CPU side. After that, it visualizes them using the Unity UI
system. Even though this method runs slower than the GPU-only method, you can do
complex processes using C# scripting.

### Pixelizer

**Pixelizer** detects people from the input video stream and applies a
pixelation effect to the person regions. It shows how to implement an image
effect with the YOLO detector.

![gif](https://i.imgur.com/ps7ppfp.gif)
