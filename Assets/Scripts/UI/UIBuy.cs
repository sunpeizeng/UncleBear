using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBuy : UIPanel {

    protected override void Awake()
    {
        base.Awake();
#if !UNITY_EDITOR
        if (SDKBridge.GetChannel() != "App Store")
        {
            transform.Find("Restore").gameObject.SetActive(false);
        }
#endif
    }

    void OnEnable()
    {
        EventCenter.Instance.RegisterGameEvent("CloseBuyUI", OnCloseBuyUI);
        EventCenter.Instance.RegisterGameEvent("BuyFoodpackage", OnBuyFoodpackage);
        EventCenter.Instance.RegisterGameEvent("BuyPizza", OnBuyPizza);
        EventCenter.Instance.RegisterGameEvent("BuyFarfalle", OnBuyFarfalle);
        EventCenter.Instance.RegisterGameEvent ("RestorePurchase", RestorePurchase);
    }

    void OnDisable()
    {
        if (EventCenter.Instance != null)
        {
            EventCenter.Instance.UnregisterGameEvent("CloseBuyUI", OnCloseBuyUI);
            EventCenter.Instance.UnregisterGameEvent("BuyFoodpackage", OnBuyFoodpackage);
            EventCenter.Instance.UnregisterGameEvent("BuyPizza", OnBuyPizza);
            EventCenter.Instance.UnregisterGameEvent("BuyFarfalle", OnBuyFarfalle);
            EventCenter.Instance.UnregisterGameEvent("RestorePurchase", RestorePurchase);
        }
    }

    void OnCloseBuyUI()
    {
        UIPanelManager.Instance.HidePanel(this).DoOnHideCompleted((panel) =>
        {
            MenuUI.Instance.ShowMenu();
        });
    }

    void OnBuyPizza()
    {
        UIPanelManager.Instance.HidePanel(this).DoOnHideCompleted((panel) =>
        {
            UIUnlockFood unlockPanel = UIPanelManager.Instance.ShowPanel("UIUnlockFood") as UIUnlockFood;
            for (int i = 0; i < GameData.FoodItemList.Count; ++i)
            {
                if (GameData.FoodItemList[i].ID == "item_finished_pizza")
                {
                    unlockPanel.SetFoodInfo(GameData.FoodItemList[i]);
                    return;
                }
            }
        });
    }

    void OnBuyFarfalle()
    {
        UIPanelManager.Instance.HidePanel(this).DoOnHideCompleted((panel) =>
        {
            UIUnlockFood unlockPanel = UIPanelManager.Instance.ShowPanel("UIUnlockFood") as UIUnlockFood;
            for (int i = 0; i < GameData.FoodItemList.Count; ++i)
            {
                if (GameData.FoodItemList[i].ID == "item_finished_farfalle")
                {
                    unlockPanel.SetFoodInfo(GameData.FoodItemList[i]);
                    return;
                }
            }
        });
    }

    void OnBuyFoodpackage()
    {
        UIPanelManager.Instance.HidePanel(this).DoOnHideCompleted((panel) =>
        {
            UIUnlockFood unlockPanel = UIPanelManager.Instance.ShowPanel("UIUnlockFood") as UIUnlockFood;
            for (int i = 0; i < GameData.FoodItemList.Count; ++i)
            {
                if (GameData.FoodItemList[i].ID == "item_foodpackage")
                {
                    unlockPanel.SetFoodInfo(GameData.FoodItemList[i]);
                    return;
                }
            }
        });
    }

    void RestorePurchase()
    {
#if !UNITY_EDITOR && UNITY_IOS
        if (!IAPHelper.IsPurchaseOngoing)
        {
            IAPHelper.Restore((success) =>
            {
                LogUtil.LogNoTag("purchase restore {0}", success ? "succeeded" : "failed");
            });
        }
#endif
    }

    protected override void OnPanelShowBegin()
    {
        base.OnPanelShowBegin();

        UIBuySingleItem[] items = GetComponentsInChildren<UIBuySingleItem>();
        for (int i = 0; i < items.Length; ++i)
        {
            items[i].UpdateStatus();
        }
    }
}
