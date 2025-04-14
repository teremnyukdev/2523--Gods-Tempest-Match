using System;
using System.Collections;
using System.Collections.Generic;
using Application.Services.UserData;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Match3
{
    [DefaultExecutionOrder(-9000)]
    public class UIHandler : MonoBehaviour
    {
        class UIAnimationEntry
        {
            public VisualElement UIElement;
            public Vector3 WorldPosition;
            public Vector3 StartPosition;
            public Vector3 StartToEnd;
            public float Time;

            public AnimationCurve Curve;

            //this is played when the animation reach its end position;
            public AudioClip EndClip;
        }

        class ShopEntry
        {
            public Button BuyButton;
            public ShopSetting.ShopItem LinkedItem;

            public void UpdateButtonState()
            {
                BuyButton.SetEnabled(GameManager.Instance.UserDataService.GetUserData().GameMainData.Coins >= LinkedItem.Price);
            }
        }

        public enum CharacterAnimation
        {
            Match,
            Win,
            LowMove,
            Lose
        }
    
        public static UIHandler Instance { get; protected set; }
    
        public VisualTreeAsset GemGoalTemplate;

        public Sprite CoinSprite;

        public VisualTreeAsset ShopItemEntryTemplate;
        public VisualTreeAsset BonusItemTemplate;
    
        private UIDocument m_Document;

        private VisualElement m_CoverElement;
        private Action m_FadeCallback;

        private VisualElement m_GemGoalContent;
        private Label m_MoveCounter;

        private VisualElement m_BottomBarRoot;

        private VisualElement m_SelectedBonusItem;

        private VisualElement m_EndTitleContent;
        private VisualElement m_WinTitle;
        private VisualElement m_LoseTitle;

        private VisualElement m_EndScreen;

        private int m_WinTriggerID, m_MatchTriggerId, m_LowMoveTriggerId, m_LoseTriggerId;

        private Dictionary<int, Label> m_GoalCountLabelLookup = new();
        private Dictionary<int, VisualElement> m_Checkmarks = new();
        private List<UIAnimationEntry> m_CurrentGemAnimations = new();

        private float m_MatchEffectEndTime = 0.0f;

        private Camera mainCamera;
        
        // Setting Menu
        private VisualElement m_SettingMenuRoot;

        private Slider m_MusicVolumeSlider;
        private Slider m_SFXVolumeSlider;
    
        // End Screen
        private Label m_CoinLabel;
        private Label m_LiveLabel;
        private Label m_StarLabel;
    
        // Shop
        private VisualElement m_ShopRoot;
        private ScrollView m_ShopScrollView;

        private List<ShopEntry> m_ShopEntries = new();
        private readonly List<VisualElement> _buttonsToDisable = new();

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private class DebugGemButton
        {
            public Button Button;
            public Gem Gem;
        }
    
        private VisualElement m_DebugMenuRoot;
        private ScrollView m_DebugGemScrollView;

        private DebugGemButton m_CurrentEnabledDebugButton;

        public bool DebugMenuOpen => m_DebugMenuRoot.style.display == DisplayStyle.Flex;
        public Gem SelectedDebugGem => m_CurrentEnabledDebugButton?.Gem;
    
#endif

        private void OnEnable()
        {
            Instance = this;
            m_Document = GetComponent<UIDocument>();
        }
        
        private void AddButtonToDisable(string buttonName)
        {
            var button = m_Document.rootVisualElement.Q<Button>(buttonName);
            if (button != null)
                _buttonsToDisable.Add(button);
            else
                Debug.LogWarning($"Button '{buttonName}' not found in UI document.");
        }
        
        public void Initialize()
        {
            AddButtonToDisable("ShopButton");
            AddButtonToDisable("SelectLevelButton");
            AddButtonToDisable("ReplayButton");
            AddButtonToDisable("ButtonMenu");
            
            m_WinTriggerID = Animator.StringToHash("Win");
            m_MatchTriggerId = Animator.StringToHash("Match");
            m_LowMoveTriggerId = Animator.StringToHash("LowMove");
            m_LoseTriggerId = Animator.StringToHash("Lose");

            m_Document.rootVisualElement.Q<Label>("LevelName");

            m_GemGoalContent = m_Document.rootVisualElement.Q<VisualElement>("GoalContainer");
            m_MoveCounter = m_Document.rootVisualElement.Q<Label>("MoveCounter");

            m_EndTitleContent = m_Document.rootVisualElement.Q<VisualElement>("EndTitleContent");
            m_WinTitle = m_EndTitleContent.Q<VisualElement>("WinTitle");
            
            m_LoseTitle = m_EndTitleContent.Q<VisualElement>("LoseTitle");
            
            m_EndScreen = m_Document.rootVisualElement.Q<VisualElement>("EndScreen");

            m_Document.rootVisualElement.Q<VisualElement>("MiddleTopSection");

            var playAgainButton = m_Document.rootVisualElement.Q<Button>("ReplayButton");
            playAgainButton.clicked += () =>
            {
                FadeOut(() =>
                {
                    ShowShop(false);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
                });
            };

            var exitButton = m_Document.rootVisualElement.Q<Button>("SelectLevelButton");
            exitButton.clicked += () =>
            {
                FadeOut(() =>
                {
                    ShowShop(false);
                    SceneManager.LoadScene(1, LoadSceneMode.Single); 
                });
            };
            
            m_CoinLabel = m_Document.rootVisualElement.Q<Label>("CoinLabel");
            m_LiveLabel = m_Document.rootVisualElement.Q<Label>("LiveLabel");
            m_StarLabel = m_Document.rootVisualElement.Q<Label>("StarLabel");

            m_BottomBarRoot = m_Document.rootVisualElement.Q<VisualElement>("BoosterZone");
            var openSettingButton = m_BottomBarRoot.parent.Q<Button>("ButtonMenu");
            openSettingButton.clicked += () =>
            {
                ToggleSettingMenu(true);
            };
            
            // Setting Menu

            m_SettingMenuRoot = m_Document.rootVisualElement.Q<VisualElement>("Settings");
            m_SettingMenuRoot.style.display = DisplayStyle.None;

            var returnButton = m_SettingMenuRoot.Q<Button>("ReturnButton");
            returnButton.clicked += () =>
            {
                FadeOut(() =>
                {
                    ToggleSettingMenu(false);
                    SceneManager.LoadScene(1, LoadSceneMode.Single); 
                });
            };

            var closeButton = m_SettingMenuRoot.Q<Button>("CloseButton");
            closeButton.clicked += () =>
            {
                ToggleSettingMenu(false);
            };

            m_SettingMenuRoot.Q<Slider>("MainVolumeSlider");
            m_MusicVolumeSlider = m_SettingMenuRoot.Q<Slider>("MusicVolumeSlider");
            m_SFXVolumeSlider = m_SettingMenuRoot.Q<Slider>("SFXVolumeSlider");

            var soundData = GameManager.Instance.Volumes;
            m_MusicVolumeSlider.value = soundData.MusicVolume;
            m_SFXVolumeSlider.value = soundData.SFXVolume;


            m_MusicVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                soundData.MusicVolume = evt.newValue;
                GameManager.Instance.UpdateVolumes();
            });
            
            m_SFXVolumeSlider.RegisterValueChangedCallback(evt =>
            {
                soundData.SFXVolume = evt.newValue;
                GameManager.Instance.UpdateVolumes();
            });

            // Shop
        
            var shopButton = m_Document.rootVisualElement.Q<Button>("ShopButton");
            shopButton.clicked += () =>
            {
                ShowShop(true);
                UpdateShopEntry();
            };

            m_ShopRoot = m_Document.rootVisualElement.Q<VisualElement>("Shop");
            m_ShopScrollView = m_Document.rootVisualElement.Q<ScrollView>("ShopContentScroll");

            foreach (var shopItem in GameManager.Instance.Settings.ShopSettings.Items)
            {
                var newElem = ShopItemEntryTemplate.Instantiate();
                var itemIcon = newElem.Q<VisualElement>("ItemIcon");
                var itemName = newElem.Q<Label>("ItemName");
                var itemPrice = newElem.Q<Label>("ItemPrice");

                itemIcon.style.backgroundImage = new StyleBackground(shopItem.ItemSprite);
                itemName.text = shopItem.ItemName;
                itemPrice.text = shopItem.Price.ToString();
         
                var newShopEntry = new ShopEntry();
            
                newShopEntry.BuyButton = newElem.Q<Button>("BuyButton");
                newShopEntry.LinkedItem = shopItem;
                newShopEntry.UpdateButtonState();
                newShopEntry.BuyButton.clicked += () =>
                {
                    newShopEntry.LinkedItem.Buy();
                    GameManager.Instance.ChangeCoins(-newShopEntry.LinkedItem.Price);
                
                    UpdateTopBarData();
                    UpdateShopEntry();
                };
              
                m_ShopEntries.Add(newShopEntry);
                m_ShopScrollView.Add(newElem);
            }

            var exitShop = m_ShopRoot.Q<Button>("ShopExitButton");
            exitShop.clicked += () =>
            {
                ShowShop(false);
            };
            
            var curve = GameManager.Instance.Settings.VisualSettings.MatchFlyCurve;
            m_MatchEffectEndTime = curve.keys[curve.keys.Length-1].time;

            m_CoverElement = m_Document.rootVisualElement.Q<VisualElement>("Cover");
            m_CoverElement.style.opacity = 1.0f;
            m_CoverElement.RegisterCallback<TransitionEndEvent>(evt =>
            {
                ShowShop(false);
                m_FadeCallback?.Invoke();
                m_FadeCallback = null;
            });
            
            CreateBottomBar();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            m_DebugMenuRoot = new VisualElement();
            m_DebugMenuRoot.name = "DebugRoot";
        
            m_Document.rootVisualElement.Add(m_DebugMenuRoot);

            m_DebugMenuRoot.style.position = Position.Absolute;
            m_DebugMenuRoot.style.top = Length.Percent(85);
            m_DebugMenuRoot.style.left = 0;
            m_DebugMenuRoot.style.right = 0;
            m_DebugMenuRoot.style.bottom = 0;
            m_DebugMenuRoot.style.backgroundColor = Color.black;

            m_DebugGemScrollView = new ScrollView();
            m_DebugGemScrollView.mode = ScrollViewMode.Horizontal;
            m_DebugGemScrollView.style.flexDirection = FlexDirection.Row;
        
            m_DebugMenuRoot.Add(m_DebugGemScrollView);
        
            ToggleDebugMenu();
            
#endif
            

            
            ApplySafeArea(m_Document.rootVisualElement.Q<VisualElement>("FullContent"));
            ApplySafeArea(m_EndScreen);
        }
    
        public void Init()
        {
            ProcessLose();

            m_WinTitle.style.scale = Vector2.zero;
            m_LoseTitle.style.scale = Vector2.zero;

            m_EndTitleContent.style.display = DisplayStyle.None;
            m_EndScreen.style.display = DisplayStyle.None;
            
            //we clear the goal container as when we reload a level, there 
            m_GemGoalContent.Clear();
            foreach (var goal in LevelData.Instance.Goals)
            {
                var newInstance = GemGoalTemplate.Instantiate();
                m_GemGoalContent.Add(newInstance);

                var label = newInstance.Q<Label>("GemGoalCount");
                label.text = goal.Count.ToString();

                var checkmark = newInstance.Q<VisualElement>("Checkmark");
                checkmark.style.display = DisplayStyle.None;

                var background = newInstance.Q<VisualElement>("GoalGemTemplate");
                background.style.backgroundImage =
                    new StyleBackground(goal.Gem.UISprite);

                m_GoalCountLabelLookup[goal.Gem.GemType] = label;
                m_Checkmarks[goal.Gem.GemType] = checkmark;
            }

            LevelData.Instance.OnGoalChanged += (type, amount) =>
            {
                if (amount == 0)
                {
                    m_GoalCountLabelLookup[type].style.display = DisplayStyle.None;
                    m_Checkmarks[type].style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_GoalCountLabelLookup[type].style.display = DisplayStyle.Flex;
                    m_Checkmarks[type].style.display = DisplayStyle.None;
                    
                    m_GoalCountLabelLookup[type].text = amount.ToString();
                }
            };

            m_MoveCounter.text = LevelData.Instance.RemainingMove.ToString();
            LevelData.Instance.OnMoveHappened += remaining =>
            {
                m_MoveCounter.text = remaining.ToString();
            };

           
            mainCamera = Camera.main;

            m_ShopRoot.style.display = DisplayStyle.None;
        }

        public void Display(bool displayed)
        {
            if (m_Document == null)
                m_Document = GetComponent<UIDocument>();
             
            m_Document.rootVisualElement.style.display = displayed ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        public static void ApplySafeArea(VisualElement root)
        {
            Rect safeArea = Screen.safeArea;

            // Calculate borders based on safe area rect
            var left = safeArea.x;
            var right = Screen.width - safeArea.xMax;
            var top = Screen.height - safeArea.yMax;
            var bottom = safeArea.y;
            
            // Set border widths regardless of orientation
            root.style.top = top;
            root.style.bottom = bottom;
            root.style.left = left;
            root.style.right = right;
        }

        public void ShowEnd()
        {
            UpdateTopBarData();
            
            if (LevelData.Instance.GoalLeft == 0)
            {
                ShowWin();
            }
            else
            {
                ShowLose();
            }
        }

        void ShowWin()
        {
            GameManager.Instance.WinTriggered();
            
            m_EndTitleContent.style.display = DisplayStyle.Flex;
            m_LoseTitle.style.display = DisplayStyle.None;
            m_WinTitle.style.display = DisplayStyle.Flex;
            m_WinTitle.style.scale = Vector2.one;

            StartCoroutine(ShowEndControl(GameManager.Instance.Settings.SoundSettings.WinSound));
        }

        void ShowLose()
        {
            GameManager.Instance.LooseTriggered();
            
            m_EndTitleContent.style.display = DisplayStyle.Flex;
            m_WinTitle.style.display = DisplayStyle.None;

            if(GameManager.Instance.UserDataService.GetUserData().GameMainData.Lives > 0)
            {
                m_LoseTitle.style.display = DisplayStyle.Flex;
                m_LoseTitle.style.scale = Vector2.one;
            }
            
            StartCoroutine(ShowEndControl(GameManager.Instance.Settings.SoundSettings.LooseSound));
        }

        IEnumerator ShowEndControl(AudioClip clip)
        {
            if(GameManager.Instance.UserDataService.GetUserData().GameMainData.Lives <= 0)
            {
                foreach (var button in _buttonsToDisable)
                    button?.SetEnabled(false);
            }

            yield return new WaitForSeconds(3.0f);
            
            GameManager.Instance.PlaySFX(clip);
            
            UpdateTopBarData();
            
            if(GameManager.Instance.UserDataService.GetUserData().GameMainData.Lives <= 0)
                ProcessLose(true);
            
            else
                m_EndScreen.style.display = DisplayStyle.Flex;
        }

        private static void ProcessLose(bool goToGameOver = false)
        {
            if(GameManager.Instance.UserDataService.GetUserData().GameMainData.Lives <= 0)
            {
                GameManager.Instance.ProcessLose();
                
                if(goToGameOver)
                    GameManager.Instance.GoToGameOverState();
            }
        }

        public void ToggleSettingMenu(bool display)
        {
            m_SettingMenuRoot.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
            GameManager.Instance.Board.ToggleInput(!display);

            if (!display)
            {
                GameManager.Instance.SaveSoundData();
            }
        }

        public void FadeIn(Action onFadeFinished)
        {
            foreach (var button in _buttonsToDisable)
                button?.SetEnabled(true);
            
            m_CoverElement.style.opacity = 0.0f;
            m_FadeCallback += onFadeFinished;
        }

        public void FadeOut(Action onFadeFinished)
        {
            foreach (var button in _buttonsToDisable)
                button?.SetEnabled(false);
            
            m_CoverElement.style.opacity = 1.0f;
            m_FadeCallback += onFadeFinished;
            
            ShowShop(false);
        }

        public void AddMatchEffect(Gem gem)
        {
            var elem = new Image();
        
            m_Document.rootVisualElement.Add(elem);
        
            elem.style.position = Position.Absolute;
        
            elem.sprite = gem.UISprite;

            var worldPosition = gem.transform.position;
            var pos = RuntimePanelUtils.CameraTransformWorldToPanel(m_Document.rootVisualElement.panel, 
                worldPosition,
                mainCamera);

            var label = m_GoalCountLabelLookup[gem.GemType];
            var target = (Vector2)label.LocalToWorld(label.transform.position);

            elem.style.left = pos.x;
            elem.style.top = pos.y;
            elem.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
        
            m_CurrentGemAnimations.Add(new UIAnimationEntry()
            {
                Time = 0.0f,
                WorldPosition = worldPosition,
                StartPosition = pos,
                StartToEnd = target - pos,
                UIElement = elem,
                Curve = null
            });
        }

        public void AddCoin(Vector3 startPoint)
        {
            var elem = new Image();
        
            m_Document.rootVisualElement.Add(elem);
        
            elem.style.position = Position.Absolute;
            elem.sprite = CoinSprite;
        
            var pos = RuntimePanelUtils.CameraTransformWorldToPanel(m_Document.rootVisualElement.panel, 
                startPoint,
                mainCamera);

            elem.style.left = pos.x;
            elem.style.top = pos.y;
            elem.style.translate = new Translate(Length.Percent(-50), Length.Percent(-50));
        
            m_CurrentGemAnimations.Add(new UIAnimationEntry()
            {
                Time = 0.0f,
                WorldPosition = startPoint,
                StartPosition = pos,
                StartToEnd = pos,
                UIElement = elem,
                EndClip = GameManager.Instance.Settings.SoundSettings.CoinSound,
                Curve = GameManager.Instance.Settings.VisualSettings.CoinFlyCurve
            });
        }

        private void ShowShop(bool opened)
        {
            m_ShopRoot.style.display = opened ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Update()
        {
            var matchCurve = GameManager.Instance.Settings.VisualSettings.MatchFlyCurve;
        
            for (int i = 0; i < m_CurrentGemAnimations.Count; ++i)
            {
                var anim = m_CurrentGemAnimations[i];

                anim.Time += Time.deltaTime;
                
                Vector3 panelVector = Vector3.zero;
                if (anim.Curve != null)
                {
                    var startToEnd = (Vector3.up * 20) - anim.WorldPosition;
                    Vector3 perpendicular;
                    var angle = Vector3.SignedAngle(Vector3.up, startToEnd, Vector3.forward);
                    if (angle < 0)
                        perpendicular = (Quaternion.AngleAxis(-angle, Vector3.forward) * Vector3.left).normalized;
                    else
                        perpendicular = (Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right).normalized;

                    float angleAmount = Mathf.Clamp01(Mathf.Abs(angle) / 10.0f);

                    float amount = anim.Curve.Evaluate(anim.Time) * angleAmount;
                    perpendicular *= amount;
                    
                    //we need the length of that vector in the panel space, so we add this perpendicular to the world start
                    //point then transform the point into the panel
                    var worldPos = anim.WorldPosition + perpendicular;
                    var panelPos = (Vector3)RuntimePanelUtils.CameraTransformWorldToPanel(m_Document.rootVisualElement.panel, worldPos,
                        mainCamera);

                    panelVector = panelPos - anim.StartPosition;
                }

                //var newPos = Vector2.Lerp(anim.StartPosition, anim.EndPosition, anim.Time);
                var newPos = anim.StartPosition + anim.StartToEnd * matchCurve.Evaluate(anim.Time) + panelVector;

                if (anim.Time >= m_MatchEffectEndTime)
                {
                    anim.UIElement.RemoveFromHierarchy();
                    m_CurrentGemAnimations.RemoveAt(i);
                    i--;

                    if(anim.EndClip != null)
                        GameManager.Instance.PlaySFX(anim.EndClip);
                }
                else
                {
                    anim.UIElement.style.left = newPos.x;
                    anim.UIElement.style.top = newPos.y;
                }
            }
        }

        public void UpdateTopBarData()
        {
            m_CoinLabel.text = GameManager.Instance.UserDataService.GetUserData().GameMainData.Coins.ToString();
            m_LiveLabel.text = GameManager.Instance.UserDataService.GetUserData().GameMainData.Lives.ToString();
            m_StarLabel.text = GameManager.Instance.UserDataService.GetUserData().GameMainData.Stars.ToString();
        }

        public void CreateBottomBar()
        {
            int currentBonus = 0;
            foreach (var child in m_BottomBarRoot.Children())
            {
                var icon = child.Q<VisualElement>("ImageBooster");
                var bonusButton = child.Q<Button>("ButtonBooster");
                
                if (currentBonus < GameManager.Instance.BonusItemEntryDatas.Count)
                {
                    var item = GameManager.Instance.BonusItemEntryDatas[currentBonus];
                    
                    icon.style.display = DisplayStyle.Flex;
                    icon.style.backgroundImage = Background.FromSprite(item.Item.DisplaySprite);
                    
                    bonusButton.clicked += () =>
                    {
                        var currentSelected = m_SelectedBonusItem;
                        DeselectBonusItem();

                        //clicking back on an already selected item just deselect it
                        if (currentSelected == child)
                        {
                            GameManager.Instance.ActivateBonusItem(null);
                            return;
                        }
                        
                        m_SelectedBonusItem = child;
                        m_SelectedBonusItem.AddToClassList("selected");

                        GameManager.Instance.ActivateBonusItem(item.Item);
                    };
                }
                else
                {
                    icon.style.display = DisplayStyle.None;
                }

                currentBonus++;
            }
            
            UpdateBottomBar();
        }

        public void UpdateBottomBar()
        {
            int currentBonus = 0;
            foreach (var child in m_BottomBarRoot.Children())
            {
                if (child != null)
                {
                    var count = child.Q<Label>("LabelBoosterNumber");
                    var bonusButton = child.Q<Button>("ButtonBooster");

                    if (currentBonus < GameManager.Instance.UserDataService.GetUserData().BonusItemEntryDatas.Count)
                    {
                        var item = GameManager.Instance.UserDataService.GetUserData().BonusItemEntryDatas[currentBonus];
                        count.text = item.Amount.ToString();
                 
                        bonusButton.SetEnabled(item.Amount != 0);
                    }
                }

                currentBonus += 1;
            }
        }

        public void DeselectBonusItem()
        {
            if (m_SelectedBonusItem == null) return;
        
            GameManager.Instance.ActivateBonusItem(null);
            m_SelectedBonusItem.RemoveFromClassList("selected");
            m_SelectedBonusItem = null;
        }

        public void UpdateShopEntry()
        {
            foreach (var shopEntry in m_ShopEntries)
            {
                shopEntry.UpdateButtonState();
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD

        public void RegisterGemToDebug(Gem gem)
        {
            var button = new Button();

            button.clicked += () =>
            {
                if (m_CurrentEnabledDebugButton != null)
                {
                    m_CurrentEnabledDebugButton.Button.SetEnabled(true);
                }

                m_CurrentEnabledDebugButton = button.userData as DebugGemButton;
                button.SetEnabled(false);
            };

            button.userData = new DebugGemButton()
            {
                Button = button,
                Gem = gem
            };

            button.style.width = 100;

            var icone = new Image();
            icone.sprite = gem.GetComponentInChildren<SpriteRenderer>().sprite;
            icone.style.width = Length.Percent(100);
            icone.style.height = Length.Percent(100);
        
            button.Add(icone);
        
            m_DebugGemScrollView.Add(button);
        }

        public void ToggleDebugMenu()
        {
            if (m_DebugMenuRoot.style.display == DisplayStyle.None)
                m_DebugMenuRoot.style.display = DisplayStyle.Flex;
            else
                m_DebugMenuRoot.style.display = DisplayStyle.None;
        }
#endif
    }
}