using UnityEngine;
using Tools.B2B.PlayerRegistration.Validators;

public class PlayerInputValidator : MonoBehaviour
{
    private EmailValidator emailValidator = new EmailValidator();
    private PhoneValidator phoneValidator = new PhoneValidator();

    public bool ValidateEmailInput(string email)
    {
        var result = emailValidator.Validate(email);
        
        if (!result.IsValid)
        {
            Debug.LogWarning($"[Validation] Email inválido: {result.ErrorMessage}");
            return false;
        }
        
        Debug.Log($"[Validation] Email válido: {email}");
        return true;
    }

    public bool ValidatePhoneInput(string phone)
    {
        var result = phoneValidator.Validate(phone);
        
        if (!result.IsValid)
        {
            Debug.LogWarning($"[Validation] Telefone inválido: {result.ErrorMessage}");
            return false;
        }
        
        Debug.Log($"[Validation] Telefone válido: {phone}");
        return true;
    }

    public string GetEmailErrorMessage(string email)
    {
        var result = emailValidator.Validate(email);
        return result.IsValid ? "" : result.ErrorMessage;
    }

    public string GetPhoneErrorMessage(string phone)
    {
        var result = phoneValidator.Validate(phone);
        return result.IsValid ? "" : result.ErrorMessage;
    }
}
