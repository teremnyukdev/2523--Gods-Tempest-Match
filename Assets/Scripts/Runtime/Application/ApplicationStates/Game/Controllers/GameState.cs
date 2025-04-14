using System.Threading;
using Application.Game;
using Application.Services.UserData;
using Core.Services.Audio;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using Match3;
using ILogger = Core.ILogger;

namespace Application.GameStateMachine
{
    public class GameState : StateController
    {
        private readonly StateMachine _stateMachine;

        private readonly UserDataStateChangeController _userDataStateChangeController;
        private readonly TitleStateController _titleStateController;
        private readonly InfoStateController _infoStateController;
        private readonly IAudioService _audioService;
        private readonly GameOverState _gameOverState;
        private readonly UserDataService _userDataService;

        public GameState(ILogger logger, TitleStateController titleStateController, StateMachine stateMachine,
            UserDataStateChangeController userDataStateChangeController,
            InfoStateController infoStateController, UserDataService userDataService,
            IAudioService audioService, GameOverState gameOverState) : base(logger)
        {
            _userDataService = userDataService;
            _audioService = audioService;
            _gameOverState = gameOverState;
            _infoStateController = infoStateController;
            _titleStateController = titleStateController;
            _stateMachine = stateMachine;
            _userDataStateChangeController = userDataStateChangeController;
        }

        public override async UniTask Enter(CancellationToken cancellationToken = default)
        {
            UpdateSession();
            
            await _userDataStateChangeController.Run(CancellationToken.None);

            _stateMachine.Initialize(_titleStateController, _infoStateController, _gameOverState);
            GameManager.Instance.Initialize(_userDataService, _audioService, _stateMachine);

            _stateMachine.GoTo<TitleStateController>(cancellationToken).Forget();
        }

        private void UpdateSession()
        {
            _userDataService.GetUserData().GameData.SessionNumber++;
            _userDataService.SaveUserData();
        }
    }
}