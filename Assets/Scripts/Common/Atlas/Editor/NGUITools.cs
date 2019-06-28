//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using UnityEditor;
using UnityEngine;

static public class NGUITools
{
    /// <summary>
    /// Helper function that returns the string name of the type.
    /// </summary>

    static public string GetTypeName<T>()
    {
        string s = typeof(T).ToString();
        if (s.StartsWith("UI")) s = s.Substring(2);
        else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
        return s;
    }

    static public void PingAssetInProject(string file, bool selectObject = false)
    {
        if (!file.StartsWith("Assets/"))
        {
            return;
        }

        // thanks to http://answers.unity3d.com/questions/37180/how-to-highlight-or-select-an-asset-in-project-win.html
        var asset = AssetDatabase.LoadMainAssetAtPath(file);
        if (asset != null)
        {
            GUISkin temp = GUI.skin;
            GUI.skin = null;

            //EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(file, typeof(Object)));
            EditorGUIUtility.PingObject(asset);
            if (selectObject) Selection.activeObject = asset;

            GUI.skin = temp;
        }
    }
}