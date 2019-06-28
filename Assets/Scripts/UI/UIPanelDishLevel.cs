using DoozyUI;
using UnityEngine;
using UnityEngine.UI;
using UncleBear;

public class UIPanelDishLevel : UIPanel {

    public RawImage imgRender;

    void OnEnable()
    {
        imgRender.texture = CameraManager.Instance.RenderCamera.targetTexture;

        EventCenter.Instance.RegisterGameEvent("ReturnToMain", OnReturnBtnClicked);
        EventCenter.Instance.RegisterButtonEvent("ContinueButton", OnContinueBtnClicked);
    }

    void OnDisable()
    {
        if (EventCenter.Instance != null)
        {
            EventCenter.Instance.UnregisterGameEvent("ReturnToMain", OnReturnBtnClicked);
            EventCenter.Instance.UnregisterButtonEvent("ContinueButton", OnContinueBtnClicked);
        }
    }

    protected override void OnPanelShowBegin()
    {
        base.OnPanelShowBegin();
    }

    protected override void OnPanelHideBegin()
    {
        base.OnPanelHideBegin();
    }

    protected override void OnPanelShowCompleted()
    {
        base.OnPanelShowCompleted();
    }

    protected override void OnPanelHideCompleted()
    {
        base.OnPanelHideCompleted();
    }

    void OnReturnBtnClicked()
    {
		#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		if (!GameData.AdsRemoved && UncleBear.GameUtilities.GetParam("isAdmobOpenedCookingFinish", "close") == "open")
		{
			AdHelper.AdShowCode code = AdHelper.ShowInterstitial();
		}
		#endif
			
        UIPanelManager.Instance.HidePanel("UIPanelDishLevel");
        HideSubElements("UIContinueButton");
        HideSubElements("UIReturnButton");

        UIPanelManager.Instance.ShowPanel("UILoading").DoOnShowCompleted((panel) =>
        {
            LevelManager.Instance.ChangeLevel(LevelEnum.Main);
        });
    }

    void OnContinueBtnClicked()
    {
        LevelManager.Instance.ContinueLevel();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
#else
         if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            if (IsActive && !IsShowing
                && UIManager.GetUiElements("UIReturnButton")[0].isVisible
                && !IsSubElementMoving("UIReturnButton"))
            {
                OnReturnBtnClicked();
            }
        }
    }
}
