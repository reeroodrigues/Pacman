using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Rewards;
using TMPro;
using Zenject;
using Cysharp.Threading.Tasks;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private RankingManager _rankingManager;

    [Header("Refs")]
    [SerializeField] private Ghost[] ghosts;
    [SerializeField] private Pacman pacman;
    [SerializeField] private Transform pellets;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Text winnerText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private int ptsAbsoluteSize = 20;

    [Header("Sounds")]
    [SerializeField] private SoundEvent musicStart;
    [SerializeField] private SoundEvent sfxGameOver;
    [SerializeField] private SoundEvent sfxWinner;

    [Header("Vitória")]
    [SerializeField] private bool goToVictoryOnClear = true;
    [SerializeField] private string victorySceneName = "Victory";
    [SerializeField] private Sprite victoryPrizeSprite;
    [SerializeField] private float winDelay = 2.5f;
    [SerializeField] private bool scaleGhostlessScore = true;

    [Header("Derrota")]
    [SerializeField] private bool goToVictoryOnGameOver = true;
    [SerializeField] private Sprite gameOverPrizeSprite;
    [SerializeField] private float loseDelay = 2.5f;

    [Header("Regras especiais de prêmio")]
    [Tooltip("Score equivalente a 100% da fase sem comer fantasmas (ex.: 2460).")]
    [SerializeField] private int pelletPerfectScore = 2460;
    [Tooltip("Score equivalente a 100% da fase (O valor que você considera cheio para a vitória: ex.2770).")]
    [SerializeField] private int allPelletsMaxScore = 2770;
    [Tooltip("ItemId no RewardConfig do Labubu (top prêmio apenas no 100% perfeito).")]
    [SerializeField] private string labubuItemId = "labubu";
    [Tooltip("ItemId no RewardConfig da Garrafinha (forçado em derrota com score > pelletPerfectScore).")]
    [SerializeField] private string garrafinhaItemId = "garrafa";

    // Estado do jogo
    public int Score { get; private set; } = 0;
    public int Lives { get; private set; } = 1;

    private int _ghostMultiplier = 1;
    private int _totalPelletCount = -1;
    private bool _isEnding;
    private bool _ateAlgumFantasma = false;
    private bool _noHasPellet = false;
    private bool _noHasOtherPrizes = false; // indica que só resta o top prize (labubu)

    private const string ZERO_PRIZE_ID = "canetazero";

    [Inject]
    public void Construct(RankingManager rankingManager)
    {
        _rankingManager = rankingManager;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }

        // confirma que o RewardService saiba qual é o top prize (labubu)
        if (RewardService.I != null)
            RewardService.I.TopPrizeItemId = labubuItemId;
    }

    private void Start()
    {
        NewGame();
        if (AudioManager.I != null)
        {
            AudioManager.I.SetMute(false);
            if (musicStart) AudioManager.I.PlayMusic(musicStart, loop: false, fade: 0.2f);
        }
    }

    private void NewGame()
    {
        _isEnding = false;
        SetScore(0);
        SetLives(1);
        _ateAlgumFantasma = false;
        NewRound();
    }

    private void NewRound()
    {
        if (gameOverText) gameOverText.enabled = false;
        if (winnerText) winnerText.enabled = false;

        foreach (Transform pellet in pellets)
            pellet.gameObject.SetActive(true);

        _ateAlgumFantasma = false;
        _totalPelletCount = -1;
        ResetState();
    }

    private void ResetState()
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].ResetState();
        }
        pacman.ResetState();
    }

    private void StartTransitionToVictory(string message, Sprite prizeSprite, float delay)
    {
        if (_isEnding) return;
        _isEnding = true;

        Time.timeScale = 0f;

        for (int i = 0; i < ghosts.Length; i++)
            if (ghosts[i] != null) ghosts[i].gameObject.SetActive(false);
        if (pacman != null) pacman.gameObject.SetActive(false);

        VictoryPayload.Message = message;
        VictoryPayload.PrizeSprite = prizeSprite;

        StartCoroutine(LoadSceneAfterDelay(victorySceneName, delay));
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void HandleWin()
    {
        if (winnerText) winnerText.enabled = true;

        if (AudioManager.I != null)
        {
            AudioManager.I.Play2D(sfxWinner);
            AudioManager.I.StopMusic(0.2f);
        }

        if (goToVictoryOnClear)
        {
            PrepareVictoryPayload(true);
            StartTransitionToVictory(VictoryPayload.Message, VictoryPayload.PrizeSprite, winDelay);
        }
    }

    private void SendResultsToGoogleSheets()
    {
        var googleSheetsService = FindObjectOfType<GoogleSheetsService>();

        if (googleSheetsService != null)
        {
            var playerName = PlayerPrefs.GetString("PlayerName", "Desconhecido");
            var playerEmail = PlayerPrefs.GetString("PlayerEmail", "");
            var playerPhone = PlayerPrefs.GetString("PlayerCellphone");
            var gameResult = $"Score: {Score}";
            var prizeWon = GetPrizeWon();
            
            googleSheetsService.SendPlayerDataWithResult(
                playerName,
                playerEmail,
                playerPhone,
                gameResult,
                prizeWon,
                success => 
            {
                if (success)
                    Debug.Log("Game results sent to Google Sheets");
                else
                {
                    Debug.LogWarning("Failed to send game results to Google Sheets");
                }
            });
        }
    }

    private string GetPrizeWon()
    {
        return "Prize info here";
    }

    public void GameOverFromTimer()
    {
        if (gameOverText) gameOverText.enabled = true;

        if (goToVictoryOnGameOver)
        {
            PrepareVictoryPayload(false);
            StartTransitionToVictory(VictoryPayload.Message, VictoryPayload.PrizeSprite, loseDelay);
        }
    }

    private void GameOverFromDeath()
    {
        if (gameOverText) gameOverText.enabled = true;

        if (AudioManager.I != null)
        {
            if (sfxGameOver) AudioManager.I.StopMusic(0.2f);
        }

        if (goToVictoryOnGameOver)
        {
            PrepareVictoryPayload(false);
            StartTransitionToVictory(VictoryPayload.Message, VictoryPayload.PrizeSprite, loseDelay);
        }
    }

    /// <summary>
    /// Monta o VictoryPayload de acordo com as regras:
    /// - Se Score == 0 => força "canetazero" (se houver estoque) e mensagem especial.
    /// - Se isWin && _noHasPellet => força labubu (top prize).
    /// - Se !isWin && Score > pelletPerfectScore => força garrafinha.
    /// - Caso normal: usa RewardService.Evaluate.
    /// - Se Evaluate não encontrar nada, verifica se só resta labubu (ignorando canetazero); se sim e for o caso especial (!isWin && !_noHasPellet && onlyTopLeft) força labubu.
    /// </summary>
    private void PrepareVictoryPayload(bool isWin)
    {
        try
        {
            var svc = RewardService.I;
            var cfg = svc != null ? svc.Config : null;

            // Limpa payload anterior
            VictoryPayload.Clear();
            VictoryPayload.Score = Score;

            SubmitScoreToLeaderboard().Forget();

            // 1) Caso especial: zero pontos = forçar canetazero e mensagem especial
            if (Score == 0)
            {
                VictoryPayload.IsZeroPoints = true;
                VictoryPayload.Message = "Obrigado pela participação! \n\n Não foi dessa vez!";

                if (svc != null && cfg != null)
                {
                    // Tenta forçar a canetazero (não passa por Evaluate)
                    if (svc.TryForceItem(ZERO_PRIZE_ID, out var rrZero))
                    {
                        VictoryPayload.PrizeSprite = rrZero.sprite;
                        VictoryPayload.PrizeItemId = rrZero.itemId;

                        var before = svc.RemainingForItem(rrZero.itemId);
                        svc.Decrement(rrZero);
                        var after = svc.RemainingForItem(rrZero.itemId);
                        RewardTelemetry.LogRewardGranted(rrZero, before, after, svc.TotalRemaining(), cause: "zero_points");
                    }
                    else
                    {
                        // sem canetazero em estoque: deixa message, sem prize
                        VictoryPayload.PrizeSprite = null;
                        VictoryPayload.PrizeItemId = null;
                    }
                }
                else
                {
                    VictoryPayload.PrizeSprite = null;
                    VictoryPayload.PrizeItemId = null;
                }

                return; // não continua para Evaluate
            }

            VictoryPayload.IsZeroPoints = false;

            // Se tem serviço e config, tenta regras especiais/fallbacks
            if (svc != null && cfg != null)
            {
                var ptsText = Score.ToString("N0");

                // Caso: vitória perfeita sem comer fantasmas = labubu (top prize)
                if (isWin && _noHasPellet && svc.TryForceItem(labubuItemId, out var rrPerfect))
                {
                    VictoryPayload.Message = $"PARABÉNS!\nVocê fez {ptsText} pontos!";
                    VictoryPayload.PrizeSprite = rrPerfect.sprite;
                    VictoryPayload.PrizeItemId = rrPerfect.itemId;
                    Score = allPelletsMaxScore;
                    VictoryPayload.Score = Score;

                    var before = svc.RemainingForItem(rrPerfect.itemId);
                    svc.Decrement(rrPerfect);
                    var after = svc.RemainingForItem(rrPerfect.itemId);
                    RewardTelemetry.LogRewardGranted(rrPerfect, before, after, svc.TotalRemaining(), cause: "win");
                    return;
                }

                // Caso: derrota, score acima do pelletPerfectScore = garrafinha
                if (!isWin && Score > pelletPerfectScore && svc.TryForceItem(garrafinhaItemId, out var rrBottle))
                {
                    VictoryPayload.Message = $"PARABÉNS!\nVocê fez {ptsText} pontos!";
                    VictoryPayload.PrizeSprite = rrBottle.sprite;
                    VictoryPayload.PrizeItemId = rrBottle.itemId;
                    VictoryPayload.Score = Score;

                    var before = svc.RemainingForItem(rrBottle.itemId);
                    svc.Decrement(rrBottle);
                    var after = svc.RemainingForItem(rrBottle.itemId);
                    RewardTelemetry.LogRewardGranted(rrBottle, before, after, svc.TotalRemaining(), cause: "game_over");
                    return;
                }

                // Mensagem padrão dependendo do score
                if (Score == 0)
                {
                    VictoryPayload.Message = $"Obrigado pela participação!";
                    VictoryPayload.Score = Score;
                }
                else
                {
                    VictoryPayload.Message = $"PARABÉNS!\nVocê fez {ptsText} pontos!";
                    VictoryPayload.Score = Score;
                }
                    

                // Fluxo normal: calcula eval e pede ao RewardService
                int maxScore = Mathf.Max(1, cfg.maxScore);
                float percent = Mathf.Clamp01(Score / (float)maxScore);
                int eval = Mathf.RoundToInt(percent * maxScore);

                var res = svc.Evaluate(eval);

                // Se Evaluate retornou um prêmio válido, usa e diminui estoque
                if (res.itemId != null && !string.IsNullOrEmpty(res.itemId))
                {
                    VictoryPayload.PrizeSprite = res.sprite;
                    VictoryPayload.PrizeItemId = res.itemId;

                    var before = svc.RemainingForItem(res.itemId);
                    svc.Decrement(res);
                    var after = svc.RemainingForItem(res.itemId);
                    RewardTelemetry.LogRewardGranted(res, before, after, svc.TotalRemaining(), cause: isWin ? "win" : "game_over");
                    return;
                }

                // Se Evaluate não encontrou nada, verifica se só resta o top prize (labubu),
                // ignorando canetazero. Nesse caso marcam _noHasOtherPrizes e, se for o caso, força labubu.
                bool onlyTopLeft = CheckOnlyTopPrizeLeft(svc, labubuItemId, ZERO_PRIZE_ID);

                // atualiza flag local
                _noHasOtherPrizes = onlyTopLeft;

                // novo caso pedido: em derrota (isWin == false), sem pellet, sem outros prêmios = dar labubu
                if (!isWin && !_noHasPellet && _noHasOtherPrizes)
                {
                    if (svc.TryForceItem(labubuItemId, out var rrFallback))
                    {
                        VictoryPayload.Message = $"PARABÉNS!\nVocê fez {ptsText} pontos!";
                        VictoryPayload.PrizeSprite = rrFallback.sprite;
                        VictoryPayload.PrizeItemId = rrFallback.itemId;

                        var before = svc.RemainingForItem(rrFallback.itemId);
                        svc.Decrement(rrFallback);
                        var after = svc.RemainingForItem(rrFallback.itemId);
                        RewardTelemetry.LogRewardGranted(rrFallback, before, after, svc.TotalRemaining(), cause: "no_other_prizes");
                        return;
                    }
                }

                // se chegou aqui sem prêmio, segue para fallback geral mais abaixo
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"RewardService Evaluate falhou: {e.Message}");
        }

        // Fallback geral
        var ptsFallback = Score.ToString("N0");
        VictoryPayload.Message = Score <= 0 ? "Obrigado pela participação!" : $"PARABÉNS!\nVocê fez {ptsFallback} pontos!";
        VictoryPayload.PrizeSprite = (Score > 0) ? victoryPrizeSprite : null;
        VictoryPayload.PrizeItemId = null;
    }

    // Verifica se só resta o topPrizeId (labubu) com estoque > 0, mas ainda ignorando o canetazero.
    private bool CheckOnlyTopPrizeLeft(RewardService svc, string topPrizeId, string ignoreId)
    {
        if (svc == null || svc.Config == null) return false;

        bool topHasStock = false;

        var cfg = svc.Config;
        for (int i = 0; i < cfg.categories.Length; i++)
        {
            var cat = cfg.categories[i];
            if (cat?.items == null) continue;

            for (int j = 0; j < cat.items.Length; j++)
            {
                var it = cat.items[j];
                if (it == null || string.IsNullOrEmpty(it.id)) continue;

                // Ignora o ignoreId (canetazero) ao calcular se "há outros prêmios"
                if (!string.IsNullOrEmpty(ignoreId) && it.id.Equals(ignoreId, StringComparison.OrdinalIgnoreCase))
                    continue;

                int remain = svc.RemainingForItem(it.id);

                if (string.Equals(it.id, topPrizeId, StringComparison.OrdinalIgnoreCase))
                {
                    if (remain > 0) topHasStock = true;
                }
                else
                {
                    if (remain > 0)
                    {
                        // encontrou outro prêmio com estoque (não é "só top")
                        return false;
                    }
                }
            }
        }

        // só retorna true se encontrou estoque do topprize e nenhum outro com estoque
        return topHasStock;
    }

    public void PacmanEaten()
    {
        if (pacman == null) return;

        pacman.DeathSequence();
        SetLives(Lives - 1);
    }

    public void GhostEaten(Ghost ghost)
    {
        _ateAlgumFantasma = true;
        int points = ghost.points * _ghostMultiplier;
        SetScore(Score + points);
        _ghostMultiplier++;
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        SetScore(Score + pellet.points);

        if (!HasRemainingPellets())
        {
            _noHasPellet = true;

            if (Score < allPelletsMaxScore)
            {
                SetScore(allPelletsMaxScore);
            }
            HandleWin();
        }
    }

    private int CountAllPellets()
    {
        int c = 0;
        foreach (Transform t in pellets)
        {
            if (t.GetComponent<Pellet>() != null || t.GetComponent<PowerPellet>() != null)
                c++;
        }
        return c;
    }

    private int CountRemainingPellets()
    {
        int c = 0;
        foreach (Transform t in pellets)
        {
            if (t.gameObject.activeSelf &&
                (t.GetComponent<Pellet>() != null || t.GetComponent<PowerPellet>() != null))
                c++;
        }
        return c;
    }

    private float GetPelletCompletion01()
    {
        if (_totalPelletCount <= 0) _totalPelletCount = CountAllPellets();
        if (_totalPelletCount <= 0) return 0f;

        int remaining = CountRemainingPellets();
        int collected = _totalPelletCount - remaining;
        return Mathf.Clamp01(collected / (float)_totalPelletCount);
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].Frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private void SetLives(int lives)
    {
        this.Lives = lives;
        if (livesText != null) livesText.text = "x" + lives.ToString();
    }

    private void SetScore(int score)
    {
        Score = score;

        if (scoreText != null)
        {
            scoreText.supportRichText = true;
            scoreText.text = $"{score:00} <size={ptsAbsoluteSize}>PTS</size>";
        }
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in pellets)
        {
            if (pellet.gameObject.activeSelf) return true;
        }
        return false;
    }

    private void ResetGhostMultiplier()
    {
        _ghostMultiplier = 1;
    }

    public void OnPacmanDeathAnimationFinished()
    {
        if (Lives > 0)
        {
            ResetState();
        }
        else
        {
            GameOverFromDeath();
        }
    }

    private async UniTaskVoid SubmitScoreToLeaderboard()
    {
        if (_rankingManager == null)
        {
            Debug.LogWarning("RankingManager not available for score submission");
            return;
        }

        if (!_rankingManager.IsPlayerRegistered())
        {
            Debug.Log("Player not registered, skipping leaderboard score submission");
            return;
        }

        var result = await _rankingManager.SubmitScoreAsync(Score);
    
        if (result.Success)
        {
            Debug.Log($"Score {Score} submitted successfully to leaderboard");
            SendResultsToGoogleSheets();
        }
        else
        {
            Debug.LogWarning($"Failed to submit score: {result.Message}");
        }
    }
}
