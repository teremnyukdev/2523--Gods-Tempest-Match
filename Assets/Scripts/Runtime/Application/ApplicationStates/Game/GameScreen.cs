using System;
using Application.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Application.ApplicationStates.Game
{
    public class GameScreen : UiScreen
    {
        [SerializeField] private Button _pauseButton;

        public event Action OnPaused;

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(() => OnPaused?.Invoke());
        }

        private void OnDisable()
        {
            _pauseButton.onClick.RemoveListener(() => OnPaused?.Invoke());
        }
    }
}