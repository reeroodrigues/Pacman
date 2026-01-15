using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Tools.B2B.PlayerRegistration.Services;
using Tools.Leaderboard.Models;
using Tools.Leaderboard.Services;
using UnityEngine;
using Zenject;

public class RankingManager : MonoBehaviour
{
    private IPlayerRegistrationService _playerRegistrationService;
    private ILeaderboardService _leaderboardService;
    
    public event Action<List<LeaderboardEntry>> OnLeaderboardUpdated;
    public event Action<ScoreSubmissionResult> OnScoreSubmitted;

    [Inject]
    public void Construct(IPlayerRegistrationService playerRegistrationService, ILeaderboardService leaderboardService)
    {
        _playerRegistrationService = playerRegistrationService;
        _leaderboardService = leaderboardService;
    }

    public async UniTask<bool> RegisterPlayerAsync(string name, string email, string phone)
    {
        var result = await _playerRegistrationService.RegisterPlayerAsync(name, email, phone);

        if (result.Success)
            return true;

        return false;
    }

    public async UniTask<ScoreSubmissionResult> SubmitScoreAsync(int score, string eventId = "")
    {
        if (!_playerRegistrationService.IsPlayerRegistered())
        {
            return new ScoreSubmissionResult
            {
                Success = false,
                Message = "Player not registered"
            };
        }

        var playerData = _playerRegistrationService.GetCurrentPlayer();

        var result = await _leaderboardService.SubmitScoreAsync(
            playerData.Name,
            playerData.Email,
            playerData.PhoneNumber,
            score,
            eventId);
        
        OnScoreSubmitted?.Invoke(result);

        if (result.Success)
            OnLeaderboardUpdated?.Invoke(_leaderboardService.GetAllEntries());
        
        return result;
    }

    public List<LeaderboardEntry> GetTopPlayers(int count = 10)
    {
        var entries = _leaderboardService.GetTopPlayers(count);
        Debug.Log($"[RankingManager] GetTopPlayers({count}) returned {entries.Count} entries");
        return entries;
    }

    public List<LeaderboardEntry> GetAllEntries()
    {
        return _leaderboardService.GetAllEntries();
    }

    public LeaderboardEntry GetCurrentPlayerEntry()
    {
        if (!_playerRegistrationService.IsPlayerRegistered())
            return null;
        
        var playerData = _playerRegistrationService.GetCurrentPlayer();
        var playerId = GeneratePlayerId(playerData.Email);
        
        return _leaderboardService.GetPlayerEntry(playerId);
    }

    public int GetCurrentPlayerRank()
    {
        if (!_playerRegistrationService.IsPlayerRegistered())
            return -1;

        var playerData = _playerRegistrationService.GetCurrentPlayer();
        var playerId = GeneratePlayerId(playerData.Email);
        
        return _leaderboardService.GetPlayerRank(playerId);
    }

    public bool IsPlayerRegistered()
    {
        return _playerRegistrationService.IsPlayerRegistered();
    }

    public void ExportLeaderboardToCsv(string filePath)
    {
        _leaderboardService.ExportToCsv(filePath);
    }

    private string GeneratePlayerId(string email)
    {
        return email.GetHashCode().ToString();
    }
}