using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Atlas), true)]
public class AtlasEditor : Editor
{
    string _mHighlight;
    Vector2 _mScrollPos;

    SerializedProperty _mSP_sprites;
    SerializedProperty _mSP_spriteNames;

    void OnEnable()
    {
        _mSP_sprites = serializedObject.FindProperty("mSprites");
        _mSP_spriteNames = serializedObject.FindProperty("mSpriteNames");
    }

    public override void OnInspectorGUI()
    {
        if (!NGUIEditorTools.DrawHeader("Sprites"))
            return;

        NGUIEditorTools.BeginContents(true);

        GUILayout.BeginHorizontal();
        GUILayout.Space(3f);

        if (_mSP_sprites.arraySize > 0)
            GUILayout.BeginVertical(GUILayout.MinHeight(300f));
        else
            GUILayout.BeginVertical();

        _mScrollPos = GUILayout.BeginScrollView(_mScrollPos);

        for (int i = 0; i < _mSP_sprites.arraySize; ++i)
        {
            string path = null;
            Texture icon = null;
            string prettyName = null;

            Sprite sprite = _mSP_sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
            if (sprite == null)
            {
                string spriteName = _mSP_spriteNames.GetArrayElementAtIndex(i).stringValue;
                prettyName = string.Format("    {0} (missing)", spriteName);
            }
            else
            {
                path = AssetDatabase.GetAssetPath(sprite);
                icon = AssetDatabase.GetCachedIcon(path);
                prettyName = string.Format("    {0}", Path.GetFileNameWithoutExtension(path));
            }

            bool highlight = prettyName == _mHighlight;
            GUI.backgroundColor = highlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
            GUI.backgroundColor = Color.white;

            GUILayout.Label((i + 1).ToString(), GUILayout.Width(20));

            if (sprite == null) GUI.color = Color.red;
            if (GUILayout.Button(new GUIContent(prettyName, icon), "OL TextField", GUILayout.Height(20f)))
            {
                _mHighlight = prettyName;
                if (path != null)
                    NGUITools.PingAssetInProject(path, false);
            }
            if (sprite == null) GUI.color = Color.white;

            GUILayout.EndHorizontal();
        }    

        GUILayout.EndScrollView();

        GUILayout.EndHorizontal();
        GUILayout.Space(3f);
        GUILayout.EndVertical();

        NGUIEditorTools.EndContents();
    }
}