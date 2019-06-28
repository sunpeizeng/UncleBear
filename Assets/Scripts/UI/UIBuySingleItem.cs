using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBuySingleItem : MonoBehaviour {

    public GameObject mBuyText;
    public GameObject mOwnedSign;
    public string mFoodItemId;

	// Use this for initialization
	void Start () {

        float scaleFactor = (1024f / 768f) / ((float)Screen.width / Screen.height);

        RectTransform rectTrans = GetComponent<RectTransform>();
        Vector3 localScale = rectTrans.localScale;
        localScale *= scaleFactor;
        rectTrans.localScale = localScale;

        Vector2 anchoredPosition = rectTrans.anchoredPosition;
        anchoredPosition.y *= scaleFactor;
        rectTrans.anchoredPosition = anchoredPosition;

        UpdateStatus();
	}
	
	public void UpdateStatus()
    {
        for (int i = 0; i < GameData.FoodItemList.Count; ++i)
        {
            if (GameData.FoodItemList[i].ID == mFoodItemId)
            {
                FoodItem foodItem = GameData.FoodItemList[i];
                if (foodItem.Locked)
                {
                    mBuyText.SetActive(true);
                    mOwnedSign.SetActive(false);
                }
                else
                {
                    mBuyText.SetActive(false);
                    mOwnedSign.SetActive(true);
                }
            }
        }
    }
}
