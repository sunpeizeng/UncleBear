using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using MiniJSONV;

public class UIStartPanel : UIPanel
{
	public Image mPromotionImg;

	private bool _mShowPromotion = false;
    private bool _mShowRateUs = false;
    private string _mRateURL = "";
	private string _mChannel = "";

	void Start()
	{
		_mShowPromotion = false;
        _mShowRateUs = false;
		StartCoroutine (RequestPromotionData ());
	}

    IEnumerator RequestPromotionData()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
		_mChannel = "google";
#elif !UNITY_EDITOR && UNITY_IOS
		_mChannel = "appstore";
#elif UNITY_EDITOR
        _mChannel = "google";
#endif

        WWW request = new WWW(string.Format("https://api.biemore.com/api/promotin?appId={0}&channel={1}&new=1", Consts.APP_ID, _mChannel));
        yield return request;

        if (string.IsNullOrEmpty(request.error))
        {
			//request.text = {"img":"https://imgbs.90123.com/children/icon/115/115_promote.png","proID":"115"};
            Dictionary<string, object> response = (Dictionary<string, object>)Json.Deserialize(request.text);

            if (response.ContainsKey("img"))
            {
                WWW imgRequest = new WWW((string)response["img"]);
                yield return imgRequest;

                if (string.IsNullOrEmpty(imgRequest.error))
                {
                    if (imgRequest.texture != null)
                    {
                        int textureWidth = imgRequest.texture.width;
                        int textureHeight = imgRequest.texture.height;

                        mPromotionImg.sprite = Sprite.Create(imgRequest.texture,
                            new Rect(0, 0, textureWidth, textureHeight),
                            new Vector2(0.5f, 0.5f));

                        mPromotionImg.SetNativeSize();
                        _mShowPromotion = true;

                        imgRequest.Dispose();
                    }
                }
            }
        }

