﻿using System.Threading;
using Application.Services.ApplicationState;
using Application.Services.UserData;
using Core;
using Cysharp.Threading.Tasks;

namespace Application.GameStateMachine
{
    public class UserDataStateChangeController : BaseController
    {
        private readonly ApplicationStateService _applicationStateService;
        private readonly UserDataService _userDataService;

        public UserDataStateChangeController(ApplicationStateService applicationStateService,
            UserDataService userDataService)
        {
            _applicationStateService = applicationStateService;
            _userDataService = userDataService;
        }

        public override UniTask Run(CancellationToken cancellationToken)
        {
            base.Run(cancellationToken);

            _applicationStateService.Initialize();

            _applicationStateService.ApplicationQuitEvent += OnQuitApplicationHandler;
            _applicationStateService.ApplicationPauseEvent += OnPauseApplicationHandler;

            return UniTask.CompletedTask;
        }

        public override UniTask Stop()
        {
            base.Stop();

            _applicationStateService.ApplicationQuitEvent -= OnQuitApplicationHandler;
            _applicationStateService.ApplicationPauseEvent -= OnPauseApplicationHandler;

            _applicationStateService.Dispose();

            return UniTask.CompletedTask;
        }

        private void OnQuitApplicationHandler()
        {
            _userDataService.GetUserData().GameData.EntriesCount = 0;
            _userDataService.SaveUserData();
        }

        private void OnPauseApplicationHandler(bool isPause)
        {
            if (isPause)
                _userDataService.SaveUserData();
        }
    }
}