using TMPro;
using Tools.B2B.PlayerRegistration.Models;
using Tools.B2B.PlayerRegistration.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    
    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField cellphoneInputField;
    
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text errorMessageText;
    [SerializeField] private GameObject registrationPanel;
    
    private const string ErrorMessageFillFields = "Por favor, preencha todos os campos.";
    private const string ErrorMessageInvalidEmail = "Por favor, insira um email válido com @.";
    private const string ErrorMessageInvalidPhone = "O telefone deve conter apenas números.";
    
    private IPlayerRegistrationService _playerRegistrationService;
    private GoogleSheetsService _googleSheetsService;
    private RankingManager _rankingManager;
    private bool _loading;

    [Inject]
    public void Construct(RankingManager rankingManager, IPlayerRegistrationService playerRegistrationService)
    {
        _rankingManager = rankingManager;
        _playerRegistrationService = playerRegistrationService;
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
        SetupGoogleSheetsService();
        SetupInputFields();
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        
        if (EventSystem.current && startButton)
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);

        CheckRegistrationStatus();
        HideErrorMessage();
    }

    private void SetupGoogleSheetsService()
    {
        _googleSheetsService = FindObjectOfType<GoogleSheetsService>();
    
        if (_googleSheetsService == null)
        {
            var serviceObj = new GameObject("GoogleSheetsService");
            _googleSheetsService = serviceObj.AddComponent<GoogleSheetsService>();
            DontDestroyOnLoad(serviceObj);
        }
    }

    private void SetupInputFields()
    {
        if (cellphoneInputField != null)
        {
            cellphoneInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            cellphoneInputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
        }

        if (emailInputField != null)
        {
            emailInputField.contentType = TMP_InputField.ContentType.EmailAddress;
        }
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

    private async void OnStartButtonClicked()
    {
        if (_loading) return;

        Debug.Log("[MainMenuController] Start button clicked");
        
        if (_rankingManager != null && _rankingManager.IsPlayerRegistered())
        {
            Debug.Log("[MainMenuController] Player already registered, starting game directly");
            StartGame();
            return;
        }

        Debug.Log("[MainMenuController] Player not registered, attempting registration...");
        
        var playerName = nameInputField.text.Trim();
        var playerEmail = emailInputField.text.Trim();
        var playerCellphone = cellphoneInputField.text.Trim();
        
        Debug.Log($"[MainMenuController] Form data - Name: '{playerName}', Email: '{playerEmail}', Phone: '{playerCellphone}'");

        var result = await _playerRegistrationService.RegisterPlayerAsync(
            playerName, 
            playerEmail,
            playerCellphone,
            consent: true);
        
        Debug.Log($"[MainMenuController] Registration result: Success={result.Success}, Message='{result.ErrorMessage}'");

        if (result.Success)
        {
            HideErrorMessage();
            
            await RegisterWithRankingManager(playerName, playerEmail, playerCellphone);
            
            SendToGoogleSheets(playerName, playerEmail, playerCellphone);
            
            if (registrationPanel != null)
                registrationPanel.SetActive(false);
            
            StartGame();
        }
        else
        {
            ShowErrorMessage(result.ErrorMessage);
        }
    }

    private async System.Threading.Tasks.Task RegisterWithRankingManager(string name, string email, string phone)
    {
        if (_rankingManager != null)
        {
            var success = await _rankingManager.RegisterPlayerAsync(name, email, phone);
            if (success)
            {
                Debug.Log($"Player {name} registered with RankingManager");
            }
            else
            {
                Debug.LogWarning("Failed to register player with RankingManager");
            }
        }
    }

    private void SendToGoogleSheets(string name, string email, string phone)
    {
        if (_googleSheetsService != null)
        {
            _googleSheetsService.SendPlayerData(name, email, phone, success =>
            {
                if (success)
                    Debug.Log("Player data sent to Google Sheets successfully");
                else
                    Debug.LogWarning("Failed to send player data to Google Sheets");
            });
        }
    }

    private void ShowErrorMessage(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            errorMessageText.gameObject.SetActive(true);
        }
    }

    private void HideErrorMessage()
    {
        if (errorMessageText != null)
            errorMessageText.gameObject.SetActive(false);
    }

    private async void StartGame()
    {
        if (_loading) return;

        Debug.Log("[MainMenuController] StartGame() called");
        
        if (_rankingManager != null && _rankingManager.IsPlayerRegistered())
        {
            Debug.Log("[MainMenuController] Player already registered, loading game directly");
            LoadGameScene();
            return;
        }

        Debug.Log("[MainMenuController] Player not registered, attempting registration...");
        
        var playerName = nameInputField.text.Trim();
        var playerEmail = emailInputField.text.Trim();
        var playerCellphone = cellphoneInputField.text.Trim();
        
        Debug.Log($"[MainMenuController] Form data - Name: '{playerName}', Email: '{playerEmail}', Phone: '{playerCellphone}'");

        var result = await _playerRegistrationService.RegisterPlayerAsync(
            playerName, 
            playerEmail,
            playerCellphone,
            consent: true);
        
        Debug.Log($"[MainMenuController] Registration result: Success={result.Success}, Message='{result.ErrorMessage}'");

        if (result.Success)
        {
            HideErrorMessage();
            
            await RegisterWithRankingManager(playerName, playerEmail, playerCellphone);
            
            SendToGoogleSheets(playerName, playerEmail, playerCellphone);
            
            if (registrationPanel != null)
                registrationPanel.SetActive(false);
            
            LoadGameScene();
        }
        else
        {
            ShowErrorMessage(result.ErrorMessage);
        }
    }

    private void LoadGameScene()
    {
        if (_loading) return;
        _loading = true;
        
        if (router != null)
        {
            router.StartGame();
        }
        else
        {
            SceneManager.LoadScene(fallbackSceneName);
        }
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }
}
