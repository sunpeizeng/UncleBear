using System;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    static public Dictionary<string, string> GameConfigs;

    static public List<FoodItem> FoodItemList;

    static public int Coins
    {
        get
        {
            return PlayerPrefs.GetInt(Consts.SAVEKEY_COINS, int.Parse(GameConfigs["InitCoins"]));
        }
        set
        {
            PlayerPrefs.SetInt(Consts.SAVEKEY_COINS, value);
        }
    }

    static public void Init()
    {
        GameConfigs = new Dictionary<string, string>();
        List<GameConfigEntry> entryList = SerializationManager.LoadFromCSV<GameConfigEntry>("Configs/GameConfigs");
        for (int i = 0; i < entryList.Count; ++i)
        {
            GameConfigs.Add(entryList[i].Key, entryList[i].Value);
        }

        FoodItemList = SerializationManager.LoadFromCSV<FoodItem>("Data/FoodItems");
    }

	static public bool AdsRemoved
	{
		get
		{
			return PlayerPrefs.GetInt (Consts.SAVEKEY_ADS_REMOVED, 0) == 1;
		}
	}

	static public void RemoveAds()
	{
		PlayerPrefs.SetInt (Consts.SAVEKEY_ADS_REMOVED, 1);
		PlayerPrefs.Save ();
	}
}