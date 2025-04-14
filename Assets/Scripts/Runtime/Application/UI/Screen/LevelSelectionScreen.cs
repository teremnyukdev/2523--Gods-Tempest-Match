using System;
using TMPro;
using UnityEngine;

namespace Application.UI
{
    public class LevelSelectionScreen : UiScreen
    {
        [SerializeField] private SimpleButton _settingsButton;
        [SerializeField] private SimpleButton _goBackButton;
        [SerializeField] private SimpleButton _playButton;
        [SerializeField] private LevelSelectionButton[] _levelSelectionButtonArray;
        [SerializeField] private Sprite _lockedSprite;

        private int _selectedLevel = 1;

        public event Action OnSettingsPressedEvent;
        public event Action OnLeavePressedEvent;
        public event Action<int> OnStartLevelPressedEvent;

        public void Initialize(int unlockedLevels)
        {
            _settingsButton.Button.onClick.AddListener(() => OnSettingsPressedEvent?.Invoke());
            _goBackButton.Button.onClick.AddListener(() => OnLeavePressedEvent?.Invoke());

            int lastUnlockedLevelID = unlockedLevels - 1;

            _selectedLevel = unlockedLevels;

            for (int i = 0; i < _levelSelectionButtonArray.Length; i++)
            {
                if(i == lastUnlockedLevelID)
                    _levelSelectionButtonArray[i].UpdateSelection(true);

                if (i > lastUnlockedLevelID)
                {
                    _levelSelectionButtonArray[i].SetLocked(_lockedSprite);
                }

                _levelSelectionButtonArray[i].OnLevelSelected += UpdateSelectedLevel;
            }
            _playButton.Button.onClick.AddListener(() => OnStartLevelPressedEvent?.Invoke(_selectedLevel));
        }

        private void UpdateSelectedLevel(int level)
        {
            if (_selectedLevel == level)
                return;

            _levelSelectionButtonArray[_selectedLevel - 1].UpdateSelection(false);
            _selectedLevel = level;
            _levelSelectionButtonArray[_selectedLevel - 1].UpdateSelection(true);
        }

        private void OnDestroy()
        {
            _settingsButton.Button.onClick.RemoveAllListeners();
            _goBackButton.Button.onClick.RemoveAllListeners();
            _playButton.Button.onClick.RemoveAllListeners();

            for (int i = 0; i < _levelSelectionButtonArray.Length; i++)
            {
                _levelSelectionButtonArray[i].OnLevelSelected -= UpdateSelectedLevel;
            }
        }
    }
}