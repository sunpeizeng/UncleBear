using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlayerPrefTools
{
    [MenuItem("PlayerPref Tools/Clear Saved Data")]
	static public void ClearSavedData()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("PlayerPref Tools/Add 100 Coins")]
    static public void Add100Coins()
    {
        int coins = PlayerPrefs.GetInt(Consts.SAVEKEY_COINS, 0);
        PlayerPrefs.SetInt(Consts.SAVEKEY_COINS, coins + 100);
    }
}
