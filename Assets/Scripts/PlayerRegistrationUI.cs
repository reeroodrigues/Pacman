using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PlayerRegistrationUI : MonoBehaviour
{
    [SerializeField] private GameObject registrationPanel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField phoneInput;
    [SerializeField] private Button  registerButton;
    [SerializeField] private TextMeshProUGUI errorText;
    
    private RankingManager _rankingManager;

    [Inject]
    public void Construct(RankingManager rankingManager)
    {
        _rankingManager = rankingManager;
    }

    private void Start()
    {
        registerButton.onClick.AddListener(() => OnRegisterButtonClicked().Forget());

        if (_rankingManager.IsPlayerRegistered())
        {
            registrationPanel.SetActive(false);
        }
        else
        {
            registrationPanel.SetActive(true);
        }
    }

    private async UniTaskVoid OnRegisterButtonClicked()
    {
        errorText.text = string.Empty;
        
        var name = nameInput.text.Trim();
        var email = emailInput.text.Trim();
        var phone = phoneInput.text.Trim();

        if (string.IsNullOrEmpty(name) ||  string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
        {
            errorText.text = "Please fill in all the fields";
        }
        
        var sucess = await _rankingManager.RegisterPlayerAsync(name, email, phone);

        if (sucess)
        {
            registrationPanel.SetActive(false);
        }
        else
        {
            errorText.text = "Registration failed";
        }
    }
}