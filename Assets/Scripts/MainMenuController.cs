using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Rewards;

public class MainMenuController : MonoBehaviour
{
    [Header("Roteamento")]
    [SerializeField] private StartGameRouter router;

    [Header("Configuração de Prêmios")]
    [SerializeField] private RewardConfig rewardConfig;

    [Header("Fallback (se não houver router na cena)")]
    [SerializeField] private string fallbackSceneName = "Pacman";

    [Header("UI")]
    [SerializeField] private Button startButton;

    private bool _loading;

    private void Awake()
    {
        Time.timeScale = 1f;

        if (rewardConfig != null)
        {
            RewardService.I.LoadConfig(rewardConfig);
        }
        
        if (router == null)
        {
#if UNITY_2023_1_OR_NEWER
            router = FindFirstObjectByType<StartGameRouter>(FindObjectsInactive.Include);
#else
            router = FindObjectOfType<StartGameRouter>(true);
#endif
        }
    }

    private void Start()
    {
        if (startButton) startButton.onClick.AddListener(StartGame);

        if (EventSystem.current && startButton)
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
    }

    private void OnDestroy()
    {
        if (startButton) startButton.onClick.RemoveListener(StartGame);
    }

    private void Update()
    {
        if (_loading) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            StartGame();

#if ENABLE_INPUT_SYSTEM
        var pad = UnityEngine.InputSystem.Gamepad.current;
        if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            StartGame();
#endif
    }

    public void StartGame()
    {
        if (_loading) return;
        _loading = true;

        if (router != null)
        {
            router.StartGame();
        }
        else
        {
            SceneManager.LoadScene(fallbackSceneName, LoadSceneMode.Single);
        }
    }
}