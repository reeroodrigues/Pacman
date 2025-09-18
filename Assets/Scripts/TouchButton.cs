using UnityEngine;
using UnityEngine.EventSystems;

public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private InputNumberController controller;
    [SerializeField] private bool isUpButton;
    [SerializeField] private int inputIndex;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isUpButton)
            controller.OnUpButtonPressed(inputIndex);
        else
            controller.OnDownButtonPressed(inputIndex);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isUpButton)
            controller.OnUpButtonReleased(inputIndex);
        else
            controller.OnDownButtonReleased(inputIndex);
    }
}