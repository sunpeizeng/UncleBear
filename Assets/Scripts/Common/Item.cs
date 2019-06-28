using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ICSVDeserializable
{
    protected string _mId = null;

    protected Sprite _mIcon = null;

    protected GameObject _mPrefab = null;

    protected string _mDescKey = null;

    protected int _mCount = -1;//-1表示从未获得过该物品，0表示该物品已获得过只是数量为0

    public string ID
    {
        get
        {
            return _mId;
        }
    }

    public Sprite Icon
    {
        get
        {
            return _mIcon;
        }
    }

    public GameObject Prefab
    {
        get
        {
            return _mPrefab;
        }
    }

    public string DescKey
    {
        get
        {
            return _mDescKey;
        }
    }

    public int Count
    {
        get
        {
            _mCount = PlayerPrefs.GetInt(_mId + "_count", -1);
            return _mCount;
        }
        set
        {
            _mCount = value;
            PlayerPrefs.SetInt(_mId + "_count", _mCount);
        }
    }

    public virtual void CSVDeserialize(Dictionary<string, string[]> data, int index)
    {
        _mId = data["ID"][index];

        //Sprite列中的值以AtlasName/SpriteName的形式存在
        string spritePath = data["Icon"][index];
        if (!string.IsNullOrEmpty(spritePath))
        {
            _mIcon = AtlasManager.Instance.GetSprite(spritePath);
        }

        string prefabPath = data["Prefab"][index];
        if (!string.IsNullOrEmpty(prefabPath))
        {
            _mPrefab = Resources.Load<GameObject>(prefabPath);
        }

        _mDescKey = data["Description"][index];
    }
}