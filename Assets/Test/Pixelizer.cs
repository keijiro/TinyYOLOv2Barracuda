using UnityEngine;
using UI = UnityEngine.UI;
using ImageSource = Klak.TestTools.ImageSource;

namespace TinyYoloV2 {

sealed class Pixelizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _shader = null;

    #endregion

    #region Internal objects

    RenderTexture _buffer;

    ObjectDetector _detector;

    Material _material;
    ComputeBuffer _drawArgs;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Texture allocation
        _buffer = new RenderTexture(1920, 1080, 0);

        // Object detector initialization
        _detector = new ObjectDetector(_resources);

        // Shader initialization
        _material = new Material(_shader);
        _drawArgs = new ComputeBuffer
          (4, sizeof(uint), ComputeBufferType.IndirectArguments);
        _drawArgs.SetData(new [] {6, 0, 0, 0});
    }

    void OnDisable()
    {
        _detector?.Dispose();
        _detector = null;

        _drawArgs?.Dispose();
        _drawArgs = null;
    }

    void OnDestroy()
    {
        if (_buffer != null) Destroy(_buffer);
        if (_material != null) Destroy(_material);
    }

    void Update()
    {
        // Input buffer update
        Graphics.Blit(_source.Texture, _buffer);

        // Run the object detector with the webcam input.
        _detector.ProcessImage
          (_buffer, _scoreThreshold, _overlapThreshold);

        // Draw bouding boxes into the alpha channel of _webcamBuffer.
        RenderTexture.active = _buffer;
        _detector.SetIndirectDrawCount(_drawArgs);
        _material.SetBuffer("_Boxes", _detector.BoundingBoxBuffer);
        _material.SetPass(0);
        Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, _drawArgs, 0);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
      => Graphics.Blit(_buffer, dst, _material, 1);

    #endregion
}

} // namespace TinyYoloV2
