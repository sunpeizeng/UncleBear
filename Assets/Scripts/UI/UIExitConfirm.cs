using UnityEngine.UI;
using UnityEngine;
using DoozyUI;
using UncleBear;

public class UIExitConfirm : UIPanel {

    public GameObject mScreenCover;

    protected override void OnPanelShowBegin()
    {
        base.OnPanelShowBegin();

        mScreenCover.SetActive(true);
    }

    protected override void OnPanelHideCompleted()
    {
        base.OnPanelHideCompleted();

        mScreenCover.SetActive(false);
    }

    void OnEnable()
    {
        EventCenter.Instance.RegisterGameEvent("ExitGame", OnExitGame);
        EventCenter.Instance.RegisterGameEvent("CancelExitGame", OnCancelExitGame);
    }

    void OnDisable()
    {
        if (EventCenter.Instance != null)
        {
            EventCenter.Instance.UnregisterButtonEvent("ExitGame", OnExitGame);
            EventCenter.Instance.UnregisterGameEvent("CancelExitGame", OnCancelExitGame);
        }
    }

    void OnExitGame()
    {
        Application.Quit();
    }

    void OnCancelExitGame()
    {
        this.HideSubElements("UIExitConfirm_Popup", () => {
            UIPanelManager.Instance.HidePanel(this);
        });
    }
}
