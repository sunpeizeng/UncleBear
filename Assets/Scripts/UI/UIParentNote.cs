using DoozyUI;
using UnityEngine;
using UnityEngine.UI;
using UncleBear;

public class UIParentNote : UIPanel
{
    public Image imgLogo;
    public Text txtDesc;
    public Text txtContact;

    void OnEnable()
    {
        EventCenter.Instance.RegisterGameEvent("ParentReturn", OnReturnBtnClicked);
    }

    void OnDisable()
    {
        if (EventCenter.Instance != null)
        {
            EventCenter.Instance.UnregisterGameEvent("ParentReturn", OnReturnBtnClicked);
        }
    }

    protected override void OnPanelShowBegin()
    {
        base.OnPanelShowBegin();
        imgLogo.gameObject.SetActive(Localization.language != "English");
    }

    protected override void OnPanelHideBegin()
    {
        base.OnPanelHideBegin();
		#if !UNITY_EDITOR
		AdHelper.HideBanner();
		#endif
    }

    protected override void OnPanelShowCompleted()
    {
        base.OnPanelShowCompleted();
		#if !UNITY_EDITOR
		if (UncleBear.GameUtilities.GetParam("isBannerOpenedParent", "open") == "open")
		{
			AdHelper.ShowBanner();
		}
		#endif
    }

    protected override void OnPanelHideCompleted()
    {
        base.OnPanelHideCompleted();
    }

    void OnReturnBtnClicked()
    {
        UIPanelManager.Instance.HidePanel("UIParentNote");
        HideSubElements("UIReturnButton");
        UIPanelManager.Instance.ShowPanel("UICover").DoOnShowCompleted((panel) =>
        {
            SpriteFrameAnim uncleAnim = UIPanelManager.Instance.GetPanel("UICover").GetComponentInChildren<SpriteFrameAnim>();
            uncleAnim.Reset();
            uncleAnim.Play();
            UIPanelManager.Instance.ShowPanel("UIStartPanel");
        });
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
#else
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            if (IsActive && !IsShowing && !IsSubElementMoving("UIReturnButton"))
                OnReturnBtnClicked();
        }
    }
}
