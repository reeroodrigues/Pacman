using UnityEngine;

namespace Rewards
{
    public class RewardBootstrap : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private RewardConfig config;
        [SerializeField] private bool enableRewards = false;
        [SerializeField] private int lowStockThreshold = 10;

        private void Awake()
        {
            RewardService.I.Enabled = enableRewards;
            RewardService.I.LowStockThreshold = lowStockThreshold;
            
            if (config != null && config.useRemote && !string.IsNullOrEmpty(config.remoteUrl))
                StartCoroutine(RewardService.I.LoadConfigAsync(config));
            else
                RewardService.I.LoadConfig(config);
        }
    }
}