using System.Threading;
using Application.Services.UserData;
using Application.UI;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Match3;
using UnityEngine.SceneManagement;
using ILogger = Core.ILogger;

namespace Application.Game
{
    public class GameOverState : StateController
    {
        private readonly IUiService _uiService;
        private readonly UserDataService _userDataService;

        private SimpleDecisionPopup _gameOverPopup;

        public GameOverState(ILogger logger, IUiService uiService, UserDataService userDataService) : base(logger)
        {
            _uiService = uiService;
            _userDataService = userDataService;
        }

        public override UniTask Enter(CancellationToken cancellationToken = default)
        {
            _gameOverPopup = _uiService.GetPopup<SimpleDecisionPopup>(ConstPopups.GameOverPopup);

            var data = new SimpleDecisionPopupData { PressOkEvent = ResumeGame };

            _gameOverPopup.Show(data, cancellationToken);
            
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            if(_gameOverPopup)
                _gameOverPopup.DestroyPopup();

            return base.Exit();
        }
        
        private void ResumeGame()
        {
            _userDataService.GetUserData().GameMainData.Lives = 5;
            UIHandler.Instance.FadeOut(() => SceneManager.LoadScene(0));
        }
    }
}