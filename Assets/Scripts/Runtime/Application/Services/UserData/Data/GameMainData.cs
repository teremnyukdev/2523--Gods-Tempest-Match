using System;
using UnityEngine;

namespace Application.Services.UserData
{
    [Serializable]
    public class GameMainData 
    {
        [SerializeField] public int Coins;
        [SerializeField] public int Stars;
        [SerializeField] public int Lives = 5;
        
        public void AddCoins(int coins) =>
                Coins += coins;

        public void AddStars(int stars) =>
                Stars += stars;

        public void AddLives(int lives) =>
                Lives += lives;

        public void RemoveLife()
        { 
            if(Lives > 0) 
                Lives--;
        } 

        public void ResetCoins() =>
                Coins = 0;

        public void ResetStars() =>
                Stars = 0;
    }
}