using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIUnlockFood : UIPanel {

    public UILocalize mTitle;
    public UILocalize mDescription;
    public Image mIcon;
	public Image mCurrencySign;
	public GameObject mAdsObj;
    public Text mCoinCost;
    public Text mMoneyCost;
    public GameObject mOwnedSignObj;
	public Image adsCounts; //广告剩余观看次数

    private FoodItem _mFoodItem;

	private Vector3 _frameVector;

    protected override void Awake()
    {
        base.Awake();
		//恢复购买
 		#if !UNITY_EDITOR
 		if (SDKBridge.GetChannel() != "App Store")
 		{
 			transform.Find("Frame/RestorePurchase").gameObject.SetActive(false);
 		}
 		#endif
		_frameVector = transform.Find ("Frame").gameObject.transform.position;
    }

    protected override void OnPanelShowBegin()
    {
        base.OnPanelShowBegin();
        mTitle.Key = _mFoodItem.LocalizedKey;
        mDescription.Key = _mFoodItem.LocalizedDescriptionKey;
        mIcon.sprite = _mFoodItem.IconForSell;
        mIcon.SetNativeSize();
        if (_mFoodItem.ID == "item_foodpackage")
        {
            mIcon.transform.FindChild("Knot").gameObject.SetActive(true);
        }
        else
        {
            mIcon.transform.FindChild("Knot").gameObject.SetActive(false);
        }
        mCoinCost.text = _mFoodItem.CoinUnlockCost.ToString();

        mCoinCost.transform.parent.gameObject.SetActive(_mFoodItem.Locked);
        mMoneyCost.transform.parent.gameObject.SetActive(_mFoodItem.Locked);
        mOwnedSignObj.SetActive(!_mFoodItem.Locked);

		if (Localization.language == "English")
			DoozyUI.UIManager.PlaySound ("SE02【英】提示家长付费");
		else
			DoozyUI.UIManager.PlaySound ("SE01【中】提示家长付费");

#if !UNITY_EDITOR
		if (!GameData.AdsRemoved && UncleBear.GameUtilities.GetParam("isBannerOpened", "close") == "open" && _mFoodItem.Locked)
		{
			mAdsObj.gameObject.SetActive(true);
		}

		string channel = SDKBridge.GetChannel();
		Sprite currencySign = null;
		string priceText = null;

		if (channel == "App Store")
		{
			if (Localization.language == "ChineseSimplified")
			{
				currencySign = GetCurrencySignByCode("CNY");
				priceText = GetDefaultPrice("CNY");
			}
            else
            {
                currencySign = GetCurrencySignByCode("USD");
                priceText = GetDefaultPrice("USD");
            }
		}
		else if (channel == "google")
		{
			currencySign = GetCurrencySignByCode("USD");
			priceText = GetDefaultPrice("USD");
		}
		else
		{
			currencySign = GetCurrencySignByCode("CNY");
            IAPItem item = IAPHelper.GetIAPItem(_mFoodItem.ID);
			int price = item.GetItemExtension<IOtherIAPBehaviour>().RMBPrice;
			priceText = string.Format("{0}.{1}{2}", price / 100, price % 100 / 10, price % 100 % 10);
		}
		mCurrencySign.sprite = currencySign;
		mMoneyCost.text = priceText;
#endif
		Transform _frame = transform.Find ("Frame").gameObject.transform;

		if (isShowVungleAds()) {
			transform.Find ("Frame/VungleAdsBtn").gameObject.SetActive (true);
			_frame.position = new Vector3 (_frameVector.x, _frameVector.y, _frameVector.z);

			adsCounts.sprite = GetAdsCountSp ();
		} else {
			transform.Find("Frame/VungleAdsBtn").gameObject.SetActive(false);
			_frame.position = new Vector3 (_frameVector.x, _frameVector.y - 1, _frameVector.z);
		}
    }

    private Sprite GetCurrencySignByCode(string currencyCode)
	{
		switch (currencyCode)
		{
		case "USD":
			return AtlasManager.Instance.GetSprite ("UIAtlas/dollar_sign");
		case "CNY":
			return AtlasManager.Instance.GetSprite ("UIAtlas/rmb_sign");
		default:
			return AtlasManager.Instance.GetSprite("UIAtlas/dollar_sign");
		}
	}

	private string GetDefaultPrice(string currencyCode)
	{
        switch (currencyCode)
        {
            case "USD":
                return _mFoodItem.DefaultPrice_USD;
            case "CNY":
                return _mFoodItem.DefaultPrice_CNY;
            default:
                return "1.99";
        }
	}


	private Sprite GetAdsCountSp()
	{
		int _adsCount = getAdsCount ();
		switch (_adsCount)
		{
		case 0:
			return AtlasManager.Instance.GetSprite ("UIFoodAtlas/5");
		case 1:
			return AtlasManager.Instance.GetSprite ("UIFoodAtlas/4");
		case 2:
			return AtlasManager.Instance.GetSprite ("UIFoodAtlas/3");
		case 3:
			return AtlasManager.Instance.GetSprite ("UIFoodAtlas/2");
		case 4:
			return AtlasManager.Instance.GetSprite ("UIFoodAtlas/1");
		default:
			return AtlasManager.Instance.GetSprite("UIFoodAtlas/1");
		}
	}

	protected override void OnPanelHideBegin()
	{
		base.OnPanelHideBegin ();
	}

    protected override void OnPanelHideCompleted()
    {
        base.OnPanelHideCompleted();
//         if (MenuUI.Instance != null)
//         {
//             MenuUI.Instance.ShowMenu();
//         }
    }

    public void SetFoodInfo(FoodItem item)
    {
        _mFoodItem = item;
    }

    void UnlockByCoin()
    {
        if (GameData.Coins >= _mFoodItem.CoinUnlockCost)
        {
            _mFoodItem.Unlock();

            if (_mFoodItem.ID == "item_foodpackage")
            {
                for (int i = 0; i < GameData.FoodItemList.Count; ++i)
                {
                    if (GameData.FoodItemList[i].ID == "item_finished_pizza" || GameData.FoodItemList[i].ID == "item_finished_farfalle"
						|| GameData.FoodItemList[i].ID == "item_finished_icecream")
                    {
                        GameData.FoodItemList[i].Unlock();
                    }
                }
            }

            GameData.Coins -= _mFoodItem.CoinUnlockCost;
            UIPanel panel = UIPanelManager.Instance.GetPanel("UIGameHUD");
            if (panel != null)
                panel.Repaint();

            UIPanelManager.Instance.HidePanel("UIUnlockFood").DoOnHideCompleted((p) => {
                if (MenuUI.Instance != null)
                {
                    MenuUI.Instance.ShowMenu();
                }
            });
        }
        else
        {
            UIPanelManager.Instance.ShowPanel("UIInsufficientCoins");
            this.CallWithDelay(() =>
            {
                UIPanelManager.Instance.HidePanel("UIInsufficientCoins");
            }, 1f);
        }
    }

    void UnlockByMoney()
    {
		if (SDKBridge.GetChannel() == "App Store" && UncleBear.GameUtilities.GetParam ("isParentControl", "open") == "open") {
			UIParentVerification verificationPanel = (UIParentVerification)UIPanelManager.Instance.ShowPanel ("UIParentVerification");
			verificationPanel.SetSuccessCallback (() => {
				#if !UNITY_EDITOR
				if (!IAPHelper.IsPurchaseOngoing)
				IAPHelper.Pay(_mFoodItem.ID);
				#endif
			});
		} 
		else
		{
			if (!IAPHelper.IsPurchaseOngoing)
				IAPHelper.Pay(_mFoodItem.ID);
		}
    }

 	void RestorePurchase()
 	{
 		#if !UNITY_EDITOR && UNITY_IOS
 		if (!IAPHelper.IsPurchaseOngoing)
 		{
 			IAPHelper.Restore ((success) => {
 			LogUtil.LogNoTag("purchase restore {0}", success ? "succeeded" : "failed");
 			});
 		}
 		#endif
 	}

    void CloseUnlockUI()
    {
        UIPanelManager.Instance.HidePanel(this).DoOnHideCompleted((panel) =>
        {
			//显示总体购买UI
			//UIPanelManager.Instance.ShowPanel("UIBuy");

			//显示菜单
			MenuUI.Instance.ShowMenu();
        });
    }

    void OnEnable()
    {
        EventCenter.Instance.RegisterGameEvent("UnlockByCoin", UnlockByCoin);
        EventCenter.Instance.RegisterGameEvent("UnlockByMoney", UnlockByMoney);
        EventCenter.Instance.RegisterGameEvent ("RestorePurchase", RestorePurchase);
        EventCenter.Instance.RegisterGameEvent("CloseUnlockUI", CloseUnlockUI);
		EventCenter.Instance.RegisterGameEvent ("seeVungleAds", seeVungleAds);

#if !UNITY_EDITOR
		IAPHelper.PurchaseSucceededEvent += OnPurchaseSucceeded;
		IAPHelper.PurchaseFailedEvent += OnPurchaseFailed;
#endif
    }

    void OnDisable()
    {
        if (EventCenter.Instance != null)
        {
            EventCenter.Instance.UnregisterGameEvent("UnlockByCoin", UnlockByCoin);
            EventCenter.Instance.UnregisterGameEvent("UnlockByMoney", UnlockByMoney);
            EventCenter.Instance.UnregisterGameEvent ("RestorePurchase", RestorePurchase);
            EventCenter.Instance.UnregisterGameEvent("CloseUnlockUI", CloseUnlockUI);
			EventCenter.Instance.UnregisterGameEvent ("seeVungleAds", seeVungleAds);
        }

		#if !UNITY_EDITOR
		IAPHelper.PurchaseSucceededEvent -= OnPurchaseSucceeded;
		IAPHelper.PurchaseFailedEvent -= OnPurchaseFailed;
		#endif
    }

	void OnPurchaseSucceeded(string internalProductId)
	{
		for (int i = 0; i < GameData.FoodItemList.Count; ++i)
		{
            if (GameData.FoodItemList[i].ID == internalProductId)
            {
                GameData.FoodItemList[i].Unlock();
                GameData.RemoveAds();

                if (internalProductId == "item_foodpackage")
                {
                    for (int j = 0; j < GameData.FoodItemList.Count; ++j)
                    {
                        if (GameData.FoodItemList[j].ID == "item_finished_pizza" || GameData.FoodItemList[j].ID == "item_finished_farfalle"
							|| GameData.FoodItemList[j].ID == "item_finished_icecream")
                        {
                            GameData.FoodItemList[j].Unlock();
                        }
                    }
                }

                UIPanelManager.Instance.HidePanel(this).DoOnHideCompleted((panel) =>
                {
                    if (MenuUI.Instance != null)
                    {
                        MenuUI.Instance.ShowMenu();
                    }
                });

                //if restore purchase
                UIPanel buyUI = UIPanelManager.Instance.GetPanel("UIBuy");
                if (buyUI != null && buyUI.IsActive)
                {
                    UIPanelManager.Instance.HidePanel(buyUI).DoOnHideCompleted((panel) =>
                    {
                        if (MenuUI.Instance != null)
                        {
                            MenuUI.Instance.ShowMenu();
                        }
                    });
                }
            }
        }
	}

	void OnPurchaseFailed(string internalProductId, string reason)
	{
		LogUtil.LogNoTag("purchase failed, product Id: {0}, reason : {1}", internalProductId, reason);
	}

	/**
	 * 	看激励广告
	 **/
	void seeVungleAds(){
		UIPanelManager.Instance.HidePanel(this).DoOnHideCompleted((panel) =>
			{
				//播放激励广告
				#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
				AdHelper.PlayVungleIncentivizedAd("Are you sure to close?",
				"Please be aware closing this video early will prevent you from getting the reward.",
				"CLOSE ANYWAY",
				"KEEP WATCHING");
				#endif
			});
	}

	bool isShowVungleAds()
	{
		if (AdHelper.IsVungleAdAvailable () == false)
			return false;

		if (SDKBridge.GetChannel () == "App Store" && UncleBear.GameUtilities.GetParam ("isIncentiveAds", "close") == "close") {
			return false;
		}

		string _lastDate = getAdsDate ();

		if (_lastDate == "") {
			updateAdsDate ();
			return true;
		}else {

			System.DateTime _start = System.Convert.ToDateTime(_lastDate);
			System.DateTime _now = System.DateTime.Now;

			System.TimeSpan   ts1=new System.TimeSpan(_start.Ticks);

			System.TimeSpan  ts2=new System.TimeSpan(_now.Ticks);

			System.TimeSpan  tsSub=ts1.Subtract(ts2).Duration();

			int _dayCount = getAdsCount ();

			if (tsSub.Days == 0 && _dayCount >= Consts.SEE_VUNGLE_ADS_DAY_COUNT) {
				return false;
			} else if (tsSub.Days == 0 && _dayCount < Consts.SEE_VUNGLE_ADS_DAY_COUNT) {
				updateAdsDate ();
				return true;
			} else if (tsSub.Days >= 1) {
				updateAdsDate ();
				PlayerPrefs.SetInt (Consts.SEE_VUNGLE_ADS_COUNT,0);
				return true;
			}
		}
		return false;
	}

	void updateAdsDate()
	{
		PlayerPrefs.SetString (Consts.SEE_VUNGLE_ADS_DATE,System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
	}

	string getAdsDate()
	{
		return PlayerPrefs.GetString (Consts.SEE_VUNGLE_ADS_DATE);
	}

	int getAdsCount()
	{
		return PlayerPrefs.GetInt (Consts.SEE_VUNGLE_ADS_COUNT);
	}

	public void updateAdsCount()
	{
		int _sumCount = getAdsCount () + 1;
		PlayerPrefs.SetInt (Consts.SEE_VUNGLE_ADS_COUNT,_sumCount);
	}

}
