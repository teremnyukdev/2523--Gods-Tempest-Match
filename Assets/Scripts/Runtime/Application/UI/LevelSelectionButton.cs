using Application.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionButton : SimpleButton
{
    private readonly Color SelectedColor = new Color(0.65f, 0.71f, 1f);
    [SerializeField, Min(1)] private int _level;
    [SerializeField] private Image _image;

    private bool _locked = false;

    public event Action<int> OnLevelSelected;

    private void Awake()
    {
        Button.onClick.AddListener(() =>
        {
            if(!_locked)
                OnLevelSelected?.Invoke(_level);
        });
    }

    private void OnDestroy()
    {
        Button.onClick.RemoveAllListeners();
    }

    public void UpdateSelection(bool selected)
    {
        _image.color = selected ? SelectedColor : Color.white;
    }

    public void SetLocked(Sprite sprite)
    {
        _locked = true;
        _image.sprite = sprite;
    }
}
