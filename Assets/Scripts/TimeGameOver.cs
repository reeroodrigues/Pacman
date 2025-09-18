using UnityEngine;

public class TimerGameOver : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;

    private void OnEnable()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (gameManager != null)
        {
            gameManager.OnPacmanDeathAnimationFinished(); 
        }
    }
}