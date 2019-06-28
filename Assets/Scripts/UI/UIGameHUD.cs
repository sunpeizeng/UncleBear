using UnityEngine.UI;
using UnityEngine;
using DoozyUI;
using UncleBear;

public class UIGameHUD : UIPanel {

    public Text mCoinNum;

    void OnEnable()
    {
        EventCenter.Instance.RegisterButtonEvent("HomeButton", OnHomeBtnClicked);

		#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		AdHelper.OnAdClosed += this.OnAdClosed;
		#endif
    }

    void OnDisable()
    {
        if (EventCenter.Instance != null)
        {
            EventCenter.Instance.UnregisterButtonEvent("HomeButton", OnHomeBtnClicked);
        }

		#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		AdHelper.OnAdClosed -= this.OnAdClosed;
		#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
#else
         if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            if (IsActive 
                && MenuUI.Instance != null && !MenuUI.Instance.isOpened && !MenuUI.Instance.isAnimating
                && !IsSubElementMoving("UIHomeButton"))
            {
                OnHomeBtnClicked();
            }
        }
    }

    protected override void OnPanelRepaint()
    {
        base.OnPanelRepaint();
        mCoinNum.text = GameData.Coins.ToString();
    }

    protected override void OnPanelShowBegin()
    {
        base.OnPanelShowBegin();
        mCoinNum.text = GameData.Coins.ToString();
        UIManager.PlayMusic("bgm02");
        UIManager.SoundCheck();
    }

    protected override void OnPanelHideBegin()
    {
        base.OnPanelHideBegin();
    }

    protected override void OnPanelShowCompleted()
    {
        base.OnPanelShowCompleted();

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		if (!GameData.AdsRemoved && UncleBear.GameUtilities.GetParam("isBannerOpened", "close") == "open")
		{
			AdHelper.ShowBanner();
		}
#endif
    }

    protected override void OnPanelHideCompleted()
    {
        base.OnPanelHideCompleted();
    }

    void OnHomeBtnClicked()
    {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		AdHelper.HideBanner();
#endif

        (UIPanelManager.Instance.HidePanel("UIGameHUD") as UIPanel).HideSubElements("UIHomeButton");

        UIPanelManager.Instance.ShowPanel("UICover").DoOnShowCompleted((panel) =>
        {
            SpriteFrameAnim uncleAnim = UIPanelManager.Instance.GetPanel("UICover").GetComponentInChildren<SpriteFrameAnim>();
            uncleAnim.Reset();
            uncleAnim.Play();
            UIPanelManager.Instance.ShowPanel("UIStartPanel");
            UIManager.isSoundOn = false;

            if (LevelManager.Instance.CurLevel is LevelMain)
            {
                LevelManager.Instance.CurLevel.ResetLevel();
            }

        });
    }

	void OnApplicationPause(bool pauseStatus) 
	{		
		if (!pauseStatus) 
		{
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
			if (!GameData.AdsRemoved && !IAPHelper.IsPurchaseOngoing && UncleBear.GameUtilities.GetParam("isAdmobOpenedSwich", "close") == "open")
			{
				AdHelper.AdShowCode code = AdHelper.ShowInterstitial();
			}
#endif
		}
	}

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
	void OnAdClosed(AdHelper.AdType adType)
	{
		LogUtil.LogNoTag ("interstitial closed");

		if (adType == AdHelper.AdType.Interstitial && !GameData.AdsRemoved) 
		{
			AdHelper.LoadInterstitial ();
		}
	}
#endif
}
