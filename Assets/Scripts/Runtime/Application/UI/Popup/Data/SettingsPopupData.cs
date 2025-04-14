using Core.UI;

namespace Application.UI
{
    public class SettingsPopupData : BasePopupData
    {
        public bool IsSoundVolume { get; }

        public bool IsMusicVolume { get; }

        public SettingsPopupData(bool isSoundVolume, bool isMusicVolume)
        {
            IsSoundVolume = isSoundVolume;
            IsMusicVolume = isMusicVolume;
        }
    }
}