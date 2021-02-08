using UnityEngine;
using Unity.Barracuda;
using UI = UnityEngine.UI;

namespace TinyYoloV2 {

sealed class ObjectDetector : MonoBehaviour
{
    #region Editable attributes

    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region External asset references

    [SerializeField, HideInInspector] NNModel _model = null;
    [SerializeField, HideInInspector] ComputeShader _preCompute = null;
    [SerializeField, HideInInspector] ComputeShader _post1Compute = null;
    [SerializeField, HideInInspector] ComputeShader _post2Compute = null;
    [SerializeField, HideInInspector] Shader _visualizerShader = null;

    #endregion

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

    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;

    ComputeBuffer _preBuffer;
    ComputeBuffer _post1Buffer;
    ComputeBuffer _post2Buffer;
    ComputeBuffer _drawArgs;

    Material _visualizer;
    IWorker _worker;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Texture allocation
        _webcamRaw = new WebCamTexture();
        _webcamBuffer = new RenderTexture(1080, 1080, 0);

        _webcamRaw.Play();
        _previewUI.texture = _webcamBuffer;

        // Compute buffer allocation
        _preBuffer = new ComputeBuffer(InputTensorSize, sizeof(float));
        _post1Buffer = new ComputeBuffer(OutputDataCount, BoundingBox.Size,
                                         ComputeBufferType.Append);
        _post2Buffer = new ComputeBuffer(OutputDataCount, BoundingBox.Size,
                                         ComputeBufferType.Append);
        _drawArgs = new ComputeBuffer(4, sizeof(uint),
                                      ComputeBufferType.IndirectArguments);
        _drawArgs.SetData(new [] {6, 0, 0, 0});

        // Visualizer initialization
        _visualizer = new Material(_visualizerShader);

        // NN model initialization
        _worker = ModelLoader.Load(_model).CreateWorker();
    }

    void OnDisable()
    {
        _preBuffer?.Dispose();
        _preBuffer = null;

        _post1Buffer?.Dispose();
        _post1Buffer = null;

        _post2Buffer?.Dispose();
        _post2Buffer = null;

        _drawArgs?.Dispose();
        _drawArgs = null;

        _worker?.Dispose();
        _worker = null;
    }

    void OnDestroy()
    {
        if (_webcamRaw != null) Destroy(_webcamRaw);
        if (_webcamBuffer != null) Destroy(_webcamBuffer);
        if (_visualizer != null) Destroy(_visualizer);
    }

    void Update()
    {
        // Check if the webcam is ready (needed for macOS support)
        if (_webcamRaw.width <= 16) return;

        // Input buffer update with aspect ratio correction
        var vflip = _webcamRaw.videoVerticallyMirrored;
        var aspect = (float)_webcamRaw.height / _webcamRaw.width;
        var scale = new Vector2(aspect, vflip ? -1 : 1);
        var offset = new Vector2((1 - aspect) / 2, vflip ? 1 : 0);
        Graphics.Blit(_webcamRaw, _webcamBuffer, scale, offset);

        // Preprocessing
        _preCompute.SetTexture(0, "_Texture", _webcamBuffer);
        _preCompute.SetBuffer(0, "_Tensor", _preBuffer);
        _preCompute.SetInt("_ImageSize", ImageSize);
        _preCompute.Dispatch(0, ImageSize / 8, ImageSize / 8, 1);

        // YOLO execution
        using (var tensor = new Tensor(1, ImageSize, ImageSize, 3, _preBuffer))
            _worker.Execute(tensor);

        // Output tensor (13x13x125) -> Temporary render texture (125x169)
        var reshape = new TensorShape
          (1, CellsInRow * CellsInRow, AnchorCount * (5 + ClassCount), 1);
        using var reshaped = _worker.PeekOutput().Reshape(reshape);
        var reshapedRT = RenderTexture.GetTemporary
          (reshape.width, reshape.height, 0, RenderTextureFormat.RFloat);
        reshaped.ToRenderTexture(reshapedRT);

        // 1st postprocessing (bounding box aggregation)
        _post1Buffer.SetCounterValue(0);
        _post1Compute.SetFloat("_Threshold", _scoreThreshold);
        _post1Compute.SetTexture(0, "_Input", reshapedRT);
        _post1Compute.SetBuffer(0, "_Output", _post1Buffer);
        _post1Compute.Dispatch(0, 1, 1, 1);

        RenderTexture.ReleaseTemporary(reshapedRT);

        // 2nd postprocessing (overlap removal)
        _post2Buffer.SetCounterValue(0);
        _post2Compute.SetFloat("_Threshold", _overlapThreshold);
        _post2Compute.SetBuffer(0, "_Input", _post1Buffer);
        _post2Compute.SetBuffer(0, "_Output", _post2Buffer);
        _post2Compute.Dispatch(0, 1, 1, 1);

        // Get the count of the entries for indirect draw call.
        ComputeBuffer.CopyCount(_post2Buffer, _drawArgs, sizeof(uint));
    }

    void OnPostRender()
    {
        // Bounding box visualization
        _visualizer.SetBuffer("_Boxes", _post2Buffer);
        _visualizer.SetPass(0);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, _drawArgs, 0);
    }

    #endregion
}

} // namespace TinyYoloV2
