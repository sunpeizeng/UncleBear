using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DoozyUI;
using System.Collections.Generic;

namespace UncleBear
{
    public class BootStrap : MonoBehaviour
    {
		#if UNITY_EDITOR
        public string mLanguage;
		#endif
        public string mDefaultLanguage = "Chinese";//refer to Unity API "SystemLanguage" for more language options
			
        void Awake()
        {
			#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
			LogUtil.LogNoTag("channel: {0}", SDKBridge.GetChannel());

			//load banner and interstitial
			if (SDKBridge.GetChannel() == "google")
				AdHelper.LoadBanner (AdHelper.BannerType.SmartBanner, AdHelper.BannerPos.Bottom, true);
			else
				AdHelper.LoadBanner (AdHelper.BannerType.Banner, AdHelper.BannerPos.Bottom, true);
			AdHelper.LoadInterstitial ();
			//激励广告初始化
			AdHelper.VungleInit();

			#region IAP
			List<IAPItem> iapItems = new List<IAPItem>();
			IAPItem pizzaIAP = new IAPItem (Consts.ITEM_FINISHED_PIZZA);
            IAPItem farfalleIAP = new IAPItem(Consts.ITEM_FINISHED_FARFALLE);
            IAPItem foodpackageIAP = new IAPItem(Consts.ITEM_FOODPACKAGE);
			IAPItem scoopsIceCreamIAP = new IAPItem(Consts.ITEM_FINISHED_ICECREAM);

			if (SDKBridge.GetChannel() == "google")
			{
				pizzaIAP.GetItemExtension<IStandardIAPBehaviour>().Create(
					new Dictionary<string, string> { 
						{"com.biemore.android.restaurant.pizza", StoreName.GooglePlay}, 
					},
					IAPType.Consumable);

                farfalleIAP.GetItemExtension<IStandardIAPBehaviour>().Create(
                    new Dictionary<string, string> {
                        {"com.biemore.android.restaurant.farfalle", StoreName.GooglePlay},
                    },
                    IAPType.Consumable);

                foodpackageIAP.GetItemExtension<IStandardIAPBehaviour>().Create(
                    new Dictionary<string, string> {
						{"com.biemore.android.restaurant.summerpack", StoreName.GooglePlay},
                    },
                    IAPType.Consumable);

				scoopsIceCreamIAP.GetItemExtension<IStandardIAPBehaviour>().Create(
					new Dictionary<string, string> {
					{"com.biemore.android.restaurant.scoops", StoreName.GooglePlay},
					},
					IAPType.Consumable);
            }
			else if (SDKBridge.GetChannel() == "App Store")
			{
				pizzaIAP.GetItemExtension<IStandardIAPBehaviour>().Create(
					new Dictionary<string, string> {
						{"com.biemore.ios.restaurant", StoreName.AppStore}
					},
					IAPType.NonConsumable);

                farfalleIAP.GetItemExtension<IStandardIAPBehaviour>().Create(
                    new Dictionary<string, string> {
                        {"com.biemore.ios.restaurant.farfalle", StoreName.AppStore}
                    },
                    IAPType.NonConsumable);

                foodpackageIAP.GetItemExtension<IStandardIAPBehaviour>().Create(
                    new Dictionary<string, string> {
						{"com.biemore.ios.restaurant.summerpack", StoreName.AppStore}
                    },
                    IAPType.NonConsumable);

				scoopsIceCreamIAP.GetItemExtension<IStandardIAPBehaviour>().Create(
					new Dictionary<string, string> {
					{"com.biemore.ios.restaurant.scoops", StoreName.AppStore}
					},
					IAPType.NonConsumable);
            }
			pizzaIAP.GetItemExtension<IOtherIAPBehaviour>().Create("披萨", "熊大叔餐厅披萨", 100);
			farfalleIAP.GetItemExtension<IOtherIAPBehaviour>().Create("蝴蝶结面", "熊大叔餐厅蝴蝶结面", 100);
			foodpackageIAP.GetItemExtension<IOtherIAPBehaviour>().Create("夏日缤纷礼包", "熊大叔餐厅夏日缤纷礼包", 100);
			scoopsIceCreamIAP.GetItemExtension<IOtherIAPBehaviour>().Create("三球冰淇淋", "熊大叔餐厅三球冰淇淋", 100);

            iapItems.Add(pizzaIAP);
            iapItems.Add(farfalleIAP);
            iapItems.Add(foodpackageIAP);
			iapItems.Add(scoopsIceCreamIAP);
			
			IAPHelper.InitUnityIAP(iapItems);
			#endregion

			#endif

			#region multiTouch
			Input.multiTouchEnabled = false;
			#endregion

            #region set traget frame rate
            Application.targetFrameRate = 60;
            #endregion

            #region set log filter
            //logs with the tag of "UIPanel" will not be displayed
            //LogUtil.SetFilters(LogMask.ALL, new string[] { "UIPanel" });
            //set log mask to 0 to turn off logging by LogUtil
            LogUtil.SetFilters(LogMask.NONE, null);
            #endregion

            #region set language
            string language = null;
			#if UNITY_EDITOR
            language = mLanguage;
			#else
            LogUtil.LogNoTag("System Language: {0}", Application.systemLanguage);
            language = Application.systemLanguage.ToString();
			if (language == "Chinese")
				language = "ChineseSimplified";
			#endif
            Localization.language = Localization.HasLanguage(language) ? language : mDefaultLanguage;
            Localization.LoadFonts(new string[] { "Arial" }, true);
            #endregion

            #region create ItemManager instance and load data
            ItemManager.GetOrCreateInstance();
            #endregion

            #region init AudioSource pool
            //initialize audio source pool
            AudioSourcePool.GetOrCreateInstance().Preload(5);
            #endregion

            #region init lean touch
            //create lean touch
            GameObject leanTouchObj = new GameObject("LeanTouch");
            leanTouchObj.AddComponent<Lean.Touch.LeanTouch>();
            leanTouchObj.AddComponent<DontDestroyOnLoad>();
            #endregion

            #region init EventCenter
            EventCenter.GetOrCreateInstance();
            #endregion

            InitGame();
        }

        // Use this for initialization
        void Start()
        {
            LevelManager.Instance.ChangeLevel(LevelEnum.Main);
            //SceneManager.LoadScene("Main");
        }

        void InitGame()
        {
            CameraManager.GetOrCreateInstance();
            LevelManager.GetOrCreateInstance();
            GuideManager.GetOrCreateInstance();
            EffectCenter.GetOrCreateInstance();

            #region init 2d UI
            Canvas canvas2dUI = UIManager.GetUiContainer.GetComponent<Canvas>();
            CanvasScaler canvasScaler2dUI = UIManager.GetUiContainer.GetComponent<CanvasScaler>();
            //force UI to scale to match width only, different games may have different settings
            canvasScaler2dUI.matchWidthOrHeight = 0f;
            canvas2dUI.worldCamera.depth = Camera.main.depth + 10;
            #endregion

            //preload BGM
			UIManager.LoadOrGetClip("bgm01");
			UIManager.LoadOrGetClip ("bgm02");

            GameData.Init();
        }
	}
}