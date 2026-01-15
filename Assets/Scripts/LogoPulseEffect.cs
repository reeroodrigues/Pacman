using UnityEngine;
using UnityEngine.UI;

public class LogoPulseEffect : MonoBehaviour
{
    [Header("Scale Pulse Settings")]
    [SerializeField] private bool enableScalePulse = true;
    [SerializeField] private float pulseScale = 1.05f;
    [SerializeField] private float pulseSpeed = 1.5f;
    
    [Header("Glow Pulse Settings")]
    [SerializeField] private bool enableGlowPulse = true;
    [SerializeField] private Image logoImage;
    [SerializeField] private float glowIntensity = 0.2f;
    
    private Vector3 originalScale;
    private Color originalColor;
    private float pulseTimer;

    private void Start()
    {
        originalScale = transform.localScale;
        
        if (logoImage != null)
        {
            originalColor = logoImage.color;
        }
        else
        {
            logoImage = GetComponent<Image>();
            if (logoImage != null)
            {
                originalColor = logoImage.color;
            }
        }
    }

    private void Update()
    {
        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulseValue = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;
        
        if (enableScalePulse)
        {
            float scaleMultiplier = Mathf.Lerp(1f, pulseScale, pulseValue);
            transform.localScale = originalScale * scaleMultiplier;
        }
        
        if (enableGlowPulse && logoImage != null)
        {
            float brightness = 1f + (pulseValue * glowIntensity);
            logoImage.color = originalColor * brightness;
        }
    }
}
