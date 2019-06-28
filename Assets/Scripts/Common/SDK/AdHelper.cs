using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using GoogleMobileAds.Api;
using System.Collections.Generic;

public class AdHelper: DoozyUI.Singleton<AdHelper>
{
	public enum AdType
	{
		Banner,
		Interstitial
	}

	public enum BannerType
	{
		Banner = 0,
		MediumRectangle = 1,
		IABBanner = 2,
		Leaderboard = 3,
		SmartBanner = 4,
		Custom,
	}

	public enum BannerPos
	{
		Top = 0,
		Bottom = 1,
		TopLeft = 2,
		TopRight = 3,
		BottomLeft = 4,
		BottomRight = 5,
		Center = 6
	}

	public enum AdShowCode
	{
		NOT_INITIALIZED,
		NOT_LOADED,
		SUCCESS,
	}

	const string BannerId_iOS = "ca-app-pub-8686690381845825/3636726993";
	const string InterstitialId_iOS = "ca-app-pub-8686690381845825/6590193398";

	const string BannerId_Android = "ca-app-pub-8686690381845825/6729794190";
	const string InterstitialId_Android = "ca-app-pub-8686690381845825/9683260597";

	const string VungleId_iOS = "58c612581ef01c0b6e0004f5";
	const string VungleId_Android = "58c6139266916d056a0004ed";

	const int VungleAwardCoinNum = 20;  //激励广告奖励金币数


	static BannerView _bannerView = null;
	static InterstitialAd _interstitial = null;

	static bool _isBannerActive = false;
	static bool _isBannerLoaded = false;

	static private UnityAction<AdType> _onAdLoaded = null;
	static private UnityAction<AdType, string> _onAdFailedToLoad = null;
	static private UnityAction<AdType> _onAdLeavingApplication = null;
	//seems BannerView.OnAdClosed and BannerView.OnAdOpening is N/A
	static private UnityAction<AdType> _onAdClosed = null;
	static private UnityAction<AdType> _onAdOpening = null;

	static private UnityAction<bool, bool, float> _vungleOnFinshed = null;

	static public bool IsBannerActive
	{
		get
		{
			return _isBannerActive;
		}
	}

	static public bool IsBannerLoaded
	{
		get
		{
			return _isBannerLoaded;
		}
	}

	static public bool IsInterstitialLoaded
	{
		get
		{
			return _interstitial == null ? false : _interstitial.IsLoaded ();
		}
	}

	#region google AdMob events
	static public UnityAction<AdType> OnAdLoaded
	{
		get
		{
			return _onAdLoaded;
		}
		set
		{
			_onAdLoaded = value;
		}
	}

	static public UnityAction<AdType, string> OnAdFailedToLoad
	{
		get
		{
			return _onAdFailedToLoad;
		}
		set
		{
			_onAdFailedToLoad = value;
		}
	}

	static public UnityAction<AdType> OnAdLeavingApplication
	{
		get
		{
			return _onAdLeavingApplication;
		}
		set
		{
			_onAdLeavingApplication = value;
		}
	}

	static public UnityAction<AdType> OnAdClosed
	{
		get
		{
			return _onAdClosed;
		}
		set
		{
			_onAdClosed = value;
		}
	}

	static public UnityAction<AdType> OnAdOpening
	{
		get
		{
			return _onAdOpening;
		}
		set
		{
			_onAdOpening = value;
		}
	}
	#endregion

	#region Vungle events
	static public UnityAction<bool, bool, float> OnVungleAdFinished
	{
		get
		{
			return _vungleOnFinshed;
		}
		set
		{
			_vungleOnFinshed = value;
		}
	}
	#endregion

	static public void VungleInit()
	{
		Vungle.init (VungleId_Android, VungleId_iOS, "");
		Vungle.onAdFinishedEvent += OnVungleFinished;
	}

	static public bool IsVungleAdAvailable()
	{
		return Vungle.isAdvertAvailable ();
	}

	static public void SetVungleSoundEnabled(bool isEnabled)
	{
		Vungle.setSoundEnabled (isEnabled);
	}

	static public void PlayVungleAd()
	{
		Dictionary<string, object> options = new Dictionary<string, object> ();
		options ["incentivized"] = false;
		Vungle.playAdWithOptions (options);
	}

