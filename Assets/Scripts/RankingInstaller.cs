using Zenject;
using UnityEngine;

public class RankingInstaller : MonoInstaller
{
    [SerializeField] private RankingManager rankingManager;

    public override void InstallBindings()
    {
        Container.Bind<RankingManager>()
            .FromInstance(rankingManager)
            .AsSingle()
            .NonLazy();
    }
}
