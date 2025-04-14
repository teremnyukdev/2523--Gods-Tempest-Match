using Application.UI;
using UnityEngine;
using Zenject;

namespace Application.Game
{
    [CreateAssetMenu(fileName = "GameInstaller", menuName = "Installers/GameInstaller")]
    public class GameInstaller : ScriptableObjectInstaller<GameInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<GameOverState>().AsSingle();
            Container.Bind<TitleStateController>().AsSingle();
            Container.Bind<SettingsPopupData>().AsSingle();
            Container.Bind<InfoStateController>().AsSingle();
        }
    }
}