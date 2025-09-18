using System;
using TMPro;
using UnityEngine;
using Rewards;

public class EditItems : MonoBehaviour
{
    [Serializable]
    public struct Row
    {
        [Tooltip("Id do item (igual ao RewardConfig)")]
        public string itemId;
        [Tooltip("Campo de texto da coluna TOTAL (exibição e edição do total combinado)")]
        public TMP_Text totalText;
        
        [Tooltip("Input da coluna DIA (novo valor desejado para hoje)")]
        public TMP_InputField todayInput;
        
        [Tooltip("Input do TOTAL (edição direta do total).")]
        public TMP_InputField totalInput;
    }

    [Header("Linhas (um por prêmio)")]
    [SerializeField] private Row[] rows;

    [Header("UI opcional")]
    [SerializeField] private TMP_Text totalHojeLabel;
    [SerializeField] private TMP_Text dataLabel;
    [SerializeField] private TMP_Text totalGeralTotalLabel;

    [Header("Controle de Input")]
    [SerializeField] private InputNumberController inputController;
    [SerializeField] private InputNumberController totalInputController;

    private void OnEnable()
    {
        if (RewardService.I != null)
        {
            RewardService.I.OnConfigLoaded += OnConfigLoaded;
            if (RewardService.I.Config != null)
            {
                OnConfigLoaded(true);
            }
        }
    }

    private void OnDisable()
    {
        if (RewardService.I != null)
            RewardService.I.OnConfigLoaded -= OnConfigLoaded;
    }

    private void OnConfigLoaded(bool _)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        var svc = RewardService.I;
        if (svc == null) return;

        int somaHoje = 0;
        int somaTotalColuna = 0;

        foreach (var r in rows)
        {
            if (string.IsNullOrEmpty(r.itemId)) continue;

            int today = svc.RemainingForItem(r.itemId);
            int total = svc.TotalRemainingForItem(r.itemId);

            if (r.totalText != null)
            {
                r.totalText.text = total.ToString();
            }
            
            if (r.totalInput != null)
                r.totalInput.text = total.ToString();

            somaHoje += today;
            somaTotalColuna += total;
        }

        if (totalHojeLabel != null)
            totalHojeLabel.text = somaHoje.ToString();
        
        if (totalGeralTotalLabel != null)
            totalGeralTotalLabel.text = somaTotalColuna.ToString();

        if (dataLabel != null)
            dataLabel.text = DateTime.Today.ToString("d");
    }

    public void AddToDayFromTotal()
    {
        var svc = RewardService.I;
        if (svc == null)
        {
            Debug.LogError("RewardService.I é nulo!");
            return;
        }

        foreach (var r in rows)
        {
            if (string.IsNullOrEmpty(r.itemId) || r.todayInput == null || string.IsNullOrWhiteSpace(r.todayInput.text))
            {
                continue;
            }

            if (!int.TryParse(r.todayInput.text, out var quantityToAdd))
            {
                r.todayInput.text = "";
                continue;
            }

            if (quantityToAdd == 0)
            {
                r.todayInput.text = "";
                continue;
            }

            svc.TopUpToday(r.itemId, quantityToAdd);
            r.todayInput.text = "";
        }

        svc.EmitCurrentLowStock();
        RefreshUI();
    }

    public void OnInputFieldSelected(int index)
    {
        if (index >= 0 && index < rows.Length && rows[index].todayInput != null && inputController != null)
        {
            // Agora, basta informar ao InputNumberController qual índice foi selecionado.
            // Isso já é controlado pelos botões, mas se quiser, pode expor um método público
            // em InputNumberController para setar o índice ativo manualmente.
        }
    }

    public void OnInputFieldDeselected(int index)
    {
        if (index >= 0 && index < rows.Length && rows[index].todayInput != null && inputController != null)
        {
            // Não precisa mais comparar com GetSelectedInput(), já que o controller trabalha por índice.
            // Se quiser limpar a seleção, pode adicionar um método no InputNumberController para resetar o índice.
        }
    }
}