	static public void PlayVungleIncentivizedAd(string alertTitle, string alertText, string closeBtnText, string continueBtnText)
	{
		Dictionary<string, object> options = new Dictionary<string, object> ();
		options ["incentivized"] = true;
		options ["alertTitle"] = alertTitle;
		options ["alertText"] = alertText;
		options ["closeText"] = closeBtnText;
		options ["continueText"] = continueBtnText;
		#if UNITY_ANDROID
		options ["orientation"] = true;
		#elif UNITY_IPHONE
		if (Screen.width > Screen.height)
			options["orientation"] = VungleAdOrientation.Landscape;
		else
			options["orientation"] = VungleAdOrientation.Portrait;
		#endif
		Vungle.playAdWithOptions (options);
	}

	static void OnVungleFinished(AdFinishedEventArgs args)
	{
		if (_vungleOnFinshed != null) {
			_vungleOnFinshed.Invoke (args.WasCallToActionClicked, args.IsCompletedView, (float)args.TotalDuration);
		} else {
			//观看次数+1
			PlayerPrefs.SetInt (Consts.SEE_VUNGLE_ADS_COUNT,PlayerPrefs.GetInt (Consts.SEE_VUNGLE_ADS_COUNT) + 1);
			//广告播放完毕，奖励
			GameData.Coins += VungleAwardCoinNum;
			UIPanel panel = UIPanelManager.Instance.GetPanel("UIGameHUD");
			if (panel != null)
				panel.Repaint();
		}
		if (MenuUI.Instance.buyType == Consts.BUY_ITEM_TYPE_FOOD) {
			//购买ui
			UIPanelManager.Instance.ShowPanel ("UIUnlockFood");
		} else 
		{
			UIPanelManager.Instance.ShowPanel ("UIUnlockPackage");
		}

	}
		
	static public void LoadBanner(bool hideWhenLoaded = false)
	{
		LoadBanner (BannerType.Banner, 0, 0, BannerPos.Top, hideWhenLoaded);	
	}

	static public void LoadBanner(BannerType type, BannerPos bannerPos, bool hideWhenLoaded = false)
	{
		LoadBanner (type, 0, 0, bannerPos, hideWhenLoaded);
	}

	static public void LoadBanner(int width, int height, BannerPos bannerPos, bool hideWhenLoaded = false)
	{
		LoadBanner (BannerType.Custom, width, height, bannerPos, hideWhenLoaded);
	}
		
	static void LoadBanner(BannerType type, int width, int height, BannerPos bannerPos, bool hideWhenLoaded)
	{
		if (_bannerView != null) 
		{
			DestroyBanner ();
		}

		AdSize adSize = null;
		switch (type) 
		{
		case BannerType.Banner:
			adSize = AdSize.Banner;
			break;

		case BannerType.MediumRectangle:
			adSize = AdSize.MediumRectangle;
			break;

		case BannerType.IABBanner:
			adSize = AdSize.IABBanner;
			break;

		case BannerType.Leaderboard:
			adSize = AdSize.Leaderboard;
			break;

		case BannerType.SmartBanner:
			adSize = AdSize.SmartBanner;
			break;

		case BannerType.Custom:
			adSize = new AdSize (width, height);
			break;
		}

		AdPosition adPos = (AdPosition)((int)bannerPos);

		#if UNITY_IOS
		_bannerView = new BannerView(BannerId_iOS, adSize, adPos);
		#elif UNITY_ANDROID
		_bannerView = new BannerView (BannerId_Android, adSize, adPos);
		#endif

		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder ().Build ();
		// Load the banner with the request.
		_bannerView.LoadAd (request);

		EventHandler<EventArgs> temp = null;
		temp = new EventHandler<EventArgs> (((object sender, EventArgs e) => {
			if (hideWhenLoaded)
			{
				HideBanner ();
			}
			else
			{
				_isBannerActive = true;
			}
			_isBannerLoaded = true;
			_bannerView.OnAdLoaded -= temp;
		}));
		_bannerView.OnAdLoaded += temp;

		_bannerView.OnAdFailedToLoad += AdFailedToLoad;
		_bannerView.OnAdLeavingApplication += AdLeavingApplication;
		_bannerView.OnAdLoaded += AdLoaded;
		_bannerView.OnAdOpening += AdOpening;
		_bannerView.OnAdClosed += AdClosed;

	}

