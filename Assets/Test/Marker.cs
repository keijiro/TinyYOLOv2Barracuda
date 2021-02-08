using UnityEngine;
using UI = UnityEngine.UI;

namespace TinyYoloV2 {

sealed class Marker : MonoBehaviour
{
    RectTransform _parent;
    RectTransform _xform;
    UI.Image _panel;
    UI.Text _label;

    void Start()
    {
        _xform = GetComponent<RectTransform>();
        _parent = (RectTransform)_xform.parent;
        _panel = GetComponent<UI.Image>();
        _label = GetComponentInChildren<UI.Text>();
    }

    public void SetAttributes(in BoundingBox box)
    {
        var rect = _parent.rect;

        // Bounding box position
        var x = box.x * rect.width;
        var y = (1 - box.y) * rect.height;
        var w = box.w * rect.width;
        var h = box.h * rect.height;

        _xform.anchoredPosition = new Vector2(x, y);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);

        // Label (class name + score)
        var name = Config.GetLabel((int)box.classIndex);
        _label.text = $"{name} {(int)(box.score * 100)}%";

        // Panel color
        var hue = box.classIndex * 0.073f % 1.0f;
        var color = Color.HSVToRGB(hue, 1, 1);
        color.a = 0.4f;
        _panel.color = color;

        // Enable
        gameObject.SetActive(true);
    }

    public void Hide()
      => gameObject.SetActive(false);
}

} // namespace TinyYoloV2
