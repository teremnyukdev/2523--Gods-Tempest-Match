using System;

namespace Application.Services.UserData
{
    [Serializable]
    public class GameData
    {
        public int SessionNumber = 0;
        public int EntriesCount;
        public bool IsAdb = false;
    }
}