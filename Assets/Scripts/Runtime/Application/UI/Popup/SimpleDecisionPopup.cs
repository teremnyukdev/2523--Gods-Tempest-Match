using System;
using System.Threading;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Application.UI
{
    public class SimpleDecisionPopup : BasePopup
    {
        [SerializeField] private SimpleButton _confirmButton;

        public override UniTask Show(BasePopupData data, CancellationToken cancellationToken = default)
        {
            var simpleDecisionPopupData = data as SimpleDecisionPopupData;

            SubscribeButton(_confirmButton.Button, simpleDecisionPopupData.PressOkEvent);

            return base.Show(data, cancellationToken);
        }

        private void SubscribeButton(Button button, Action action) =>
                button.onClick.AddListener(() =>
                {
                    action?.Invoke();
                    Hide();
                });
    }
}