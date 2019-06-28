using System;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : ICSVDeserializable
{
    protected string _mId = null;
    protected bool _mLockAtBeginning = false;
    protected int _mCoinUnlockCost = 0;
	protected string _mLocalizedKey = null;
    protected string _mLocalizedDescriptionKey = null;
    protected Sprite _mIconForSell = null;
	protected string _mDefaultPrice_CNY = "";
	protected string _mDefaultPrice_USD = "";
    protected bool _mShowOnMenu = false;

    public string ID
    {
        get
        {
            return _mId;
        }
    }

    public bool LockAtBeginning
    {
        get
        {
            return _mLockAtBeginning;
        }
    }

    public bool ShowOnMenu
    {
        get
        {
            return _mShowOnMenu;
        }
    }

    public bool Locked
    {
        get
        {
            if (!_mLockAtBeginning)
                return false;

            return PlayerPrefs.GetInt(_mId + "_locked", 1) == 1;
        }
    }

    public int Stars
    {
        get
        {
            return PlayerPrefs.GetInt(_mId + "_stars", 0);
        }
        set
        {
            var oldVal = PlayerPrefs.GetInt(_mId + "_stars", 0);
            if (value > oldVal)
                PlayerPrefs.SetInt(_mId + "_stars", value);
        }
    } 

    public bool PlayedOnce
    {
        get
        {
            return PlayerPrefs.GetInt(_mId + "_playedOnce", 0) == 1;
        }
    }

    public bool Completed
    {
        get
        {
            return PlayerPrefs.GetInt(_mId + "_completed", 0) == 1;
        }
    }

    public int CoinUnlockCost
    {
        get
        {
            return _mCoinUnlockCost;
        }
    }

	public string LocalizedKey
	{
		get
		{
			return _mLocalizedKey;
		}
	}

    public string LocalizedDescriptionKey
    {
        get
        {
            return _mLocalizedDescriptionKey;
        }
    }

    public Sprite IconForSell
    {
        get
        {
            return _mIconForSell;
        }
    }

	public string DefaultPrice_CNY
	{
		get
		{
			return _mDefaultPrice_CNY;
		}
	}

	public string DefaultPrice_USD
	{
		get
		{
			return _mDefaultPrice_USD;
		}
	}

    public void CSVDeserialize(Dictionary<string, string[]> data, int index)
    {
        _mId = data["ID"][index];
        _mLockAtBeginning = data["LockAtBeginning"][index] == "true";
        _mShowOnMenu = data["ShowOnMenu"][index] == "true";
        if (_mLockAtBeginning)
        {
            _mCoinUnlockCost = int.Parse(data["CoinUnlockCost"][index]);
        }
		_mLocalizedKey = data["LocalizedKey"][index];
        _mLocalizedDescriptionKey = data["LocalizedDescriptionKey"][index];

        string spritePath = data["IconForSell"][index];
        if (!string.IsNullOrEmpty(spritePath))
        {
            _mIconForSell = AtlasManager.Instance.GetSprite(spritePath);
        }

		_mDefaultPrice_CNY = data ["DefaultPrice_CNY"][index];
		_mDefaultPrice_USD = data ["DefaultPrice_USD"] [index];
    }

    public void Unlock()
    {
        if (!_mLockAtBeginning)
            return;

        PlayerPrefs.SetInt(_mId + "_locked", 0);
    }

    public void SetPlayedOnce()
    {
        PlayerPrefs.SetInt(_mId + "_playedOnce", 1);
    }

    public void SetCompleted()
    {
        PlayerPrefs.SetInt(_mId + "_completed", 1);
    }
}