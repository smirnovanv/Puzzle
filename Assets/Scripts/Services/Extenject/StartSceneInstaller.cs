using UnityEngine;
using Zenject;

public class StartSceneInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<StateManager>().AsSingle().NonLazy();
        Container.Bind<SceneChanger>().AsSingle().NonLazy();

        Container.Bind<IInitializable>().To<GameInitializer>().AsSingle().NonLazy();
    }
}
