using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverOnTimer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Timer timer;
    [SerializeField] private GameObject container;

    [Header("Mensagem")]
    [TextArea] [SerializeField] private string message = "Valeu por jogar! 🎉";

    [SerializeField] private Text messageUI;

    private void OnEnable()
    {
        if (timer != null) timer.onCompleted.AddListener(OnTimeUp);
    }

    private void OnDisable()
    {
        if (timer != null) timer.onCompleted.RemoveListener(OnTimeUp);
    }

    private void OnTimeUp()
    {
        if (container != null) container.SetActive(true);

        if (messageUI != null) messageUI.text = message;
        
        // Chama o método no GameManager para iniciar a transição
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOverFromTimer();
        }
    }
}