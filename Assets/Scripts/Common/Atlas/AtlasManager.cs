using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtlasManager : DoozyUI.Singleton<AtlasManager>
{
    private List<Atlas> _mAtlases = new List<Atlas>();

    void Awake()
    {
        foreach (Atlas atlasPrefab in Resources.LoadAll<Atlas>("UI/Atlas"))
        {
            Atlas cloned = Instantiate<Atlas>(atlasPrefab);
            cloned.transform.SetParent(this.transform);
            _mAtlases.Add(cloned);
        }

        Resources.UnloadUnusedAssets();
    }

    public Sprite GetSprite(string spritePath)
    {
        string[] tmp = spritePath.Split('/');
        return GetSprite(tmp[0], tmp[1]);
    }

    public Sprite GetSprite(string atlasName, string spriteName)
    {
        foreach (Atlas atlas in _mAtlases)
        {
            if (atlas.gameObject.name.StartsWith(atlasName))
            {
                return atlas.TryGetSprite(spriteName);
            }
        }
        return null;
    }
}