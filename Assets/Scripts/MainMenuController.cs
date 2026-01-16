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

        ClearPlayerRegistrationForNewSession();
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
        if (nameInputField != null)
        {
            nameInputField.lineType = TMP_InputField.LineType.SingleLine;
            nameInputField.onSubmit.AddListener(OnInputFieldSubmit);
        }
        
        if (emailInputField != null)
        {
            emailInputField.contentType = TMP_InputField.ContentType.EmailAddress;
            emailInputField.lineType = TMP_InputField.LineType.SingleLine;
            emailInputField.onSubmit.AddListener(OnInputFieldSubmit);
        }
        
        if (cellphoneInputField != null)
        {
            cellphoneInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            cellphoneInputField.characterValidation = TMP_InputField.CharacterValidation.Integer;
            cellphoneInputField.lineType = TMP_InputField.LineType.SingleLine;
            cellphoneInputField.onSubmit.AddListener(OnInputFieldSubmit);
        }
    }
    
    private void OnInputFieldSubmit(string text)
    {
        if (nameInputField != null && nameInputField.isFocused)
        {
            emailInputField?.Select();
            return;
        }
        
        if (emailInputField != null && emailInputField.isFocused)
        {
            cellphoneInputField?.Select();
            return;
        }
        
        if (cellphoneInputField != null && cellphoneInputField.isFocused)
        {
            cellphoneInputField.DeactivateInputField();
        }
    }

    private void ClearPlayerRegistrationForNewSession()
    {
        PlayerPrefs.DeleteKey("PlayerRegistered");
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("PlayerEmail");
        PlayerPrefs.DeleteKey("PlayerPhone");
        PlayerPrefs.Save();
        
        ClearInputFields();
        
        if (registrationPanel != null)
            registrationPanel.SetActive(true);
    }
    
    private void ClearInputFields()
    {
        if (nameInputField != null)
            nameInputField.text = "";
        
        if (emailInputField != null)
            emailInputField.text = "";
        
        if (cellphoneInputField != null)
            cellphoneInputField.text = "";
    }

    public async void OnStartButtonClicked()
    {
        if (_loading) return;
        
        var playerName = nameInputField.text.Trim();
        var playerEmail = emailInputField.text.Trim();
        var playerCellphone = cellphoneInputField.text.Trim();

        var result = await _playerRegistrationService.RegisterPlayerAsync(
            playerName, 
            playerEmail,
            playerCellphone,
            consent: true);

        if (result.Success)
        {
            HideErrorMessage();
            
            await RegisterWithRankingManager(playerName, playerEmail, playerCellphone);
            
            if (registrationPanel != null)
                registrationPanel.SetActive(false);
            
            LoadGameScene();
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
        
        if (nameInputField != null)
        {
            nameInputField.onSubmit.RemoveListener(OnInputFieldSubmit);
        }
        
        if (emailInputField != null)
        {
            emailInputField.onSubmit.RemoveListener(OnInputFieldSubmit);
        }
        
        if (cellphoneInputField != null)
        {
            cellphoneInputField.onSubmit.RemoveListener(OnInputFieldSubmit);
        }
    }
}
