using Rewards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[System.Serializable]
public struct PrizeView
{
    public string itemId;
    public GameObject root;
}

public class VictorySceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI messageUI;
    [SerializeField] private Image prizeImage;
    [SerializeField] private Button backButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private RectTransform prizeArea;
    [SerializeField] private float areaPadding = 16f;
    [SerializeField] private bool onlyDownscale = true;
    
    [Header("UI de 0 Pontos")]
    [Tooltip("Objeto que contém a mensagem 'Obrigado pela participação!' e 'Não foi dessa vez.'.")]
    [SerializeField] private GameObject zeroPointsMessageRoot;

    [Tooltip("Objeto que contém a mensagem 'PARABÉNS! Você fez X pontos!'.")]
    [SerializeField] private GameObject prizeMessageRoot;

    [Header("Prize Views")]
    [SerializeField] private PrizeView[] prizeViews;
    [SerializeField] private GameObject defaultPrizeRoot;
    [SerializeField] private string ignorePrizeId = "canetazero";

    [Header("Defaults (caso não venha payload)")]
    [TextArea] [SerializeField] private string defaultMessage = "Parabéns! Você venceu! 🎉";
    [SerializeField] private Sprite defaultPrizeSprite;
    
    [Header("Router Gameplay")]
    [SerializeField] private StartGameRouter router;
    [SerializeField] private string gameplayFallback = "Pacman";

    [Header("Scenes")]
    [SerializeField] private string menuSceneName = "MainMenu";
    
    private bool _loading;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (backButton) backButton.onClick.AddListener(OnBackToGameplay);
        if (exitButton) exitButton.onClick.AddListener(OnExitToMenu);
        
        if (router == null)
        {
#if UNITY_2023_1_OR_NEWER
            router = FindFirstObjectByType<StartGameRouter>(FindObjectsInactive.Include);
#else
            router = FindObjectOfType<StartGameRouter>(true);
#endif
        }

        ApplyPayloadOrDefaults();
    }

    private void OnDestroy()
    {
        if (backButton) backButton.onClick.RemoveListener(OnBackToGameplay);
        if (exitButton) exitButton.onClick.RemoveListener(OnExitToMenu);
    }

    private void ApplyPayloadOrDefaults()
    {
        var svc = RewardService.I;
        var cfg = svc != null ? svc.Config : null;

        // Se o payload já veio com prêmio (ou com sprite) ou é o caso zero points,
        // devemos respeitar o payload e não executar a lógica que limpa o payload
        // baseada no inventário atual (NoPrizesLeftIgnoring).
        bool payloadHasPrize = !string.IsNullOrEmpty(VictoryPayload.PrizeItemId) || VictoryPayload.PrizeSprite != null;
        bool payloadIsZeroPoints = false;
        try { payloadIsZeroPoints = VictoryPayload.IsZeroPoints; } catch { /* some builds may not have it, ignore */ }

        // Somente se NÃO houver payload já definido, checaremos "NoPrizesLeftIgnoring"
        if (!payloadHasPrize && !payloadIsZeroPoints)
        {
            if (NoPrizesLeftIgnoring(ignorePrizeId))
            {
                if (zeroPointsMessageRoot) zeroPointsMessageRoot.SetActive(true);
                if (prizeMessageRoot) prizeMessageRoot.SetActive(false);

                if (messageUI) messageUI.text = "Obrigado pela participação!";
                if (prizeImage)
                {
                    prizeImage.sprite = null;
                    prizeImage.enabled = false;
                }

                if (prizeViews != null)
                    foreach (var pv in prizeViews)
                        if (pv.root) pv.root.SetActive(false);
                if (defaultPrizeRoot) defaultPrizeRoot.SetActive(false);

                VictoryPayload.Clear();
                return;
            }
        }

        // A partir daqui: ou havia payload definido (respeitamos) ou não havia e não fechou pelo NoPrizesLeftIgnoring.
        // Exibe a mensagem de zero pontos, se marcada.
        if (zeroPointsMessageRoot) zeroPointsMessageRoot.SetActive(payloadIsZeroPoints);
        
        // Decide com base no score do payload (independente do GameManager existir ou não nesta cena)
        int score = VictoryPayload.Score;

        // Ativa "Retire seu brinde!" (prizeMessageRoot) se score > 0
        if (prizeMessageRoot) prizeMessageRoot.SetActive(score > 0);

        // (opcional) Espelhar a raiz de 0 pontos:
        if (zeroPointsMessageRoot) zeroPointsMessageRoot.SetActive(score <= 0 || VictoryPayload.IsZeroPoints);

        // // Se existe score > 0, showprizemessageroot exceto quando é zero points
        // if (GameManager.Instance != null && GameManager.Instance.Score > 0)
        // {
        //     if (prizeMessageRoot) prizeMessageRoot.SetActive(!payloadIsZeroPoints);
        // }
        // else
        // {
        //     if (prizeMessageRoot) prizeMessageRoot.SetActive(false);
        // }

        // Usa a mensagem do payload ou a default
        var msg = !string.IsNullOrEmpty(VictoryPayload.Message) ? VictoryPayload.Message : defaultMessage;
        if (messageUI) messageUI.text = msg;

        // Sprite do premio: prioridade para o payload, senão default
        var prize = VictoryPayload.PrizeSprite != null ? VictoryPayload.PrizeSprite : defaultPrizeSprite;
        if (prizeImage)
        {
            prizeImage.sprite = prize;
            prizeImage.enabled = (prize != null);
            prizeImage.preserveAspect = true;
            if (prize != null) FitPrizeToNativeInsideArea();
        }

        if (!string.IsNullOrEmpty(VictoryPayload.MenuSceneName))
            menuSceneName = VictoryPayload.MenuSceneName;

        // Mostra a imagem específica se payload indicar um itemId (ex.: labubu / canetazero)
        ShowPrizeView(VictoryPayload.PrizeItemId);

        // limpa o payload só após aplicar. Assim a informação é preservada para o frame
        VictoryPayload.Clear();
    }


    private bool NoPrizesLeftIgnoring(string ignoreId)
    {
        var svc = RewardService.I;
        var cfg = svc != null ? svc.Config : null;
        if (svc == null || cfg == null || cfg.categories == null) return false;

        for (int i = 0; i < cfg.categories.Length; i++)
        {
            var cat = cfg.categories[i];
            if (cat?.items == null) continue;

            for (int j = 0; j < cat.items.Length; j++)
            {
                var it = cat.items[j];
                if (it == null || string.IsNullOrEmpty(it.id)) continue;

                if (!string.IsNullOrEmpty(ignoreId) &&
                    it.id.Equals(ignoreId, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                int today = svc.RemainingForItem(it.id);           // >= 0
                int total = svc.TotalRemainingForItem(it.id);      // pode ser negativo agora

                // Se ainda há algo para entregar hoje, ou se o total líquido for positivo
                if (today > 0 || total > 0)
                    return false;
            }
        }
        return true;
    }


    private void FitPrizeToNativeInsideArea()
    {
        if (prizeImage.sprite == null) return;
        
        prizeImage.SetNativeSize();
        
        if (prizeArea != null)
        {
            var rt = prizeImage.rectTransform;
            var size = rt.sizeDelta;
            var max = prizeArea.rect.size - new Vector2(areaPadding * 2f, areaPadding * 2f);

            var scale = 1f;
            var tooWide = size.x > max.x;
            var tooTall = size.y > max.y;

            if (tooWide || tooTall)
            {
                var sx = max.x / size.x;
                var sy = max.y / size.y;
                scale = Mathf.Min(sx, sy);
            }
            else if (!onlyDownscale)
            {
                var sx = max.x / size.x;
                var sy = max.y / size.y;
                scale = Mathf.Min(1f, Mathf.Min(sx, sy));
            }

            rt.sizeDelta = size * scale;
            
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }
    }

    private void ShowPrizeView(string itemId)
    {
        if (prizeViews != null)
            foreach (var prize in prizeViews)
                if (prize.root)
                {
                    prize.root.SetActive(false);
                }

        if (defaultPrizeRoot)
        {
            defaultPrizeRoot.SetActive(false);
        }

        if (!string.IsNullOrEmpty(itemId) && prizeViews != null)
        {
            for (int i = 0; i < prizeViews.Length; i++)
            {
                if (string.Equals(prizeViews[i].itemId, itemId, System.StringComparison.OrdinalIgnoreCase))
                {
                    if(prizeViews[i].root) prizeViews[i].root.SetActive(true);
                    return;
                }
            }
        }

        if (defaultPrizeRoot)
        {
            defaultPrizeRoot.SetActive(true);
        }
    }

    public void OnBackToGameplay()
    {
        if (_loading) return;
        _loading = true;
        Time.timeScale = 1f;

        if (router != null)
        {
            router.StartGame();
        }
        else
        {
            SceneManager.LoadScene(gameplayFallback, LoadSceneMode.Single);
        }
    }

    public void OnExitToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
    
    private void Update()
    {
        if (!_loading)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                OnBackToGameplay();

            if (Input.GetKeyDown(KeyCode.Escape))
                OnExitToMenu();

#if ENABLE_INPUT_SYSTEM
            var pad = UnityEngine.InputSystem.Gamepad.current;
            if (pad != null)
            {
                if (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)
                    OnBackToGameplay();
                if (pad.buttonEast.wasPressedThisFrame || pad.selectButton.wasPressedThisFrame)
                    OnExitToMenu();
            }
#endif
        }
    }
}