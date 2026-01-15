using Tools.B2B.PlayerRegistration.Services;
using Tools.Leaderboard.Services;
using UnityEngine;
using Zenject;

public class LeaderboardController : MonoBehaviour
{
    [Inject] private IPlayerRegistrationService _registration;
    [Inject] private ILeaderboardService _leaderboard;

    public async void OnGameComplete(int finalScore)
    {
        var player = _registration.GetCurrentPlayer();

        var result = await _leaderboard.SubmitScoreAsync(
            player.Name,
            player.Email,
            player.PhoneNumber,
            finalScore,
            "event123"
            );

        if (result.Success)
            Debug.Log($"Your rank: {result.Rank}");
    }
}
