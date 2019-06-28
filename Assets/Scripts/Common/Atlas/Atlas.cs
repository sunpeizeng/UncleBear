using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atlas : MonoBehaviour
{
    public List<Sprite> mSprites;
    public List<string> mSpriteNames;

#if UNITY_EDITOR
    public bool mAndroidOverride;
    public bool miOSOverride;

    public int mAndroidTextureFormat;
    public int miOSTextureFormat;
#endif

    /// <summary>
    /// 字典的用途主要是考虑到快速访问，提升查找效率
    /// </summary>
    private Dictionary<string, Sprite> _mDict = new Dictionary<string, Sprite>();

    void Awake()
    {
        //构建sprite字典
        foreach (Sprite sprite in mSprites)
        {
            _mDict.Add(sprite.name, sprite);
        }
    }

    public Sprite TryGetSprite(string spriteName)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            foreach (Sprite sprite in mSprites)
            {
                if (sprite.name == spriteName)
                {
                    return sprite;
                }
            }

            return null;
        }
#endif
        if (_mDict.ContainsKey(spriteName))
            return _mDict[spriteName];

        return null;
    }
}