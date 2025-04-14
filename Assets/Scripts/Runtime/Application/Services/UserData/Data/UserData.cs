using System;
using System.Collections.Generic;
using Match3;
using UnityEngine;

namespace Application.Services.UserData
{
    [Serializable]
    public class UserData
    {
        public List<GameSessionData> GameSessionData = new();
        public UserProgressData UserProgressData = new();
        public SettingsData SettingsData = new();
        public GameData GameData = new();
        public GameMainData GameMainData = new();
        public List<BonusItemEntryData> BonusItemEntryDatas = new();
        public bool IsDoneBonusItems;

        public void RemoveItem(BonusItem bonusItem)
        {
            foreach (var bonusItemEntryData in BonusItemEntryDatas)
            {
                if (bonusItemEntryData.Item != bonusItem)
                    continue;

                if (!bonusItemEntryData.Item || !bonusItem)
                {
                    ResetItemCount(bonusItemEntryData);
                    return;
                }

                DeacreaseItemAmount(bonusItemEntryData);

                return;
            }
        }

        private void DeacreaseItemAmount(BonusItemEntryData bonusItemEntryData)
        {
            if (bonusItemEntryData.Amount <= 0)
                ResetItemCount(bonusItemEntryData);

            else
            {
                bonusItemEntryData.Amount -= 1;
            }
        }

        private void ResetItemCount(BonusItemEntryData bonusItemEntryData)
        {
            bonusItemEntryData.Amount = 0;
        }
    }
}