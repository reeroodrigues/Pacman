using UnityEngine;
using UnityEngine.EventSystems;

public class DPadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public enum Dir {Up, Down, Left, Right}
    public Dir direction;
    public VirtualDpad pad;

    private void Awake()
    {
        pad = GetComponentInParent<VirtualDpad>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pad.Press(direction);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pad.Release(direction);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pad.Release(direction);
    } 
}