using System.Collections;
using TMPro;
using UnityEngine;

public class InfosScreen : MonoBehaviour
{
    [SerializeField] private float animationDuration = 1f; 

    [Header("Primeiro Objeto (Dono do Script)")]
    [SerializeField] private RectTransform firstObjectRectTransform;

    [Header("Segundo Objeto (Referenciado)")]
    [SerializeField] private RectTransform secondObjectRectTransform; 

    [Header("Inputs que precisam iniciar com 0")]
    [SerializeField] private TMP_InputField[] inputFields;

    private bool _isFirstAnimating = false;
    private bool _isSecondAnimating = false;

    private void Awake()
    {
        if (firstObjectRectTransform == null)
            firstObjectRectTransform = GetComponent<RectTransform>();
        if (secondObjectRectTransform == null)
        {
            enabled = false;
        }
    }
    
    public void MoveFirstToZeroY()
    {
        if (_isFirstAnimating || firstObjectRectTransform == null) return;
        StartCoroutine(SmoothMove(firstObjectRectTransform, 0f));
    }
    
    public void MoveFirstToY()
    {
        if (_isFirstAnimating || firstObjectRectTransform == null) return;
        StartCoroutine(SmoothMove(firstObjectRectTransform, 3840f));
    }
    
    public void MoveSecondToZeroY()
    {
        if (_isSecondAnimating || secondObjectRectTransform == null) return;
        StartCoroutine(SmoothMove(secondObjectRectTransform, 0f));
        Invoke("ResetInputs", 0.2f);
    }

    private void ResetInputs()
    {
        if (inputFields == null) return;

        foreach (var field in inputFields)
        {
            if (field != null && string.IsNullOrEmpty(field.text))
                field.text = "0";
        }
    }
    
    public void MoveSecondTo2012Y()
    {
        if (_isSecondAnimating || secondObjectRectTransform == null) return;
        StartCoroutine(SmoothMove(secondObjectRectTransform, 3840f));
    }

    private IEnumerator SmoothMove(RectTransform targetRect, float targetYPosition)
    {
        if (targetRect == firstObjectRectTransform) _isFirstAnimating = true;
        else if (targetRect == secondObjectRectTransform) _isSecondAnimating = true;

        var elapsedTime = 0f;
        Vector3 startPosition = targetRect.anchoredPosition;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / animationDuration;
            var newY = Mathf.Lerp(startPosition.y, targetYPosition, t);
            targetRect.anchoredPosition = new Vector3(startPosition.x, newY, startPosition.z);
            yield return null;
        }

        targetRect.anchoredPosition = new Vector3(startPosition.x, targetYPosition, startPosition.z);
        if (targetRect == firstObjectRectTransform) _isFirstAnimating = false;
        else if (targetRect == secondObjectRectTransform) _isSecondAnimating = false;
    }
}