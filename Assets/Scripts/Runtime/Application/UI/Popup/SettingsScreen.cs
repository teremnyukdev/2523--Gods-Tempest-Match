using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Application.UI
{
    public class SettingsScreen : UiScreen
    {
        [SerializeField] private SimpleButton _soundSettingsButton;
        [SerializeField] private SimpleButton _leaveButton;

        public event Action OnSoundSettingsPressedEvent;
        public event Action OnLeavePressedEvent;

        private void OnDestroy()
        {
            _soundSettingsButton.Button.onClick.RemoveAllListeners();
            _leaveButton.Button.onClick.RemoveAllListeners();
        }

        public override UniTask ShowAsync(CancellationToken cancellationToken = default)
        {
            Subscribe();
            
            return base.ShowAsync(cancellationToken);
        }

        private void Subscribe()
        {
            _soundSettingsButton.Button.onClick.AddListener(() => OnSoundSettingsPressedEvent?.Invoke());
            _leaveButton.Button.onClick.AddListener(() => OnLeavePressedEvent?.Invoke());
        }
    }
}