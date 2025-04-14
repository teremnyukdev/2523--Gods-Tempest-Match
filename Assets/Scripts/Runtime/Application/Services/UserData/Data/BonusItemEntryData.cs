using System;
using Match3;

namespace Application.Services.UserData
{
    [Serializable]
    public class BonusItemEntryData
    {
        public int Amount;
        public BonusItem Item;
    }
}