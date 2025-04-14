using System.Threading;
using Application.UI;
using Core;
using Core.StateMachine;
using Cysharp.Threading.Tasks;

namespace Application.Game
{
    public class InfoStateController : StateController
    {
        private readonly IUiService _uiService;
        
        private InfoPopup _infoPopup;

        public InfoStateController(ILogger logger, IUiService uiService) : base(logger)
        {
            _uiService = uiService;
        }

        public override UniTask Enter(CancellationToken cancellationToken = default)
        {
            var infoPopup = _uiService.GetPopup<InfoPopup>(ConstPopups.InfoPopup);

            _infoPopup = infoPopup;
            _infoPopup.Show(new InfoPopupData(), cancellationToken);

            _infoPopup.DestroyPopupEvent += GoToTitleScreen;
            return UniTask.CompletedTask;
        }

        public override UniTask Exit()
        {
            if (_infoPopup)
                _infoPopup.DestroyPopupEvent -= GoToTitleScreen;

            return base.Exit();
        }

        private void GoToTitleScreen()
        {
            _ = GoTo<TitleStateController>(); 
        }
    }
}