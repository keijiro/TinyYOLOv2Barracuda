using UnityEngine;
using UI = UnityEngine.UI;
using ImageSource = Klak.TestTools.ImageSource;

namespace TinyYoloV2 {

sealed class VisualizerGpu : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Shader _visualizer = null;
    [SerializeField] UI.RawImage _previewUI = null;

    #endregion

    #region Internal objects

    ObjectDetector _detector;

    Material _material;
    ComputeBuffer _drawArgs;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Object detector initialization
        _detector = new ObjectDetector(_resources);

        // Visualizer initialization
        _material = new Material(_visualizer);
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
        if (_material != null) Destroy(_material);
    }

    void Update()
    {
        // Run the object detector with the webcam input.
        _detector.ProcessImage
          (_source.Texture, _scoreThreshold, _overlapThreshold);

        _previewUI.texture = _source.Texture;
    }

    void OnPostRender()
    {
        // Bounding box visualization
        _detector.SetIndirectDrawCount(_drawArgs);
        _material.SetBuffer("_Boxes", _detector.BoundingBoxBuffer);
        _material.SetPass(0);
        Graphics.DrawProceduralIndirectNow
          (MeshTopology.Triangles, _drawArgs, 0);
    }

    #endregion
}

} // namespace TinyYoloV2
