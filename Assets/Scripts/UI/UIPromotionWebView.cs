using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPromotionWebView : UIPanel {

	void OnEnable()
	{
		EventCenter.Instance.RegisterGameEvent ("ClosePromotionWeb", OnClosePromotionWeb);
	}

	void OnDisable()
	{
		if (EventCenter.Instance != null) 
		{
			EventCenter.Instance.UnregisterGameEvent ("ClosePromotionWeb", OnClosePromotionWeb);
		}
	}

	void OnClosePromotionWeb()
	{
		DoozyUI.UIManager.ToggleMusic ();
        UIPanelManager.Instance.HidePanel(this);

		#if !UNITY_EDITOR
		SDKBridge.ClosePromotionWebView ();
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
            if (IsActive && !IsShowing)
                OnClosePromotionWeb();
        }
    }
}
