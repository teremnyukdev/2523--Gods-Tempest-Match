using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Application.UI
{
    public class TitleScreen : UiScreen
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _infoButton;

        public event Action PlayButtonPressEvent;
        public event Action InfoButtonPressEvent;

        private void OnEnable()
        {
            _playButton.onClick.AddListener(() => PlayButtonPressEvent?.Invoke());
            _infoButton.onClick.AddListener(() => InfoButtonPressEvent?.Invoke());
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(() => PlayButtonPressEvent?.Invoke());
            _infoButton.onClick.RemoveListener(() => InfoButtonPressEvent?.Invoke());
        }
    }
}