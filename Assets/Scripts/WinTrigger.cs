using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTrigger : MonoBehaviour
{
    [SerializeField] private Sprite prizeSprite;

    public void OnPlayerWon()
    {
        VictoryPayload.PrizeSprite = prizeSprite;
        VictoryPayload.Message = "Parabéns! Você venceu! 🎉";
        VictoryPayload.MenuSceneName = "MainMenu";
        SceneManager.LoadScene("Victory");
    }
}