using System;
using UnityEngine;
using Zenject;

public class GameScoreSubmitter : MonoBehaviour
{
    [SerializeField] private string eventId = "pacman_game";
    
    private RankingManager _rankingManager;

    [Inject]
    private void Construct(RankingManager rankingManager)
    {
        _rankingManager = rankingManager;
        _rankingManager.OnScoreSubmitted += OnScoreSubmitted;
    }

    private void OnDestroy()
    {
        if (_rankingManager != null)
            _rankingManager.OnScoreSubmitted -= OnScoreSubmitted;
    }

    public async void SubmitScore(int score)
    {
        if (!_rankingManager.IsPlayerRegistered())
            return;

        var result = await _rankingManager.SubmitScoreAsync(score, eventId);

        if (result.Success)
            Debug.Log($"Score submitted successfully! New rank: {result.Rank}");
        else
            Debug.LogError($"Failed to submit score: {result.Message}");
    }

    private void OnScoreSubmitted(Tools.Leaderboard.Models.ScoreSubmissionResult result)
    {
        if (result.Success)
            Debug.Log($"Score submitted successfully! Rank: {result.Rank}");
    }
}