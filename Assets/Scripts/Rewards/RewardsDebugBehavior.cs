using UnityEngine;

namespace Rewards
{
    public class RewardsDebugBehaviour : MonoBehaviour
    {
        public RewardConfig config;
        [Range(0, 1000)] public int testScore = 200;

        private void Awake()
        {
            RewardService.I.Enabled = false;
            RewardService.I.LoadConfig(config);
            RewardService.I.ComputeAutoBands();
        }

        [ContextMenu("Test Evaluate()")]
        private void TestEvaluate()
        {
            var res = RewardService.I.Evaluate(testScore);
            Debug.Log($"score={testScore} -> %={res.percentUsed:0.##} | cat={res.categoryName} | item={(string.IsNullOrEmpty(res.itemId) ? "<nenhum>" : res.itemName)}");
        }
    }
}