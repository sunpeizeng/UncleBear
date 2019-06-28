//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(MaskableGraphic))]
public class UILocalize : MonoBehaviour
{
	/// <summary>
	/// Localization key.
	/// </summary>
	[SerializeField]
	private string key;

	public string Key
	{
		get
		{ 
			return key;
		}
		set
		{
			if (value != this.key) 
			{
				key = value;
				OnLocalize ();
			}
		}
	}

	/// <summary>
	/// Manually change the value of whatever the localization component is attached to.
	/// </summary>

	public string value
	{
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				Text t = GetComponent<Text>();
                if (t != null && t.text != value)
                {
                    t.text = value;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        EditorUtility.SetDirty(t);
                    }
#endif
                }

                Image img = GetComponent<Image>();
                if (img != null)
                {

                    Sprite sprite = null;
#if !UNITY_EDITOR
                    sprite = AtlasManager.Instance.GetSprite(value);
#else
                    if (!Application.isPlaying)
                    {
                        string atlasName = value.Split('/')[0];
                        string spriteName = value.Split('/')[1];

                        Atlas atlas = Resources.Load<Atlas>("UI/Atlas/" + atlasName);
                        if (atlas == null)
                        {
                            LogUtil.LogError("UILocalize", "Atlas < {0} > cannot be found", atlasName);
                        }
                        else
                        {
                            sprite = atlas.TryGetSprite(spriteName);
                        }

                        Resources.UnloadUnusedAssets();
                    }
                    else
                        sprite = AtlasManager.Instance.GetSprite(value);
#endif


                    if (sprite != null && sprite != img.sprite)
                    {
                        img.sprite = sprite;
						img.SetNativeSize ();
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            EditorUtility.SetDirty(img);
                        }
#endif
                    }
                }
            }
        }
	}
		
	void Start ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		OnLocalize();
	}

	/// <summary>
	/// This function is called by the Localization manager via a broadcast SendMessage.
	/// </summary>

	void OnLocalize ()
	{
		// If no localization key has been specified, use the label's text as the key
		if (string.IsNullOrEmpty(key))
		{
			Text t = GetComponent<Text>();
			if (t != null) key = t.text;
		}

		// If we still don't have a key, leave the value as blank
		if (!string.IsNullOrEmpty(key)) value = Localization.Get(key);
	}
}
