using Cysharp.Threading.Tasks;
using Tools.B2B.PlayerRegistration.Models;
using Tools.B2B.PlayerRegistration.Services;
using UnityEngine;

public class PersistentPlayerRegistrationService : IPlayerRegistrationService
{
    private readonly PlayerRegistrationService _innerService;
    
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string PLAYER_EMAIL_KEY = "PlayerEmail";
    private const string PLAYER_PHONE_KEY = "PlayerCellphone";
    private const string PLAYER_REGISTERED_KEY = "PlayerRegistered";
    
    private PlayerRegistrationData _currentPlayer;

    public PersistentPlayerRegistrationService(PlayerRegistrationService innerService)
    {
        _innerService = innerService;
        LoadFromPlayerPrefs();
    }

    public async UniTask<RegistrationResult> RegisterPlayerAsync(string name, string email, string phone, bool consent = true)
    {
        var result = await _innerService.RegisterPlayerAsync(name, email, phone, consent);
        
        if (result.Success)
        {
            _currentPlayer = result.PlayerData;
            SaveToPlayerPrefs(result.PlayerData);
        }
        
        return result;
    }

    public ValidationResult ValidateName(string name)
    {
        return _innerService.ValidateName(name);
    }

    public ValidationResult ValidateEmail(string email)
    {
        return _innerService.ValidateEmail(email);
    }

    public ValidationResult ValidatePhoneNumber(string phone)
    {
        return _innerService.ValidatePhoneNumber(phone);
    }

    public PlayerRegistrationData GetCurrentPlayer()
    {
        return _currentPlayer;
    }

    public bool IsPlayerRegistered()
    {
        var isRegistered = _currentPlayer != null;
        Debug.Log($"[PersistentPlayerRegistration] IsPlayerRegistered called: {isRegistered}");
        return isRegistered;
    }

    public void ClearRegistration()
    {
        _currentPlayer = null;
        ClearPlayerPrefs();
        _innerService.ClearRegistration();
    }

    private void SaveToPlayerPrefs(PlayerRegistrationData playerData)
    {
        PlayerPrefs.SetString(PLAYER_NAME_KEY, playerData.Name);
        PlayerPrefs.SetString(PLAYER_EMAIL_KEY, playerData.Email);
        PlayerPrefs.SetString(PLAYER_PHONE_KEY, playerData.PhoneNumber);
        PlayerPrefs.SetInt(PLAYER_REGISTERED_KEY, 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[PersistentPlayerRegistration] Saved to PlayerPrefs: {playerData.Name}");
    }

    private void LoadFromPlayerPrefs()
    {
        Debug.Log("[PersistentPlayerRegistration] Attempting to load from PlayerPrefs...");
        
        var isRegistered = PlayerPrefs.GetInt(PLAYER_REGISTERED_KEY, 0);
        Debug.Log($"[PersistentPlayerRegistration] PlayerRegistered flag: {isRegistered}");
        
        if (isRegistered == 1)
        {
            var name = PlayerPrefs.GetString(PLAYER_NAME_KEY, "");
            var email = PlayerPrefs.GetString(PLAYER_EMAIL_KEY, "");
            var phone = PlayerPrefs.GetString(PLAYER_PHONE_KEY, "");
            
            Debug.Log($"[PersistentPlayerRegistration] Retrieved - Name: '{name}', Email: '{email}', Phone: '{phone}'");
            
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
            {
                _currentPlayer = new PlayerRegistrationData(name, email, phone, true);
                Debug.Log($"[PersistentPlayerRegistration] ✅ Successfully loaded player: {name}");
            }
            else
            {
                Debug.LogWarning("[PersistentPlayerRegistration] ❌ Name or Email is empty!");
            }
        }
        else
        {
            Debug.LogWarning("[PersistentPlayerRegistration] ❌ No registration found in PlayerPrefs");
        }
    }

    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(PLAYER_NAME_KEY);
        PlayerPrefs.DeleteKey(PLAYER_EMAIL_KEY);
        PlayerPrefs.DeleteKey(PLAYER_PHONE_KEY);
        PlayerPrefs.DeleteKey(PLAYER_REGISTERED_KEY);
        PlayerPrefs.Save();
        
        Debug.Log("[PersistentPlayerRegistration] Cleared PlayerPrefs");
    }
}
