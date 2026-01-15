using System.Collections;
using UnityEngine;

public class LeaderboardPanelSlide : MonoBehaviour
{
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private RectTransform panelRectTransform;
    
    private const float OffScreenY = 3840f;
    private const float OnScreenY = 0f;
    
    private bool _isAnimating = false;

    private void Awake()
    {
        if (panelRectTransform == null)
        {
            panelRectTransform = GetComponent<RectTransform>();
        }
    }

    public void SlideDown()
    {
        if (_isAnimating || panelRectTransform == null) return;
        StartCoroutine(SmoothMove(OnScreenY));
    }

    public void SlideUp()
    {
        if (_isAnimating || panelRectTransform == null) return;
        StartCoroutine(SmoothMove(OffScreenY));
    }

    private IEnumerator SmoothMove(float targetYPosition)
    {
        _isAnimating = true;

        var elapsedTime = 0f;
        float startY = panelRectTransform.anchoredPosition.y;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / animationDuration;
            var newY = Mathf.Lerp(startY, targetYPosition, t);
            
            Vector2 currentPos = panelRectTransform.anchoredPosition;
            panelRectTransform.anchoredPosition = new Vector2(currentPos.x, newY);
            yield return null;
        }

        Vector2 finalPos = panelRectTransform.anchoredPosition;
        panelRectTransform.anchoredPosition = new Vector2(finalPos.x, targetYPosition);
        _isAnimating = false;
    }
}
