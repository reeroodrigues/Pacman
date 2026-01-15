using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonVFXHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float animationSpeed = 10f;
    
    [Header("Glow Effect")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private Color glowColor = new Color(1f, 0.9f, 0.3f, 0.5f);
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private CanvasGroup canvasGroup;
    private Image glowImage;
    private bool isHovering;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        if (enableGlow)
        {
            SetupGlowEffect();
        }
    }

    private void SetupGlowEffect()
    {
        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(transform, false);
        glowObj.transform.SetAsFirstSibling();
        
        RectTransform glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.sizeDelta = new Vector2(20f, 20f);
        glowRect.anchoredPosition = Vector2.zero;
        
        glowImage = glowObj.AddComponent<Image>();
        glowImage.color = glowColor;
        glowImage.raycastTarget = false;
        
        canvasGroup = glowObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale, 
            targetScale, 
            Time.deltaTime * animationSpeed
        );
        
        if (enableGlow && canvasGroup != null)
        {
            float targetAlpha = isHovering ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(
                canvasGroup.alpha, 
                targetAlpha, 
                Time.deltaTime * animationSpeed
            );
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = originalScale;
    }
}
