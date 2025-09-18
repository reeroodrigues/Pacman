using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform background;   // círculo/base do joystick
    [SerializeField] private RectTransform handle;       // alavanca

    [Header("Config")]
    [SerializeField] private float maxRadius = 80f;      // raio em px
    [SerializeField] private float deadZone = 0.2f;      // 0..1 (20% do raio)
    [SerializeField] private bool snapTo4Directions = true;

    public Vector2 Direction { get; private set; }       // -1..1

    private Canvas _canvas;
    private Camera _uiCamera;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            _uiCamera = _canvas.worldCamera;
        ResetHandle();
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, _uiCamera, out localPoint))
            return;

        // limita posição do handle ao círculo
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, maxRadius);
        handle.anchoredPosition = clamped;

        // direção normalizada -1..1
        Vector2 raw = clamped / maxRadius;

        // dead-zone
        if (raw.magnitude < deadZone)
        {
            Direction = Vector2.zero;
        }
        else
        {
            if (snapTo4Directions)
            {
                // escolhe o eixo dominante e “trava” em 4 direções
                if (Mathf.Abs(raw.x) > Mathf.Abs(raw.y))
                    Direction = new Vector2(Mathf.Sign(raw.x), 0f);
                else
                    Direction = new Vector2(0f, Mathf.Sign(raw.y));
            }
            else
            {
                Direction = raw.normalized;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Direction = Vector2.zero;
        ResetHandle();
    }

    private void ResetHandle()
    {
        if (handle != null) handle.anchoredPosition = Vector2.zero;
        Direction = Vector2.zero;
    }
}
