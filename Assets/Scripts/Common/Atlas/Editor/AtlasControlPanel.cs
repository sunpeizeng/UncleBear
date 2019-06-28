using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AtlasControlPanel : EditorWindow
{
    const int SPRITE_NO_CHANGE = 0;
    const int SPRITE_UPDATE = 1;
    const int SPRITE_ADD = 2;
    const int SPRITE_MISSING = 3;

    const string ETC_RGB4 = "RGB ETC 4 bits";
    const string ETC2_RGB4 = "RGB ETC2 4 bits";
    const string ETC2_RGB4_PUNCHTHROUGH_ALPHA = "RGB + 1-bit Alpha ETC2 4 bits";
    const string ETC2_RGBA8 = "RGBA ETC2 8 bits";

    const string PVRTC_RGB2 = "RGB PVRTC 2 bits";
    const string PVRTC_RGBA2 = "RGBA PVRTC 2 bits";
    const string PVRTC_RGB4 = "RGB PVRTC 4 bits";
    const string PVRTC_RGBA4 = "RGBA PVRTC 4 bits";

    const string RGB16 = "RGB 16 bits";
    const string RGB24 = "RGB 24 bits";
    const string RGBA16 = "RGBA 16 bits";
    const string RGBA32 = "RGBA 32 bits";

    const string atlasPrefabPath = "Assets/Resources/UI/Atlas/";

    public static AtlasControlPanel Instance;

    Atlas _mAtlas;
    string _mAtlasToCreate = string.Empty;
    List<string> _mSpriteNamesDelete = new List<string>();
    Dictionary<string, string> _mSpriteRenames = new Dictionary<string, string>();
    SortedDictionary<string, Sprite> _mSpritesSelected = new SortedDictionary<string, Sprite>();

    string _mHighlight;
    Vector2 _mScrollPos;

    SerializedObject _mSO_atlas;
    SerializedProperty _mSP_atlasSprites;
    SerializedProperty _mSP_atlasSpriteNames;
    SerializedProperty _mSP_atlasOverrideAndroid;
    SerializedProperty _mSP_atlasOverrideiOS;
    SerializedProperty _mSP_atlasAndroidTextureFormat;
    SerializedProperty _mSP_atlasiOSTextureFormat;

    bool _mOverrideAndroid;
    bool _mOverrideiOS;
    int _mAndroidTextureFormat;
    int _miOSTextureFormat;

    string[] _mAndroidTextureFormatOptions = new string[]
    {
        ETC_RGB4,
        ETC2_RGB4,
        ETC2_RGB4_PUNCHTHROUGH_ALPHA,
        ETC2_RGBA8,
        RGB16,
        RGB24,
        RGBA16,
        RGBA32,
    };

    string[] _miOSTextureFormatOptions = new string[]
    {
        PVRTC_RGB2,
        PVRTC_RGBA2,
        PVRTC_RGB4,
        PVRTC_RGBA4,
        RGB16,
        RGB24,
        RGBA16,
        RGBA32,
    };

    void OnEnable()
    {
        Instance = this;
        if (_mAtlas != null)
        {
            InitSerializedProperties();
        }
    }

    void OnDisable()
    {
        Instance = null;

        //         if (_mAtlas != null)
        //         {
        //             AssetDatabase.SaveAssets();
        //         }
    }

    void OnSelectionChange()
    {
        _mSpriteNamesDelete.Clear();
        Repaint();
    }

    void InitSerializedProperties()
    {
        _mSO_atlas = new SerializedObject(_mAtlas);
        _mSP_atlasSprites = _mSO_atlas.FindProperty("mSprites");
        _mSP_atlasSpriteNames = _mSO_atlas.FindProperty("mSpriteNames");

        _mSP_atlasOverrideAndroid = _mSO_atlas.FindProperty("mAndroidOverride");
        _mSP_atlasOverrideiOS = _mSO_atlas.FindProperty("miOSOverride");
        _mSP_atlasAndroidTextureFormat = _mSO_atlas.FindProperty("mAndroidTextureFormat");
        _mSP_atlasiOSTextureFormat = _mSO_atlas.FindProperty("miOSTextureFormat");

        _mOverrideAndroid = _mSP_atlasOverrideAndroid.boolValue;
        _mOverrideiOS = _mSP_atlasOverrideiOS.boolValue;
        _mAndroidTextureFormat = _mSP_atlasAndroidTextureFormat.intValue;
        _miOSTextureFormat = _mSP_atlasiOSTextureFormat.intValue;
    }

    void OnGUI()
    {
        if (NGUIEditorTools.DrawHeader("Atlas", true))
        {
            NGUIEditorTools.BeginContents(false);

            EditorGUILayout.BeginHorizontal();
            ComponentSelector.Draw<Atlas>("Atlas", _mAtlas, OnSelectAtlas, true, GUILayout.MinWidth(200f));

            EditorGUI.BeginDisabledGroup(_mAtlas == null);
            if (GUILayout.Button("New", GUILayout.Width(40f)))
                _mAtlas = null;

            if (GUILayout.Button("Fix", GUILayout.Width(40f)))
            {
                List<string> spritesFixed = Fix();
                if (spritesFixed.Count > 0)
                {
                    string allNames = string.Empty;
                    foreach (string spriteName in spritesFixed)
                    {
                        allNames += spriteName + '\n';
                    }
                    string message = string.Format("The following sprites have been fixed:\n\n {0}", allNames);
                    EditorUtility.DisplayDialog("Fix Atlas", message, "Confirm");
                }
                else
                {
                    EditorUtility.DisplayDialog("Fix Atlas", "No sprite needs to be fixed.", "Confirm");
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            NGUIEditorTools.EndContents();

            if (_mAtlas != null)
            {
                _mOverrideAndroid = EditorGUILayout.ToggleLeft("Override Android", _mOverrideAndroid, GUILayout.Width(160));
                if (_mOverrideAndroid)
                    _mAndroidTextureFormat = EditorGUILayout.Popup("Texture Format", _mAndroidTextureFormat, _mAndroidTextureFormatOptions);

                _mOverrideiOS = EditorGUILayout.ToggleLeft("Override iOS", _mOverrideiOS, GUILayout.Width(160));
                if (_mOverrideiOS)
                    _miOSTextureFormat = EditorGUILayout.Popup("Texture Format", _miOSTextureFormat, _miOSTextureFormatOptions);

                if (GUILayout.Button("Apply Settings"))
                {
                    List<string> updatedSprites = ApplySettings();
                    if (updatedSprites.Count > 0)
                    {
                        string paths = string.Empty;
                        foreach (string path in updatedSprites)
                        {
                            paths += path + '\n';
                        }
                        string message = string.Format("The following sprites have been updated to the current settings:\n\n {0}", paths);
                        EditorUtility.DisplayDialog("Apply Settings", message, "Confirm");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Apply Settings", "No sprite needs to be updated.", "Confirm");
                    }
                }

                if (GUILayout.Button("Clear Atlas"))
                {
                    if (EditorUtility.DisplayDialog("Clear Atlas", "Are your sure to clear all sprites?", "Yes", "No"))
                    {
                        List<string> spriteNames = new List<string>();
                        for (int i = 0; i < _mSP_atlasSpriteNames.arraySize; ++i)
                        {
                            spriteNames.Add(_mSP_atlasSpriteNames.GetArrayElementAtIndex(i).stringValue);
                        }
                        DeleteSprites(spriteNames);
                    }
                }

                Debug.Assert(_mSP_atlasSprites.arraySize == _mSP_atlasSpriteNames.arraySize, "_mSP_atlasSprites.arraySize != _mSP_atlasSpriteNames.arraySize");

                Dictionary<string, SpriteStateInfo> spritesState = GetSpritesState();

                EditorGUI.BeginDisabledGroup(!(_mSpriteNamesDelete.Count > 0 || _mSpritesSelected.Count > 0 || _mSpriteRenames.Count > 0));
                GUILayout.Space(20f);
                bool update = GUILayout.Button("Update Atlas");
                EditorGUI.EndDisabledGroup();

                if (spritesState.Count > 0)
                {
                    if (NGUIEditorTools.DrawHeader("Sprites"))
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(3f);
                        GUILayout.BeginVertical();

                        _mScrollPos = GUILayout.BeginScrollView(_mScrollPos);

                        int index = 0;
                        foreach (KeyValuePair<string, SpriteStateInfo> itr in spritesState)
                        {
                            Texture icon = null;
                            string path = null;
                            string prettyName = null;

                            if (itr.Value.mState == SPRITE_MISSING)
                            {
                                prettyName = string.Format("    {0} (missing)", itr.Key);
                            }
                            else
                            {
                                path = AssetDatabase.GetAssetPath(itr.Value.mSprite);
                                icon = AssetDatabase.GetCachedIcon(path);
                                prettyName = string.Format("    {0}", itr.Key);
                            }

                            bool highlight = prettyName == _mHighlight;
                            GUI.backgroundColor = highlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                            GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));
                            GUI.backgroundColor = Color.white;

                            GUILayout.Label((index + 1).ToString(), GUILayout.Width(20));

                            if (itr.Value.mState == SPRITE_MISSING) GUI.color = Color.red;
                            if (GUILayout.Button(new GUIContent(prettyName, icon), "OL TextField", GUILayout.Height(20f)))
                            {
                                _mHighlight = prettyName;
                                if (path != null)
                                    NGUITools.PingAssetInProject(path, true);
                            }
                            if (itr.Value.mState == SPRITE_MISSING) GUI.color = Color.white;

                            if (itr.Value.mState == SPRITE_ADD)
                            {
                                GUI.color = Color.green;
                                GUILayout.Label("Add", GUILayout.Width(27f));
                                GUI.color = Color.white;
                            }
                            else if (itr.Value.mState == SPRITE_UPDATE)
                            {
                                GUI.color = Color.cyan;
                                GUILayout.Label("Update", GUILayout.Width(45f));
                                GUI.color = Color.white;
                            }
                            else
                            {
                                if (_mSpriteRenames.ContainsKey(itr.Key))
                                {
                                    _mSpriteRenames[itr.Key] = GUILayout.TextField(_mSpriteRenames[itr.Key], GUILayout.Width(100f));

                                    GUI.backgroundColor = Color.red;
                                    if (GUILayout.Button("Rename", GUILayout.Width(60f)))
                                    {
                                        RenameSprites(new Dictionary<string, string>() { { itr.Key, _mSpriteRenames[itr.Key] } });
                                        _mSpriteRenames.Remove(itr.Key);
                                    }
                                    GUI.backgroundColor = Color.green;
                                    if (GUILayout.Button("Cancel", GUILayout.Width(60f)))
                                    {
                                        _mSpriteRenames.Remove(itr.Key);
                                    }
                                    GUI.backgroundColor = Color.white;
                                }
                                else if (_mSpriteNamesDelete.Contains(itr.Key))
                                {
                                    GUI.backgroundColor = Color.red;

                                    if (GUILayout.Button("Delete", GUILayout.Width(60f)))
                                    {
                                        DeleteSprites(new List<string>() { itr.Key });
                                        _mSpriteNamesDelete.Remove(itr.Key);
                                        Selection.activeObject = null;
                                    }
                                    GUI.backgroundColor = Color.green;
                                    if (GUILayout.Button("Cancel", GUILayout.Width(60f)))
                                    {
                                        _mSpriteNamesDelete.Remove(itr.Key);
                                    }
                                    GUI.backgroundColor = Color.white;
                                }
                                else
                                {
                                    if (GUILayout.Button("Rename", GUILayout.Width(60f)))
                                    {
                                        _mSpriteRenames.Add(itr.Key, string.Empty);
                                    }

                                    // mark this sprite to be deleted
                                    if (GUILayout.Button("X", GUILayout.Width(25f)))
                                    {
                                        _mSpriteNamesDelete.Add(itr.Key);
                                    }
                                }

                            }
                            GUILayout.EndHorizontal();

                            ++index;
                        }

                        if (update)
                        {
                            //delete sprites
                            DeleteSprites(_mSpriteNamesDelete);
                            _mSpriteNamesDelete.Clear();

                            //rename sprites
                            RenameSprites(_mSpriteRenames);
                            _mSpriteRenames.Clear();

                            //add or update sprites
                            AddOrUpdateSprites(_mSpritesSelected);
                            _mSpritesSelected.Clear();

                            Selection.activeObject = null;
                        }

                        GUILayout.EndScrollView();

                        EditorGUILayout.HelpBox("Press ctrl + s to save all changes", MessageType.Info);

                        GUILayout.EndHorizontal();
                        GUILayout.Space(3f);
                        GUILayout.EndVertical();
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("You can create a new atlas by input a name.", MessageType.Info);

                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Atlas Name", GUILayout.Width(100f));
                _mAtlasToCreate = EditorGUILayout.TextArea(_mAtlasToCreate, GUILayout.Width(100f));

                EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_mAtlasToCreate.Trim()));

                GUILayout.Space(20f);

                bool create = GUILayout.Button("Create", GUILayout.Width(100f));

                EditorGUI.EndDisabledGroup();

                GUILayout.EndHorizontal();

                if (create)
                {
                    string prefabName = _mAtlasToCreate.Trim();
                    string prefabPath = atlasPrefabPath + prefabName;

                    if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + ".prefab") != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorUtility.DisplayDialog("Error", string.Format("A prefab '{0}' already exists.", prefabPath), "Confirm");
                        EditorGUILayout.EndHorizontal();

                        Resources.UnloadUnusedAssets();
                        return;
                    }

                    GameObject obj = new GameObject(prefabName);
                    obj.AddComponent<Atlas>();

                    _mAtlas = PrefabUtility.CreatePrefab(prefabPath + ".prefab", obj).GetComponent<Atlas>();
                    InitSerializedProperties();

                    Selection.activeObject = _mAtlas;
                    GameObject.DestroyImmediate(obj);
                }
            }

            if (_mSO_atlas != null && _mSO_atlas.targetObject != null)
            {
                _mSO_atlas.ApplyModifiedProperties();
                //AssetDatabase.SaveAssets();
            }
        }
    }

    void OnSelectAtlas(Object obj)
    {
        if (obj == null)
            return;

        //if _mAtlas been destroyed, need to do a check
        if (_mAtlas == null)
        {
            _mAtlas = null;
        }

        if (_mAtlas != obj)
        {
            _mAtlas = obj as Atlas;
            InitSerializedProperties();
        }

        Repaint();
    }

    class SpriteStateInfo
    {
        public Sprite mSprite;
        public int mState;

        public SpriteStateInfo(Sprite sprite, int state)
        {
            mSprite = sprite;
            mState = state;
        }
    }

    Dictionary<string, SpriteStateInfo> GetSpritesState()
    {
        Dictionary<string, SpriteStateInfo> spritesState = new Dictionary<string, SpriteStateInfo>();

        UpdateSelectedSprites(_mSpritesSelected);
        if (_mSpritesSelected.Count > 0)
        {
            foreach (KeyValuePair<string, Sprite> itr in _mSpritesSelected)
            {
                spritesState[itr.Key] = new SpriteStateInfo(itr.Value, SPRITE_ADD);
            }
        }

        if (_mAtlas != null)
        {
            SortedDictionary<string, SpriteStateInfo> sorted = new SortedDictionary<string, SpriteStateInfo>();
            for (int i = 0; i < _mSP_atlasSprites.arraySize; ++i)
            {
                SerializedProperty sp_sprite = _mSP_atlasSprites.GetArrayElementAtIndex(i);
                Sprite sprite = sp_sprite.objectReferenceValue as Sprite;
                if (sprite != null)
                {
                    if (spritesState.ContainsKey(sprite.name))
                    {
                        spritesState[sprite.name].mState = SPRITE_UPDATE;
                    }
                    else
                    {
                        //spritesState.Add(sprite.name, new SpriteStateInfo(sprite, SPRITE_NO_CHANGE));
                        sorted.Add(sprite.name, new SpriteStateInfo(sprite, SPRITE_NO_CHANGE));
                    }
                }
                else
                {
                    string spriteName = _mSP_atlasSpriteNames.GetArrayElementAtIndex(i).stringValue;
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        //spritesState.Add(spriteName, new SpriteStateInfo(sprite, SPRITE_MISSING));
                        sorted.Add(spriteName, new SpriteStateInfo(sprite, SPRITE_MISSING));
                    }
                }
            }

            foreach (KeyValuePair<string, SpriteStateInfo> itr in sorted)
            {
                spritesState.Add(itr.Key, itr.Value);
            }
        }

        return spritesState;
    }

    void UpdateSelectedSprites(SortedDictionary<string, Sprite> sprites)
    {
        sprites.Clear();
        if (Selection.objects != null && Selection.objects.Length > 0)
        {
            foreach (Object o in Selection.objects)
            {
                Sprite sprite = null;

                if (o is Texture2D || o is Sprite)
                {
                    string path = AssetDatabase.GetAssetPath(o);
                    Sprite tmp = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                    if (tmp == null)
                        continue;

                    TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                    //check if the sprite already got a packing tag
                    if (string.IsNullOrEmpty(ti.spritePackingTag))
                    {
                        sprite = tmp;
                    }

                    Resources.UnloadAsset(ti);
                }

                if (sprite == null) continue;

                bool exclude = false;
                //exclude sprites that already exists in the current atlas
                if (_mSP_atlasSprites != null)
                {
                    for (int i = 0; i < _mSP_atlasSprites.arraySize; ++i)
                    {
                        SerializedProperty sp_sprite = _mSP_atlasSprites.GetArrayElementAtIndex(i);
                        Sprite s = sp_sprite.objectReferenceValue as Sprite;
                        if (s == sprite)
                        {
                            exclude = true;
                            break;
                        }
                    }

                    if (exclude) continue;
                }

                sprites[sprite.name] = sprite;
            }
        }
    }

    void DeleteSprites(List<string> spriteNamesDelete)
    {
        if (spriteNamesDelete.Count > 0)
        {
            int arraySize = _mSP_atlasSprites.arraySize;
            for (int i = 0; i < spriteNamesDelete.Count; ++i)
            {
                for (int j = 0; j < arraySize; ++j)
                {
                    SerializedProperty sp_sprite = _mSP_atlasSprites.GetArrayElementAtIndex(j);
                    Sprite sprite = sp_sprite.objectReferenceValue as Sprite;

                    if (sprite != null && sprite.name == spriteNamesDelete[i])
                    {
                        //set sprite packing tag to empty when removed from atlas
                        UpdateSpriteSettings(sprite, string.Empty);

                        sp_sprite.objectReferenceValue = null;
                        _mSP_atlasSpriteNames.GetArrayElementAtIndex(j).stringValue = string.Empty;
                        break;
                    }
                    else if (sprite == null)
                    {
                        string spriteName = _mSP_atlasSpriteNames.GetArrayElementAtIndex(j).stringValue;
                        if (spriteName == spriteNamesDelete[i])
                        {
                            //delete a missing sprite
                            _mSP_atlasSpriteNames.GetArrayElementAtIndex(j).stringValue = string.Empty;
                            break;
                        }
                    }
                }
            }

            SyncSpriteAndNames();
        }
    }

    void RenameSprites(Dictionary<string, string> renames)
    {
        if (renames.Count > 0)
        {
            bool syncSpriteAndNames = false;

            foreach (KeyValuePair<string, string> itr in renames)
            {
                string renameTo = itr.Value.Trim();
                string renameFrom = itr.Key;

                if (string.IsNullOrEmpty(renameTo))
                    continue;

                bool skip = false;
                for (int i = 0; i < _mSP_atlasSpriteNames.arraySize; ++i)
                {
                    SerializedProperty sp_spriteName = _mSP_atlasSpriteNames.GetArrayElementAtIndex(i);
                    string spriteName = sp_spriteName.stringValue;

                    if (spriteName == renameTo)
                    {
                        Debug.LogError(string.Format("Failed to rename: the atlas already contains sprite '{0}'", spriteName));
                        skip = true;
                        break;
                    }
                }

                if (skip) continue;

                for (int i = 0; i < _mSP_atlasSprites.arraySize; ++i)
                {
                    SerializedProperty sp_sprite = _mSP_atlasSprites.GetArrayElementAtIndex(i);
                    Sprite sprite = sp_sprite.objectReferenceValue as Sprite;

                    if (sprite != null && renameFrom == sprite.name)
                    {
                        string path = AssetDatabase.GetAssetPath(sprite);
                        string error = AssetDatabase.RenameAsset(path, renameTo);
                        if (string.IsNullOrEmpty(error))
                        {
                            syncSpriteAndNames = true;
                        }
                        else
                        {
                            Debug.LogError(string.Format("Renaming sprite '{0}' to '{1}' error: {2}", renameFrom, renameTo, error));
                            continue;
                        }
                    }
                }
            }

            if (syncSpriteAndNames)
                SyncSpriteAndNames();
        }
    }

    void SyncSpriteAndNames()
    {
        Debug.Assert(_mSP_atlasSprites != null
            && _mSP_atlasSpriteNames != null
            && _mSP_atlasSprites.arraySize == _mSP_atlasSpriteNames.arraySize,
            "_mSP_atlasSprites.arraySize != _mSP_atlasSpriteNames.arraySize");

        List<string> names = new List<string>();
        List<Sprite> sprites = new List<Sprite>();

        int arraySize = _mSP_atlasSprites.arraySize;
        for (int i = 0; i < arraySize; ++i)
        {
            SerializedProperty sp_name = _mSP_atlasSpriteNames.GetArrayElementAtIndex(i);
            SerializedProperty sp_sprite = _mSP_atlasSprites.GetArrayElementAtIndex(i);
            Sprite sprite = sp_sprite.objectReferenceValue as Sprite;

            if (sprite == null && sp_name.stringValue == string.Empty)
            {
                continue;
            }

            names.Add(sprite == null ? sp_name.stringValue : sprite.name);
            sprites.Add(sp_sprite.objectReferenceValue as Sprite);
        }

        _mSP_atlasSpriteNames.ClearArray();
        for (int i = 0; i < names.Count; ++i)
        {
            _mSP_atlasSpriteNames.InsertArrayElementAtIndex(i);
            _mSP_atlasSpriteNames.GetArrayElementAtIndex(i).stringValue = names[i];
        }

        _mSP_atlasSprites.ClearArray();
        for (int i = 0; i < sprites.Count; ++i)
        {
            _mSP_atlasSprites.InsertArrayElementAtIndex(i);
            _mSP_atlasSprites.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        //         EditorUtility.SetDirty(_mAtlas);
        //         AssetDatabase.SaveAssets();
    }

    //     public class SpriteNameComparer : IComparer<string>
    //     {
    //         public int Compare(string x, string y)
    //         {
    //             return x.CompareTo(y);
    //         }
    //     }

    void AddOrUpdateSprites(SortedDictionary<string, Sprite> sprites)
    {
        if (sprites.Count > 0)
        {
            //SortedDictionary<string, Sprite> spritesAll = new SortedDictionary<string, Sprite>(new SpriteNameComparer());
            SortedDictionary<string, Sprite> spritesAll = new SortedDictionary<string, Sprite>();

            for (int i = 0; i < _mSP_atlasSprites.arraySize; ++i)
            {
                SerializedProperty sp_sprite = _mSP_atlasSprites.GetArrayElementAtIndex(i);
                Sprite sprite = sp_sprite.objectReferenceValue as Sprite;

                if (sprite != null)
                {
                    spritesAll.Add(sprite.name, sprite);
                }
                else
                {
                    string spriteName = _mSP_atlasSpriteNames.GetArrayElementAtIndex(i).stringValue;
                    if (!string.IsNullOrEmpty(spriteName))
                    {
                        //missing sprites
                        spritesAll.Add(spriteName, null);
                    }
                }
            }

            foreach (KeyValuePair<string, Sprite> itr in sprites)
            {
                spritesAll[itr.Key] = itr.Value;
            }

            _mSP_atlasSprites.ClearArray();
            _mSP_atlasSpriteNames.ClearArray();

            int index = 0;
            foreach (KeyValuePair<string, Sprite> itr in spritesAll)
            {
                _mSP_atlasSprites.InsertArrayElementAtIndex(index);
                _mSP_atlasSprites.GetArrayElementAtIndex(index).objectReferenceValue = itr.Value;

                _mSP_atlasSpriteNames.InsertArrayElementAtIndex(index);
                _mSP_atlasSpriteNames.GetArrayElementAtIndex(index).stringValue = itr.Key;

                UpdateSpriteSettings(itr.Value);

                ++index;
            }

            //             EditorUtility.SetDirty(_mAtlas);
            //             AssetDatabase.SaveAssets();
        }
    }

    List<string> ApplySettings()
    {
        _mSP_atlasOverrideAndroid.boolValue = _mOverrideAndroid;
        if (_mOverrideAndroid)
            _mSP_atlasAndroidTextureFormat.intValue = _mAndroidTextureFormat;

        _mSP_atlasOverrideiOS.boolValue = _mOverrideiOS;
        if (_mOverrideiOS)
            _mSP_atlasiOSTextureFormat.intValue = _miOSTextureFormat;

        List<string> updatedSprites = new List<string>();
        for (int i = 0; i < _mSP_atlasSprites.arraySize; ++i)
        {
            SerializedProperty sp_sprite = _mSP_atlasSprites.GetArrayElementAtIndex(i);
            Sprite sprite = sp_sprite.objectReferenceValue as Sprite;

            if (sprite == null) continue;

            if (UpdateSpriteSettings(sprite))
            {
                updatedSprites.Add(AssetDatabase.GetAssetPath(sprite));
            }
        }

        //         EditorUtility.SetDirty(_mAtlas);
        //         AssetDatabase.SaveAssets();

        return updatedSprites;
    }

    List<string> Fix()
    {
        List<string> fixedSprites = new List<string>();
        for (int i = 0; i < _mSP_atlasSprites.arraySize; ++i)
        {
            SerializedProperty sp_sprite = _mSP_atlasSprites.GetArrayElementAtIndex(i);
            Sprite sprite = sp_sprite.objectReferenceValue as Sprite;

            if (sprite == null) continue;

            SerializedProperty sp_spriteName = _mSP_atlasSpriteNames.GetArrayElementAtIndex(i);

            //fix if sprite name and the name stored in atlas are inconsistent
            if (sprite.name != sp_spriteName.stringValue)
            {
                sp_spriteName.stringValue = sprite.name;
                fixedSprites.Add(sprite.name);
            }
        }

        return fixedSprites;
    }

    bool UpdateSpriteSettings(Sprite sprite)
    {
        List<TextureImporterPlatformSettings> settingsList = GetSpritePlatformSettings(sprite, new string[] { "Android", "iPhone" });
        foreach (TextureImporterPlatformSettings settings in settingsList)
        {
            if (settings.name == "Android")
            {
                settings.overridden = _mOverrideAndroid;
                if (_mOverrideAndroid) settings.format = ConvertToTextureImporterFormat(_mAndroidTextureFormatOptions[_mAndroidTextureFormat]);
            }
            else if (settings.name == "iPhone")
            {
                settings.overridden = _mOverrideiOS;
                if (_mOverrideiOS) settings.format = ConvertToTextureImporterFormat(_miOSTextureFormatOptions[_miOSTextureFormat]);
            }
        }

        return UpdateSpriteSettings(sprite, _mAtlas.gameObject.name, false, true, false, settingsList);
    }
        
    bool UpdateSpriteSettings(Sprite sprite, string packingTag, bool mipmapEnabled = false, bool alphaIsTransparency = true, bool isReadable = false, List<TextureImporterPlatformSettings> settingsList = null)
    {
        string path = AssetDatabase.GetAssetPath(sprite);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        bool reimport = ti.spritePackingTag != packingTag
                || ti.mipmapEnabled != mipmapEnabled
                || ti.alphaIsTransparency != alphaIsTransparency
                || ti.isReadable != isReadable;

        ti.spritePackingTag = packingTag;
        ti.mipmapEnabled = mipmapEnabled;
        ti.alphaIsTransparency = alphaIsTransparency;
        ti.isReadable = isReadable;

        if (settingsList != null)
        {
            foreach (TextureImporterPlatformSettings settings in settingsList)
            {
                if (settings == null) continue;

                TextureImporterPlatformSettings currentSettings = ti.GetPlatformTextureSettings(settings.name);

                if (settings.overridden && currentSettings.overridden)
                {
                    reimport |= currentSettings.format != settings.format;
                }
                else
                {
                    reimport |= settings.overridden != currentSettings.overridden;
                }

                ti.SetPlatformTextureSettings(settings);
            }
        }

        if (reimport)
        {
            AssetDatabase.ImportAsset(path);
        }

        Resources.UnloadAsset(ti);

        return reimport;
    }

    List<TextureImporterPlatformSettings> GetSpritePlatformSettings(Sprite sprite, string[] platforms)
    {
        string path = AssetDatabase.GetAssetPath(sprite);
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        
        List<TextureImporterPlatformSettings> ret = new List<TextureImporterPlatformSettings>();
        foreach(string platform in platforms)
        {
            ret.Add(ti.GetPlatformTextureSettings(platform));
        }

        Resources.UnloadAsset(ti);
        return ret;
    }

    TextureImporterFormat ConvertToTextureImporterFormat(string format)
    {
        switch(format)
        {
            case ETC_RGB4:
                return TextureImporterFormat.ETC_RGB4;
            case ETC2_RGB4:
                return TextureImporterFormat.ETC2_RGB4;
            case ETC2_RGB4_PUNCHTHROUGH_ALPHA:
                return TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA;
            case ETC2_RGBA8:
                return TextureImporterFormat.ETC2_RGBA8;
            case PVRTC_RGB2:
                return TextureImporterFormat.PVRTC_RGB2;
            case PVRTC_RGB4:
                return TextureImporterFormat.PVRTC_RGB4;
            case PVRTC_RGBA2:
                return TextureImporterFormat.PVRTC_RGBA2;
            case PVRTC_RGBA4:
                return TextureImporterFormat.PVRTC_RGBA4;
            case RGB16:
                return TextureImporterFormat.RGB16;
            case RGB24:
                return TextureImporterFormat.RGB24;
            case RGBA16:
                return TextureImporterFormat.RGBA16;
            case RGBA32:
                return TextureImporterFormat.RGBA32;
            default:
                return TextureImporterFormat.RGBA32;
        }
    }

    [MenuItem("Atlas/Atlas Manager")]
    static void ShowAtlasControlPanel()
    {
        EditorWindow.GetWindow<AtlasControlPanel>(false, "Atlas Manager", true).Show();
    }

}
