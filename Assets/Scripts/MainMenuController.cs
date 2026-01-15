using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using Cysharp.Threading.Tasks;
using Rewards;
using Zenject;

public class MainMenuController : MonoBehaviour
{
    [Header("Router")]
    [SerializeField] private StartGameRouter router;
    
    [Header("Prize Settings")]
    [SerializeField] private RewardConfig rewardConfig;
    
    [Header("Fallback")]
    [SerializeField] private string fallbackSceneName = "Pacman";
    
    [Header("UI")]
    [SerializeField] private Button startButton;
    
    [Header("Registration UI")]
    [SerializeField] private GameObject registrationPanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField phoneInput;
    [SerializeField] private TextMeshProUGUI errorText;
    
    private bool _loading;
    private RankingManager _rankingManager;

    [Inject]
    public void Construct(RankingManager rankingManager)
    {
        _rankingManager = rankingManager;
    }

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
        if (startButton) 
            startButton.onClick.AddListener(OnStartButtonClicked);
        
        if (EventSystem.current && startButton)
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);

        CheckRegistrationStatus();
    }

    private void OnDestroy()
    {
        if (startButton) 
            startButton.onClick.RemoveListener(OnStartButtonClicked);
    }

    private void Update()
    {
        if (_loading) return;
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnStartButtonClicked();
            
#if ENABLE_INPUT_SYSTEM
        var pad = UnityEngine.InputSystem.Gamepad.current;
        if (pad != null && pad.buttonSouth.wasPressedThisFrame)
            OnStartButtonClicked();
#endif
    }

    private void CheckRegistrationStatus()
    {
        if (_rankingManager != null && _rankingManager.IsPlayerRegistered())
        {
            if (registrationPanel != null)
                registrationPanel.SetActive(false);
        }
        else
        {
            if (registrationPanel != null)
                registrationPanel.SetActive(true);
        }
    }

    private async void OnStartButtonClicked()
    {
        if (_loading) return;

        if (_rankingManager == null)
        {
            Debug.LogWarning("RankingManager not available, starting game without registration");
            StartGame();
            return;
        }

        if (_rankingManager.IsPlayerRegistered())
        {
            StartGame();
            return;
        }

        var name = nameInput != null ? nameInput.text.Trim() : string.Empty;
        var email = emailInput != null ? emailInput.text.Trim() : string.Empty;
        var phone = phoneInput != null ? phoneInput.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
        {
            ShowError("Por favor, preencha todos os campos");
            return;
        }

        ShowError("");
        
        if (startButton != null)
            startButton.interactable = false;

        var success = await _rankingManager.RegisterPlayerAsync(name, email, phone);
        
        if (success)
        {
            Debug.Log($"Player {name} registered successfully");
            
            if (registrationPanel != null)
                registrationPanel.SetActive(false);
            
            StartGame();
        }
        else
        {
            ShowError("Falha no registro. Tente novamente.");
            
            if (startButton != null)
                startButton.interactable = true;
        }
    }

    private void ShowError(string message)
    {
        if (errorText != null)
            errorText.text = message;
    }

    private void StartGame()
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
