using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SDKBridge
{
	static AndroidJavaObject _mCurrentActivity = null;

	static public AndroidJavaObject AndroidActivity
	{
		get 
		{
			if (_mCurrentActivity == null) 
			{
				AndroidJavaClass jc = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
				_mCurrentActivity = jc.GetStatic<AndroidJavaObject> ("currentActivity");
			}
			return _mCurrentActivity;
		}
	}

	#if UNITY_IOS
	[DllImport("__Internal")]
	static extern string getPTParam (string key, string defaultVal);
	#endif
	static public string GetPTParam(string key, string defaultVal)
	{
#if UNITY_IOS
		return getPTParam(key, defaultVal);
#elif UNITY_ANDROID
		return AndroidActivity.Call<string>("getParams", key, defaultVal);
#endif
        //返回空值 不让调用出错 2018/8/2
        return null;
	}

	static private string channel = null;
	static public string GetChannel()
	{
#if UNITY_IOS
		return "App Store";
#elif UNITY_ANDROID
		if (channel == null)
			channel = AndroidActivity.Call<string>("getChannel");
		return channel;
#endif
        //返回空值 不让调用出错 2018/8/2
        return null;
	}

	#if UNITY_IOS
	[DllImport("__Internal")]
	static extern void setEvent (string id, string json);
	[DllImport("__Internal")]
	static extern void setEventBegin (string id, string json);
	[DllImport("__Internal")]
	static extern void setEventEnd (string id, string json);
	#endif

	static public void SetEvent(string id, Dictionary<string, string> data)
	{
		string json = MiniJSONV.Json.Serialize (data);
		#if UNITY_IOS
		setEvent(id, json);
		#elif UNITY_ANDROID
		AndroidActivity.Call("setEvent", id, json);
		#endif
	}

	static public void SetEventBegin(string id, Dictionary<string, string> data)
	{
		string json = MiniJSONV.Json.Serialize (data);
		#if UNITY_IOS
		setEventBegin(id, json);
		#elif UNITY_ANDROID
		AndroidActivity.Call("setEventBegin", id, json);
		#endif
	}

	static public void SetEventEnd(string id, Dictionary<string, string> data)
	{
		string json = MiniJSONV.Json.Serialize (data);
		#if UNITY_IOS
		setEventEnd(id, json);
		#elif UNITY_ANDROID
		AndroidActivity.Call("setEventEnd", id, json);
		#endif
	}

	#if UNITY_IOS
	[DllImport("__Internal")]
	static extern void showWebView (string url, float leftMargin, float topMargin, float width, float height);
	[DllImport("__Internal")]
	static extern void removeWebView ();
	#endif

	static public void OpenPromotionWebView(string url, float leftMargin, float topMargin, float width, float height)
	{
		#if UNITY_ANDROID
		AndroidActivity.Call ("showWebView", url, 
			(int)(leftMargin * Screen.width), 
			(int)(topMargin * Screen.height), 
			(int)(width * Screen.width), 
			(int)(height * Screen.height));
		#elif UNITY_IOS
		showWebView(url, leftMargin, topMargin, width, height);
		#endif
	}

	static public void ClosePromotionWebView()
	{
		#if UNITY_ANDROID
		AndroidActivity.Call ("removeWebView");
		#elif UNITY_IOS
		removeWebView();
		#endif
	}
}
