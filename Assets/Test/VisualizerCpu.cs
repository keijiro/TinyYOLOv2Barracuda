using UnityEngine;
using UI = UnityEngine.UI;
using ImageSource = Klak.TestTools.ImageSource;

namespace TinyYoloV2 {

sealed class VisualizerCpu : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
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

    ObjectDetector _detector;
    Marker[] _markers = new Marker[50];

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
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
        for (var i = 0; i < _markers.Length; i++) Destroy(_markers[i]);
    }

    void Update()
    {
        // Run the object detector with the image input.
        _detector.ProcessImage
          (_source.Texture, _scoreThreshold, _overlapThreshold);

        // Marker update
        var i = 0;

        foreach (var box in _detector.DetectedObjects)
        {
            if (i == _markers.Length) break;
            _markers[i++].SetAttributes(box);
        }

        for (; i < _markers.Length; i++) _markers[i].Hide();

        _previewUI.texture = _source.Texture;
    }

    #endregion
}

} // namespace TinyYoloV2
