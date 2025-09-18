using TMPro;
using UnityEngine;

namespace Rewards
{
    public class LowStockBanner : MonoBehaviour
    {
        public enum WatchSource { Total, SpecificItem, PrizeItemFromPayload }

        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI label;

        [Header("Comportamento")]
        [SerializeField] private WatchSource source = WatchSource.Total;

        [Tooltip("ID do item a vigiar quando Source = SpecificItem (ex.: \"labubu\").")]
        [SerializeField] private string watchItemId = "";

        [Tooltip("Se >= 0, substitui o limiar global do RewardService.")]
        [SerializeField] private int thresholdOverride = -1;

        [Tooltip("Checar de novo a cada N segundos (0 = desliga).")]
        [SerializeField] private float autoRefreshSeconds = 0f;

        private Coroutine _loop;

        private void OnEnable()
        {
            if (panel) panel.SetActive(false);

            if (thresholdOverride > -1)
                RewardService.I.LowStockThreshold = thresholdOverride;
            
            if (source == WatchSource.Total)
            {
                RewardService.I.OnLowStock += HandleTotalLowStock;
                RewardService.I.OnLowStockCleared += HandleTotalCleared;
                RewardService.I.EmitCurrentLowStock();
            }
            else
            {
                if (source == WatchSource.PrizeItemFromPayload)
                    watchItemId = VictoryPayload.PrizeItemId;

                Refresh();

                if (autoRefreshSeconds > 0f)
                    _loop = StartCoroutine(AutoRefresh());
            }
        }

        private void OnDisable()
        {
            RewardService.I.OnLowStock -= HandleTotalLowStock;
            RewardService.I.OnLowStockCleared -= HandleTotalCleared;

            if (_loop != null) { StopCoroutine(_loop); _loop = null; }
        }

        private System.Collections.IEnumerator AutoRefresh()
        {
            var wait = new WaitForSecondsRealtime(Mathf.Max(0.1f, autoRefreshSeconds));
            while (enabled)
            {
                Refresh();
                yield return wait;
            }
        }
        
        public void SetWatchItem(string itemId)
        {
            source = WatchSource.SpecificItem;
            watchItemId = itemId;
            Refresh();
        }
        
        public void SetWatchFromPayload()
        {
            source = WatchSource.PrizeItemFromPayload;
            watchItemId = VictoryPayload.PrizeItemId;
            Refresh();
        }

        public void Refresh()
        {
            var svc = RewardService.I;
            if (svc == null || panel == null || label == null) return;

            int threshold = (thresholdOverride > -1) ? thresholdOverride : svc.LowStockThreshold;
            threshold = Mathf.Max(0, threshold);

            if (source == WatchSource.Total || string.IsNullOrEmpty(watchItemId))
            {
                int remainingTotal = svc.TotalRemaining();
                bool low = remainingTotal <= threshold;
                panel.SetActive(low);
                if (low) label.text = $"Restam apenas {remainingTotal} brindes hoje.";
                return;
            }
            
            int remaining = svc.RemainingForItem(watchItemId);
            string nice = ResolveItemName(watchItemId) ?? watchItemId;
            bool show = remaining <= threshold;
            panel.SetActive(show);
            if (show) label.text = $"Restam apenas {remaining} {nice} hoje.";
        }

        private string ResolveItemName(string itemId)
        {
            var cfg = RewardService.I.Config;
            if (cfg?.categories == null) return null;

            foreach (var c in cfg.categories)
            {
                if (c?.items == null) continue;
                foreach (var it in c.items)
                    if (it != null && it.id == itemId)
                        return string.IsNullOrEmpty(it.name) ? it.id : it.name;
            }
            return null;
        }
        
        private void HandleTotalLowStock(int remaining)
        {
            if (source != WatchSource.Total) return;
            if (panel) panel.SetActive(true);
            if (label) label.text = $"Restam apenas {remaining} brindes hoje.";
        }

        private void HandleTotalCleared(int remaining)
        {
            if (source != WatchSource.Total) return;
            if (panel) panel.SetActive(false);
        }
    }
}
