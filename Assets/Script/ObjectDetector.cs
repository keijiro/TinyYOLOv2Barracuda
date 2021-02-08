using UnityEngine;
using Unity.Barracuda;

namespace TinyYoloV2 {

sealed class ObjectDetector : System.IDisposable
{
    #region Compile-time constants

    // Pre-defined constants from our Tiny YOLOv2 model
    const int ImageSize = 416;
    const int CellsInRow = 13;
    const int AnchorCount = 5;
    const int ClassCount = 20;

    const int InputTensorSize = ImageSize * ImageSize * 3;
    const int OutputDataCount = CellsInRow * CellsInRow * AnchorCount;

    #endregion

    #region Internal objects

    ResourceSet _resources;
    ComputeBuffer _preBuffer;
    ComputeBuffer _post1Buffer;
    ComputeBuffer _post2Buffer;
    ComputeBuffer _countBuffer;
    IWorker _worker;

    #endregion

    #region Public constructor

    public ObjectDetector(ResourceSet resources)
    {
        _resources = resources;

        _preBuffer = new ComputeBuffer(InputTensorSize, sizeof(float));

        _post1Buffer = new ComputeBuffer
          (OutputDataCount, BoundingBox.Size, ComputeBufferType.Append);

        _post2Buffer = new ComputeBuffer
          (OutputDataCount, BoundingBox.Size, ComputeBufferType.Append);

        _countBuffer = new ComputeBuffer
          (1, sizeof(uint), ComputeBufferType.Raw);

        _worker = ModelLoader.Load(_resources.model).CreateWorker();
    }

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
        _preBuffer?.Dispose();
        _preBuffer = null;

        _post1Buffer?.Dispose();
        _post1Buffer = null;

        _post2Buffer?.Dispose();
        _post2Buffer = null;

        _countBuffer?.Dispose();
        _countBuffer = null;

        _worker?.Dispose();
        _worker = null;
    }

    #endregion

    #region Public methods

    public ComputeBuffer BoundingBoxBuffer
      => _post2Buffer;

    public void SetIndirectDrawCount(ComputeBuffer drawArgs)
      => ComputeBuffer.CopyCount(_post2Buffer, drawArgs, sizeof(uint));

    public void ProcessImage
      (Texture sourceTexture, float scoreThreshold, float overlapThreshold)
    {
        // Reset the compute buffer counters.
        _post1Buffer.SetCounterValue(0);
        _post2Buffer.SetCounterValue(0);

        // Preprocessing
        var pre = _resources.preprocess;
        pre.SetTexture(0, "_Texture", sourceTexture);
        pre.SetBuffer(0, "_Tensor", _preBuffer);
        pre.SetInt("_ImageSize", ImageSize);
        pre.Dispatch(0, ImageSize / 8, ImageSize / 8, 1);

        // Run the YOLO model.
        using (var tensor = new Tensor(1, ImageSize, ImageSize, 3, _preBuffer))
            _worker.Execute(tensor);

        // Output tensor (13x13x125) -> Temporary render texture (125x169)
        var reshape = new TensorShape
          (1, CellsInRow * CellsInRow, AnchorCount * (5 + ClassCount), 1);

        var reshapedRT = RenderTexture.GetTemporary
          (reshape.width, reshape.height, 0, RenderTextureFormat.RFloat);

        using (var tensor = _worker.PeekOutput().Reshape(reshape))
            tensor.ToRenderTexture(reshapedRT);

        // 1st postprocess (bounding box aggregation)
        var post1 = _resources.postprocess1;
        post1.SetFloat("_Threshold", scoreThreshold);
        post1.SetTexture(0, "_Input", reshapedRT);
        post1.SetBuffer(0, "_Output", _post1Buffer);
        post1.Dispatch(0, 1, 1, 1);

        RenderTexture.ReleaseTemporary(reshapedRT);

        // Bounding box count
        ComputeBuffer.CopyCount(_post1Buffer, _countBuffer, 0);

        // 2nd postprocess (overlap removal)
        var post2 = _resources.postprocess2;
        post2.SetFloat("_Threshold", overlapThreshold);
        post2.SetBuffer(0, "_Input", _post1Buffer);
        post2.SetBuffer(0, "_Count", _countBuffer);
        post2.SetBuffer(0, "_Output", _post2Buffer);
        post2.Dispatch(0, 1, 1, 1);
    }

    #endregion
}

} // namespace TinyYoloV2
