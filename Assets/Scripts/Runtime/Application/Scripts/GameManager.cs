using System;
using System.Collections;
using System.Collections.Generic;
using Application.Game;
using Application.Services.UserData;
using Core.Services.Audio;
using Core.StateMachine;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;
using AudioType = Core.Services.Audio.AudioType;

namespace Match3
{
    /// <summary>
    /// The GameManager is the interface between all the system in the game. It is either instantiated by the Loading scene
    /// which is the first at the start of the game, or Loaded from the Resource Folder dynamically at first access in editor
    /// so we can press play from any point of the game without having to add it to every scene and test if it already exist
    /// </summary>
    [DefaultExecutionOrder(-9999)]
    [Serializable]
    public class GameManager : MonoBehaviour
    {
        //This is set to true when the manager is deleted. This is useful as the manager can be deleted before other
        //objects 
        private static bool s_IsShuttingDown = false;
        
        public static GameManager Instance
        {
            get
            {
                if (s_Instance == null && !s_IsShuttingDown)
                {
                    var newInstance = Instantiate(Resources.Load<GameManager>(nameof(GameManager)));
                    newInstance.OnEnable();
                    DontDestroyOnLoad(newInstance);
                }
                
                return s_Instance;
            }

            private set => s_Instance = value;
        }

        public static bool IsShuttingDown()
        {
            return s_IsShuttingDown;
        }

        [Serializable]
        public class SoundData
        {
            public float MusicVolume = 1.0f;
            public float SFXVolume = 1.0f;
        }


    
        private static GameManager s_Instance;

        public Board Board;
        public GameSettings Settings;

        public SoundData Volumes => m_SoundData;

        public VFXPoolSystem PoolSystem { get; private set; } = new();

        private Queue<AudioSource> m_SFXSourceQueue = new();

        private GameObject m_BonusModePrefab;
    
        private VisualEffect m_WinEffect;
        private VisualEffect m_LoseEffect;
        
        private SoundData m_SoundData = new();
        public UserDataService UserDataService;
        private bool _isInitialized;

        public List<BonusItemEntryData> BonusItemEntryDatas;
        private IAudioService _audioService;
        private StateMachine _stateMachine;

        public void Initialize(UserDataService userDataService, IAudioService audioService, StateMachine stateMachine)
        {
            if(_isInitialized)
                return;

            _stateMachine = stateMachine;
            _isInitialized = true;
            
            _audioService = audioService;
            UserDataService = userDataService;

            if(UserDataService.GetUserData().IsDoneBonusItems == false)
            {
                UserDataService.GetUserData().BonusItemEntryDatas.Clear();
                foreach (var bonusItemEntryData in BonusItemEntryDatas)
                {
                    UserDataService.GetUserData().BonusItemEntryDatas.Add(bonusItemEntryData);
                }

                UserDataService.GetUserData().IsDoneBonusItems = true;
            }

            Volumes.MusicVolume = UserDataService.GetUserData().SettingsData.MusicVolume;
            
            UIHandler.Instance.Initialize();
        }

        private void OnEnable()
        {
            if (s_Instance == this)
            {
                return;
            }

            if (s_Instance == null)
            {
                s_Instance = this;
                DontDestroyOnLoad(gameObject);
                
                UnityEngine.Application.targetFrameRate = 60;
            
                for (int i = 0; i < 16; ++i)
                {
                    var sourceInst = Instantiate(Settings.SoundSettings.SFXSourcePrefab, transform);
                    m_SFXSourceQueue.Enqueue(sourceInst);
                }

                if (Settings.VisualSettings.BonusModePrefab != null)
                {
                    m_BonusModePrefab = Instantiate(Settings.VisualSettings.BonusModePrefab);
                    m_BonusModePrefab.SetActive(false);
                }

                m_WinEffect = Instantiate(Settings.VisualSettings.WinEffect, transform);
                m_LoseEffect = Instantiate(Settings.VisualSettings.LoseEffect, transform);

                LoadSoundData();
            }
            
        }


        private void OnDestroy()
        {
            if (s_Instance == this) s_IsShuttingDown = true;
        }

        void GetReferences()
        {
            Board = FindFirstObjectByType<Board>();
        }

        /// <summary>
        /// Called by the LevelData when it awake, notify the GameManager we started a new level.
        /// </summary>
        public void StartLevel()
        {
            GetReferences();
            UIHandler.Instance.Display(true);
            
            m_WinEffect.gameObject.SetActive(false);
            m_LoseEffect.gameObject.SetActive(false);
            
            LevelData.Instance.OnAllGoalFinished += () =>
            {
                Instance.Board.ToggleInput(false);
                Instance.Board.TriggerFinalStretch();
            };

            LevelData.Instance.OnNoMoveLeft += () =>
            {
                Instance.Board.ToggleInput(false);
                Instance.Board.TriggerFinalStretch();
            };

            PoolSystem.AddNewInstance(Settings.VisualSettings.CoinVFX, 12);

            //we delay the board init to leave enough time for all the tile to init
            StartCoroutine(DelayedInit());
        }