        request.Dispose();

#if !UNITY_EDITOR
        if (SDKBridge.GetChannel() == "google" || SDKBridge.GetChannel() == "App Store")
#endif
        {
            request = new WWW(string.Format("https://api.biemore.com/api/getMarketUrl?appId={0}&channel={1}", Consts.APP_ID, _mChannel));

            yield return request;

            if (string.IsNullOrEmpty(request.error))
            {
                _mRateURL = request.text;
                _mShowRateUs = true;
            }

            request.Dispose();
        }
	}

    void OnEnable()
    {
        EventCenter.Instance.RegisterGameEvent("EnterGame", OnEnterGame);
        EventCenter.Instance.RegisterGameEvent("OpenParentNote", OnParentNote);
		EventCenter.Instance.RegisterGameEvent ("OpenPromotionWeb", OnOpenPromotionWebView);
        EventCenter.Instance.RegisterGameEvent("RateUs", OnRateUs);

        //		#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        //		AdHelper.OnVungleAdFinished += (userRedirected, finishedWatching, totalDuration) => {
        //			Debug.Log("Vungle ad - user redirected: " + userRedirected);
        //			Debug.Log("Vungle ad - finished watching: " + finishedWatching);
        //			Debug.Log("Vungle ad - total duration: " + totalDuration);
        //		};
        //		#endif
    }

    void OnDisable()
    {
        if (EventCenter.Instance != null)
        {
            EventCenter.Instance.UnregisterGameEvent("EnterGame", OnEnterGame);
            EventCenter.Instance.UnregisterGameEvent("OpenParentNote", OnParentNote);
			EventCenter.Instance.UnregisterGameEvent ("OpenPromotionWeb", OnOpenPromotionWebView);
        }
    }

	protected override void OnPanelShowBegin()
	{
		base.OnPanelShowBegin ();

		if (DoozyUI.UIManager.GetCurrentMusicName() != "bgm01")
			DoozyUI.UIManager.PlayMusic("bgm01");
	}

	protected override void OnPanelShowCompleted()
	{
		base.OnPanelShowCompleted ();

		if (_mShowPromotion)
			ShowSubElements ("UIStartPanel_Promotion");

		ShowSubElements ("UIStartPanel_Parent");

        if (_mShowRateUs)
        {
            ShowSubElements ("UIStartPanel_RateUs");
        }
	}

	protected override void OnPanelHideBegin()
	{
		base.OnHideBegin ();
	}

    void OnEnterGame()
    {
        HideSubElements ("UIStartPanel_RateUs");
        HideSubElements ("UIStartPanel_Promotion");
		HideSubElements ("UIStartPanel_Parent", () => {
			UIPanelManager.Instance.HidePanel(this).DoOnHideCompleted((panel) =>
				{
					(UIPanelManager.Instance.ShowPanel("UIGameHUD") as UIPanel).ShowSubElements("UIHomeButton");
					UIPanelManager.Instance.HidePanel("UICover");

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
					//			if (AdHelper.IsVungleAdAvailable())
					//			{
					//				AdHelper.PlayVungleIncentivizedAd("Are you sure to close?",
					//					"Please be aware closing this video early will prevent you from getting the reward.",
					//					"CLOSE ANYWAY",
					//					"KEEP WATCHING");
					//			}
#endif
				});
		});
        
    }

    void OnParentNote()
    {
		UnityAction openParentNote = new UnityAction (() => {
            HideSubElements ("UIStartPanel_RateUs");
            HideSubElements ("UIStartPanel_Promotion");
			HideSubElements ("UIStartPanel_Parent", () => {
				UIPanelManager.Instance.HidePanel (this).DoOnHideCompleted ((panel) => {
					(UIPanelManager.Instance.ShowPanel ("UIParentNote") as UIPanel).ShowSubElements ("UIReturnButton");
					UIPanelManager.Instance.HidePanel ("UICover");
				});
			});
		});

#if !UNITY_EDITOR
        if (SDKBridge.GetChannel() == "App Store" && UncleBear.GameUtilities.GetParam ("isParentControl", "open") == "open")
		{
			UIParentVerification verificationPanel = (UIParentVerification)UIPanelManager.Instance.ShowPanel ("UIParentVerification");
			verificationPanel.SetSuccessCallback (() => {
				openParentNote.Invoke();
			});
		} 
		else
#endif
        {
            openParentNote.Invoke ();
		}
    }

	void OnOpenPromotionWebView()
	{
		UnityAction openWebView = new UnityAction (() => {

			UIPanelManager.Instance.ShowPanel("UIPromotionWebView");

			DoozyUI.UIManager.ToggleMusic ();

			string lang = "";
			if (Localization.language == "Chinese") {
				lang = "1";
			} else if (Localization.language == "English") {
				lang = "3";
			} else if (Localization.language == "ChineseTraditional") {
				lang = "2";
			}
			string deviceType = "phone";
			if (SystemInfo.deviceModel.Contains ("pad")) {
				deviceType = "pad";
			}

			string url = string.Format("https://api.biemore.com/promotin?appId={0}&channel={1}&lang={2}&deviceType={3}", Consts.APP_ID, _mChannel, lang, deviceType);

#if !UNITY_EDITOR
			Vector2 uiScreenSize = DoozyUI.UIManager.GetUiContainer.GetComponent<RectTransform> ().sizeDelta;
			SDKBridge.OpenPromotionWebView (url, 0, 50f / uiScreenSize.y, 1f, (uiScreenSize.y - 50f) / uiScreenSize.y);
#endif
		});

#if !UNITY_EDITOR
		if (SDKBridge.GetChannel() == "App Store" && UncleBear.GameUtilities.GetParam ("isParentControl", "open") == "open") 
		{
			UIParentVerification verificationPanel = (UIParentVerification)UIPanelManager.Instance.ShowPanel ("UIParentVerification");
			verificationPanel.SetSuccessCallback (() => {
				openWebView.Invoke();
			});
		} 
		else
#endif
		{
			openWebView.Invoke ();
		}
	}

    void OnRateUs()
    {
#if !UNITY_EDITOR
        if (SDKBridge.GetChannel() == "App Store")
        {
            UIParentVerification verificationPanel = (UIParentVerification)UIPanelManager.Instance.ShowPanel("UIParentVerification");
            verificationPanel.SetSuccessCallback(() =>
            {
                if (!string.IsNullOrEmpty(_mRateURL))
                {
                    Application.OpenURL(_mRateURL);
                }
            });
        }
        else if (SDKBridge.GetChannel() == "google")
        {
            if (!string.IsNullOrEmpty(_mRateURL))
            {
                Application.OpenURL(_mRateURL);
            }
        }
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
            UIPanel promotionWeb = UIPanelManager.Instance.GetPanel("UIPromotionWebView");
            if (IsActive 
                && !IsShowing
                && (promotionWeb == null || !promotionWeb.IsActive)
                && !IsSubElementMoving("UIStartPanel_RateUs") 
                && !IsSubElementMoving("UIStartPanel_Promotion") 
                && !IsSubElementMoving("UIStartPanel_Parent"))
            {
                UIPanel exitPanel = UIPanelManager.Instance.GetPanel("UIExitConfirm");
                if (exitPanel == null || !exitPanel.IsActive)
                {
                    exitPanel = UIPanelManager.Instance.ShowPanel("UIExitConfirm") as UIPanel;
                    exitPanel.ShowSubElements("UIExitConfirm_Popup");
                }
            }
        }
    }
}
