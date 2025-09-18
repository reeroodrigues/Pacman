using UnityEngine;

[ExecuteAlways]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform _rt;
    Rect _last;

    void OnEnable() { _rt = GetComponent<RectTransform>(); Apply(); }
    void Update() { if (Screen.safeArea != _last) Apply(); }

    void Apply()
    {
        if (_rt == null) return;
        Rect area = Screen.safeArea;
        _last = area;

        Vector2 anchorMin = area.position;
        Vector2 anchorMax = area.position + area.size;
        anchorMin.x /= Screen.width;  anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;  anchorMax.y /= Screen.height;
        _rt.anchorMin = anchorMin;
        _rt.anchorMax = anchorMax;
        _rt.offsetMin = _rt.offsetMax = Vector2.zero;
    }
}