        IEnumerator DelayedInit()
        {
            yield return null;

            Board.Init();
            ComputeCamera();
        }

        public void ComputeCamera()
        {
            //setup the camera so it look at the center of the play area, and change its ortho setting so it perfectly frame
            var bounds = Board.Bounds;
            Vector3 center = Board.Grid.CellToLocalInterpolated(bounds.center) + new Vector3(0.5f, 0.5f, 0.0f);
            center = Board.transform.TransformPoint(center);
            
            //we offset of 1 up as the top bar is thicker, so this center it better between the top & bottom bar
            Camera.main.transform.position = center + Vector3.back * 10.0f + Vector3.up * 0.75f;

            float halfSize = 0.0f;
            
            if (Screen.height > Screen.width)
            {
                float screenRatio = Screen.height / (float)Screen.width;
                halfSize = ((bounds.size.x + 1) * 0.5f + LevelData.Instance.BorderMargin) * screenRatio;
            }
            else
            {
                //On Wide screen, we fit vertically
                halfSize = (bounds.size.y + 3) * 0.5f + LevelData.Instance.BorderMargin;
            }

            halfSize += LevelData.Instance.BorderMargin;
        
            Camera.main.orthographicSize = halfSize;
        }

        /// <summary>
        /// Called by the Main Menu when it open, notify the GameManager we are back in the menu so need to hide the Game UI.
        /// </summary>
        public void MainMenuOpened()
        {
            PoolSystem.Clean();
            m_WinEffect.gameObject.SetActive(false);
            m_LoseEffect.gameObject.SetActive(false);
            
            UIHandler.Instance.Display(false);
        }

        public void ChangeCoins(int amount)
        {
            UserDataService.GetUserData().GameMainData.AddCoins(amount);
            
            if (UserDataService.GetUserData().GameMainData.Coins < 0)
                UserDataService.GetUserData().GameMainData.ResetCoins();

            UIHandler.Instance.UpdateTopBarData();
        }

        public void WinStar()
        {
            UserDataService.GetUserData().GameMainData.AddStars(1);
        }

        public void AddLive(int amount)
        {
            UserDataService.GetUserData().GameMainData.AddLives(amount);
        }

        public void LoseLife() =>
                UserDataService.GetUserData().GameMainData.RemoveLife();

        public void UpdateVolumes()
        {
            Settings.SoundSettings.Mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(0.0001f, m_SoundData.SFXVolume)) * 30.0f);
            _audioService?.SetVolume(AudioType.Music, m_SoundData.MusicVolume);
            if (UserDataService != null)
            {
                UserDataService.GetUserData().SettingsData.MusicVolume = m_SoundData.MusicVolume;
                UserDataService.SaveUserData();
            }
        }

        public void SaveSoundData()
        {
            System.IO.File.WriteAllText(UnityEngine.Application.persistentDataPath + "/sounds.json", JsonUtility.ToJson
                (m_SoundData));
        }

        void LoadSoundData()
        {
            if (System.IO.File.Exists(UnityEngine.Application.persistentDataPath + "/sounds.json"))
            {
                JsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(UnityEngine.Application.persistentDataPath+"/sounds.json"), m_SoundData);
            }
            
            UpdateVolumes();
        }

        public void AddBonusItem(BonusItem item)
        {
            var existingItem = UserDataService.GetUserData().BonusItemEntryDatas.Find(entry => entry.Item == item);

            if (existingItem != null)
            {
                existingItem.Amount += 1;
            }
            else
            {
                UserDataService.GetUserData().BonusItemEntryDatas.Add(new BonusItemEntryData
                {
                    Amount = 1,
                    Item = item
                });
            }

            UIHandler.Instance.UpdateBottomBar();
        }

        public void ActivateBonusItem(BonusItem item)
        {
            LevelData.Instance.DarkenBackground(item != null);
            m_BonusModePrefab?.SetActive(item != null);
            Board.ActivateBonusItem(item);
        }

        public void UseBonusItem(BonusItem item, Vector3Int cell)
        {
            item.Use(cell);
            UserDataService.GetUserData().RemoveItem(item);
            m_BonusModePrefab?.SetActive(false);
            UIHandler.Instance.UpdateBottomBar();
            UIHandler.Instance.DeselectBonusItem();
        }

        public AudioSource PlaySFX(AudioClip clip)
        {
            var source = m_SFXSourceQueue.Dequeue();
            m_SFXSourceQueue.Enqueue(source);

            source.clip = clip;
            source.Play();

            return source;
        }

        public void WinTriggered()
        {
            m_WinEffect.gameObject.SetActive(true);
        }

        public void LooseTriggered()
        {
            LoseLife();
            
            m_LoseEffect.gameObject.SetActive(true);
        }

        public void ProcessLose()
        {
            UserDataService.GetUserData().GameMainData.ResetCoins();
            UserDataService.GetUserData().GameMainData.ResetStars();
            
            UIHandler.Instance.UpdateTopBarData();
        }
        
        public void GoToGameOverState() =>
                _stateMachine.GoTo<GameOverState>().Forget();
    }
}