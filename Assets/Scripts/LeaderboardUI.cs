using System.Collections.Generic;
using TMPro;
using Tools.Leaderboard.Models;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LeaderboardUI  : MonoBehaviour
{ 
    [Header("UI")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI playerRankText;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private int maxEntriesToShow = 10;
    
    private RankingManager _rankingManager;
    private List<GameObject>  _entriesInstances = new List<GameObject>();

    [Inject]
    public void Construct(RankingManager rankingManager)
    {
        _rankingManager = rankingManager;
        
        if (_rankingManager != null)
        {
            _rankingManager.OnLeaderboardUpdated += OnLeaderboardUpdated;
        }
    }

    private void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);

        if (closeButton != null)
            closeButton.onClick.AddListener(() => leaderboardPanel.SetActive(false));

        RefreshLeaderboard();
    }

    private void OnDestroy()
    {
        if (_rankingManager != null)
            _rankingManager.OnLeaderboardUpdated -= OnLeaderboardUpdated;
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        ClearEntries();
        
        if (_rankingManager == null)
            return;
        
        var topPlayers = _rankingManager.GetTopPlayers(maxEntriesToShow);

        foreach (var entry in topPlayers)
        {
            CreateLeaderboardEntry(entry);
        }

        UpdatePlayerInfo();
    }

    private void UpdatePlayerInfo()
    {
        if (_rankingManager == null)
        {
            if (playerRankText != null)
                playerRankText.text = "";
            if (playerScoreText != null)
                playerScoreText.text = "";
            return;
        }
        
        if (!_rankingManager.IsPlayerRegistered())
        {
            if(playerRankText != null)
                playerRankText.text = "";

            if (playerScoreText != null)
                playerScoreText.text = "";
            return;
        }

        var rank = _rankingManager.GetCurrentPlayerRank();
        var entry = _rankingManager.GetCurrentPlayerEntry();

        if (playerRankText != null)
            playerRankText.text = rank > 0 ? $"Rank: #{rank}" : "";
        
        if(playerScoreText != null &&  entry != null)
            playerScoreText.text = $"Score: {entry.Score}";
    }

    private void CreateLeaderboardEntry(LeaderboardEntry entry)
    {
        if (entryPrefab == null || entryContainer == null)
            return;
        
        var entryObj =  Instantiate(entryPrefab, entryContainer);
        _entriesInstances.Add(entryObj);
        
        var rankText = entryObj.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
        var nameText = entryObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var scoreText = entryObj.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>();

        if (rankText != null)
            rankText.text = $"#{entry.Rank}";

        if (nameText != null)
            nameText.text = entry.PlayerName;

        if (scoreText != null)
            scoreText.text = entry.Score.ToString();
    }

    private void ClearEntries()
    {
        foreach (var entry in _entriesInstances)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        _entriesInstances.Clear();
    }

    private void OnLeaderboardUpdated(List<LeaderboardEntry> entries)
    {
        RefreshLeaderboard();
    }
}