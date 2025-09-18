using UnityEngine;

public class DpadAlwaysOn : MonoBehaviour
{
    [SerializeField] private CanvasGroup dpadCanvas;
    [SerializeField, Range(0f,1f)] private float alphaWithGamepad = 1f;

    private void Awake()
    {
        if (!dpadCanvas) dpadCanvas = GetComponent<CanvasGroup>();
        Apply(true);
    }

    private void Apply(bool show)
    {
        if (!dpadCanvas) return;
        dpadCanvas.alpha = show ? alphaWithGamepad : 1f;
        dpadCanvas.interactable = true;
        dpadCanvas.blocksRaycasts = true;
    }
}