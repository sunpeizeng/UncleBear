using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoading : UIPanel {

	protected override void OnPanelShowBegin()
	{
		base.OnPanelShowBegin();

		#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		AdHelper.HideBanner();
		#endif
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

		#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
		if (!GameData.AdsRemoved && UncleBear.GameUtilities.GetParam("isBannerOpened", "close") == "open")
		{
			AdHelper.ShowBanner();
		}
		#endif
	}
}
