using UnityEngine;
using UI = UnityEngine.UI;

namespace TinyYoloV2 {

sealed class VisualizerCpu : MonoBehaviour
{
    #region Editable attributes

    [SerializeField, Range(0, 1)] float _scoreThreshold = 0.1f;
    [SerializeField, Range(0, 1)] float _overlapThreshold = 0.5f;
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] UI.RawImage _previewUI = null;
    [SerializeField] Marker _markerPrefab = null;

    // Thresholds are exposed to the runtime UI.
    public float scoreThreshold { set => _scoreThreshold = value; }
    public float overlapThreshold { set => _overlapThreshold = value; }

    #endregion

    #region Internal objects

    WebCamTexture _webcamRaw;
    RenderTexture _webcamBuffer;
    ObjectDetector _detector;
    Marker[] _markers = new Marker[50];

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Texture allocation
        _webcamRaw = new WebCamTexture();
        _webcamBuffer = new RenderTexture(1080, 1080, 0);

        _webcamRaw.Play();
        _previewUI.texture = _webcamBuffer;

        // Object detector initialization
        _detector = new ObjectDetector(_resources);

        // Marker populating
        for (var i = 0; i < _markers.Length; i++)
            _markers[i] = Instantiate(_markerPrefab, _previewUI.transform);
    }

    void OnDisable()
    {
        _detector?.Dispose();
        _detector = null;
    }

    void OnDestroy()
    {
        if (_webcamRaw != null) Destroy(_webcamRaw);
        if (_webcamBuffer != null) Destroy(_webcamBuffer);
        for (var i = 0; i < _markers.Length; i++) Destroy(_markers[i]);
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

        // Run the object detector with the webcam input.
        _detector.ProcessImage
          (_webcamBuffer, _scoreThreshold, _overlapThreshold);

        // Marker update
        var i = 0;

        foreach (var box in _detector.DetectedObjects)
        {
            if (i == _markers.Length) break;
            _markers[i++].SetAttributes(box);
        }

        for (; i < _markers.Length; i++) _markers[i].Hide();
    }

    #endregion
}

} // namespace TinyYoloV2
