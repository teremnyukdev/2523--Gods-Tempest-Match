using System.Threading;
using Application.UI;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using ILogger = Core.ILogger;

namespace Application.Game
{
    public class TitleStateController : StateController
    {
        private readonly IUiService _uiService;

        private TitleScreen _titleScreen;
        private CancellationTokenSource _cancellationTokenSource;

        public TitleStateController(IUiService uiService, ILogger logger) : base(logger)
        {
            _uiService = uiService;
        }

        public override UniTask Enter(CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            if (_titleScreen == null)
            {
                _titleScreen = _uiService.GetScreen<TitleScreen>(ConstScreens.TitleScreen);
                Subscribe();
            }

            if (_titleScreen != null)
                _titleScreen.ShowAsync(cancellationToken).Forget();

            return UniTask.CompletedTask;
        }

        public override async UniTask Exit()
        {
            UnSubscribeTitleScreen();
            await HideTitleScreen(_cancellationTokenSource.Token);

            _cancellationTokenSource.Cancel();

            await base.Exit();
        }

        private async UniTask HideTitleScreen(CancellationToken cancellationToken)
        {
            await _uiService.HideScreen(ConstScreens.TitleScreen, cancellationToken);
        }

        private void UnSubscribeTitleScreen()
        {
            _titleScreen.PlayButtonPressEvent -= () => _ = GoToGame();
            _titleScreen.InfoButtonPressEvent -= () => _ = ShowInfoPopup();
        }

        private void Subscribe()
        {
            _titleScreen.PlayButtonPressEvent += () => _ = GoToGame();
            _titleScreen.InfoButtonPressEvent += () => _ = ShowInfoPopup();
        }

        private async UniTask GoToGame()
        {
            await Exit();
            await SceneManager.LoadSceneAsync(ConstGame.MainScene);
        }

        private async UniTask ShowInfoPopup()
        {
            await GoTo<InfoStateController>();
        }
    }
}