	/// <summary>
	/// make sure a banner has alread been loaded before you call
	/// </summary>
	static public AdShowCode ShowBanner()
	{
		if (_bannerView == null) 
		{
			return AdShowCode.NOT_INITIALIZED;
		}
		if (!_isBannerLoaded)
		{
			return AdShowCode.NOT_LOADED;
		}

		_bannerView.Show ();
		_isBannerActive = true;
		return AdShowCode.SUCCESS;
	}

	static public void HideBanner()
	{
		_bannerView.Hide ();
		_isBannerActive = false;
	}

	static public void DestroyBanner()
	{
		if (_bannerView != null) 
		{
			_bannerView.OnAdFailedToLoad -= AdFailedToLoad;
			_bannerView.OnAdLeavingApplication -= AdLeavingApplication;
			_bannerView.OnAdLoaded -= AdLoaded;
			_bannerView.OnAdOpening -= AdOpening;
			_bannerView.OnAdClosed -= AdClosed;

			_bannerView.Destroy ();
			_bannerView = null;
			_isBannerActive = false;
			_isBannerLoaded = false;
		}
	}

	static public void LoadInterstitial()
	{
		if (_interstitial == null) 
		{
			// Initialize an InterstitialAd.
			#if UNITY_IOS
			_interstitial = new InterstitialAd(InterstitialId_iOS);
			#elif UNITY_ANDROID
			_interstitial = new InterstitialAd (InterstitialId_Android);
			#endif

			_interstitial.OnAdFailedToLoad += AdFailedToLoad;
			_interstitial.OnAdLeavingApplication += AdLeavingApplication;
			_interstitial.OnAdLoaded += AdLoaded;
			_interstitial.OnAdOpening += AdOpening;
			_interstitial.OnAdClosed += AdClosed;
		}
			
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder ().Build ();
		// Load the interstitial with the request.
		_interstitial.LoadAd (request);
	}

	static public AdShowCode ShowInterstitial()
	{
		if (_interstitial == null) 
		{
			return AdShowCode.NOT_INITIALIZED;
		}
		
		if (!_interstitial.IsLoaded()) 
		{
			return AdShowCode.NOT_LOADED;
		}

		_interstitial.Show ();
		return AdShowCode.SUCCESS;
	}

	static public void DestroyInterstitial()
	{
		if (_interstitial != null) 
		{
			_interstitial.OnAdFailedToLoad -= AdFailedToLoad;
			_interstitial.OnAdLeavingApplication -= AdLeavingApplication;
			_interstitial.OnAdLoaded -= AdLoaded;
			_interstitial.OnAdOpening -= AdOpening;
			_interstitial.OnAdClosed -= AdClosed;

			_interstitial.Destroy ();
			_interstitial = null;
		}
	}

	static void AdOpening(object sender, EventArgs args)
	{
		AdType type = sender == _bannerView ? AdType.Banner : AdType.Interstitial;

		LogUtil.Log ("AdHelper", "{0} opening", type.ToString ());

		if (_onAdOpening != null)
			_onAdOpening.Invoke (type);
	}

	static void AdClosed(object sender, EventArgs args)
	{
		AdType type = sender == _bannerView ? AdType.Banner : AdType.Interstitial;

		LogUtil.Log ("AdHelper", "{0} closed", type.ToString ());

		if (_onAdClosed != null) 
			_onAdClosed.Invoke (type);
	}

	static void AdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
	{
		AdType type = sender == _bannerView ? AdType.Banner : AdType.Interstitial;

		LogUtil.Log ("AdHelper", "{0} failed to load, {1}", type.ToString (), args.Message);

		if (_onAdFailedToLoad != null)
			_onAdFailedToLoad.Invoke (type, args.Message);
	}

	static void AdLeavingApplication(object sender, EventArgs args)
	{
		AdType type = sender == _bannerView ? AdType.Banner : AdType.Interstitial;

		LogUtil.Log ("AdHelper", "{0} leaving application", type.ToString ());

		if (_onAdLeavingApplication != null)
			_onAdLeavingApplication.Invoke (type);
	}

	static void AdLoaded(object sender, EventArgs args)
	{
		AdType type = sender == _bannerView ? AdType.Banner : AdType.Interstitial;

		LogUtil.Log ("AdHelper", "{0} loaded", type.ToString ());

		if (_onAdLoaded != null)
			_onAdLoaded.Invoke (type);
	}

	static void OnApplicationPause(bool pauseStatus) 
	{		
		if (pauseStatus) 
		{
			Vungle.onPause ();
		} 
		else
		{
			Vungle.onResume ();
		}
	}
}