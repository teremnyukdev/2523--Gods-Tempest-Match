using System.Threading;
using Application.Services.Audio;
using Application.Services.UserData;
using Core;
using Core.Services.Audio;
using Cysharp.Threading.Tasks;
using AudioType = Core.Services.Audio.AudioType;

namespace Application.GameStateMachine
{
    public class AudioSettingsBootstrapController : BaseController
    {
        private readonly IAudioService _audioService;
        private readonly UserDataService _userDataService;

        public AudioSettingsBootstrapController(IAudioService audioService, UserDataService userDataService)
        {
            _audioService = audioService;
            _userDataService = userDataService;
        }

        public override UniTask Run(CancellationToken cancellationToken)
        {
            base.Run(cancellationToken);

            SetVolume();
            _audioService.PlayMusic(ConstAudio.GameMusic, 1, true, true);
            return UniTask.CompletedTask;
        }

        private void SetVolume()
        {
            var musicVolume = _userDataService.GetUserData().SettingsData.MusicVolume;
            
            _audioService.SetVolume(AudioType.Sound, musicVolume);
            _audioService.SetVolume(AudioType.Music, musicVolume);
        }
    }
}