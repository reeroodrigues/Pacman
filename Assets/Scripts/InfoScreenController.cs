using System;
using System.Collections;
using UnityEngine;
using Rewards;
using TMPro;

public class InfoScreenController : MonoBehaviour
{
    [System.Serializable]
    public struct Row
    {
        [Tooltip("ItemId tem que estar exatamente como está no RewardConfig")]
        public string itemId;
        
        public TMP_Text valueLabel;
    }
    
    [Header("Bindings")]
    [SerializeField] private Row[] rows;
    
    [SerializeField] private TMP_Text totalLabel;
    [SerializeField] private string numberFormat = "NO";
    
    [Header("Atualização automática")]
    [SerializeField] private bool autoRefresh = true;
    [SerializeField] private float refreshInterval = 2f;
    
    private Coroutine _loop;

    private void OnEnable()
    {
        if (RewardService.I != null)
            RewardService.I.OnConfigLoaded += OnConfigLoaded;

        Refresh();

        if (autoRefresh)
            _loop = StartCoroutine(AutoRefreshLoop());
    }

    private void OnDisable()
    {
        if (RewardService.I != null)
            RewardService.I.OnConfigLoaded -= OnConfigLoaded;

        if (_loop != null)
            StopCoroutine(_loop);
        _loop = null;
    }

    private void OnConfigLoaded(bool fromRemote)
    {
        Refresh();
    }

    private IEnumerator AutoRefreshLoop()
    {
        var wait = new WaitForSecondsRealtime(refreshInterval);
        while (enabled)
        {
            Refresh();
            yield return wait;
        }
    }

    public void Refresh()
    {
        var svc = RewardService.I;
        if (svc == null)
            return;

        if (rows != null)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                if (row.valueLabel == null || string.IsNullOrWhiteSpace(row.itemId))
                    continue;

                var remaining = svc.RemainingForItem(row.itemId);
                row.valueLabel.text = remaining.ToString(numberFormat);
            }
        }

        if (totalLabel != null)
            totalLabel.text = svc.TotalRemaining().ToString(numberFormat);
    }
}
