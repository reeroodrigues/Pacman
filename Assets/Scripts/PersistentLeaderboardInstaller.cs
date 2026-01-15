using Tools.Leaderboard.Models;
using Tools.Leaderboard.Services;
using Tools.Leaderboard.Storage;
using UnityEngine;
using Zenject;

public class PersistentLeaderboardInstaller : MonoInstaller
{
    [SerializeField] private LeaderboardConfig leaderboardConfig;

    public override void InstallBindings()
    {
        if (leaderboardConfig == null)
        {
            Debug.LogError("[PersistentLeaderboardInstaller] LeaderboardConfig is null!");
            leaderboardConfig = ScriptableObject.CreateInstance<LeaderboardConfig>();
        }
        
        Container.Bind<LeaderboardConfig>().FromInstance(leaderboardConfig).AsSingle();
        
        Container.Bind<ILeaderboardStorage>()
            .To<PersistentLeaderboardStorage>()
            .AsSingle()
            .WithArguments(leaderboardConfig.saveKey);
        
        Container.Bind<ILeaderboardService>()
            .To<LeaderboardService>()
            .AsSingle()
            .NonLazy();
        
        Debug.Log("[PersistentLeaderboardInstaller] Installed persistent leaderboard with storage key: " + leaderboardConfig.saveKey);
    }
}
