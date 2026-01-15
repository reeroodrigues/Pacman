using UnityEngine;
using Tools.B2B.PlayerRegistration.Validators;

public class ValidationTester : MonoBehaviour
{
    [ContextMenu("Test Valid Email")]
    private void TestValidEmail()
    {
        var validator = new EmailValidator();
        var result = validator.Validate("renan.pocket@gmail.com");
        Debug.Log($"Email 'renan.pocket@gmail.com': {(result.IsValid ? "VÁLIDO ✓" : $"INVÁLIDO ✗ - {result.ErrorMessage}")}");
    }

    [ContextMenu("Test Invalid Email - No @")]
    private void TestInvalidEmailNoAt()
    {
        var validator = new EmailValidator();
        var result = validator.Validate("renan.pocketgmail.com");
        Debug.Log($"Email 'renan.pocketgmail.com': {(result.IsValid ? "VÁLIDO ✓" : $"INVÁLIDO ✗ - {result.ErrorMessage}")}");
    }

    [ContextMenu("Test Invalid Email - No .com")]
    private void TestInvalidEmailNoCom()
    {
        var validator = new EmailValidator();
        var result = validator.Validate("renan.pocket@gmail");
        Debug.Log($"Email 'renan.pocket@gmail': {(result.IsValid ? "VÁLIDO ✓" : $"INVÁLIDO ✗ - {result.ErrorMessage}")}");
    }

    [ContextMenu("Test Valid Phone")]
    private void TestValidPhone()
    {
        var validator = new PhoneValidator();
        var result = validator.Validate("11984640689");
        Debug.Log($"Phone '11984640689': {(result.IsValid ? "VÁLIDO ✓" : $"INVÁLIDO ✗ - {result.ErrorMessage}")}");
    }

    [ContextMenu("Test Invalid Phone - Too Short")]
    private void TestInvalidPhoneTooShort()
    {
        var validator = new PhoneValidator();
        var result = validator.Validate("1198464");
        Debug.Log($"Phone '1198464': {(result.IsValid ? "VÁLIDO ✓" : $"INVÁLIDO ✗ - {result.ErrorMessage}")}");
    }

    [ContextMenu("Test Invalid Phone - Has Letters")]
    private void TestInvalidPhoneLetters()
    {
        var validator = new PhoneValidator();
        var result = validator.Validate("1198464abc9");
        Debug.Log($"Phone '1198464abc9': {(result.IsValid ? "VÁLIDO ✓" : $"INVÁLIDO ✗ - {result.ErrorMessage}")}");
    }

    [ContextMenu("Run All Tests")]
    private void RunAllTests()
    {
        Debug.Log("========== INICIANDO TESTES DE VALIDAÇÃO ==========");
        TestValidEmail();
        TestInvalidEmailNoAt();
        TestInvalidEmailNoCom();
        TestValidPhone();
        TestInvalidPhoneTooShort();
        TestInvalidPhoneLetters();
        Debug.Log("========== TESTES CONCLUÍDOS ==========");
    }
}
