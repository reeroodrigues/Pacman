using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfosScreen : MonoBehaviour
{
    [SerializeField] private float animationDuration = 1f; // Duração da animação em segundos, ajustável no Inspector

    [Header("Primeiro Objeto (Dono do Script)")]
    [SerializeField] private RectTransform firstObjectRectTransform; // Opcional, usa o do dono por padrão

    [Header("Segundo Objeto (Referenciado)")]
    [SerializeField] private RectTransform secondObjectRectTransform; // Obrigatório, a ser vinculado no Inspector

    [Header("Inputs que precisam iniciar com 0")]
    [SerializeField] private TMP_InputField[] inputFields;

    private bool isFirstAnimating = false; // Controle para o primeiro objeto
    private bool isSecondAnimating = false; // Controle para o segundo objeto

    private void Awake()
    {
        // Usa o RectTransform do dono do script se não for vinculado manualmente
        if (firstObjectRectTransform == null)
            firstObjectRectTransform = GetComponent<RectTransform>();
        // Garante que secondObjectRectTransform seja válido
        if (secondObjectRectTransform == null)
        {
            Debug.LogError("Segundo objeto não foi referenciado no Inspector! Vincule o RectTransform do segundo objeto.");
            enabled = false; // Desativa o script se não houver referência
            return;
        }
    }

    // Método público para mover o primeiro objeto suavemente até Y = 0
    public void MoveFirstToZeroY()
    {
        if (isFirstAnimating || firstObjectRectTransform == null) return;
        StartCoroutine(SmoothMove(firstObjectRectTransform, 0f));
    }

    // Método público para mover o primeiro objeto suavemente até Y = 2012
    public void MoveFirstTo2012Y()
    {
        if (isFirstAnimating || firstObjectRectTransform == null) return;
        StartCoroutine(SmoothMove(firstObjectRectTransform, 2012f));
    }

    // Método público para mover o segundo objeto suavemente até Y = 0
    public void MoveSecondToZeroY()
    {
        if (isSecondAnimating || secondObjectRectTransform == null) return;
        StartCoroutine(SmoothMove(secondObjectRectTransform, 0f));
        //ResetInputs();
        Invoke("ResetInputs", 0.2f); // Chama novamente após 0.1 segundos para garantir
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

    // Método público para mover o segundo objeto suavemente até Y = 2012
    public void MoveSecondTo2012Y()
    {
        if (isSecondAnimating || secondObjectRectTransform == null) return;
        StartCoroutine(SmoothMove(secondObjectRectTransform, 2012f));
    }

    private IEnumerator SmoothMove(RectTransform targetRect, float targetYPosition)
    {
        if (targetRect == firstObjectRectTransform) isFirstAnimating = true;
        else if (targetRect == secondObjectRectTransform) isSecondAnimating = true;

        float elapsedTime = 0f;
        Vector3 startPosition = targetRect.anchoredPosition;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;
            float newY = Mathf.Lerp(startPosition.y, targetYPosition, t);
            targetRect.anchoredPosition = new Vector3(startPosition.x, newY, startPosition.z);
            yield return null;
        }

        targetRect.anchoredPosition = new Vector3(startPosition.x, targetYPosition, startPosition.z);
        if (targetRect == firstObjectRectTransform) isFirstAnimating = false;
        else if (targetRect == secondObjectRectTransform) isSecondAnimating = false;
    }
}