using Tools.B2B.PlayerRegistration.Services;
using Zenject;

public class PersistentPlayerRegistrationInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PlayerRegistrationService>().AsSingle();
        
        Container.Bind<IPlayerRegistrationService>()
            .To<PersistentPlayerRegistrationService>()
            .AsSingle()
            .NonLazy();
    }
}
