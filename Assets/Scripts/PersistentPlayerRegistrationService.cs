using Cysharp.Threading.Tasks;
using Tools.B2B.PlayerRegistration.Models;
using Tools.B2B.PlayerRegistration.Services;
using UnityEngine;

public class PersistentPlayerRegistrationService : IPlayerRegistrationService
{
    private readonly PlayerRegistrationService _innerService;
    
    private const string PlayerNameKey = "PlayerName";
    private const string PlayerEmailKey = "PlayerEmail";
    private const string PlayerPhoneKey = "PlayerCellphone";
    private const string PlayerRegisteredKey = "PlayerRegistered";
    
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
        PlayerPrefs.SetString(PlayerNameKey, playerData.Name);
        PlayerPrefs.SetString(PlayerEmailKey, playerData.Email);
        PlayerPrefs.SetString(PlayerPhoneKey, playerData.PhoneNumber);
        PlayerPrefs.SetInt(PlayerRegisteredKey, 1);
        PlayerPrefs.Save();
    }

    private void LoadFromPlayerPrefs()
    {
        var isRegistered = PlayerPrefs.GetInt(PlayerRegisteredKey, 0);
        
        if (isRegistered == 1)
        {
            var name = PlayerPrefs.GetString(PlayerNameKey, "");
            var email = PlayerPrefs.GetString(PlayerEmailKey, "");
            var phone = PlayerPrefs.GetString(PlayerPhoneKey, "");
            
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                _currentPlayer = new PlayerRegistrationData(name, email, phone, true);
        }
    }

    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(PlayerNameKey);
        PlayerPrefs.DeleteKey(PlayerEmailKey);
        PlayerPrefs.DeleteKey(PlayerPhoneKey);
        PlayerPrefs.DeleteKey(PlayerRegisteredKey);
        PlayerPrefs.Save();
    }
}
